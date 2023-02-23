using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using System;
using System.Xml.Serialization;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class ForestFireModel : DisasterBaseModel
    {
        public int WarmupDays = 180;
        [XmlIgnore] public float noRainDays = 0;

        public ForestFireModel()
        {
            DType = DisasterType.ForestFire;
            OccurrenceAreaBeforeUnlock = OccurrenceAreas.LockedAreas;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.Everywhere;
            BaseOccurrencePerYear = 10.0f; // In case of dry weather
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            calmDays = 7;
            probabilityWarmupDays = 0;
            intensityWarmupDays = 0;
        }

        protected override void OnSimulationFrameLocal()
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

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            return base.GetCurrentOccurrencePerYearLocal() * Math.Min(1f, noRainDays / WarmupDays);
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as ForestFireAI != null;
        }

        public override string GetName()
        {
            return "Forest Fire";
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            ForestFireModel d = disaster as ForestFireModel;
            if (d != null)
            {
                WarmupDays = d.WarmupDays;
            }
        }
        
    }
}