using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Disaster;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.Tornado;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Tornado;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Tornado;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override string GetName()
        {
            return "Tornado";
        }

        public override float CalculateDestructionRadio(byte intensity)
        {
            int unitSize = 8;
            int unitsBase = 72;
            float unitCalculation;
            int intensityInt = intensity / 10;
            int intensityDec = intensity % 10;

            switch (intensity)
            {
                case byte n when (n <= 25):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + ((intensityDec * 2.48f) + (intensityInt * 32.8f));
                    break;

                case byte n when (n > 25 && n <= 50):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase - 4f + ((intensityDec * 2.64f) + (intensityInt * 34.4f));
                    break;

                case byte n when (n > 50 && n <= 75):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 170f + (intensityDec * 0.16f) + ((intensityDec - 1) * 2) + ((intensityInt - 5) * 29.6f);
                    break;

                case byte n when (n > 75 && n <= 100):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 240f + ((intensityDec - 5) * 0.48f) + ((intensityDec - 6) * 2) + ((intensityInt - 7) * 32.8f);
                    break;

                case byte n when (n > 100 && n <= 125):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 326f + (intensityDec * 0.08f) + ((intensityDec - 1) * 2) + (((intensityInt - 10) * 28.8f));
                    break;

                case byte n when (n > 125 && n <= 150):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 393f + ((intensityDec - 5) * 0.68f) + ((intensityDec - 6)) + ((intensityInt - 12) * 24.8f);
                    break;

                case byte n when (n > 150 && n <= 175):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 461f + (intensityDec * 0.28f) + ((intensityDec - 1) * 3) + ((intensityInt - 15) * 40.8f);
                    break;

                case byte n when (n > 175 && n <= 200):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 534f + ((intensityDec - 5) * 0.24f) + ((intensityDec + 6) * 2) + ((intensityInt - 17) * 30.4f);
                    break;

                case byte n when (n > 200 && n <= 250):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 638f + ((intensityDec - 1) * 2) + ((intensityInt - 20) * 28);
                    break;

                case byte n when (n > 225 && n <= 250):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 702 + ((intensityDec - 5) * 2.16f) + ((intensityInt - 22) * 29.6f);
                    break;

                default:
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 702 + ((intensityDec - 5) * 2.16f) + ((intensityInt - 22) * 29.6f) + (intensityDec * 0.24f);
                    break;
            }

            return (float)Math.Sqrt((unitCalculation / 2) * unitSize);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            TornadoModel tornado = disaster as TornadoModel;
            if (tornado != null)
            {
                MaxProbabilityMonth = tornado.MaxProbabilityMonth;
                NoTornadoDuringFog = tornado.NoTornadoDuringFog;
            }
        }        
    }

   
}