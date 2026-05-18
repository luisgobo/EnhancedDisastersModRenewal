using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class TsunamiModel : DisasterBaseModel
    {
        private const float GuaranteedOccurrencePerFrame = 1f;
        private const float SecondsPerMinute = 60f;
        private const float MaxRealTimeDeltaSeconds = 5f;
        private const float CurrentWaveBandLength = 3000f;
        private const float MinFutureEvacuationDistance = 6000f;
        private const float MaxFutureEvacuationDistance = 16000f;
        private const float MinCoastalWaterSearchRadius = 768f;
        private const float MaxCoastalWaterSearchRadius = 1920f;
        private const float MaxCoastalElevationAboveWater = 80f;
        private const string ExtendedInfoPanel2ModKey = "extendedInfoPanel2";

        private static readonly RealTimeDisasterFrequencyPreset[] RealTimeTsunamiFrequencyOptionValues =
        {
            RealTimeDisasterFrequencyPreset.Apocalypse,
            RealTimeDisasterFrequencyPreset.Frequent,
            RealTimeDisasterFrequencyPreset.Occasional,
            RealTimeDisasterFrequencyPreset.Uncommon,
            RealTimeDisasterFrequencyPreset.Rare
        };

        [XmlIgnore] private float _lastRealTimeScheduleUpdateSeconds = -1f;
        [XmlIgnore] public float RealTimeCurrentTsunamiPeriodMinutes = -1f;
        [XmlIgnore] public float RealTimeMinutesUntilNextTsunami = -1f;

        public RealTimeDisasterFrequencyPreset RealTimeTsunamiFrequency =
            RealTimeDisasterFrequencyPreset.Occasional;

        public TsunamiModel()
        {
            DType = DisasterType.Tsunami;
            BaseOccurrencePerYear = 1.0f;
            ProbabilityDistribution = ProbabilityDistributions.PowerLow;
            WarmupYears = 4;
        }

        public float WarmupYears
        {
            get { return probabilityWarmupDays / 360f; }

            set
            {
                probabilityWarmupDays = (int)(360 * value);
                intensityWarmupDays = probabilityWarmupDays / 2;
                calmDays = probabilityWarmupDays;
            }
        }

        protected override void OnSimulationFrameLocal()
        {
            if (IsRealTimePatternActive())
                UpdateRealTimeTsunamiSchedule();
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            if (IsRealTimePatternActive())
                return IsRealTimeTsunamiDue()
                    ? 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame
                    : 0f;

            return IsTsunamiCharged()
                ? 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame
                : 0f;
        }

        protected override float GetSimulationDaysPerFrame()
        {
            return IsRealTimePatternActive()
                ? DisasterSimulationUtils.VanillaSimulationDaysPerFrame
                : base.GetSimulationDaysPerFrame();
        }

        public override byte GetMaximumIntensity()
        {
            return IsRealTimePatternActive()
                ? ScaleIntensityByPopulation(baseIntensity)
                : base.GetMaximumIntensity();
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.Tsunami;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Tsunami;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.FinishOnDeactivate = false;
            disasterInfoUnified.IgnoreDestructionZone = true;

            if (!IsEvacuating())
                base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Tsunami;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.FinishOnDeactivate = false;
            disasterInfoUnified.IgnoreDestructionZone = true;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterFinished(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            // TODO: Split tsunami evacuation behavior into two explicit modes.
            // Current tsunami automatic evacuation behaves like "auto evacuate/release": it starts evacuation
            // on detection and releases shelters when the tsunami finishes. Preserve this functional flow as
            // the future auto-evacuate/release option, then add a separate auto-evacuate-only option that
            // starts the selected shelters but leaves citizen release under manual/player control.
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Tsunami;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.FinishOnDeactivate = true;
            disasterInfoUnified.IgnoreDestructionZone = true;

            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterStarted(byte intensity)
        {
            if (IsRealTimePatternActive())
            {
                ResetRealTimeSchedule();
                ClearRealTimeCooldownState();
                return;
            }

            base.OnDisasterStarted(intensity);
        }

        public override string GetProbabilityTooltip(float value)
        {
            if (IsRealTimePatternActive() && calmDaysLeft <= 0)
                return GetRealTimeProbabilityTooltip(value);

            return base.GetProbabilityTooltip(value);
        }

        public override void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel,
            ref List<DisasterInfoModel> activeDisasters)
        {
            // TODO: When tsunami gets separate auto-evacuate and auto-evacuate/release options, keep the
            // existing SetupTsunamiEvacuation path for auto-evacuate/release and route the new auto-evacuate-only
            // option through the same shelter selection without automatic release on finish.
            SetupTsunamiEvacuation(disasterInfoModel, null, ref activeDisasters);
        }

        protected override void SetupAutomaticFocusedEvacuation(DisasterInfoModel disasterInfoModel,
            float disasterRadius, ref List<DisasterInfoModel> activeDisasters)
        {
            SetupTsunamiEvacuation(disasterInfoModel, disasterRadius, ref activeDisasters);
        }

        private void SetupTsunamiEvacuation(DisasterInfoModel disasterInfoModel, float? focusedRadius,
            ref List<DisasterInfoModel> activeDisasters)
        {
            if (NaturalDisasterHandler.GetDisasterInfo(DType) == null)
                return;

            var disasterTargetPosition = new Vector3(
                disasterInfoModel.DisasterInfo.targetX,
                disasterInfoModel.DisasterInfo.targetY,
                disasterInfoModel.DisasterInfo.targetZ);

            var buildingManager = Services.Buildings;
            var serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);
            if (serviceBuildings == null)
                return;

            var focusedEvacuationRadius = focusedRadius.HasValue ? (float)Math.Sqrt(focusedRadius.Value) : 0f;

            for (var i = 0; i < serviceBuildings.m_size; i++)
            {
                var shelterId = serviceBuildings.m_buffer[i];
                if (shelterId == 0)
                    continue;

                var buildingInfo = buildingManager.m_buildings.m_buffer[shelterId];
                if (buildingInfo.Info == null)
                    continue;

                var shelterAI = buildingInfo.Info.m_buildingAI as ShelterAI;
                if (shelterAI == null)
                    continue;

                var shelterPosition = buildingInfo.m_position;
                float priority;
                if (!CanTsunamiAffectShelter(disasterInfoModel.DisasterId, shelterPosition, disasterTargetPosition,
                        out priority))
                    continue;

                float shelterRadius =
                    (buildingInfo.Length < buildingInfo.Width ? buildingInfo.Width : buildingInfo.Length) * 8 / 2f;

                if (focusedRadius.HasValue &&
                    !IsShelterInDisasterZone(disasterTargetPosition, shelterPosition, shelterRadius,
                        focusedEvacuationRadius))
                    continue;

                disasterInfoModel.ShelterList.Add(shelterId);
                SetBuidingEvacuationStatus(shelterAI, shelterId, ref buildingManager.m_buildings.m_buffer[shelterId],
                    false);

                DebugLogger.Log(string.Format(
                    "Tsunami evacuation enabled for shelter {0}. DisasterId: {1}, Priority: {2:0.000}, Focused: {3}",
                    shelterId,
                    disasterInfoModel.DisasterId,
                    priority,
                    focusedRadius.HasValue));
            }

            if (disasterInfoModel.ShelterList.Count == 0)
            {
                DebugLogger.Log(string.Format(
                    "No shelters found inside tsunami affectation range. DisasterId: {0}",
                    disasterInfoModel.DisasterId));
                return;
            }

            AddOrReplaceActiveDisaster(disasterInfoModel, ref activeDisasters);
        }

        private bool CanTsunamiAffectShelter(ushort disasterId, Vector3 shelterPosition,
            Vector3 disasterTargetPosition, out float priority)
        {
            priority = 0f;

            var disasterManager = Services.Disasters;
            if (disasterManager == null || disasterId >= disasterManager.m_disasters.m_buffer.Length)
                return false;

            var disasterData = disasterManager.m_disasters.m_buffer[disasterId];
            if (CanAffectAt(disasterId, ref disasterData, shelterPosition, disasterTargetPosition, out priority))
                return true;

            if (CanTsunamiReachShelterLater(ref disasterData, shelterPosition, disasterTargetPosition, out priority))
                return true;

            return IsCoastalShelterAtRisk(shelterPosition, disasterData.m_intensity, out priority);
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as TsunamiAI != null;
        }

        public override string GetName()
        {
            return LocalizationService.GetDisasterName(DType);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            var tsunami = disaster as TsunamiModel;
            if (tsunami != null)
            {
                WarmupYears = tsunami.WarmupYears;
                SetRealTimeTsunamiFrequency(tsunami.RealTimeTsunamiFrequency);
            }
        }

        public override bool CanAffectAt(ushort disasterID, ref DisasterData disasterData,
            Vector3 buildingPosition, Vector3 closestShelter, out float priority)
        {
            var itCanAffect = base.CanAffectAt(disasterID, ref disasterData, buildingPosition, new Vector3(),
                out priority);
            if (!itCanAffect)
                return false;

            var simulationFrame = Services.Simulation.m_currentFrameIndex;
            var disasterStartFrame = disasterData.m_startFrame;

            var dot1 = 0f - Mathf.Sin(disasterData.m_angle);
            var dot3 = Mathf.Cos(disasterData.m_angle);
            var rhsVector = new Vector3(dot1, 0f, dot3);
            var lhsVector = buildingPosition - closestShelter;

            var num = Vector3.Dot(lhsVector, rhsVector);
            var num2 = simulationFrame - disasterStartFrame;
            var num3 = num2 * 0.125f - 3000f;
            var num4 = num2 * 0.125f;
            priority = Mathf.Clamp01(Mathf.Min((num4 - num) * 0.01f, (num - num3) * 0.0005f));
            return num >= num3 && num <= num4;
        }

        private bool CanTsunamiReachShelterLater(ref DisasterData disasterData, Vector3 shelterPosition,
            Vector3 disasterTargetPosition, out float priority)
        {
            priority = 0f;

            if ((disasterData.m_flags &
                 (DisasterData.Flags.Emerging | DisasterData.Flags.Active | DisasterData.Flags.Clearing)) == 0)
                return false;

            var waveDirection = GetTsunamiWaveDirection(disasterData.m_angle);
            var projectedDistance = Vector3.Dot(shelterPosition - disasterTargetPosition, waveDirection);
            var futureReachDistance = GetFutureEvacuationDistance(disasterData.m_intensity);

            if (projectedDistance < -CurrentWaveBandLength || projectedDistance > futureReachDistance)
                return false;

            priority = Mathf.Clamp01(1f - Mathf.Max(0f, projectedDistance) / futureReachDistance);
            return true;
        }

        private static Vector3 GetTsunamiWaveDirection(float angle)
        {
            return new Vector3(0f - Mathf.Sin(angle), 0f, Mathf.Cos(angle));
        }

        private static float GetFutureEvacuationDistance(byte intensity)
        {
            return Mathf.Lerp(
                MinFutureEvacuationDistance,
                MaxFutureEvacuationDistance,
                Mathf.Clamp01(intensity / 255f));
        }

        private static bool IsCoastalShelterAtRisk(Vector3 shelterPosition, byte intensity, out float priority)
        {
            priority = 0f;

            var terrain = Services.Terrain;
            if (terrain == null)
                return false;

            var terrainHeight = terrain.SampleRawHeightSmooth(shelterPosition);
            var waterSurfaceHeight = terrain.SampleRawHeightSmoothWithWater(shelterPosition, false, 0f);
            var waterDepthAtShelter = waterSurfaceHeight - terrainHeight;
            if (waterDepthAtShelter > 0f)
            {
                priority = 1f;
                return true;
            }

            var searchRadius = Mathf.Lerp(
                MinCoastalWaterSearchRadius,
                MaxCoastalWaterSearchRadius,
                Mathf.Clamp01(intensity / 255f));
            var waterDistance = terrain.CalculateWaterProximity(shelterPosition, searchRadius, out waterSurfaceHeight);
            if (waterDistance >= searchRadius)
                return false;

            var elevationAboveWater = terrainHeight - waterSurfaceHeight;
            if (elevationAboveWater > MaxCoastalElevationAboveWater)
                return false;

            var distanceFactor = Mathf.Clamp01(1f - waterDistance / searchRadius);
            var elevationFactor = Mathf.Clamp01(1f - Mathf.Max(0f, elevationAboveWater) / MaxCoastalElevationAboveWater);
            priority = Mathf.Clamp01(Mathf.Max(0.25f, distanceFactor * elevationFactor));
            return true;
        }

        public bool IsRealTimePatternActive()
        {
            return DisasterSimulationUtils.IsRealTimeModActive();
        }

        public override void OnEnabledChanged(bool enabled)
        {
            _lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        public float GetProbabilityProgress()
        {
            if (IsRealTimePatternActive())
                return GetRealTimePatternProbabilityProgress();

            if (calmDaysLeft > 0)
                return 0f;

            if (probabilityWarmupDays <= 0 || probabilityWarmupDaysLeft <= 0f)
                return 1f;

            return Mathf.Clamp01(1f - probabilityWarmupDaysLeft / probabilityWarmupDays);
        }

        private bool IsTsunamiCharged()
        {
            return probabilityWarmupDays <= 0 || probabilityWarmupDaysLeft <= 0f;
        }

        public float GetRealTimePatternProbabilityProgress()
        {
            EnsureRealTimeSchedule();
            return Mathf.Clamp01(1f - RealTimeMinutesUntilNextTsunami / RealTimeCurrentTsunamiPeriodMinutes);
        }

        private string GetRealTimeProbabilityTooltip(float probabilityValue)
        {
            if (!unlocked) return LocalizationService.Get("tooltip.not_unlocked");

            EnsureRealTimeSchedule();
            return string.Format(
                "{0}: {1}{2}{3}{2}{4}{2}{5}{2}{6}",
                LocalizationService.Get("tooltip.progress"),
                string.Format("{0:00.00}%", probabilityValue * 100),
                CommonProperties.NewLine,
                LocalizationService.Get("tooltip.tsunami.realtime_active"),
                LocalizationService.Format("tooltip.tsunami.realtime_reference", GetRealTimeTsunamiFrequencyName()),
                LocalizationService.Format(
                    "tooltip.tsunami.current_tsunami_interval",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeCurrentTsunamiPeriodMinutes)),
                LocalizationService.Format(
                    "tooltip.tsunami.tsunami_time_remaining",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeMinutesUntilNextTsunami)));
        }

        private void UpdateRealTimeTsunamiSchedule()
        {
            EnsureRealTimeSchedule();

            RealTimeMinutesUntilNextTsunami = Mathf.Max(
                0f,
                RealTimeMinutesUntilNextTsunami - GetRealTimeElapsedMinutes());

            ClearRealTimeCooldownState();
        }

        private bool IsRealTimeTsunamiDue()
        {
            EnsureRealTimeSchedule();
            return RealTimeMinutesUntilNextTsunami <= 0f;
        }

        private void EnsureRealTimeSchedule()
        {
            if (RealTimeCurrentTsunamiPeriodMinutes <= 0f || RealTimeMinutesUntilNextTsunami < 0f)
                ScheduleNextRealTimeTsunami();

            if (RealTimeMinutesUntilNextTsunami > RealTimeCurrentTsunamiPeriodMinutes)
                RealTimeMinutesUntilNextTsunami = RealTimeCurrentTsunamiPeriodMinutes;
        }

        private void ResetRealTimeSchedule()
        {
            ScheduleNextRealTimeTsunami();
        }

        private void ScheduleNextRealTimeTsunami()
        {
            ScheduleNextRealTimeTsunami(0f);
        }

        private void ScheduleNextRealTimeTsunami(float progressToKeep)
        {
            var progress = Mathf.Clamp01(progressToKeep);
            RealTimeCurrentTsunamiPeriodMinutes = GetRandomRealTimeIntervalMinutes();
            RealTimeMinutesUntilNextTsunami = RealTimeCurrentTsunamiPeriodMinutes * (1f - progress);
            _lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        private void ClearRealTimeCooldownState()
        {
            calmDaysLeft = 0f;
            probabilityWarmupDaysLeft = 0f;
            intensityWarmupDaysLeft = 0f;
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
            switch (RealTimeTsunamiFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    minMinutes = 120f;
                    maxMinutes = 240f;
                    break;
                case RealTimeDisasterFrequencyPreset.Frequent:
                    minMinutes = 240f;
                    maxMinutes = 480f;
                    break;
                case RealTimeDisasterFrequencyPreset.Occasional:
                default:
                    minMinutes = 480f;
                    maxMinutes = 960f;
                    break;
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    minMinutes = 960f;
                    maxMinutes = 1920f;
                    break;
                case RealTimeDisasterFrequencyPreset.Rare:
                    minMinutes = 1920f;
                    maxMinutes = 3840f;
                    break;
            }
        }

        public void SetRealTimeTsunamiFrequency(RealTimeDisasterFrequencyPreset frequency)
        {
            if (RealTimeTsunamiFrequency == frequency)
                return;

            var currentProgress = GetRealTimePatternProbabilityProgress();
            RealTimeTsunamiFrequency = frequency;
            ScheduleNextRealTimeTsunami(currentProgress);
        }

        public override void SetDebugProbabilityProgress(float progress)
        {
            base.SetDebugProbabilityProgress(progress);

            if (IsRealTimePatternActive())
            {
                ScheduleNextRealTimeTsunami(progress);
                ClearRealTimeCooldownState();
            }
        }

        public static string[] GetRealTimeTsunamiFrequencyOptions()
        {
            return new[]
            {
                LocalizationService.Get("settings.tsunami.frequency.apocalypse"),
                LocalizationService.Get("settings.tsunami.frequency.frequent"),
                LocalizationService.Get("settings.tsunami.frequency.occasional"),
                LocalizationService.Get("settings.tsunami.frequency.uncommon"),
                LocalizationService.Get("settings.tsunami.frequency.rare")
            };
        }

        public int GetRealTimeTsunamiFrequencySelectionIndex()
        {
            for (var i = 0; i < RealTimeTsunamiFrequencyOptionValues.Length; i++)
                if (RealTimeTsunamiFrequencyOptionValues[i] == RealTimeTsunamiFrequency)
                    return i;

            return 2;
        }

        public static RealTimeDisasterFrequencyPreset GetRealTimeTsunamiFrequencyFromSelection(int selection)
        {
            if (selection < 0 || selection >= RealTimeTsunamiFrequencyOptionValues.Length)
                return RealTimeDisasterFrequencyPreset.Occasional;

            return RealTimeTsunamiFrequencyOptionValues[selection];
        }

        public string GetRealTimeTsunamiFrequencyTooltip()
        {
            return LocalizationService.Format(
                "settings.tsunami.realtime_frequency.tooltip.selected",
                GetRealTimeTsunamiFrequencyName());
        }

        private string GetRealTimeTsunamiFrequencyName()
        {
            switch (RealTimeTsunamiFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    return LocalizationService.Get("settings.tsunami.frequency_name.apocalypse");
                case RealTimeDisasterFrequencyPreset.Frequent:
                    return LocalizationService.Get("settings.tsunami.frequency_name.frequent");
                case RealTimeDisasterFrequencyPreset.Occasional:
                    return LocalizationService.Get("settings.tsunami.frequency_name.occasional");
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    return LocalizationService.Get("settings.tsunami.frequency_name.uncommon");
                case RealTimeDisasterFrequencyPreset.Rare:
                    return LocalizationService.Get("settings.tsunami.frequency_name.rare");
                default:
                    return RealTimeTsunamiFrequency.ToString();
            }
        }
    }
}
