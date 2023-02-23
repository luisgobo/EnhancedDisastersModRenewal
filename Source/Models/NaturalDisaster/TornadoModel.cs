using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Serialization.NaturalDisaster;
using System;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class TornadoModel : DisasterBaseModel
    {
        public int MaxProbabilityMonth = 5;
        public bool NoTornadoDuringFog = true;

        public TornadoModel()
        {
            DType = DisasterType.Tornado;
            BaseOccurrencePerYear = 1.5f;
            ProbabilityDistribution = ProbabilityDistributions.PowerLow;

            calmDays = 360 * 2;
            probabilityWarmupDays = 180;
            intensityWarmupDays = 180;
            intensityWarmupDays = 180;
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            if (NoTornadoDuringFog && Singleton<WeatherManager>.instance.m_currentFog > 0)
            {
                return 0;
            }

            DateTime dt = Singleton<SimulationManager>.instance.m_currentGameTime;
            int delta_month = Math.Abs(dt.Month - MaxProbabilityMonth);
            if (delta_month > 6) delta_month = 12 - delta_month;

            float occurrence = base.GetCurrentOccurrencePerYearLocal() * (1f - delta_month / 6f);

            return occurrence;
        }

        public override string GetProbabilityTooltip()
        {
            if (calmDaysLeft <= 0)
            {
                if (NoTornadoDuringFog && Singleton<WeatherManager>.instance.m_currentFog > 0)
                {
                    return "No " + GetName() + " during fog.";
                }
            }

            return base.GetProbabilityTooltip();
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as TornadoAI != null;
        }

        public override string GetName()
        {
            return "Tornado";
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            TornadoModel d = disaster as TornadoModel;
            if (d != null)
            {
                MaxProbabilityMonth = d.MaxProbabilityMonth;
                NoTornadoDuringFog = d.NoTornadoDuringFog;
            }
        }

        //public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified)
        //{
        //    disasterInfoUnified.DisasterInfo.type = DisasterType.Tornado;
        //    base.OnDisasterDetected(disasterInfoUnified);
        //}
    }
}