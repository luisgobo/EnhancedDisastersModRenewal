using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class SinkholeModel : DisasterBaseModel
    {
        private const float GuaranteedOccurrencePerFrame = 1f;
        private const float SecondsPerMinute = 60f;
        private const float MaxRealTimeDeltaSeconds = 5f;
        private const float RealTimeGroundwaterFillFactor = 2f;
        private const float RealTimeGroundwaterDrainFactor = 0.5f;
        private const int LocalizedWetnessGridResolution = 25;
        private const int LocalizedWetnessGridCellCount =
            LocalizedWetnessGridResolution * LocalizedWetnessGridResolution;
        private const int LocalizedWetnessTargetAttempts = 24;
        private const float LocalizedWetnessRainFillFactor = 1.25f;
        private const float LocalizedWetnessDrainFactor = 1f;
        private const float LocalizedWetnessMinimumTargetWeight = 0.02f;
        private const float LocalizedWetnessMaxWaterDepth = 8f;
        private const float LocalizedWetnessMaxTargetWaterDepth = 0.35f;
        private const float LocalizedWetnessActiveCityMaxFactor = 5f;
        private const float LocalizedWetnessActiveBuildingFactorStep = 0.2f;
        private const float LocalizedWetnessNeighborBuildingFactorStep = 0.05f;
        private const string ExtendedInfoPanel2ModKey = "extendedInfoPanel2";
        private static readonly RealTimeDisasterFrequencyPreset[] RealTimeSinkholeFrequencyOptionValues =
        {
            RealTimeDisasterFrequencyPreset.Apocalypse,
            RealTimeDisasterFrequencyPreset.Frequent,
            RealTimeDisasterFrequencyPreset.Occasional,
            RealTimeDisasterFrequencyPreset.Uncommon,
            RealTimeDisasterFrequencyPreset.Rare
        };

        [XmlIgnore] private float _lastRealTimeScheduleUpdateSeconds = -1f;
        [XmlIgnore] private readonly float[] _localizedWetnessCityFactors = new float[LocalizedWetnessGridCellCount];
        [XmlIgnore] private readonly float[] _localizedWetnessGrid = new float[LocalizedWetnessGridCellCount];
        [XmlIgnore] private readonly float[] _localizedWetnessTerrainFactors = new float[LocalizedWetnessGridCellCount];
        [XmlIgnore] private bool _localizedWetnessGridInitialized;
        [XmlIgnore] private float _localizedWetnessMaxX;
        [XmlIgnore] private float _localizedWetnessMaxZ;
        [XmlIgnore] private float _localizedWetnessMinX;
        [XmlIgnore] private float _localizedWetnessMinZ;

        [XmlIgnore] public float groundwaterAmount; // groundwaterAmount=1 means rain of intensity 1 during 1 day
        [XmlIgnore] public float RealTimeCurrentWetPeriodMinutes = -1f;
        [XmlIgnore] public float RealTimeMinutesUntilNextSinkhole = -1f;
        public float GroundwaterCapacity = 50;
        public RealTimeDisasterFrequencyPreset RealTimeSinkholeFrequency =
            RealTimeDisasterFrequencyPreset.Occasional;

        public SinkholeModel()
        {
            DType = DisasterType.Sinkhole;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
            BaseOccurrencePerYear = 1.5f; // When groundwater is full
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            calmDays = 30;
            probabilityWarmupDays = 0;
            intensityWarmupDays = 0;
        }

        public override string GetProbabilityTooltip(float value)
        {
            if (!unlocked) return "Not unlocked yet";

            if (calmDaysLeft <= 0)
            {
                if (IsRealTimePatternActive())
                    return GetRealTimeProbabilityTooltip(value);

                var groundWaterPercent = (int)(100 * groundwaterAmount / GroundwaterCapacity);
                return LocalizationService.Format("tooltip.sinkhole.groundwater", groundWaterPercent);
            }

            return base.GetProbabilityTooltip(value);
        }

        protected override void OnSimulationFrameLocal()
        {
            if (IsRealTimePatternActive())
            {
                UpdateRealTimeSinkholeSchedule();
                return;
            }

            var daysPerFrame = DisasterSimulationUtils.DaysPerFrame;

            var wm = Services.Weather;
            if (wm.m_currentRain > 0) groundwaterAmount += wm.m_currentRain * daysPerFrame;

            groundwaterAmount -= groundwaterAmount / GroundwaterCapacity * daysPerFrame;

            if (groundwaterAmount < 0) groundwaterAmount = 0;

            UpdateLocalizedWetness(daysPerFrame, GetCurrentRain());
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.Sinkhole;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
            DetectActiveSinkhole(disasterId);
        }

        private void DetectActiveSinkhole(ushort disasterId)
        {
            if (!Enabled || disasterId == 0)
                return;

            var dm = Services.Disasters;
            var flags = dm.m_disasters.m_buffer[disasterId].m_flags;
            if ((flags & DisasterData.Flags.Active) == 0 || (flags & DisasterData.Flags.Detected) != 0)
                return;

            dm.DetectDisaster(disasterId, true);
            DebugLogger.Log(GetDebugStr() + string.Format(
                "Sinkhole detected after activation. Id: {0}, Flags: {1}",
                disasterId,
                dm.m_disasters.m_buffer[disasterId].m_flags));
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Sinkhole;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Sinkhole;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterStarted(byte intensity)
        {
            groundwaterAmount = 0;
            ResetLocalizedWetnessGrid();

            if (IsRealTimePatternActive())
                ResetRealTimeSchedule();

            base.OnDisasterStarted(intensity);
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            if (IsRealTimePatternActive())
                return IsRealTimeSinkholeDue()
                    ? 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame
                    : 0f;

            return base.GetCurrentOccurrencePerYearLocal() * groundwaterAmount / GroundwaterCapacity;
        }

        protected override float GetSimulationDaysPerFrame()
        {
            return IsRealTimePatternActive()
                ? DisasterSimulationUtils.VanillaSimulationDaysPerFrame
                : base.GetSimulationDaysPerFrame();
        }

        protected override bool FindTarget(DisasterInfo disasterInfo, out Vector3 targetPosition, out float angle)
        {
            if (TryFindLocalizedWetnessTarget(out targetPosition, out angle))
                return true;

            return base.FindTarget(disasterInfo, out targetPosition, out angle);
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as SinkholeAI != null;
        }

        public override string GetName()
        {
            return LocalizationService.GetDisasterName(DType);
        }

        public override float CalculateDestructionRadio(byte intensity)
        {
            var unitSize = 8;
            var unitsBase = 24; //24 + 4 Original, Distance Fix for proximity
            float unitCalculation;
            var intensityInt = intensity / 10;
            var intensityDec = intensity % 10;

            switch (intensity)
            {
                case byte n when n < 26:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f;
                    break;

                case byte n when n >= 26 && n < 101:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                                      (intensityInt - 2) * 0.4f;
                    break;

                case byte n when n >= 101 && n < 126:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f - 3f;
                    break;

                case byte n when n >= 126 && n < 151:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                                      (intensityInt - 2) * 0.3f - (intensityDec - 5) * 0.04f -
                                      0.5f * (intensityInt - 12);
                    break;

                case byte n when n >= 151 && n < 176:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                                      (intensityInt - 2) * 0.4f;
                    break;

                case byte n when n >= 176 && n < 201:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                        0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                        (intensityInt - 2) * 0.4f + (intensityDec - 5) * 0.08f + (intensityInt - 17) * 0.8f;
                    break;

                case byte n when n >= 201 && n < 226:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                        0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                        (intensityInt - 2) * 0.3f - (intensityDec - 5) * 0.04f - 0.5f * (intensityInt - 12) + 4f;
                    break;

                case byte n when n >= 226 && n < 251:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                        0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                        (intensityInt - 2) * 0.4f + 1;
                    break;

                default:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                                      (intensityInt - 2) * 0.3f - (intensityDec - 5) * 0.04f -
                                      0.5f * (intensityInt - 12) + 5 +
                                      intensityDec * 0.36f;
                    break;
            }

            return (float)Math.Sqrt(unitCalculation / 2 * unitSize);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            var d = disaster as SinkholeModel;
            if (d != null)
            {
                GroundwaterCapacity = d.GroundwaterCapacity;
                SetRealTimeSinkholeFrequency(d.RealTimeSinkholeFrequency);
            }
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
            EnsureRealTimeSchedule();
            return Mathf.Clamp01(1f - RealTimeMinutesUntilNextSinkhole / RealTimeCurrentWetPeriodMinutes);
        }

        private string GetRealTimeProbabilityTooltip(float probabilityValue)
        {
            EnsureRealTimeSchedule();
            var groundWaterPercent = (int)(GetGroundwaterSaturation() * 100f);
            return string.Format(
                "{0}: {1}{2}{3}{2}{4}{2}{5}{2}{6}",
                LocalizationService.Get("tooltip.progress"),
                string.Format("{0:00.00}%", probabilityValue * 100),
                CommonProperties.NewLine,
                LocalizationService.Get("tooltip.sinkhole.realtime_active"),
                LocalizationService.Format("tooltip.sinkhole.realtime_reference", GetRealTimeSinkholeFrequencyName()),
                LocalizationService.Format("tooltip.sinkhole.groundwater", groundWaterPercent),
                LocalizationService.Format(
                    "tooltip.sinkhole.current_wet_interval",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeCurrentWetPeriodMinutes)) +
                CommonProperties.NewLine +
                LocalizationService.Format(
                    "tooltip.sinkhole.wet_time_remaining",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeMinutesUntilNextSinkhole)));
        }

        private void UpdateRealTimeSinkholeSchedule()
        {
            EnsureRealTimeSchedule();

            var elapsedMinutes = GetRealTimeElapsedMinutes();
            UpdateRealTimeGroundwater(elapsedMinutes);
            UpdateLocalizedWetnessRealTime(elapsedMinutes, GetCurrentRain());

            RealTimeMinutesUntilNextSinkhole = Mathf.Max(
                0f,
                RealTimeMinutesUntilNextSinkhole - elapsedMinutes * GetGroundwaterSaturation());
        }

        private bool IsRealTimeSinkholeDue()
        {
            EnsureRealTimeSchedule();
            return RealTimeMinutesUntilNextSinkhole <= 0f;
        }

        private void EnsureRealTimeSchedule()
        {
            if (RealTimeCurrentWetPeriodMinutes <= 0f || RealTimeMinutesUntilNextSinkhole < 0f)
                ScheduleNextRealTimeSinkhole();

            if (RealTimeMinutesUntilNextSinkhole > RealTimeCurrentWetPeriodMinutes)
                RealTimeMinutesUntilNextSinkhole = RealTimeCurrentWetPeriodMinutes;
        }

        private void ResetRealTimeSchedule()
        {
            ScheduleNextRealTimeSinkhole();
        }

        private void ScheduleNextRealTimeSinkhole()
        {
            ScheduleNextRealTimeSinkhole(0f);
        }

        private void ScheduleNextRealTimeSinkhole(float progressToKeep)
        {
            var progress = Mathf.Clamp01(progressToKeep);
            RealTimeCurrentWetPeriodMinutes = GetRandomRealTimeIntervalMinutes();
            RealTimeMinutesUntilNextSinkhole = RealTimeCurrentWetPeriodMinutes * (1f - progress);
            _lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        private void UpdateRealTimeGroundwater(float elapsedMinutes)
        {
            if (elapsedMinutes <= 0f)
                return;

            var periodMinutes = Mathf.Max(1f, RealTimeCurrentWetPeriodMinutes);
            var capacity = Mathf.Max(1f, GroundwaterCapacity);
            var rain = GetCurrentRain();

            if (rain > 0f)
                groundwaterAmount += rain * capacity * elapsedMinutes / periodMinutes * RealTimeGroundwaterFillFactor;

            groundwaterAmount -= groundwaterAmount * elapsedMinutes / periodMinutes * RealTimeGroundwaterDrainFactor;
            groundwaterAmount = Mathf.Clamp(groundwaterAmount, 0f, capacity);
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
            switch (RealTimeSinkholeFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    minMinutes = 10f;
                    maxMinutes = 20f;
                    break;
                case RealTimeDisasterFrequencyPreset.Frequent:
                    minMinutes = 20f;
                    maxMinutes = 60f;
                    break;
                case RealTimeDisasterFrequencyPreset.Occasional:
                default:
                    minMinutes = 60f;
                    maxMinutes = 120f;
                    break;
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    minMinutes = 120f;
                    maxMinutes = 240f;
                    break;
                case RealTimeDisasterFrequencyPreset.Rare:
                    minMinutes = 240f;
                    maxMinutes = 480f;
                    break;
            }
        }

        private float GetGroundwaterSaturation()
        {
            return Mathf.Clamp01(groundwaterAmount / Mathf.Max(1f, GroundwaterCapacity));
        }

        private static float GetCurrentRain()
        {
            var wm = Services.Weather;
            return wm == null ? 0f : Mathf.Clamp01(wm.m_currentRain);
        }

        private void UpdateLocalizedWetness(float elapsedDays, float rain)
        {
            if (elapsedDays <= 0f)
                return;

            EnsureLocalizedWetnessGrid();

            var capacity = Mathf.Max(1f, GroundwaterCapacity);
            for (var i = 0; i < _localizedWetnessGrid.Length; i++)
            {
                var wetness = _localizedWetnessGrid[i];
                if (rain > 0f)
                    wetness += rain * elapsedDays / capacity * LocalizedWetnessRainFillFactor *
                               _localizedWetnessTerrainFactors[i];

                wetness -= wetness * elapsedDays / capacity * LocalizedWetnessDrainFactor;
                _localizedWetnessGrid[i] = Mathf.Clamp01(wetness);
            }
        }

        private void UpdateLocalizedWetnessRealTime(float elapsedMinutes, float rain)
        {
            if (elapsedMinutes <= 0f)
                return;

            EnsureLocalizedWetnessGrid();

            var periodMinutes = Mathf.Max(1f, RealTimeCurrentWetPeriodMinutes);
            for (var i = 0; i < _localizedWetnessGrid.Length; i++)
            {
                var wetness = _localizedWetnessGrid[i];
                if (rain > 0f)
                    wetness += rain * elapsedMinutes / periodMinutes * RealTimeGroundwaterFillFactor *
                               _localizedWetnessTerrainFactors[i];

                wetness -= wetness * elapsedMinutes / periodMinutes * RealTimeGroundwaterDrainFactor;
                _localizedWetnessGrid[i] = Mathf.Clamp01(wetness);
            }
        }

        private void ResetLocalizedWetnessGrid()
        {
            for (var i = 0; i < _localizedWetnessGrid.Length; i++)
                _localizedWetnessGrid[i] = 0f;
        }

        private bool TryFindLocalizedWetnessTarget(out Vector3 targetPosition, out float angle)
        {
            EnsureLocalizedWetnessGrid();
            RefreshLocalizedWetnessCityFactors();

            for (var attempt = 0; attempt < LocalizedWetnessTargetAttempts; attempt++)
                if (TrySelectWetnessCell(out var cellIndex))
                {
                    targetPosition = GetRandomPointInWetnessCell(cellIndex);
                    if (!IsTargetPositionAllowed(targetPosition) || HasExcessiveWaterDepth(targetPosition))
                        continue;

                    targetPosition.y = Services.Terrain.SampleRawHeightSmooth(targetPosition);
                    angle = Services.Simulation.m_randomizer.Int32(0, 10000) * 0.0006283185f;
                    return true;
                }

            targetPosition = Vector3.zero;
            angle = 0f;
            return false;
        }

        private bool TrySelectWetnessCell(out int cellIndex)
        {
            var totalWeight = 0f;
            for (var i = 0; i < _localizedWetnessGrid.Length; i++)
            {
                if (!IsWetnessCellAllowed(i))
                    continue;

                var wetness = _localizedWetnessGrid[i];
                if (wetness <= 0f)
                    continue;

                totalWeight += wetness * wetness * _localizedWetnessTerrainFactors[i] *
                               _localizedWetnessCityFactors[i];
            }

            if (totalWeight < LocalizedWetnessMinimumTargetWeight)
            {
                cellIndex = -1;
                return false;
            }

            var randomValue = Services.Simulation.m_randomizer.Int32(0, 10000) / 10000f * totalWeight;
            var accumulatedWeight = 0f;
            for (var i = 0; i < _localizedWetnessGrid.Length; i++)
            {
                if (!IsWetnessCellAllowed(i))
                    continue;

                var wetness = _localizedWetnessGrid[i];
                if (wetness <= 0f)
                    continue;

                accumulatedWeight += wetness * wetness * _localizedWetnessTerrainFactors[i] *
                                     _localizedWetnessCityFactors[i];
                if (randomValue <= accumulatedWeight)
                {
                    cellIndex = i;
                    return true;
                }
            }

            cellIndex = -1;
            return false;
        }

        private bool IsWetnessCellAllowed(int cellIndex)
        {
            return IsTargetPositionAllowed(GetWetnessCellCenter(cellIndex));
        }

        private bool IsTargetPositionAllowed(Vector3 position)
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

        private void EnsureLocalizedWetnessGrid()
        {
            if (_localizedWetnessGridInitialized)
                return;

            CalculateMapBounds(
                out _localizedWetnessMinX,
                out _localizedWetnessMinZ,
                out _localizedWetnessMaxX,
                out _localizedWetnessMaxZ);

            for (var i = 0; i < _localizedWetnessTerrainFactors.Length; i++)
                _localizedWetnessTerrainFactors[i] = CalculateWetnessTerrainFactor(GetWetnessCellCenter(i));

            for (var i = 0; i < _localizedWetnessCityFactors.Length; i++)
                _localizedWetnessCityFactors[i] = 1f;

            SeedLocalizedWetnessFromGroundwater();
            _localizedWetnessGridInitialized = true;
        }

        private void RefreshLocalizedWetnessCityFactors()
        {
            for (var i = 0; i < _localizedWetnessCityFactors.Length; i++)
                _localizedWetnessCityFactors[i] = 1f;

            var buildingManager = Services.Buildings;
            if (buildingManager == null)
                return;

            var buildings = buildingManager.m_buildings.m_buffer;
            for (ushort buildingId = 1; buildingId < buildings.Length; buildingId++)
            {
                var flags = buildings[buildingId].m_flags;
                if (!IsActiveCityBuilding(flags))
                    continue;

                var position = buildings[buildingId].m_position;
                if (!IsTargetPositionAllowed(position) || !TryGetWetnessCellIndex(position, out var cellIndex))
                    continue;

                AddCityFactor(cellIndex, LocalizedWetnessActiveBuildingFactorStep);
                AddNeighborCityFactors(cellIndex);
            }
        }

        private static bool IsActiveCityBuilding(Building.Flags flags)
        {
            var requiredFlags = Building.Flags.Created | Building.Flags.Completed;
            var excludedFlags = Building.Flags.Deleted | Building.Flags.Collapsed |
                                Building.Flags.BurnedDown | Building.Flags.Demolishing;
            return (flags & requiredFlags) == requiredFlags && (flags & excludedFlags) == 0;
        }

        private void AddNeighborCityFactors(int cellIndex)
        {
            var cellX = cellIndex % LocalizedWetnessGridResolution;
            var cellZ = cellIndex / LocalizedWetnessGridResolution;

            for (var dx = -1; dx <= 1; dx++)
            for (var dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0)
                    continue;

                var neighborX = cellX + dx;
                var neighborZ = cellZ + dz;
                if (neighborX < 0 || neighborZ < 0 ||
                    neighborX >= LocalizedWetnessGridResolution ||
                    neighborZ >= LocalizedWetnessGridResolution)
                    continue;

                AddCityFactor(
                    neighborZ * LocalizedWetnessGridResolution + neighborX,
                    LocalizedWetnessNeighborBuildingFactorStep);
            }
        }

        private void AddCityFactor(int cellIndex, float value)
        {
            _localizedWetnessCityFactors[cellIndex] = Mathf.Min(
                LocalizedWetnessActiveCityMaxFactor,
                _localizedWetnessCityFactors[cellIndex] + value);
        }

        private void SeedLocalizedWetnessFromGroundwater()
        {
            var saturation = GetGroundwaterSaturation();
            if (saturation <= 0f)
                return;

            for (var i = 0; i < _localizedWetnessGrid.Length; i++)
                _localizedWetnessGrid[i] = Mathf.Clamp01(saturation * _localizedWetnessTerrainFactors[i]);
        }

        private static void CalculateMapBounds(out float minX, out float minZ, out float maxX, out float maxZ)
        {
            minX = float.MaxValue;
            minZ = float.MaxValue;
            maxX = float.MinValue;
            maxZ = float.MinValue;

            var gameArea = Services.GameArea;
            for (var x = 0; x < 5; x++)
            for (var z = 0; z < 5; z++)
            {
                float areaMinX;
                float areaMinZ;
                float areaMaxX;
                float areaMaxZ;
                gameArea.GetAreaBounds(x, z, out areaMinX, out areaMinZ, out areaMaxX, out areaMaxZ);
                minX = Mathf.Min(minX, areaMinX);
                minZ = Mathf.Min(minZ, areaMinZ);
                maxX = Mathf.Max(maxX, areaMaxX);
                maxZ = Mathf.Max(maxZ, areaMaxZ);
            }
        }

        private Vector3 GetWetnessCellCenter(int cellIndex)
        {
            var cellX = cellIndex % LocalizedWetnessGridResolution;
            var cellZ = cellIndex / LocalizedWetnessGridResolution;
            var cellWidth = (_localizedWetnessMaxX - _localizedWetnessMinX) / LocalizedWetnessGridResolution;
            var cellDepth = (_localizedWetnessMaxZ - _localizedWetnessMinZ) / LocalizedWetnessGridResolution;

            var position = new Vector3(
                _localizedWetnessMinX + (cellX + 0.5f) * cellWidth,
                0f,
                _localizedWetnessMinZ + (cellZ + 0.5f) * cellDepth);
            position.y = Services.Terrain.SampleRawHeightSmooth(position);
            return position;
        }

        private bool TryGetWetnessCellIndex(Vector3 position, out int cellIndex)
        {
            if (position.x < _localizedWetnessMinX || position.x > _localizedWetnessMaxX ||
                position.z < _localizedWetnessMinZ || position.z > _localizedWetnessMaxZ)
            {
                cellIndex = -1;
                return false;
            }

            var normalizedX = Mathf.Clamp01(
                (position.x - _localizedWetnessMinX) / (_localizedWetnessMaxX - _localizedWetnessMinX));
            var normalizedZ = Mathf.Clamp01(
                (position.z - _localizedWetnessMinZ) / (_localizedWetnessMaxZ - _localizedWetnessMinZ));
            var cellX = Mathf.Min(
                LocalizedWetnessGridResolution - 1,
                Mathf.FloorToInt(normalizedX * LocalizedWetnessGridResolution));
            var cellZ = Mathf.Min(
                LocalizedWetnessGridResolution - 1,
                Mathf.FloorToInt(normalizedZ * LocalizedWetnessGridResolution));

            cellIndex = cellZ * LocalizedWetnessGridResolution + cellX;
            return true;
        }

        private Vector3 GetRandomPointInWetnessCell(int cellIndex)
        {
            var cellX = cellIndex % LocalizedWetnessGridResolution;
            var cellZ = cellIndex / LocalizedWetnessGridResolution;
            var cellWidth = (_localizedWetnessMaxX - _localizedWetnessMinX) / LocalizedWetnessGridResolution;
            var cellDepth = (_localizedWetnessMaxZ - _localizedWetnessMinZ) / LocalizedWetnessGridResolution;
            var randomX = Services.Simulation.m_randomizer.Int32(0, 10000) / 10000f;
            var randomZ = Services.Simulation.m_randomizer.Int32(0, 10000) / 10000f;

            return new Vector3(
                _localizedWetnessMinX + (cellX + randomX) * cellWidth,
                0f,
                _localizedWetnessMinZ + (cellZ + randomZ) * cellDepth);
        }

        private static float CalculateWetnessTerrainFactor(Vector3 position)
        {
            var terrain = Services.Terrain;
            var sampleDistance = 96f;
            var centerHeight = terrain.SampleRawHeightSmooth(position);
            var northHeight = terrain.SampleRawHeightSmooth(position.x, position.z + sampleDistance);
            var southHeight = terrain.SampleRawHeightSmooth(position.x, position.z - sampleDistance);
            var eastHeight = terrain.SampleRawHeightSmooth(position.x + sampleDistance, position.z);
            var westHeight = terrain.SampleRawHeightSmooth(position.x - sampleDistance, position.z);
            var averageNeighborHeight = (northHeight + southHeight + eastHeight + westHeight) * 0.25f;
            var averageSlope =
                (Mathf.Abs(northHeight - centerHeight) +
                 Mathf.Abs(southHeight - centerHeight) +
                 Mathf.Abs(eastHeight - centerHeight) +
                 Mathf.Abs(westHeight - centerHeight)) * 0.25f;

            var depressionFactor = Mathf.Clamp01((averageNeighborHeight - centerHeight + 2f) / 12f);
            var flatnessFactor = 1f - Mathf.Clamp01(averageSlope / 24f);
            var waterFactor = GetWaterInfluence(position, centerHeight);

            return Mathf.Clamp(0.5f + depressionFactor * 0.75f + flatnessFactor * 0.35f + waterFactor * 0.9f,
                0.25f,
                2.5f);
        }

        private static float GetWaterInfluence(Vector3 position, float terrainHeight)
        {
            var terrain = Services.Terrain;
            var waterSurfaceHeight = terrain.SampleRawHeightSmoothWithWater(position, false, 0f);
            var waterDepth = Mathf.Max(0f, waterSurfaceHeight - terrainHeight);
            if (waterDepth > 0f)
                return Mathf.Clamp01(waterDepth / LocalizedWetnessMaxWaterDepth);

            var waterDistance = terrain.CalculateWaterProximity(position, 256f, out waterSurfaceHeight);
            return Mathf.Clamp01(1f - waterDistance / 256f) * 0.5f;
        }

        private static bool HasExcessiveWaterDepth(Vector3 position)
        {
            var terrain = Services.Terrain;
            var terrainHeight = terrain.SampleRawHeightSmooth(position);
            var waterSurfaceHeight = terrain.SampleRawHeightSmoothWithWater(position, false, 0f);
            return waterSurfaceHeight - terrainHeight > LocalizedWetnessMaxTargetWaterDepth;
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

        public void SetRealTimeSinkholeFrequency(RealTimeDisasterFrequencyPreset frequency)
        {
            if (RealTimeSinkholeFrequency == frequency)
                return;

            var currentProgress = GetRealTimePatternProbabilityProgress();
            RealTimeSinkholeFrequency = frequency;
            ScheduleNextRealTimeSinkhole(currentProgress);
        }

        public static string[] GetRealTimeSinkholeFrequencyOptions()
        {
            return new[]
            {
                LocalizationService.Get("settings.sinkhole.frequency.apocalypse"),
                LocalizationService.Get("settings.sinkhole.frequency.frequent"),
                LocalizationService.Get("settings.sinkhole.frequency.occasional"),
                LocalizationService.Get("settings.sinkhole.frequency.uncommon"),
                LocalizationService.Get("settings.sinkhole.frequency.rare")
            };
        }

        public int GetRealTimeSinkholeFrequencySelectionIndex()
        {
            for (var i = 0; i < RealTimeSinkholeFrequencyOptionValues.Length; i++)
                if (RealTimeSinkholeFrequencyOptionValues[i] == RealTimeSinkholeFrequency)
                    return i;

            return 2;
        }

        public static RealTimeDisasterFrequencyPreset GetRealTimeSinkholeFrequencyFromSelection(int selection)
        {
            if (selection < 0 || selection >= RealTimeSinkholeFrequencyOptionValues.Length)
                return RealTimeDisasterFrequencyPreset.Occasional;

            return RealTimeSinkholeFrequencyOptionValues[selection];
        }

        public string GetRealTimeSinkholeFrequencyTooltip()
        {
            return LocalizationService.Format(
                "settings.sinkhole.realtime_frequency.tooltip.selected",
                GetRealTimeSinkholeFrequencyName());
        }

        private string GetRealTimeSinkholeFrequencyName()
        {
            switch (RealTimeSinkholeFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    return LocalizationService.Get("settings.sinkhole.frequency_name.apocalypse");
                case RealTimeDisasterFrequencyPreset.Frequent:
                    return LocalizationService.Get("settings.sinkhole.frequency_name.frequent");
                case RealTimeDisasterFrequencyPreset.Occasional:
                    return LocalizationService.Get("settings.sinkhole.frequency_name.occasional");
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    return LocalizationService.Get("settings.sinkhole.frequency_name.uncommon");
                case RealTimeDisasterFrequencyPreset.Rare:
                    return LocalizationService.Get("settings.sinkhole.frequency_name.rare");
                default:
                    return RealTimeSinkholeFrequency.ToString();
            }
        }
    }
}
