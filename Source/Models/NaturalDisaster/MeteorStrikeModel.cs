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
        private const float GuaranteedOccurrencePerFrame = 1f;

        [XmlIgnore] public MeteorEvent[] meteorEvents;
        [XmlIgnore] public float realTimeDaysUntilNextMeteor = -1f;
        private float realTimeFrequencyMultiplier = DefaultRealTimeFrequencyMultiplier;

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
            get { return realTimeFrequencyMultiplier; }
            set { realTimeFrequencyMultiplier = Mathf.Max(1f, value); }
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
                realTimeDaysUntilNextMeteor = Mathf.Max(
                    0f,
                    realTimeDaysUntilNextMeteor - DisasterSimulationUtils.VanillaSimulationDaysPerFrame);
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
            var baseValue = base.GetCurrentOccurrencePerYearLocal();

            if (IsRealTimePatternActive())
                return IsRealTimeMeteorDue()
                    ? 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame
                    : 0f;

            float result = 0;

            for (var i = 0; i < meteorEvents.Length; i++)
                result += baseValue * meteorEvents[i].GetProbabilityMultiplier();

            return result;
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
                    "Progress: {0}{1}Real Time active: meteor periods are disabled.{1}Configured occurrence: {2:0.##} times per year.{1}Real Time reduction: {3:0.##}x.{1}Effective occurrence: {4:0.##} times per year.",
                    string.Format("{0:00.00}%", probabilityValue * 100),
                    CommonProperties.newLine,
                    BaseOccurrencePerYear,
                    RealTimeFrequencyMultiplier,
                    GetRealTimeOccurrencePerYear(BaseOccurrencePerYear));

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

        public float GetRealTimePatternProbabilityProgress()
        {
            EnsureRealTimeSchedule();
            var periodDays = GetRealTimePeriodDays();
            return Mathf.Clamp01(1f - realTimeDaysUntilNextMeteor / periodDays);
        }

        private float GetRealTimeOccurrencePerYear(float occurrencePerYear)
        {
            return occurrencePerYear / RealTimeFrequencyMultiplier;
        }

        private bool IsRealTimeMeteorDue()
        {
            EnsureRealTimeSchedule();
            return realTimeDaysUntilNextMeteor <= 0f;
        }

        private void EnsureRealTimeSchedule()
        {
            var periodDays = GetRealTimePeriodDays();

            if (realTimeDaysUntilNextMeteor < 0f)
                realTimeDaysUntilNextMeteor = GetRandomRealTimeScheduleOffset(periodDays);

            if (realTimeDaysUntilNextMeteor > periodDays)
                realTimeDaysUntilNextMeteor = periodDays;
        }

        private void ResetRealTimeSchedule()
        {
            realTimeDaysUntilNextMeteor = GetRealTimePeriodDays();
        }

        private float GetRealTimePeriodDays()
        {
            return 365f / Mathf.Max(0.0001f, GetRealTimeOccurrencePerYear(BaseOccurrencePerYear));
        }

        private static float GetRandomRealTimeScheduleOffset(float periodDays)
        {
            var maxDays = Mathf.Max(1, Mathf.CeilToInt(periodDays));
            return Services.Simulation.m_randomizer.Int32((uint)maxDays);
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

        public struct MeteorPeriodStatus
        {
            public string Name;
            public bool Enabled;
            public float ProbabilityMultiplier;
            public byte MaxIntensity;
            public string Description;
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
            }
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

                float fallPeriod_half = 30; // Days
                var daysDiffFromPeak = Mathf.Abs(DaysUntilNextEvent - fallPeriod_half);
                var multiplier = Mathf.Max(0, 1f - daysDiffFromPeak / fallPeriod_half);

                return multiplier;
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

                if (DaysUntilNextEvent <= 0)
                {
                    DaysUntilNextEvent = PeriodDays;
                    MeteorsFallen = 0;
                }
            }

            public void OnMeteorFallen()
            {
                if (MeteorsFallen > 0) return;

                MeteorsFallen++;
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

                if (DaysUntilNextEvent <= 60) return Name + " is approaching.";

                return Name + " will be close in " + DisasterSimulationUtils.FormatTimeSpan(DaysUntilNextEvent);
            }
        }
    }
}

//TODO
// Autoevacuate/release is not working
