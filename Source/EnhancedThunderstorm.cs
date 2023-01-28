using System;
using ICities;
using ColossalFramework;
using ColossalFramework.IO;
using UnityEngine;

namespace NaturalDisastersRenewal
{
    public class EnhancedThunderstorm: EnhancedDisaster
    {
        public class Data : SerializableDataCommon, IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                EnhancedThunderstorm d = Singleton<EnhancedDisastersManager>.instance.container.Thunderstorm;
                serializeCommonParameters(s, d);
                s.WriteInt32(d.MaxProbabilityMonth);
                s.WriteFloat(d.RainFactor);
            }

            public void Deserialize(DataSerializer s)
            {
                EnhancedThunderstorm d = Singleton<EnhancedDisastersManager>.instance.container.Thunderstorm;
                deserializeCommonParameters(s, d);
                d.MaxProbabilityMonth = s.ReadInt32();
                d.RainFactor = s.ReadFloat();
            }

            public void AfterDeserialize(DataSerializer s)
            {
                afterDeserializeLog("Thunderstorm");
            }
        }

        public float RainFactor = 2.0f;
        public int MaxProbabilityMonth = 7;

        public EnhancedThunderstorm()
        {
            DType = DisasterType.ThunderStorm;
            OccurrenceAreaBeforeUnlock = OccurrenceAreas.LockedAreas;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.Everywhere;
            BaseOccurrencePerYear = 2.0f;
            ProbabilityDistribution = ProbabilityDistributions.PowerLow;

            calmDays = 60;
            probabilityWarmupDays = 30;
            intensityWarmupDays = 60;
            EvacuationMode = 0;
        }

        public override string GetProbabilityTooltip()
        {
            if (!unlocked)
            {
                return "Not unlocked yet (occurs only outside of your area).";
            }

            if (calmDaysLeft <= 0)
            {
                if (Singleton<WeatherManager>.instance.m_currentRain > 0 && RainFactor > 1)
                {
                    return "Increased because of rain.";
                }
            }

            return base.GetProbabilityTooltip();
        }

        protected override float getCurrentOccurrencePerYear_local()
        {
            DateTime dt = Singleton<SimulationManager>.instance.m_currentGameTime;
            int delta_month = Math.Abs(dt.Month - MaxProbabilityMonth);
            if (delta_month > 6) delta_month = 12 - delta_month;

            float occurence = base.getCurrentOccurrencePerYear_local() * (1f - delta_month / 6f);

            WeatherManager wm = Singleton<WeatherManager>.instance;
            if (wm.m_currentRain > 0)
            {
                occurence *= RainFactor;
            }

            return occurence;
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as ThunderStormAI != null;
        }

        public override string GetName()
        {
            return "Thunderstorm";
        }

        public override void CopySettings(EnhancedDisaster disaster)
        {
            base.CopySettings(disaster);

            EnhancedThunderstorm d = disaster as EnhancedThunderstorm;
            if (d != null)
            {
                RainFactor = d.RainFactor;
                MaxProbabilityMonth = d.MaxProbabilityMonth;
            }
        }
    }
}
