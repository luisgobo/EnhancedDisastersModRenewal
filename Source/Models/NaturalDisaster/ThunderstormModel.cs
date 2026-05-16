using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Disaster;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class ThunderstormModel : DisasterBaseModel
    {
        private const float GuaranteedOccurrencePerFrame = 1f;
        private const float SecondsPerMinute = 60f;
        private const float MaxRealTimeDeltaSeconds = 5f;
        private const string ExtendedInfoPanel2ModKey = "extendedInfoPanel2";
        private static readonly RealTimeDisasterFrequencyPreset[] RealTimeThunderstormFrequencyOptionValues =
        {
            RealTimeDisasterFrequencyPreset.Apocalypse,
            RealTimeDisasterFrequencyPreset.Frequent,
            RealTimeDisasterFrequencyPreset.Occasional,
            RealTimeDisasterFrequencyPreset.Uncommon,
            RealTimeDisasterFrequencyPreset.Rare
        };

        [XmlIgnore] private float _lastRealTimeScheduleUpdateSeconds = -1f;

        public float RainFactor = 2.0f;
        public int MaxProbabilityMonth = 7;
        [XmlIgnore] public float RealTimeCurrentStormPeriodMinutes = -1f;
        [XmlIgnore] public float RealTimeMinutesUntilNextThunderstorm = -1f;
        public RealTimeDisasterFrequencyPreset RealTimeThunderstormFrequency =
            RealTimeDisasterFrequencyPreset.Occasional;

        public ThunderstormModel()
        {
            DType = DisasterType.ThunderStorm;
            OccurrenceAreaBeforeUnlock = OccurrenceAreas.LockedAreas;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.Everywhere;
            BaseOccurrencePerYear = 2.0f;
            ProbabilityDistribution = ProbabilityDistributions.PowerLow;

            calmDays = 60;
            probabilityWarmupDays = 30;
            intensityWarmupDays = 60;
            EvacuationMode = EvacuationOptions.ManualEvacuation;
        }

        public override string GetProbabilityTooltip(float value)
        {
            if (!unlocked)
            {
                return LocalizationService.Get("tooltip.thunderstorm.outside_area");
            }

            if (calmDaysLeft <= 0)
            {
                if (IsRealTimePatternActive())
                    return GetRealTimeProbabilityTooltip(value);

                if (Services.Weather.m_currentRain > 0 && RainFactor > 1)
                {
                    return LocalizationService.Get("tooltip.thunderstorm.rain_increase");
                }
            }

            return base.GetProbabilityTooltip(value);
        }

        protected override void OnSimulationFrameLocal()
        {
            if (IsRealTimePatternActive())
            {
                UpdateRealTimeThunderstormSchedule();
            }
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            if (IsRealTimePatternActive())
                return IsRealTimeThunderstormDue()
                    ? 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame
                    : 0f;

            var occurence = base.GetCurrentOccurrencePerYearLocal() * GetSeasonFactor();

            return occurence * GetRainOccurrenceFactor();
        }

        protected override float GetSimulationDaysPerFrame()
        {
            return IsRealTimePatternActive()
                ? DisasterSimulationUtils.VanillaSimulationDaysPerFrame
                : base.GetSimulationDaysPerFrame();
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.ThunderStorm;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.ThunderStorm;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.ThunderStorm;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterStarted(byte intensity)
        {
            if (IsRealTimePatternActive())
                ResetRealTimeSchedule();

            base.OnDisasterStarted(intensity);            
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as ThunderStormAI != null;
        }

        public override string GetName()
        {
            return LocalizationService.GetDisasterName(DType);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            ThunderstormModel d = disaster as ThunderstormModel;
            if (d != null)
            {
                RainFactor = d.RainFactor;
                MaxProbabilityMonth = d.MaxProbabilityMonth;
                SetRealTimeThunderstormFrequency(d.RealTimeThunderstormFrequency);
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
            return Mathf.Clamp01(1f - RealTimeMinutesUntilNextThunderstorm / RealTimeCurrentStormPeriodMinutes);
        }

        private string GetRealTimeProbabilityTooltip(float probabilityValue)
        {
            EnsureRealTimeSchedule();
            return string.Format(
                "{0}: {1}{2}{3}{2}{4}{2}{5}{2}{6}",
                LocalizationService.Get("tooltip.progress"),
                string.Format("{0:00.00}%", probabilityValue * 100),
                CommonProperties.NewLine,
                LocalizationService.Get("tooltip.thunderstorm.realtime_active"),
                LocalizationService.Format(
                    "tooltip.thunderstorm.realtime_reference",
                    GetRealTimeThunderstormFrequencyName()),
                LocalizationService.Format(
                    "tooltip.thunderstorm.current_storm_interval",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeCurrentStormPeriodMinutes)),
                LocalizationService.Format(
                    "tooltip.thunderstorm.storm_time_remaining",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeMinutesUntilNextThunderstorm)));
        }

        private void UpdateRealTimeThunderstormSchedule()
        {
            EnsureRealTimeSchedule();

            var elapsedMinutes = GetRealTimeElapsedMinutes();
            RealTimeMinutesUntilNextThunderstorm = Mathf.Max(
                0f,
                RealTimeMinutesUntilNextThunderstorm - elapsedMinutes * GetRealTimeStormProgressFactor());
        }

        private bool IsRealTimeThunderstormDue()
        {
            EnsureRealTimeSchedule();
            return RealTimeMinutesUntilNextThunderstorm <= 0f;
        }

        private void EnsureRealTimeSchedule()
        {
            if (RealTimeCurrentStormPeriodMinutes <= 0f || RealTimeMinutesUntilNextThunderstorm < 0f)
                ScheduleNextRealTimeThunderstorm();

            if (RealTimeMinutesUntilNextThunderstorm > RealTimeCurrentStormPeriodMinutes)
                RealTimeMinutesUntilNextThunderstorm = RealTimeCurrentStormPeriodMinutes;
        }

        private void ResetRealTimeSchedule()
        {
            ScheduleNextRealTimeThunderstorm();
        }

        private void ScheduleNextRealTimeThunderstorm()
        {
            ScheduleNextRealTimeThunderstorm(0f);
        }

        private void ScheduleNextRealTimeThunderstorm(float progressToKeep)
        {
            var progress = Mathf.Clamp01(progressToKeep);
            RealTimeCurrentStormPeriodMinutes = GetRandomRealTimeIntervalMinutes();
            RealTimeMinutesUntilNextThunderstorm = RealTimeCurrentStormPeriodMinutes * (1f - progress);
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
            switch (RealTimeThunderstormFrequency)
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

        private float GetRealTimeStormProgressFactor()
        {
            return GetRainOccurrenceFactor();
        }

        private float GetSeasonFactor()
        {
            DateTime dt = Services.Simulation.m_currentGameTime;
            int deltaMonth = Math.Abs(dt.Month - MaxProbabilityMonth);
            if (deltaMonth > 6) deltaMonth = 12 - deltaMonth;

            return Mathf.Clamp01(1f - deltaMonth / 6f);
        }

        private float GetRainOccurrenceFactor()
        {
            var wm = Services.Weather;
            return wm != null && wm.m_currentRain > 0
                ? RainFactor
                : 1f;
        }

        public void SetRealTimeThunderstormFrequency(RealTimeDisasterFrequencyPreset frequency)
        {
            if (RealTimeThunderstormFrequency == frequency)
                return;

            var currentProgress = GetRealTimePatternProbabilityProgress();
            RealTimeThunderstormFrequency = frequency;
            ScheduleNextRealTimeThunderstorm(currentProgress);
        }

        public static string[] GetRealTimeThunderstormFrequencyOptions()
        {
            return new[]
            {
                LocalizationService.Get("settings.thunderstorm.frequency.apocalypse"),
                LocalizationService.Get("settings.thunderstorm.frequency.frequent"),
                LocalizationService.Get("settings.thunderstorm.frequency.occasional"),
                LocalizationService.Get("settings.thunderstorm.frequency.uncommon"),
                LocalizationService.Get("settings.thunderstorm.frequency.rare")
            };
        }

        public int GetRealTimeThunderstormFrequencySelectionIndex()
        {
            for (var i = 0; i < RealTimeThunderstormFrequencyOptionValues.Length; i++)
                if (RealTimeThunderstormFrequencyOptionValues[i] == RealTimeThunderstormFrequency)
                    return i;

            return 2;
        }

        public static RealTimeDisasterFrequencyPreset GetRealTimeThunderstormFrequencyFromSelection(int selection)
        {
            if (selection < 0 || selection >= RealTimeThunderstormFrequencyOptionValues.Length)
                return RealTimeDisasterFrequencyPreset.Occasional;

            return RealTimeThunderstormFrequencyOptionValues[selection];
        }

        public string GetRealTimeThunderstormFrequencyTooltip()
        {
            return LocalizationService.Format(
                "settings.thunderstorm.realtime_frequency.tooltip.selected",
                GetRealTimeThunderstormFrequencyName());
        }

        private string GetRealTimeThunderstormFrequencyName()
        {
            switch (RealTimeThunderstormFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    return LocalizationService.Get("settings.thunderstorm.frequency_name.apocalypse");
                case RealTimeDisasterFrequencyPreset.Frequent:
                    return LocalizationService.Get("settings.thunderstorm.frequency_name.frequent");
                case RealTimeDisasterFrequencyPreset.Occasional:
                    return LocalizationService.Get("settings.thunderstorm.frequency_name.occasional");
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    return LocalizationService.Get("settings.thunderstorm.frequency_name.uncommon");
                case RealTimeDisasterFrequencyPreset.Rare:
                    return LocalizationService.Get("settings.thunderstorm.frequency_name.rare");
                default:
                    return RealTimeThunderstormFrequency.ToString();
            }
        }

        //public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified)
        //{
        //    disasterInfoUnified.DisasterInfo.type = DisasterType.ThunderStorm;
        //    base.OnDisasterDetected(disasterInfoUnified);
        //}
    }
}
