using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using NaturalDisastersRenewal.Serialization.NaturalDisaster;
using System;
using System.Collections.Generic;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class ThunderstormModel : DisasterBaseModel
    {
        public float RainFactor = 2.0f;
        public int MaxProbabilityMonth = 7;

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

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            DateTime dt = Singleton<SimulationManager>.instance.m_currentGameTime;
            int delta_month = Math.Abs(dt.Month - MaxProbabilityMonth);
            if (delta_month > 6) delta_month = 12 - delta_month;

            float occurence = base.GetCurrentOccurrencePerYearLocal() * (1f - delta_month / 6f);

            WeatherManager wm = Singleton<WeatherManager>.instance;
            if (wm.m_currentRain > 0)
            {
                occurence *= RainFactor;
            }

            return occurence;
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
            base.OnDisasterStarted(intensity);            
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as ThunderStormAI != null;
        }

        public override string GetName()
        {
            return CommonProperties.thunderstormName;
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            ThunderstormModel d = disaster as ThunderstormModel;
            if (d != null)
            {
                RainFactor = d.RainFactor;
                MaxProbabilityMonth = d.MaxProbabilityMonth;
            }
        }

        //public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified)
        //{
        //    disasterInfoUnified.DisasterInfo.type = DisasterType.ThunderStorm;
        //    base.OnDisasterDetected(disasterInfoUnified);
        //}
    }
}