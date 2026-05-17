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
    public class EarthquakeModel : DisasterBaseModel
    {
        private const float GuaranteedOccurrencePerFrame = 2f;
        private const float SecondsPerMinute = 60f;
        private const float MaxRealTimeDeltaSeconds = 5f;
        private const int NearbyAftershockTargetAttempts = 16;
        private const float MinimumAftershockRadius = 128f;
        private const float MaximumAftershockRadius = 960f;
        private const float AftershockRadiusPerIntensityPoint = 10f;
        private const string ExtendedInfoPanel2ModKey = "extendedInfoPanel2";
        private static readonly RealTimeDisasterFrequencyPreset[] RealTimeEarthquakeFrequencyOptionValues =
        {
            RealTimeDisasterFrequencyPreset.Apocalypse,
            RealTimeDisasterFrequencyPreset.Frequent,
            RealTimeDisasterFrequencyPreset.Occasional,
            RealTimeDisasterFrequencyPreset.Uncommon,
            RealTimeDisasterFrequencyPreset.Rare
        };

        [XmlIgnore] private float _lastRealTimeScheduleUpdateSeconds = -1f;
        [XmlIgnore] private readonly HashSet<ushort> _pendingDetectionEarthquakes = new HashSet<ushort>();
        [XmlIgnore] public byte aftershockMaxIntensity;
        [XmlIgnore] public byte aftershocksCount;
        public bool AftershocksEnabled = true;
        public EarthquakeCrackOptions EarthquakeCrackMode = EarthquakeCrackOptions.NoCracks;
        [XmlIgnore] public float lastAngle;
        [XmlIgnore] public Vector3 lastTargetPosition;
        [XmlIgnore] public byte mainStrikeIntensity;
        public float MinimalIntensityForCracks = 12.0f;
        [XmlIgnore] public float RealTimeCurrentAftershockPeriodMinutes = -1f;
        [XmlIgnore] public float RealTimeCurrentSeismicPeriodMinutes = -1f;
        public RealTimeDisasterFrequencyPreset RealTimeEarthquakeFrequency =
            RealTimeDisasterFrequencyPreset.Occasional;
        [XmlIgnore] public float RealTimeMinutesUntilNextAftershock = -1f;
        [XmlIgnore] public float RealTimeMinutesUntilNextEarthquake = -1f;

        [XmlIgnore] private bool NoCracksInTheGroud;

        public EarthquakeModel()
        {
            DType = DisasterType.Earthquake;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
            BaseOccurrencePerYear = 1.0f;
            ProbabilityDistribution = ProbabilityDistributions.PowerLow;

            WarmupYears = 3;
        }

        [XmlElement]
        public float WarmupYears
        {
            get => probabilityWarmupDays / 360f;

            set
            {
                probabilityWarmupDays = (int)(360 * value);
                intensityWarmupDays = probabilityWarmupDays / 2;
                calmDays = probabilityWarmupDays / 2;
            }
        }

        public override string GetProbabilityTooltip(float value)
        {
            if (aftershocksCount > 0)
            {
                if (IsRealTimePatternActive())
                    return GetRealTimeAftershockProbabilityTooltip(value);

                return LocalizationService.Format("tooltip.earthquake.aftershocks", aftershocksCount);
            }

            if (IsRealTimePatternActive())
                return GetRealTimeProbabilityTooltip(value);

            return base.GetProbabilityTooltip(value);
        }

        protected override void OnSimulationFrameLocal()
        {
            RetryPendingEarthquakeDetections();

            if (!IsRealTimePatternActive())
                return;

            ClearRealTimeCooldownState();

            if (aftershocksCount > 0)
            {
                UpdateRealTimeAftershockSchedule();
                return;
            }

            UpdateRealTimeEarthquakeSchedule();
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            if (aftershocksCount > 0)
                return IsRealTimePatternActive()
                    ? IsRealTimeAftershockDue()
                        ? 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame
                        : 0f
                    : 12 * aftershocksCount;

            if (IsRealTimePatternActive())
                return IsRealTimeEarthquakeDue()
                    ? 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame
                    : 0f;

            return IsEarthquakeCharged()
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
            disasterInfo.type |= DisasterType.Earthquake;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
            DetectActiveEarthquake(disasterId);
        }

        protected override void AfterStartDisaster(ushort disasterId)
        {
            if (!TryForceDetectEarthquake(disasterId))
                _pendingDetectionEarthquakes.Add(disasterId);
        }

        private void DetectActiveEarthquake(ushort disasterId)
        {
            TryForceDetectEarthquake(disasterId);
        }

        private void RetryPendingEarthquakeDetections()
        {
            if (_pendingDetectionEarthquakes.Count == 0)
                return;

            var detected = new List<ushort>();
            foreach (var disasterId in _pendingDetectionEarthquakes)
                if (TryForceDetectEarthquake(disasterId))
                    detected.Add(disasterId);

            for (var i = 0; i < detected.Count; i++)
                _pendingDetectionEarthquakes.Remove(detected[i]);
        }

        private bool TryForceDetectEarthquake(ushort disasterId)
        {
            if (!Enabled || disasterId == 0)
                return true;

            var dm = Services.Disasters;
            var flags = dm.m_disasters.m_buffer[disasterId].m_flags;
            if ((flags & DisasterData.Flags.Detected) != 0)
                return true;

            if ((flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active)) == 0)
                return true;

            dm.DetectDisaster(disasterId, true);
            DebugLogger.Log(GetDebugStr() + string.Format(
                "Earthquake forced detection. Id: {0}, Flags: {1}",
                disasterId,
                dm.m_disasters.m_buffer[disasterId].m_flags));

            flags = dm.m_disasters.m_buffer[disasterId].m_flags;
            return (flags & DisasterData.Flags.Active) != 0 || (flags & DisasterData.Flags.Detected) != 0;
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Earthquake;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Earthquake;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;

            //setup Cracks based on intensity and game setup
            SetupCracksOnMap(disasterInfoUnified.DisasterInfo.intensity);

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterStarted(byte intensity)
        {
            if (!AftershocksEnabled)
            {
                aftershocksCount = 0;
                InvalidateRealTimeAftershockSchedule();
                if (IsRealTimePatternActive())
                {
                    ResetRealTimeEarthquakeSchedule();
                    ClearRealTimeCooldownState();
                    return;
                }

                base.OnDisasterStarted(intensity);
                return;
            }

            var isAftershock = aftershocksCount > 0;

            if (!isAftershock)
            {
                mainStrikeIntensity = intensity;
                aftershockMaxIntensity = (byte)(10 + (intensity - 10) * 3 / 4);
                if (intensity > 20)
                    aftershocksCount = (byte)(1 + Services.Simulation.m_randomizer.Int32(1 + (uint)intensity / 20));
            }
            else
            {
                aftershocksCount--;
                aftershockMaxIntensity = (byte)(10 + (aftershockMaxIntensity - 10) * 3 / 4);
            }

            if (aftershocksCount > 0)
            {
                if (IsRealTimePatternActive())
                {
                    ScheduleNextRealTimeAftershock();
                    ClearRealTimeCooldownState();
                }
                else
                {
                    calmDays = 15;
                    probabilityWarmupDaysLeft = 0;
                    intensityWarmupDaysLeft = 0;
                }

                Debug.Log(string.Format(
                    CommonProperties.LogMessagePrefix + "{0} aftershocks are still going to happen.",
                    aftershocksCount));
            }
            else
            {
                if (IsRealTimePatternActive())
                {
                    ResetRealTimeEarthquakeSchedule();
                    InvalidateRealTimeAftershockSchedule();
                    ClearRealTimeCooldownState();
                    return;
                }

                calmDays = GetMainEarthquakeCalmDays();
                base.OnDisasterStarted(mainStrikeIntensity);
                calmDays = GetMainEarthquakeCalmDays();
            }
        }

        protected override bool FindTarget(DisasterInfo disasterInfo, out Vector3 targetPosition, out float angle)
        {
            if (aftershocksCount == 0)
            {
                var result = base.FindTarget(disasterInfo, out targetPosition, out angle);
                if (!result && TryFindRandomTargetInConfiguredArea(out targetPosition, out angle))
                {
                    DebugLogger.Log(GetDebugStr() + string.Format(
                        "Earthquake fallback target selected. Target: x:{0:#.##} y:{1:#.##} z:{2:#.##}",
                        targetPosition.x,
                        targetPosition.y,
                        targetPosition.z));
                    result = true;
                }

                if (result)
                {
                    lastTargetPosition = targetPosition;
                    lastAngle = angle;
                }

                return result;
            }

            if (TryFindNearbyAftershockTarget(out targetPosition, out angle))
                return true;

            targetPosition = lastTargetPosition;
            angle = lastAngle;
            return true;
        }

        protected override byte GetRandomIntensity(byte maxIntensity)
        {
            if (aftershocksCount > 0) return (byte)Services.Simulation.m_randomizer.Int32(10, aftershockMaxIntensity);

            return base.GetRandomIntensity(maxIntensity);
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as EarthquakeAI != null;
        }

        public override string GetName()
        {
            return LocalizationService.GetDisasterName(DType);
        }

        public override float CalculateDestructionRadio(byte intensity)
        {
            var unitSize = 8;
            var unitsBase = 30; //24 Original, Distance Fix for proximity
            float unitCalculation;
            var intensityInt = intensity / 10;
            var intensityDec = intensity % 10;

            switch (intensity)
            {
                case byte n when n < 25:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase +
                                      (intensityDec * 0.24f + intensityInt * 10.4f);
                    break;

                case byte n when n >= 25 && n <= 50:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 22f +
                                      ((intensityDec - 2f) * 11.6f + (intensityInt - 5f) * 0.36f);
                    break;

                case byte n when n > 50 && n <= 75:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 55f +
                                      (intensityInt - 5f) * 8f;
                    break;

                case byte n when n > 75 && n <= 100:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 71f +
                                      +((intensityInt - 7f) * 10.8f + (intensityDec - 5f) * 0.28f);
                    break;

                case byte n when n > 100 && n <= 125:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 102f +
                                      ((intensityInt - 10f) * 11.2f + intensityDec * 0.32f);
                    break;

                case byte n when n > 125 && n <= 150:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 126f +
                                      (intensityInt - 12f) * 8f;
                    break;

                case byte n when n > 150 && n <= 175:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 150 +
                                      intensityDec * 0.2f + (intensityInt - 15f) * 10f;
                    break;

                case byte n when n > 175 && n <= 200:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 171 +
                                      ((intensityDec - 5) * 0.12f + (intensityInt - 17) * 9.2f);
                    break;

                case byte n when n > 200 && n <= 250:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 198 +
                                      (intensityDec * 0.2f + (intensityInt - 20f) * 10f);
                    break;

                default:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 248;
                    break;
            }

            return (float)Math.Sqrt(unitCalculation / 2 * unitSize);
        }

        public override void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel,
            ref List<DisasterInfoModel> activeDisasters)
        {
            var disasterTargetPosition = new Vector3(disasterInfoModel.DisasterInfo.targetX,
                disasterInfoModel.DisasterInfo.targetY, disasterInfoModel.DisasterInfo.targetZ);

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
                    var shelterPosition = buildingInfo.m_position;


                    if (buildingInfo.Info.m_buildingAI as ShelterAI != null)
                    {
                        //Add Building/Shelter Data to disaster
                        disasterInfoModel.ShelterList.Add(num);

                        //Getting diaster core
                        var disasterDestructionRadius =
                            CalculateDestructionRadio(disasterInfoModel.DisasterInfo.intensity);
                        float shelterRadius = (buildingInfo.Length < buildingInfo.Width
                            ? buildingInfo.Width
                            : buildingInfo.Length) * 8 / 2;

                        bool IgnoreDestructionZoneForEarthquake;
                        switch (EarthquakeCrackMode)
                        {
                            case EarthquakeCrackOptions.NoCracks:
                                IgnoreDestructionZoneForEarthquake = true;
                                break;

                            case EarthquakeCrackOptions.AlwaysCracks:
                                IgnoreDestructionZoneForEarthquake = false;
                                break;

                            case EarthquakeCrackOptions eco when eco == EarthquakeCrackOptions.CracksBasedOnIntensity &&
                                                                 disasterInfoModel.DisasterInfo.intensity >=
                                                                 MinimalIntensityForCracks * 10:
                                IgnoreDestructionZoneForEarthquake = false;
                                break;

                            default:
                                IgnoreDestructionZoneForEarthquake = true;
                                break;
                        }

                        //if Shelter will be destroyed, don't evacuate
                        if (IsShelterInDisasterZone(disasterTargetPosition, shelterPosition, shelterRadius,
                                disasterDestructionRadius) && !IgnoreDestructionZoneForEarthquake)
                            DebugLogger.Log("Shelter is located in Destruction Zone. Won't be avacuated");
                        else
                            SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num,
                                ref buildingManager.m_buildings.m_buffer[num], false);
                    }
                }
            }

            AddOrReplaceActiveDisaster(disasterInfoModel, ref activeDisasters);
        }

        private void SetupCracksOnMap(byte intensity)
        {
            switch (EarthquakeCrackMode)
            {
                case EarthquakeCrackOptions.NoCracks:
                    NoCracksInTheGroud = true;
                    break;

                case EarthquakeCrackOptions.AlwaysCracks:
                    NoCracksInTheGroud = false;
                    break;

                case EarthquakeCrackOptions ecp when ecp == EarthquakeCrackOptions.CracksBasedOnIntensity &&
                                                     intensity >= MinimalIntensityForCracks * 10:
                    NoCracksInTheGroud = false;
                    break;

                default:
                    NoCracksInTheGroud = true;
                    break;
            }

            UpdateDisasterProperties(true);
        }

        public bool IsRealTimePatternActive()
        {
            return DisasterSimulationUtils.IsRealTimeModActive();
        }

        public override void OnEnabledChanged(bool enabled)
        {
            _lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        public float GetRealTimePatternProbabilityProgress()
        {
            if (aftershocksCount > 0)
            {
                EnsureRealTimeAftershockSchedule();
                return Mathf.Clamp01(1f - RealTimeMinutesUntilNextAftershock /
                    RealTimeCurrentAftershockPeriodMinutes);
            }

            EnsureRealTimeEarthquakeSchedule();
            return Mathf.Clamp01(1f - RealTimeMinutesUntilNextEarthquake / RealTimeCurrentSeismicPeriodMinutes);
        }

        public float GetProbabilityProgress()
        {
            if (!unlocked)
                return 0f;

            if (IsRealTimePatternActive())
                return GetRealTimePatternProbabilityProgress();

            if (aftershocksCount > 0)
                return 1f;

            if (calmDaysLeft > 0)
                return 0f;

            if (probabilityWarmupDays <= 0 || probabilityWarmupDaysLeft <= 0f)
                return 1f;

            return Mathf.Clamp01(1f - probabilityWarmupDaysLeft / probabilityWarmupDays);
        }

        private bool IsEarthquakeCharged()
        {
            return probabilityWarmupDays <= 0 || probabilityWarmupDaysLeft <= 0f;
        }

        private string GetRealTimeProbabilityTooltip(float probabilityValue)
        {
            if (!unlocked) return LocalizationService.Get("tooltip.not_unlocked");

            EnsureRealTimeEarthquakeSchedule();
            return string.Format(
                "{0}: {1}{2}{3}{2}{4}{2}{5}{2}{6}",
                LocalizationService.Get("tooltip.progress"),
                string.Format("{0:00.00}%", probabilityValue * 100),
                CommonProperties.NewLine,
                LocalizationService.Get("tooltip.earthquake.realtime_active"),
                LocalizationService.Format("tooltip.earthquake.realtime_reference", GetRealTimeEarthquakeFrequencyName()),
                LocalizationService.Format(
                    "tooltip.earthquake.current_seismic_interval",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeCurrentSeismicPeriodMinutes)),
                LocalizationService.Format(
                    "tooltip.earthquake.seismic_time_remaining",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeMinutesUntilNextEarthquake)));
        }

        private string GetRealTimeAftershockProbabilityTooltip(float probabilityValue)
        {
            EnsureRealTimeAftershockSchedule();
            return string.Format(
                "{0}: {1}{2}{3}{2}{4}{2}{5}{2}{6}",
                LocalizationService.Get("tooltip.progress"),
                string.Format("{0:00.00}%", probabilityValue * 100),
                CommonProperties.NewLine,
                LocalizationService.Format("tooltip.earthquake.aftershocks", aftershocksCount),
                LocalizationService.Get("tooltip.earthquake.realtime_aftershocks_active"),
                LocalizationService.Format(
                    "tooltip.earthquake.current_aftershock_interval",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeCurrentAftershockPeriodMinutes)),
                LocalizationService.Format(
                    "tooltip.earthquake.aftershock_time_remaining",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeMinutesUntilNextAftershock)));
        }

        private void UpdateRealTimeEarthquakeSchedule()
        {
            EnsureRealTimeEarthquakeSchedule();

            RealTimeMinutesUntilNextEarthquake = Mathf.Max(
                0f,
                RealTimeMinutesUntilNextEarthquake - GetRealTimeElapsedMinutes());
        }

        private void UpdateRealTimeAftershockSchedule()
        {
            EnsureRealTimeAftershockSchedule();

            RealTimeMinutesUntilNextAftershock = Mathf.Max(
                0f,
                RealTimeMinutesUntilNextAftershock - GetRealTimeElapsedMinutes());
        }

        private bool IsRealTimeEarthquakeDue()
        {
            EnsureRealTimeEarthquakeSchedule();
            return RealTimeMinutesUntilNextEarthquake <= 0f;
        }

        private bool IsRealTimeAftershockDue()
        {
            EnsureRealTimeAftershockSchedule();
            return RealTimeMinutesUntilNextAftershock <= 0f;
        }

        private void EnsureRealTimeEarthquakeSchedule()
        {
            if (RealTimeCurrentSeismicPeriodMinutes <= 0f || RealTimeMinutesUntilNextEarthquake < 0f)
                ScheduleNextRealTimeEarthquake();

            if (RealTimeMinutesUntilNextEarthquake > RealTimeCurrentSeismicPeriodMinutes)
                RealTimeMinutesUntilNextEarthquake = RealTimeCurrentSeismicPeriodMinutes;
        }

        private void EnsureRealTimeAftershockSchedule()
        {
            if (RealTimeCurrentAftershockPeriodMinutes <= 0f || RealTimeMinutesUntilNextAftershock < 0f)
                ScheduleNextRealTimeAftershock();

            if (RealTimeMinutesUntilNextAftershock > RealTimeCurrentAftershockPeriodMinutes)
                RealTimeMinutesUntilNextAftershock = RealTimeCurrentAftershockPeriodMinutes;
        }

        private void ResetRealTimeEarthquakeSchedule()
        {
            ScheduleNextRealTimeEarthquake();
        }

        private void ScheduleNextRealTimeEarthquake()
        {
            ScheduleNextRealTimeEarthquake(0f);
        }

        private void ScheduleNextRealTimeEarthquake(float progressToKeep)
        {
            var progress = Mathf.Clamp01(progressToKeep);
            RealTimeCurrentSeismicPeriodMinutes = GetRandomRealTimeIntervalMinutes();
            RealTimeMinutesUntilNextEarthquake = RealTimeCurrentSeismicPeriodMinutes * (1f - progress);
            _lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        private void ScheduleNextRealTimeAftershock()
        {
            RealTimeCurrentAftershockPeriodMinutes = GetRandomRealTimeAftershockIntervalMinutes();
            RealTimeMinutesUntilNextAftershock = RealTimeCurrentAftershockPeriodMinutes;
            _lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        private void InvalidateRealTimeAftershockSchedule()
        {
            RealTimeCurrentAftershockPeriodMinutes = -1f;
            RealTimeMinutesUntilNextAftershock = -1f;
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
            switch (RealTimeEarthquakeFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    minMinutes = 30f;
                    maxMinutes = 60f;
                    break;
                case RealTimeDisasterFrequencyPreset.Frequent:
                    minMinutes = 90f;
                    maxMinutes = 180f;
                    break;
                case RealTimeDisasterFrequencyPreset.Occasional:
                default:
                    minMinutes = 240f;
                    maxMinutes = 480f;
                    break;
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    minMinutes = 480f;
                    maxMinutes = 960f;
                    break;
                case RealTimeDisasterFrequencyPreset.Rare:
                    minMinutes = 960f;
                    maxMinutes = 1920f;
                    break;
            }
        }

        private float GetRandomRealTimeAftershockIntervalMinutes()
        {
            var randomValue = Services.Simulation.m_randomizer.Int32(0, 10000) / 10000f;
            return 5f + 25f * randomValue;
        }

        private bool TryFindNearbyAftershockTarget(out Vector3 targetPosition, out float angle)
        {
            targetPosition = Vector3.zero;
            angle = 0f;

            if (lastTargetPosition == Vector3.zero)
                return false;

            var simulation = Services.Simulation;
            var terrain = Services.Terrain;
            if (simulation == null || terrain == null)
                return false;

            var radius = Mathf.Clamp(
                mainStrikeIntensity * AftershockRadiusPerIntensityPoint,
                MinimumAftershockRadius,
                MaximumAftershockRadius);

            for (var attempt = 0; attempt < NearbyAftershockTargetAttempts; attempt++)
            {
                var randomAngle = simulation.m_randomizer.Int32(0, 10000) * 0.0006283185f;
                var randomDistance = Mathf.Sqrt(simulation.m_randomizer.Int32(0, 10000) / 10000f) * radius;
                var candidate = lastTargetPosition;
                candidate.x += Mathf.Cos(randomAngle) * randomDistance;
                candidate.z += Mathf.Sin(randomAngle) * randomDistance;
                candidate.y = terrain.SampleRawHeightSmoothWithWater(candidate, false, 0f);

                if (!IsAftershockTargetAllowed(candidate))
                    continue;

                targetPosition = candidate;
                angle = randomAngle;
                return true;
            }

            return false;
        }

        private bool IsAftershockTargetAllowed(Vector3 position)
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

        private bool TryFindRandomTargetInConfiguredArea(out Vector3 targetPosition, out float angle)
        {
            targetPosition = Vector3.zero;
            angle = 0f;

            var gameArea = Services.GameArea;
            var simulation = Services.Simulation;
            var terrain = Services.Terrain;
            if (gameArea == null || simulation == null || terrain == null)
                return false;

            var area = unlocked ? OccurrenceAreaAfterUnlock : OccurrenceAreaBeforeUnlock;
            var allowedAreaCount = 0;
            var selectedX = -1;
            var selectedZ = -1;

            for (var z = 0; z < 5; z++)
            for (var x = 0; x < 5; x++)
            {
                var isUnlocked = IsAreaUnlocked(x, z);
                var allowed = area == OccurrenceAreas.Everywhere ||
                              area == OccurrenceAreas.UnlockedAreas && isUnlocked ||
                              area == OccurrenceAreas.LockedAreas && !isUnlocked;
                if (!allowed)
                    continue;

                allowedAreaCount++;
                if (simulation.m_randomizer.Int32((uint)allowedAreaCount) == 0)
                {
                    selectedX = x;
                    selectedZ = z;
                }
            }

            if (selectedX < 0 || selectedZ < 0)
                return false;

            float minX;
            float minZ;
            float maxX;
            float maxZ;
            gameArea.GetAreaBounds(selectedX, selectedZ, out minX, out minZ, out maxX, out maxZ);

            var randomX = simulation.m_randomizer.Int32(0, 10000) * 0.0001f;
            var randomZ = simulation.m_randomizer.Int32(0, 10000) * 0.0001f;
            targetPosition.x = minX + (maxX - minX) * randomX;
            targetPosition.z = minZ + (maxZ - minZ) * randomZ;
            targetPosition.y = terrain.SampleRawHeightSmoothWithWater(targetPosition, false, 0f);
            angle = simulation.m_randomizer.Int32(0, 10000) * 0.0006283185f;
            return true;
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

        private int GetMainEarthquakeCalmDays()
        {
            return probabilityWarmupDays / 2;
        }

        public void SetRealTimeEarthquakeFrequency(RealTimeDisasterFrequencyPreset frequency)
        {
            if (RealTimeEarthquakeFrequency == frequency)
                return;

            EnsureRealTimeEarthquakeSchedule();
            var currentProgress = Mathf.Clamp01(1f - RealTimeMinutesUntilNextEarthquake /
                RealTimeCurrentSeismicPeriodMinutes);
            RealTimeEarthquakeFrequency = frequency;
            ScheduleNextRealTimeEarthquake(currentProgress);
        }

        public static string[] GetRealTimeEarthquakeFrequencyOptions()
        {
            return new[]
            {
                LocalizationService.Get("settings.earthquake.frequency.apocalypse"),
                LocalizationService.Get("settings.earthquake.frequency.frequent"),
                LocalizationService.Get("settings.earthquake.frequency.occasional"),
                LocalizationService.Get("settings.earthquake.frequency.uncommon"),
                LocalizationService.Get("settings.earthquake.frequency.rare")
            };
        }

        public int GetRealTimeEarthquakeFrequencySelectionIndex()
        {
            for (var i = 0; i < RealTimeEarthquakeFrequencyOptionValues.Length; i++)
                if (RealTimeEarthquakeFrequencyOptionValues[i] == RealTimeEarthquakeFrequency)
                    return i;

            return 2;
        }

        public static RealTimeDisasterFrequencyPreset GetRealTimeEarthquakeFrequencyFromSelection(int selection)
        {
            if (selection < 0 || selection >= RealTimeEarthquakeFrequencyOptionValues.Length)
                return RealTimeDisasterFrequencyPreset.Occasional;

            return RealTimeEarthquakeFrequencyOptionValues[selection];
        }

        public string GetRealTimeEarthquakeFrequencyTooltip()
        {
            return LocalizationService.Format(
                "settings.earthquake.realtime_frequency.tooltip.selected",
                GetRealTimeEarthquakeFrequencyName());
        }

        private string GetRealTimeEarthquakeFrequencyName()
        {
            switch (RealTimeEarthquakeFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    return LocalizationService.Get("settings.earthquake.frequency_name.apocalypse");
                case RealTimeDisasterFrequencyPreset.Frequent:
                    return LocalizationService.Get("settings.earthquake.frequency_name.frequent");
                case RealTimeDisasterFrequencyPreset.Occasional:
                    return LocalizationService.Get("settings.earthquake.frequency_name.occasional");
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    return LocalizationService.Get("settings.earthquake.frequency_name.uncommon");
                case RealTimeDisasterFrequencyPreset.Rare:
                    return LocalizationService.Get("settings.earthquake.frequency_name.rare");
                default:
                    return RealTimeEarthquakeFrequency.ToString();
            }
        }

        public void UpdateDisasterProperties(bool isSet)
        {
            var prefabsCount = PrefabCollection<DisasterInfo>.PrefabCount();

            for (uint i = 0; i < prefabsCount; i++)
            {
                var disasterInfo = PrefabCollection<DisasterInfo>.GetPrefab(i);
                if (disasterInfo == null)
                    continue;

                if (disasterInfo.m_disasterAI is EarthquakeAI earthquakeAI)
                {
                    if (isSet && NoCracksInTheGroud)
                    {
                        earthquakeAI.m_crackLength = 0;
                        earthquakeAI.m_crackWidth = 0;
                    }
                    else
                    {
                        earthquakeAI.m_crackLength = 1000;
                        earthquakeAI.m_crackWidth = 100;
                    }
                }
            }
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            if (disaster is EarthquakeModel earthquake)
            {
                AftershocksEnabled = earthquake.AftershocksEnabled;
                WarmupYears = earthquake.WarmupYears;
                SetRealTimeEarthquakeFrequency(earthquake.RealTimeEarthquakeFrequency);
            }
        }
    }
}
