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
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.Sinkhole;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
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
