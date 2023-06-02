using ColossalFramework;
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
    public class MeteorStrikeModel : DisasterBaseModel
    {
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

            public static MeteorEvent Init(string name, float periodYears, byte maxIntensity)
            {
                SimulationManager sm = Singleton<SimulationManager>.instance;

                float periodDays = periodYears * 365;
                return new MeteorEvent(
                    name,
                    periodDays * 0.95f + sm.m_randomizer.Int32((uint)(periodDays * 0.1f)),
                    maxIntensity,
                    periodDays / 2 + sm.m_randomizer.Int32((uint)(periodDays / 2))
                    );
            }

            public float GetProbabilityMultiplier()
            {
                if (!Enabled) return 0;

                if (MeteorsFallen > 0)
                {
                    return 0;
                }

                float fallPeriod_half = 30; // Days
                float daysDiffFromPeak = Mathf.Abs(DaysUntilNextEvent - fallPeriod_half);
                float multiplier = Mathf.Max(0, 1f - daysDiffFromPeak / fallPeriod_half);

                return multiplier;
            }

            public byte GetActualMaxIntensity()
            {
                if (!Enabled) return 1;

                if (DaysUntilNextEvent < 60)
                {
                    return MaxIntensity;
                }

                return 1;
            }

            public void OnSimulationFrame()
            {
                if (!Enabled) return;

                DaysUntilNextEvent -= Helper.DaysPerFrame;

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
                    PeriodDays / 365, MaxIntensity, Helper.FormatTimeSpan(DaysUntilNextEvent));
            }

            public string GetStateDescription()
            {
                if (!Enabled) return "";

                if (MeteorsFallen > 0)
                {
                    return Name + " already fallen.";
                }

                if (DaysUntilNextEvent <= 60)
                {
                    return Name + " is approaching.";
                }
                else
                {
                    return Name + " will be close in " + Helper.FormatTimeSpan(DaysUntilNextEvent);
                }
            }
        }

        [XmlIgnore] public MeteorEvent[] meteorEvents;

        public MeteorStrikeModel()
        {
            DType = DisasterType.MeteorStrike;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
            BaseOccurrencePerYear = 10.0f;
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            //Original
            //meteorEvents = new MeteorEvent[] {
            //    MeteorEvent.Init("Long period meteor", 9, 100),
            //    MeteorEvent.Init("Medium period meteor", 5, 70),
            //    MeteorEvent.Init("Short period meteor", 2, 30)
            //};

            meteorEvents = new MeteorEvent[] {
                MeteorEvent.Init("Long period meteor", 9, 190),
                MeteorEvent.Init("Medium period meteor", 5, 120),
                MeteorEvent.Init("Short period meteor", 2, 30)
            };
        }

        [System.Xml.Serialization.XmlElement]
        public bool MeteorLongPeriodEnabled
        {
            get
            {
                return GetEnabled(0);
            }

            set
            {
                SetEnabled(0, value);
            }
        }

        [System.Xml.Serialization.XmlElement]
        public bool MeteorMediumPeriodEnabled
        {
            get
            {
                return GetEnabled(1);
            }

            set
            {
                SetEnabled(1, value);
            }
        }

        [System.Xml.Serialization.XmlElement]
        public bool MeteorShortPeriodEnabled
        {
            get
            {
                return GetEnabled(2);
            }

            set
            {
                SetEnabled(2, value);
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
            for (int i = 0; i < meteorEvents.Length; i++)
            {
                meteorEvents[i].OnSimulationFrame();
            }
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.MeteorStrike;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.MeteorStrike;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.MeteorStrike;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterStarted(byte intensity)
        {
            int meteorIndex = -1;
            float maxProbability = 0;

            for (int i = 0; i < meteorEvents.Length; i++)
            {
                float prob = meteorEvents[i].GetProbabilityMultiplier();
                if (prob > maxProbability)
                {
                    maxProbability = prob;
                    meteorIndex = i;
                }
            }

            // Should not happen
            if (meteorIndex == -1)
            {
                meteorIndex = 2;
            }

            meteorEvents[meteorIndex].OnMeteorFallen();

            base.OnDisasterStarted(intensity);
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            float baseValue = base.GetCurrentOccurrencePerYearLocal();

            float result = 0;

            for (int i = 0; i < meteorEvents.Length; i++)
            {
                result += baseValue * meteorEvents[i].GetProbabilityMultiplier();
            }

            return result;
        }

        public override byte GetMaximumIntensity()
        {
            byte intensity = baseIntensity;

            for (int i = 0; i < meteorEvents.Length; i++)
            {
                intensity = Math.Max(intensity, meteorEvents[i].GetActualMaxIntensity());
            }

            intensity = ScaleIntensityByPopulation(intensity);

            return intensity;
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as MeteorStrikeAI != null;
        }

        public override string GetName()
        {
            return CommonProperties.meteorStrikeName;
        }

        public override string GetProbabilityTooltip(float value)
        {
            if (!unlocked)
            {
                return "Not unlocked yet";
            }

            string result = value == 0 ? "Probability: 0.00" : $"Probability: {value * 10:#.#}";

            for (int i = 0; i < meteorEvents.Length; i++)
            {
                if (i == 0)
                    result += ". ";

                result += meteorEvents[i].GetStateDescription() + Environment.NewLine;
            }

            return result;
        }

        public override float CalculateDestructionRadio(byte intensity)
        {
            int unitSize = 8;
            int unitsBase = 24; //24 Original, Distance Fix for proximity
            float unitCalculation;
            int intensityInt = intensity / 10;
            int intensityDec = intensity % 10;

            switch (intensity)
            {
                case byte n when (n < 25):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase + 4 - (intensityDec * 0.04f) - (intensityInt * 0.4f);
                    break;

                case byte n when (n >= 25 && n < 50):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase + 3 - ((intensityInt - 2) * 0.8f) - ((intensityDec - 5) * 0.08f);
                    break;

                case byte n when (n >= 50 && n <= 55):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase + 1;
                    break;

                case byte n when (n > 55 && n <= 75):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase + 1 - ((intensityInt - 5) * 0.5f) - ((intensityDec - 5) * 0.05f);
                    break;

                case byte n when (n > 75 && n <= 100):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - ((intensityInt - 7) * 0.8f) - ((intensityDec - 5) * 0.08f);
                    break;

                case byte n when (n > 100 && n <= 125):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - 2 - ((intensityInt - 10) * 0.4f) - ((intensityDec) * 0.04f);
                    break;

                case byte n when (n > 125 && n <= 150):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - 3 + ((intensityInt - 12) * 0.4f) + ((intensityDec - 5) * 0.04f);
                    break;

                case byte n when (n > 150 && n <= 175):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - 2 - ((intensityInt - 15) * 1.2f) - (intensityDec * 0.12f);
                    break;

                case byte n when (n > 175 && n <= 250):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - 5 - ((intensityInt - 17) * 0.4f) - ((intensityDec - 5) * 0.04f);
                    break;

                default:
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - 5 - ((intensityInt - 17) * 0.4f) - ((intensityDec - 5) * 0.04f) + (intensityDec * 0.24f);
                    break;
            }

            return (float)Math.Sqrt((unitCalculation / 2) * unitSize);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            MeteorStrikeModel d = disaster as MeteorStrikeModel;
            if (d != null)
            {
                MeteorLongPeriodEnabled = d.MeteorLongPeriodEnabled;
                MeteorMediumPeriodEnabled = d.MeteorMediumPeriodEnabled;
                MeteorShortPeriodEnabled = d.MeteorShortPeriodEnabled;
            }
        }
    }
}