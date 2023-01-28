using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Serialization;
using System;

namespace NaturalDisastersRenewal.DisasterServices
{
    public class ForestFireService : DisasterSerialization
    {
        public class Data : SerializableDataCommon, IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                ForestFireService d = Singleton<DisasterManager>.instance.container.ForestFire;
                serializeCommonParameters(s, d);
                s.WriteInt32(d.WarmupDays);
                s.WriteFloat(d.noRainDays);
            }

            public void Deserialize(DataSerializer s)
            {
                ForestFireService d = Singleton<DisasterManager>.instance.container.ForestFire;
                deserializeCommonParameters(s, d);
                d.WarmupDays = s.ReadInt32();
                if (s.version <= 2)
                {
                    float daysPerFrame = Helper.DaysPerFrame;
                    d.noRainDays = s.ReadInt32() * daysPerFrame;
                }
                else
                {
                    d.noRainDays = s.ReadFloat();
                }
            }

            public void AfterDeserialize(DataSerializer s)
            {
                afterDeserializeLog("ForestFire");
            }
        }

        public int WarmupDays = 180;
        float noRainDays = 0;

        public ForestFireService()
        {
            DType = DisasterType.ForestFire;
            OccurrenceAreaBeforeUnlock = OccurrenceAreas.LockedAreas;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.Everywhere;
            BaseOccurrencePerYear = 10.0f; // In case of dry weather
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            calmDays = 7;
            probabilityWarmupDays = 0;
            intensityWarmupDays = 0;
            EvacuationMode = 0;
        }

        protected override void onSimulationFrame_local()
        {
            WeatherManager wm = Singleton<WeatherManager>.instance;
            if (wm.m_currentRain > 0)
            {
                noRainDays = 0;
            }
            else
            {
                noRainDays += Helper.DaysPerFrame;
            }
        }

        public override string GetProbabilityTooltip()
        {
            string tooltip = "";

            if (!unlocked)
            {
                tooltip = "Not unlocked yet (occurs only outside of your area)." + Environment.NewLine;
            }

            if (calmDaysLeft == 0)
            {
                if (noRainDays <= 0)
                {
                    return tooltip + "No " + GetName() + " during rain.";
                }
                else
                {
                    if (noRainDays >= WarmupDays)
                    {
                        return tooltip + "Maximum because there was no rain for more than " + WarmupDays.ToString() + " days.";
                    }

                    return tooltip + "Increasing because there was no rain for " + Helper.FormatTimeSpan(noRainDays);
                }
            }

            return base.GetProbabilityTooltip();
        }

        protected override float getCurrentOccurrencePerYear_local()
        {
            return base.getCurrentOccurrencePerYear_local() * Math.Min(1f, noRainDays / WarmupDays);
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as ForestFireAI != null;
        }

        public override string GetName()
        {
            return "Forest Fire";
        }

        public override void CopySettings(DisasterSerialization disaster)
        {
            base.CopySettings(disaster);

            ForestFireService d = disaster as ForestFireService;
            if (d != null)
            {
                WarmupDays = d.WarmupDays;
            }
        }
    }
}