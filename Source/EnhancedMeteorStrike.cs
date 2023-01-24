﻿using ICities;
using ColossalFramework;
using ColossalFramework.IO;
using UnityEngine;
using System;

namespace EnhancedDisastersMod
{
    public class EnhancedMeteorStrike : EnhancedDisaster
    {
        public class Data : SerializableDataCommon, IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                EnhancedMeteorStrike d = Singleton<EnhancedDisastersManager>.instance.container.MeteorStrike;
                serializeCommonParameters(s, d);

                for (int i = 0; i < d.meteorEvents.Length; i++)
                {
                    s.WriteBool(d.meteorEvents[i].Enabled);
                    s.WriteFloat(d.meteorEvents[i].PeriodDays);
                    s.WriteInt8(d.meteorEvents[i].MaxIntensity);
                    s.WriteFloat(d.meteorEvents[i].DaysUntilNextEvent);
                    s.WriteInt32(d.meteorEvents[i].MeteorsFallen);
                }
            }

            public void Deserialize(DataSerializer s)
            {
                EnhancedMeteorStrike d = Singleton<EnhancedDisastersManager>.instance.container.MeteorStrike;
                deserializeCommonParameters(s, d);

                if (s.version <= 2)
                {
                    float daysPerFrame = Helper.DaysPerFrame;
                    for (int i = 0; i < d.meteorEvents.Length; i++)
                    {
                        d.meteorEvents[i].Enabled = s.ReadBool();
                        d.meteorEvents[i].PeriodDays = s.ReadInt32() * daysPerFrame;
                        d.meteorEvents[i].MaxIntensity = (byte)s.ReadInt8();
                        d.meteorEvents[i].DaysUntilNextEvent = s.ReadInt32() * daysPerFrame;
                        d.meteorEvents[i].MeteorsFallen = s.ReadInt32();
                    }
                }
                else
                {
                    for (int i = 0; i < d.meteorEvents.Length; i++)
                    {
                        d.meteorEvents[i].Enabled = s.ReadBool();
                        d.meteorEvents[i].PeriodDays = s.ReadFloat();
                        d.meteorEvents[i].MaxIntensity = (byte)s.ReadInt8();
                        d.meteorEvents[i].DaysUntilNextEvent = s.ReadFloat();
                        d.meteorEvents[i].MeteorsFallen = s.ReadInt32();
                    }
                }
            }

            public void AfterDeserialize(DataSerializer s)
            {
                afterDeserializeLog("EnhancedMeteorStrike");
            }
        }

        private struct MeteorEvent
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

        private MeteorEvent[] meteorEvents;

        public EnhancedMeteorStrike()
        {
            DType = DisasterType.MeteorStrike;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
            BaseOccurrencePerYear = 10.0f;
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            meteorEvents = new MeteorEvent[] {
                MeteorEvent.Init("Long period meteor", 9, 100),
                MeteorEvent.Init("Medium period meteor", 5, 70),
                MeteorEvent.Init("Short period meteor", 2, 30)
            };
        }

        [System.Xml.Serialization.XmlElement]
        public bool Meteor1Enabled
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
        public bool Meteor2Enabled
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
        public bool Meteor3Enabled
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

        protected override void onSimulationFrame_local()
        {
            for (int i = 0; i < meteorEvents.Length; i++)
            {
                meteorEvents[i].OnSimulationFrame();
            }
        }

        protected override float getCurrentOccurrencePerYear_local()
        {
            float baseValue = base.getCurrentOccurrencePerYear_local();

            float result = 0;

            for (int i = 0; i < meteorEvents.Length; i++)
            {
                result += baseValue * meteorEvents[i].GetProbabilityMultiplier();
            }

            return result;
        }

        public override byte GetMaximumIntensity()
        {
            byte intensity = 10;

            for (int i = 0; i < meteorEvents.Length; i++)
            {
                intensity = Math.Max(intensity, meteorEvents[i].GetActualMaxIntensity());
            }

            intensity = scaleIntensityByPopulation(intensity);

            return intensity;
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

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as MeteorStrikeAI != null;
        }

        public override string GetName()
        {
            return "Meteor Strike";
        }

        public override string GetProbabilityTooltip()
        {
            if (!unlocked)
            {
                return "Not unlocked yet";
            }

            string result = "";

            for (int i = 0; i < meteorEvents.Length; i++)
            {
                result += meteorEvents[i].GetStateDescription() + Environment.NewLine;
            }

            return result;
        }

        public override void CopySettings(EnhancedDisaster disaster)
        {
            base.CopySettings(disaster);

            EnhancedMeteorStrike d = disaster as EnhancedMeteorStrike;
            if (d != null)
            {
                Meteor1Enabled = d.Meteor1Enabled;
                Meteor2Enabled = d.Meteor2Enabled;
                Meteor3Enabled = d.Meteor3Enabled;
            }
        }
    }
}
