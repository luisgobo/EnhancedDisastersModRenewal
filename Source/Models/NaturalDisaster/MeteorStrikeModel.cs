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
    public class MeteorStrikeModel : DisasterBaseModel
    {
        private const float DefaultRealTimeFrequencyMultiplier = 4f;
        private const float DefaultRealTimePeriodDays = 146f;
        private const float GuaranteedOccurrencePerFrame = 1f;
        private const float SecondsPerMinute = 60f;
        private const float MaxRealTimeDeltaSeconds = 5f;
        private const string ExtendedInfoPanel2ModKey = "extendedInfoPanel2";
        [XmlIgnore] private float lastRealTimeScheduleUpdateSeconds = -1f;

        [XmlIgnore] public MeteorEvent[] meteorEvents;
        [XmlIgnore] public float realTimeCurrentPeriodMinutes = -1f;
        [XmlIgnore] public float realTimeDaysUntilNextMeteor = -1f;
        private float realTimeFrequencyMultiplier = DefaultRealTimeFrequencyMultiplier;
        public RealTimeMeteorFrequencyPreset RealTimeMeteorFrequency = RealTimeMeteorFrequencyPreset.Occasional;
        [XmlIgnore] public float realTimeMinutesUntilNextMeteor = -1f;
        private float realTimePeriodDays = DefaultRealTimePeriodDays;

        public MeteorStrikeModel()
        {
            DType = DisasterType.MeteorStrike;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
            BaseOccurrencePerYear = 10.0f;
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            //Change
            meteorEvents = new[]
            {
                MeteorEvent.Init("Long period meteor", 9, 200),
                MeteorEvent.Init("Medium period meteor", 5, 120),
                MeteorEvent.Init("Short period meteor", 2, 30)
            };
        }

        [XmlElement]
        public bool MeteorLongPeriodEnabled
        {
            get => GetEnabled(0);

            set => SetEnabled(0, value);
        }

        [XmlElement]
        public bool MeteorMediumPeriodEnabled
        {
            get => GetEnabled(1);

            set => SetEnabled(1, value);
        }

        [XmlElement]
        public bool MeteorShortPeriodEnabled
        {
            get => GetEnabled(2);

            set => SetEnabled(2, value);
        }

        [XmlElement]
        public float RealTimeFrequencyMultiplier
        {
            get => realTimeFrequencyMultiplier;
            set
            {
                realTimeFrequencyMultiplier = Mathf.Max(1f, value);
                RealTimePeriodDays = GetPeriodDaysFromFrequencyMultiplier();
            }
        }

        [XmlElement]
        public float RealTimePeriodDays
        {
            get => realTimePeriodDays;
            set
            {
                realTimePeriodDays = Mathf.Max(1f, value);
                if (realTimeDaysUntilNextMeteor > realTimePeriodDays)
                    realTimeDaysUntilNextMeteor = realTimePeriodDays;
            }
        }

        public bool GetEnabled(int index)
        {
            return meteorEvents[index].Enabled;
        }

        public void SetEnabled(int index, bool value)
        {
            meteorEvents[index].Enabled = value;
        }

        protected override void OnSimulationFrameLocal()
        {
            if (IsRealTimePatternActive())
            {
                EnsureRealTimeSchedule();
                realTimeMinutesUntilNextMeteor = Mathf.Max(
                    0f,
                    realTimeMinutesUntilNextMeteor - GetRealTimeElapsedMinutes());
                return;
            }

            var daysPerFrame = DisasterSimulationUtils.VanillaSimulationDaysPerFrame;

            for (var i = 0; i < meteorEvents.Length; i++) meteorEvents[i].OnSimulationFrame(daysPerFrame);
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.MeteorStrike;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.MeteorStrike;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.MeteorStrike;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterStarted(byte intensity)
        {
            if (IsRealTimePatternActive())
            {
                base.OnDisasterStarted(intensity);
                ResetRealTimeSchedule();
                return;
            }

            var meteorIndex = -1;
            float maxProbability = 0;

            for (var i = 0; i < meteorEvents.Length; i++)
            {
                var prob = meteorEvents[i].GetProbabilityMultiplier();
                if (prob > maxProbability)
                {
                    maxProbability = prob;
                    meteorIndex = i;
                }
            }

            // Should not happen
            if (meteorIndex == -1) meteorIndex = 2;

            meteorEvents[meteorIndex].OnMeteorFallen();

            base.OnDisasterStarted(intensity);
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            if (IsRealTimePatternActive())
                return IsRealTimeMeteorDue()
                    ? 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame
                    : 0f;

            for (var i = 0; i < meteorEvents.Length; i++)
                if (meteorEvents[i].IsDue())
                    return 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame;

            return 0f;
        }

        protected override float GetSimulationDaysPerFrame()
        {
            return IsRealTimePatternActive()
                ? DisasterSimulationUtils.VanillaSimulationDaysPerFrame
                : base.GetSimulationDaysPerFrame();
        }

        public override byte GetMaximumIntensity()
        {
            if (IsRealTimePatternActive())
                return ScaleIntensityByPopulation(baseIntensity);

            var intensity = baseIntensity;

            for (var i = 0; i < meteorEvents.Length; i++)
                intensity = Math.Max(intensity, meteorEvents[i].GetActualMaxIntensity());

            intensity = ScaleIntensityByPopulation(intensity);

            return intensity;
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as MeteorStrikeAI != null;
        }

        public override string GetName()
        {
            return LocalizationService.GetDisasterName(DType);
        }

        public override string GetProbabilityTooltip(float probabilityValue)
        {
            if (!unlocked) return "Not unlocked yet";

            if (IsRealTimePatternActive())
                return string.Format(
                    "Progress: {0}{1}Real Time active: meteor periods are disabled.{1}Current interval: {2}.{1}Time remaining: {3}.",
                    string.Format("{0:00.00}%", probabilityValue * 100),
                    CommonProperties.newLine,
                    DisasterSimulationUtils.FormatRealTimeMinutes(realTimeCurrentPeriodMinutes),
                    DisasterSimulationUtils.FormatRealTimeMinutes(realTimeMinutesUntilNextMeteor));

            var probability =
                string.Format("Probability: {0:00.00}% {1}", probabilityValue * 100, CommonProperties.newLine);

            for (var i = 0; i < meteorEvents.Length; i++)
                probability += meteorEvents[i].GetStateDescription() + Environment.NewLine;

            return probability;
        }

        public bool AreMeteorPeriodsEnabled()
        {
            return !IsRealTimePatternActive();
        }

        public float GetMeteorPeriodProbabilityProgress()
        {
            float progress = 0f;

            for (var i = 0; i < meteorEvents.Length; i++)
                progress = Mathf.Max(progress, meteorEvents[i].GetProbabilityMultiplier());

            return progress;
        }

        public float GetRealTimePatternProbabilityProgress()
        {
            EnsureRealTimeSchedule();
            return Mathf.Clamp01(1f - realTimeMinutesUntilNextMeteor / realTimeCurrentPeriodMinutes);
        }

        private bool IsRealTimeMeteorDue()
        {
            EnsureRealTimeSchedule();
            return realTimeMinutesUntilNextMeteor <= 0f;
        }

        private void EnsureRealTimeSchedule()
        {
            if (realTimeCurrentPeriodMinutes <= 0f || realTimeMinutesUntilNextMeteor < 0f)
                ScheduleNextRealTimeMeteor();

            if (realTimeMinutesUntilNextMeteor > realTimeCurrentPeriodMinutes)
                realTimeMinutesUntilNextMeteor = realTimeCurrentPeriodMinutes;
        }

        private void ResetRealTimeSchedule()
        {
            ScheduleNextRealTimeMeteor();
        }

        private float GetPeriodDaysFromFrequencyMultiplier()
        {
            var effectiveOccurrencePerYear = BaseOccurrencePerYear / RealTimeFrequencyMultiplier;
            return 365f / Mathf.Max(0.0001f, effectiveOccurrencePerYear);
        }

        private void ScheduleNextRealTimeMeteor()
        {
            ScheduleNextRealTimeMeteor(0f);
        }

        private void ScheduleNextRealTimeMeteor(float progressToKeep)
        {
            var progress = Mathf.Clamp01(progressToKeep);
            realTimeCurrentPeriodMinutes = GetRandomRealTimeIntervalMinutes();
            realTimeMinutesUntilNextMeteor = realTimeCurrentPeriodMinutes * (1f - progress);
            lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        private float GetRealTimeElapsedMinutes()
        {
            var currentSeconds = Time.realtimeSinceStartup;

            if (lastRealTimeScheduleUpdateSeconds < 0f)
            {
                lastRealTimeScheduleUpdateSeconds = currentSeconds;
                return 0f;
            }

            var elapsedSeconds = Mathf.Clamp(
                currentSeconds - lastRealTimeScheduleUpdateSeconds,
                0f,
                MaxRealTimeDeltaSeconds);
            lastRealTimeScheduleUpdateSeconds = currentSeconds;
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

            if (speed == 2)
                return 1.5f;

            return 2f;
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
            switch (RealTimeMeteorFrequency)
            {
                case RealTimeMeteorFrequencyPreset.Apocalypse:
                    minMinutes = 4f;
                    maxMinutes = 10f;
                    break;
                case RealTimeMeteorFrequencyPreset.DailyThreat:
                    minMinutes = 10f;
                    maxMinutes = 30f;
                    break;
                case RealTimeMeteorFrequencyPreset.Occasional:
                default:
                    minMinutes = 30f;
                    maxMinutes = 60f;
                    break;
                case RealTimeMeteorFrequencyPreset.Rare:
                    minMinutes = 60f;
                    maxMinutes = 180f;
                    break;
            }
        }

        public void SetRealTimeMeteorFrequency(RealTimeMeteorFrequencyPreset frequency)
        {
            if (RealTimeMeteorFrequency == frequency)
                return;

            var currentProgress = GetRealTimePatternProbabilityProgress();
            RealTimeMeteorFrequency = frequency;
            ScheduleNextRealTimeMeteor(currentProgress);
        }

        public static string[] GetRealTimeMeteorFrequencyOptions()
        {
            return new[]
            {
                LocalizationService.Get("settings.meteor.frequency.apocalypse"),
                LocalizationService.Get("settings.meteor.frequency.daily_threat"),
                LocalizationService.Get("settings.meteor.frequency.occasional"),
                LocalizationService.Get("settings.meteor.frequency.rare")
            };
        }

        public MeteorPeriodStatus[] GetMeteorPeriodStatuses()
        {
            var statuses = new MeteorPeriodStatus[meteorEvents.Length];
            var periodsEnabled = AreMeteorPeriodsEnabled();

            for (var i = 0; i < meteorEvents.Length; i++)
                statuses[i] = new MeteorPeriodStatus
                {
                    Name = meteorEvents[i].Name,
                    Enabled = periodsEnabled && meteorEvents[i].Enabled,
                    ProbabilityMultiplier = periodsEnabled ? meteorEvents[i].GetProbabilityMultiplier() : 0f,
                    MaxIntensity = meteorEvents[i].MaxIntensity,
                    Description = periodsEnabled
                        ? meteorEvents[i].GetStateDescription()
                        : "Disabled while Real Time is active."
                };

            return statuses;
        }

        private static bool IsRealTimePatternActive()
        {
            return DisasterSimulationUtils.IsRealTimeModActive();
        }

        public override float CalculateDestructionRadio(byte intensity)
        {
            var unitSize = 8;
            var unitsBase = 24; //24 Original, Distance Fix for proximity
            float unitCalculation;
            var intensityInt = intensity / 10;
            var intensityDec = intensity % 10;

            switch (intensity)
            {
                case byte n when n < 25:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase + 4 -
                                      intensityDec * 0.04f - intensityInt * 0.4f;
                    break;

                case byte n when n >= 25 && n < 50:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase + 3 -
                                      (intensityInt - 2) * 0.8f - (intensityDec - 5) * 0.08f;
                    break;

                case byte n when n >= 50 && n <= 55:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase + 1;
                    break;

                case byte n when n > 55 && n <= 75:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase + 1 -
                                      (intensityInt - 5) * 0.5f - (intensityDec - 5) * 0.05f;
                    break;

                case byte n when n > 75 && n <= 100:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      (intensityInt - 7) * 0.8f - (intensityDec - 5) * 0.08f;
                    break;

                case byte n when n > 100 && n <= 125:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase - 2 -
                                      (intensityInt - 10) * 0.4f - intensityDec * 0.04f;
                    break;

                case byte n when n > 125 && n <= 150:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase - 3 +
                                      (intensityInt - 12) * 0.4f + (intensityDec - 5) * 0.04f;
                    break;

                case byte n when n > 150 && n <= 175:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase - 2 -
                                      (intensityInt - 15) * 1.2f - intensityDec * 0.12f;
                    break;

                case byte n when n > 175 && n <= 250:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase - 5 -
                                      (intensityInt - 17) * 0.4f - (intensityDec - 5) * 0.04f;
                    break;

                default:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase - 5 -
                        (intensityInt - 17) * 0.4f - (intensityDec - 5) * 0.04f + intensityDec * 0.24f;
                    break;
            }

            return (float)Math.Sqrt(unitCalculation / 2 * unitSize);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            var d = disaster as MeteorStrikeModel;
            if (d != null)
            {
                MeteorLongPeriodEnabled = d.MeteorLongPeriodEnabled;
                MeteorMediumPeriodEnabled = d.MeteorMediumPeriodEnabled;
                MeteorShortPeriodEnabled = d.MeteorShortPeriodEnabled;
                RealTimeFrequencyMultiplier = d.RealTimeFrequencyMultiplier;
                RealTimePeriodDays = d.RealTimePeriodDays;
                RealTimeMeteorFrequency = d.RealTimeMeteorFrequency;
            }
        }

        public struct MeteorPeriodStatus
        {
            public string Name;
            public bool Enabled;
            public float ProbabilityMultiplier;
            public byte MaxIntensity;
            public string Description;
        }

        public struct MeteorEvent
        {
            public string Name;
            public float PeriodDays;
            public byte MaxIntensity;
            public float DaysUntilNextEvent;
            public int MeteorsFallen;
            public bool Enabled;

            public MeteorEvent(string name, float periodDays, byte maxIntensity, float daysUntilNextEvent)
            {
                Name = name;
                PeriodDays = periodDays;
                MaxIntensity = maxIntensity;
                DaysUntilNextEvent = daysUntilNextEvent;
                MeteorsFallen = 0;
                Enabled = true;
            }

            public static MeteorEvent Init(string eventName, float periodYears, byte maxIntensity)
            {
                var sm = Services.Simulation;

                var periodDays = periodYears * 365;

                var periodDaysCalc = periodDays * 0.95f + sm.m_randomizer.Int32((uint)(periodDays * 0.1f));
                var daysUntilNextEventCalc = periodDays / 2 + sm.m_randomizer.Int32((uint)(periodDays / 2));

                return new MeteorEvent(
                    eventName,
                    periodDaysCalc,
                    maxIntensity,
                    daysUntilNextEventCalc
                );
            }

            public float GetProbabilityMultiplier()
            {
                if (!Enabled) return 0;

                if (MeteorsFallen > 0) return 0;

                if (IsDue())
                    return 1f;

                return Mathf.Clamp01(1f - DaysUntilNextEvent / PeriodDays);
            }

            public bool IsDue()
            {
                return Enabled && MeteorsFallen == 0 && DaysUntilNextEvent <= 0f;
            }

            public byte GetActualMaxIntensity()
            {
                if (!Enabled) return 1;

                if (DaysUntilNextEvent < 60) return MaxIntensity;

                return 1;
            }

            public void OnSimulationFrame(float daysPerFrame)
            {
                if (!Enabled) return;

                DaysUntilNextEvent -= daysPerFrame;
            }

            public void OnMeteorFallen()
            {
                DaysUntilNextEvent = PeriodDays;
                MeteorsFallen = 0;
            }

            public override string ToString()
            {
                return string.Format("Period {0} years, max intensity {1}, next meteor in {2}",
                    PeriodDays / 365, MaxIntensity, DisasterSimulationUtils.FormatTimeSpan(DaysUntilNextEvent));
            }

            public string GetStateDescription()
            {
                if (!Enabled) return "";

                if (MeteorsFallen > 0) return Name + " already fallen.";

                if (GetProbabilityMultiplier() >= 0.9f)
                    return Name + " is approaching. Time remaining: " +
                           DisasterSimulationUtils.FormatTimeSpan(DaysUntilNextEvent);

                return Name + " will be close in " + DisasterSimulationUtils.FormatTimeSpan(DaysUntilNextEvent);
            }
        }
    }
}

//TODO
// Autoevacuate/release is not working
