using System;
using System.Collections.Generic;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Disaster;
using CommonServices = NaturalDisastersRenewal.Common.Services;

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

            CalmDays = 60;
            ProbabilityWarmupDays = 30;
            IntensityWarmupDays = 60;
            EvacuationMode = EvacuationOptions.ManualEvacuation;
        }

        public override string GetTooltipInformation()
        {
            if (!Unlocked)
            {
                return LocalizationService.Get("tooltip.notUnlockedOutsideArea");
            }

            if (!(CalmDaysLeft <= 0)) return base.GetTooltipInformation();

            if (IsRainActive() && RainFactor > 1)
            {
                return LocalizationService.Get("tooltip.thunderstorm.rainBoost");
            }

            return base.GetTooltipInformation();
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            var dt = CommonServices.Simulation.m_currentGameTime;
            int delta_month = Math.Abs(dt.Month - MaxProbabilityMonth);
            if (delta_month > 6) delta_month = 12 - delta_month;

            float occurence = base.GetCurrentOccurrencePerYearLocal() * (1f - delta_month / 6f);

            if (IsRainActive())
            {
                occurence *= RainFactor;
            }

            return occurence;
        }

        private static bool IsRainActive()
        {
            var weatherManager = CommonServices.Weather;
            return weatherManager is not null && (weatherManager.m_currentRain > 0.01f || weatherManager.m_targetRain > 0.01f);
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
            return (disasterAI as ThunderStormAI) is not null;
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
            }
        }

        //public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified)
        //{
        //    disasterInfoUnified.DisasterInfo.type = DisasterType.ThunderStorm;
        //    base.OnDisasterDetected(disasterInfoUnified);
        //}
    }
}
