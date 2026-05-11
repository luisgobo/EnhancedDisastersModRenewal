using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class ForestFireModel : DisasterBaseModel
    {
        private const float GuaranteedOccurrencePerFrame = 1f;
        private const float SecondsPerMinute = 60f;
        private const float MaxRealTimeDeltaSeconds = 5f;
        private const float DenseFogThreshold = 0.6f;
        private const float LightFogDryProgressFactor = 0.5f;
        private const float DenseFogDryProgressFactor = 0.25f;
        private const string ExtendedInfoPanel2ModKey = "extendedInfoPanel2";
        private static readonly RealTimeDisasterFrequencyPreset[] RealTimeForestFireFrequencyOptionValues =
        {
            RealTimeDisasterFrequencyPreset.Apocalypse,
            RealTimeDisasterFrequencyPreset.Frequent,
            RealTimeDisasterFrequencyPreset.Occasional,
            RealTimeDisasterFrequencyPreset.Uncommon,
            RealTimeDisasterFrequencyPreset.Rare
        };

        private int _warmupDays = 180;

        [XmlIgnore] private float _lastRealTimeScheduleUpdateSeconds = -1f;

        [XmlIgnore] public float NoRainDays;
        [XmlIgnore] public float RealTimeCurrentDryPeriodMinutes = -1f;
        [XmlIgnore] public float RealTimeMinutesUntilNextForestFire = -1f;
        public bool FogRetardsDryTime;
        public RealTimeDisasterFrequencyPreset RealTimeForestFireFrequency =
            RealTimeDisasterFrequencyPreset.Occasional;

        public int WarmupDays
        {
            get => _warmupDays;
            set => _warmupDays = Math.Max(1, value);
        }

        public ForestFireModel()
        {
            DType = DisasterType.ForestFire;
            OccurrenceAreaBeforeUnlock = OccurrenceAreas.LockedAreas;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.Everywhere;
            BaseOccurrencePerYear = 10.0f; // In case of dry weather
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            calmDays = 7;
            probabilityWarmupDays = 0;
            intensityWarmupDays = 0;
        }

        protected override void OnSimulationFrameLocal()
        {
            if (IsRealTimePatternActive())
            {
                UpdateRealTimeDrySchedule();
                return;
            }

            var wm = Services.Weather;
            if (wm.m_currentRain > 0)
            {
                NoRainDays = 0;
                return;
            }

            if (IsDryTimePausedByDenseFog())
                return;

            NoRainDays += DisasterSimulationUtils.DaysPerFrame * GetFogDryProgressFactor();
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.ForestFire;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
            DetectActiveForestFire(disasterId);
        }

        private void DetectActiveForestFire(ushort disasterId)
        {
            if (!Enabled || disasterId == 0)
                return;

            var dm = Services.Disasters;
            var flags = dm.m_disasters.m_buffer[disasterId].m_flags;
            if ((flags & (DisasterData.Flags.Active | DisasterData.Flags.Detected | DisasterData.Flags.UnDetected)) !=
                DisasterData.Flags.Active)
                return;

            dm.DetectDisaster(disasterId, true);
            DebugLogger.Log(GetDebugStr() + string.Format(
                "Forest Fire detected after activation. Id: {0}, Flags: {1}",
                disasterId,
                dm.m_disasters.m_buffer[disasterId].m_flags));
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.ForestFire;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.ForestFire;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel,
            ref List<DisasterInfoModel> activeDisasters)
        {
            //Get disaster Info
            var disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);

            if (disasterInfo == null)
                return;

            //Identify Shelters
            var buildingManager = Services.Buildings;
            var serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);

            if (serviceBuildings == null)
                return;

            //Release all shelters but Potentyally destroyed
            for (var i = 0; i < serviceBuildings.m_size; i++)
            {
                var num = serviceBuildings.m_buffer[i];
                if (num != 0)
                {
                    //here we got all shelter buildings
                    var buildingInfo = buildingManager.m_buildings.m_buffer[num];

                    if (buildingInfo.Info.m_buildingAI as ShelterAI != null)
                    {
                        //Add Building/Shelter Data to disaster
                        disasterInfoModel.ShelterList.Add(num);
                        SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num,
                            ref buildingManager.m_buildings.m_buffer[num], false);
                    }
                }
            }

            AddOrReplaceActiveDisaster(disasterInfoModel, ref activeDisasters);
        }

        public override void OnDisasterStarted(byte intensity)
        {
            if (IsRealTimePatternActive())
                ResetRealTimeDrySchedule();

            base.OnDisasterStarted(intensity);
        }

        public override string GetProbabilityTooltip(float value)
        {
            var tooltip = "";

            if (!unlocked) tooltip = LocalizationService.Get("tooltip.forest_fire.locked") + Environment.NewLine;

            if (calmDaysLeft <= 0)
            {
                if (IsRealTimePatternActive())
                    return GetRealTimeProbabilityTooltip(tooltip, value);

                if (IsDryTimePausedByDenseFog())
                    return tooltip + LocalizationService.Get("tooltip.forest_fire.fog_paused");

                if (NoRainDays <= 0)
                    return tooltip + LocalizationService.Format("tooltip.forest_fire.no_during_rain", GetName());

                var fogLine = GetFogRetardantTooltipLine();
                if (NoRainDays >= WarmupDays)
                    return tooltip + LocalizationService.Format("tooltip.forest_fire.maximum_no_rain", WarmupDays) +
                           fogLine;

                return tooltip + LocalizationService.Format("tooltip.forest_fire.increasing_no_rain",
                    DisasterSimulationUtils.FormatTimeSpan(NoRainDays)) + fogLine;
            }

            return base.GetProbabilityTooltip(value);
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            if (IsRealTimePatternActive())
            {
                if (IsDryTimePausedByDenseFog())
                    return 0f;

                return IsRealTimeForestFireDue()
                    ? 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame
                    : 0f;
            }

            if (IsDryTimePausedByDenseFog())
                return 0f;

            return base.GetCurrentOccurrencePerYearLocal() * Math.Min(1f, NoRainDays / WarmupDays);
        }

        protected override float GetSimulationDaysPerFrame()
        {
            return IsRealTimePatternActive()
                ? DisasterSimulationUtils.VanillaSimulationDaysPerFrame
                : base.GetSimulationDaysPerFrame();
        }

        protected override bool FindTarget(DisasterInfo disasterInfo, out Vector3 targetPosition, out float angle)
        {
            if (TryFindRandomTreeTarget(out targetPosition, out angle))
            {
                DebugLogger.Log(GetDebugStr() + string.Format(
                    "Forest Fire target selected from living tree. Target: x:{0:#.##} y:{1:#.##} z:{2:#.##}",
                    targetPosition.x,
                    targetPosition.y,
                    targetPosition.z));
                return true;
            }

            LogDisasterStartFailure("Forest Fire target: no valid living tree found in the configured occurrence area");
            return base.FindTarget(disasterInfo, out targetPosition, out angle);
        }

        private bool TryFindRandomTreeTarget(out Vector3 targetPosition, out float angle)
        {
            targetPosition = Vector3.zero;
            angle = 0f;

            var treeManager = Singleton<TreeManager>.instance;
            var simulation = Services.Simulation;
            if (treeManager == null || simulation == null)
                return false;

            var trees = treeManager.m_trees.m_buffer;
            uint selectedTree = 0;
            var validTreeCount = 0;

            for (uint treeId = 1; treeId < trees.Length; treeId++)
            {
                var tree = trees[treeId];
                if ((tree.m_flags & 0x43) != 1 || tree.GrowState == 0)
                    continue;

                if (!IsTreeTargetAllowed(tree.Position))
                    continue;

                validTreeCount++;
                if (simulation.m_randomizer.Int32((uint)validTreeCount) == 0)
                    selectedTree = treeId;
            }

            if (selectedTree == 0)
                return false;

            targetPosition = trees[selectedTree].Position;
            angle = simulation.m_randomizer.Int32(0, 10000) * 0.0006283185f;
            return true;
        }

        private bool IsTreeTargetAllowed(Vector3 position)
        {
            var area = unlocked ? OccurrenceAreaAfterUnlock : OccurrenceAreaBeforeUnlock;

            switch (area)
            {
                case OccurrenceAreas.Everywhere:
                    return true;

                case OccurrenceAreas.UnlockedAreas:
                    return TryGetAreaCoordinates(position, out var unlockedX, out var unlockedZ) &&
                           IsAreaUnlocked(unlockedX, unlockedZ);

                case OccurrenceAreas.LockedAreas:
                    return TryGetAreaCoordinates(position, out var lockedX, out var lockedZ) &&
                           !IsAreaUnlocked(lockedX, lockedZ);

                default:
                    return false;
            }
        }

        private static bool TryGetAreaCoordinates(Vector3 position, out int areaX, out int areaZ)
        {
            var gameArea = Services.GameArea;
            for (var x = 0; x < 5; x++)
            for (var z = 0; z < 5; z++)
            {
                float minX;
                float minZ;
                float maxX;
                float maxZ;
                gameArea.GetAreaBounds(x, z, out minX, out minZ, out maxX, out maxZ);
                if (position.x >= minX && position.x <= maxX && position.z >= minZ && position.z <= maxZ)
                {
                    areaX = x;
                    areaZ = z;
                    return true;
                }
            }

            areaX = 0;
            areaZ = 0;
            return false;
        }

        private static bool IsAreaUnlocked(int areaX, int areaZ)
        {
            return areaX >= 0 && areaZ >= 0 && areaX < 5 && areaZ < 5 &&
                   Services.GameArea.m_areaGrid[areaZ * 5 + areaX] != 0;
        }

        public bool IsRealTimePatternActive()
        {
            return DisasterSimulationUtils.IsRealTimeModActive();
        }

        public float GetRealTimePatternProbabilityProgress()
        {
            if (IsRaining())
                return 0f;

            EnsureRealTimeDrySchedule();
            return Mathf.Clamp01(1f - RealTimeMinutesUntilNextForestFire / RealTimeCurrentDryPeriodMinutes);
        }

        private string GetRealTimeProbabilityTooltip(string prefix, float probabilityValue)
        {
            if (IsRaining())
                return prefix + LocalizationService.Format("tooltip.forest_fire.no_during_rain", GetName());

            EnsureRealTimeDrySchedule();
            var fogLine = GetFogRetardantTooltipLine();
            return string.Format(
                "{0}{1}: {2}{3}{4}{3}{5}{3}{6}{7}",
                prefix,
                LocalizationService.Get("tooltip.progress"),
                string.Format("{0:00.00}%", probabilityValue * 100),
                CommonProperties.NewLine,
                LocalizationService.Get("tooltip.forest_fire.realtime_active"),
                LocalizationService.Format("tooltip.forest_fire.realtime_reference", GetRealTimeForestFireFrequencyName()),
                LocalizationService.Format(
                    "tooltip.forest_fire.current_dry_interval",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeCurrentDryPeriodMinutes)) +
                CommonProperties.NewLine +
                LocalizationService.Format(
                    "tooltip.forest_fire.dry_time_remaining",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeMinutesUntilNextForestFire)),
                fogLine);
        }

        private void UpdateRealTimeDrySchedule()
        {
            if (IsRaining())
            {
                NoRainDays = 0f;
                InvalidateRealTimeDrySchedule();
                return;
            }

            EnsureRealTimeDrySchedule();
            var elapsedMinutes = GetRealTimeElapsedMinutes();
            if (IsDryTimePausedByDenseFog())
            {
                NoRainDays = WarmupDays * GetRealTimePatternProbabilityProgress();
                return;
            }

            RealTimeMinutesUntilNextForestFire = Mathf.Max(
                0f,
                RealTimeMinutesUntilNextForestFire - elapsedMinutes * GetFogDryProgressFactor());

            NoRainDays = WarmupDays * GetRealTimePatternProbabilityProgress();
        }

        private bool IsRealTimeForestFireDue()
        {
            if (IsRaining())
                return false;

            if (IsDryTimePausedByDenseFog())
                return false;

            EnsureRealTimeDrySchedule();
            return RealTimeMinutesUntilNextForestFire <= 0f;
        }

        private void EnsureRealTimeDrySchedule()
        {
            if (RealTimeCurrentDryPeriodMinutes <= 0f || RealTimeMinutesUntilNextForestFire < 0f)
                ScheduleNextRealTimeForestFire();

            if (RealTimeMinutesUntilNextForestFire > RealTimeCurrentDryPeriodMinutes)
                RealTimeMinutesUntilNextForestFire = RealTimeCurrentDryPeriodMinutes;
        }

        private void ResetRealTimeDrySchedule()
        {
            NoRainDays = 0f;
            ScheduleNextRealTimeForestFire();
        }

        private void InvalidateRealTimeDrySchedule()
        {
            RealTimeCurrentDryPeriodMinutes = -1f;
            RealTimeMinutesUntilNextForestFire = -1f;
            _lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        private void ScheduleNextRealTimeForestFire()
        {
            ScheduleNextRealTimeForestFire(0f);
        }

        private void ScheduleNextRealTimeForestFire(float progressToKeep)
        {
            var progress = Mathf.Clamp01(progressToKeep);
            RealTimeCurrentDryPeriodMinutes = GetRandomRealTimeIntervalMinutes();
            RealTimeMinutesUntilNextForestFire = RealTimeCurrentDryPeriodMinutes * (1f - progress);
            _lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        private float GetRealTimeElapsedMinutes()
        {
            var currentSeconds = Time.realtimeSinceStartup;

            if (_lastRealTimeScheduleUpdateSeconds < 0f)
            {
                _lastRealTimeScheduleUpdateSeconds = currentSeconds;
                return 0f;
            }

            var elapsedSeconds = Mathf.Clamp(
                currentSeconds - _lastRealTimeScheduleUpdateSeconds,
                0f,
                MaxRealTimeDeltaSeconds);
            _lastRealTimeScheduleUpdateSeconds = currentSeconds;
            return elapsedSeconds / SecondsPerMinute * GetRealTimeSpeedFactor();
        }

        private static float GetRealTimeSpeedFactor()
        {
            var simulation = Services.Simulation;
            if (simulation == null || simulation.SimulationPaused)
                return 0f;

            var speed = Mathf.Max(1, simulation.FinalSimulationSpeed);
            if (ModCompatibilityService.IsActive(ExtendedInfoPanel2ModKey))
                return GetExtendedInfoPanelSpeedFactor(speed);

            return GetVanillaSpeedFactor(speed);
        }

        private static float GetVanillaSpeedFactor(int speed)
        {
            if (speed <= 1)
                return 1f;

            return speed == 2 ? 1.5f : 2f;
        }

        private static float GetExtendedInfoPanelSpeedFactor(int speed)
        {
            if (speed <= 3)
                return GetVanillaSpeedFactor(speed);

            switch (speed)
            {
                case 4:
                    return 2.5f;
                case 5:
                    return 3f;
                default:
                    return 3.5f;
            }
        }

        private float GetRandomRealTimeIntervalMinutes()
        {
            GetRealTimeIntervalRange(out var minMinutes, out var maxMinutes);

            var randomValue = Services.Simulation.m_randomizer.Int32(0, 10000) / 10000f;
            return minMinutes + (maxMinutes - minMinutes) * randomValue;
        }

        private void GetRealTimeIntervalRange(out float minMinutes, out float maxMinutes)
        {
            switch (RealTimeForestFireFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    minMinutes = 10f;
                    maxMinutes = 30f;
                    break;
                case RealTimeDisasterFrequencyPreset.Frequent:
                    minMinutes = 30f;
                    maxMinutes = 90f;
                    break;
                case RealTimeDisasterFrequencyPreset.Occasional:
                default:
                    minMinutes = 60f;
                    maxMinutes = 180f;
                    break;
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    minMinutes = 180f;
                    maxMinutes = 360f;
                    break;
                case RealTimeDisasterFrequencyPreset.Rare:
                    minMinutes = 360f;
                    maxMinutes = 720f;
                    break;
            }
        }

        private static bool IsRaining()
        {
            var wm = Services.Weather;
            return wm != null && wm.m_currentRain > 0;
        }

        private static float GetCurrentFog()
        {
            var wm = Services.Weather;
            return wm == null ? 0f : Mathf.Clamp01(wm.m_currentFog);
        }

        private bool IsDryTimePausedByDenseFog()
        {
            return FogRetardsDryTime && GetCurrentFog() >= DenseFogThreshold;
        }

        private float GetFogDryProgressFactor()
        {
            if (!FogRetardsDryTime)
                return 1f;

            var fog = GetCurrentFog();
            if (fog <= 0f)
                return 1f;

            if (fog >= DenseFogThreshold)
                return 0f;

            return Mathf.Lerp(LightFogDryProgressFactor, DenseFogDryProgressFactor, fog / DenseFogThreshold);
        }

        private string GetFogRetardantTooltipLine()
        {
            if (!FogRetardsDryTime || GetCurrentFog() <= 0f)
                return string.Empty;

            if (IsDryTimePausedByDenseFog())
                return CommonProperties.NewLine + LocalizationService.Get("tooltip.forest_fire.fog_paused");

            return CommonProperties.NewLine + LocalizationService.Format(
                "tooltip.forest_fire.fog_slowed",
                string.Format("{0:0}", GetFogDryProgressFactor() * 100));
        }

        public void SetRealTimeForestFireFrequency(RealTimeDisasterFrequencyPreset frequency)
        {
            if (RealTimeForestFireFrequency == frequency)
                return;

            var currentProgress = GetRealTimePatternProbabilityProgress();
            RealTimeForestFireFrequency = frequency;
            ScheduleNextRealTimeForestFire(currentProgress);
        }

        public static string[] GetRealTimeForestFireFrequencyOptions()
        {
            return new[]
            {
                LocalizationService.Get("settings.forest_fire.frequency.apocalypse"),
                LocalizationService.Get("settings.forest_fire.frequency.frequent"),
                LocalizationService.Get("settings.forest_fire.frequency.occasional"),
                LocalizationService.Get("settings.forest_fire.frequency.uncommon"),
                LocalizationService.Get("settings.forest_fire.frequency.rare")
            };
        }

        public int GetRealTimeForestFireFrequencySelectionIndex()
        {
            for (var i = 0; i < RealTimeForestFireFrequencyOptionValues.Length; i++)
                if (RealTimeForestFireFrequencyOptionValues[i] == RealTimeForestFireFrequency)
                    return i;

            return 2;
        }

        public static RealTimeDisasterFrequencyPreset GetRealTimeForestFireFrequencyFromSelection(int selection)
        {
            if (selection < 0 || selection >= RealTimeForestFireFrequencyOptionValues.Length)
                return RealTimeDisasterFrequencyPreset.Occasional;

            return RealTimeForestFireFrequencyOptionValues[selection];
        }

        public string GetRealTimeForestFireFrequencyTooltip()
        {
            return LocalizationService.Format(
                "settings.forest_fire.realtime_frequency.tooltip.selected",
                GetRealTimeForestFireFrequencyName());
        }

        private string GetRealTimeForestFireFrequencyName()
        {
            switch (RealTimeForestFireFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    return LocalizationService.Get("settings.forest_fire.frequency_name.apocalypse");
                case RealTimeDisasterFrequencyPreset.Frequent:
                    return LocalizationService.Get("settings.forest_fire.frequency_name.frequent");
                case RealTimeDisasterFrequencyPreset.Occasional:
                    return LocalizationService.Get("settings.forest_fire.frequency_name.occasional");
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    return LocalizationService.Get("settings.forest_fire.frequency_name.uncommon");
                case RealTimeDisasterFrequencyPreset.Rare:
                    return LocalizationService.Get("settings.forest_fire.frequency_name.rare");
                default:
                    return RealTimeForestFireFrequency.ToString();
            }
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as ForestFireAI != null;
        }

        public override string GetName()
        {
            return LocalizationService.GetDisasterName(DType);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            if (!(disaster is ForestFireModel forestFireModel)) return;
            
            WarmupDays = forestFireModel.WarmupDays;
            FogRetardsDryTime = forestFireModel.FogRetardsDryTime;
            RealTimeForestFireFrequency = forestFireModel.RealTimeForestFireFrequency;
        }
    }
}
