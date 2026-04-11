using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Disaster;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class SinkholeModel : DisasterBaseModel
    {
        [XmlIgnore] public float groundwaterAmount; // groundwaterAmount=1 means rain of intensity 1 during 1 day
        public float GroundwaterCapacity = 50;

        public SinkholeModel()
        {
            DType = DisasterType.Sinkhole;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
            BaseOccurrencePerYear = 1.5f; // When groundwater is full
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            calmDays = 30;
            probabilityWarmupDays = 0;
            intensityWarmupDays = 0;
        }

        public override string GetProbabilityTooltip(float value)
        {
            if (!unlocked) return "Not unlocked yet";

            if (calmDaysLeft <= 0)
            {
                var groundWaterPercent = (int)(100 * groundwaterAmount / GroundwaterCapacity);
                return LocalizationService.Format("tooltip.sinkhole.groundwater", groundWaterPercent);
            }

            return base.GetProbabilityTooltip(value);
        }

        protected override void OnSimulationFrameLocal()
        {
            var daysPerFrame = DisasterSimulationUtils.DaysPerFrame;

            var wm = Services.Weather;
            if (wm.m_currentRain > 0) groundwaterAmount += wm.m_currentRain * daysPerFrame;

            groundwaterAmount -= groundwaterAmount / GroundwaterCapacity * daysPerFrame;

            if (groundwaterAmount < 0) groundwaterAmount = 0;
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.Sinkhole;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Sinkhole;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Sinkhole;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterStarted(byte intensity)
        {
            groundwaterAmount = 0;
            base.OnDisasterStarted(intensity);
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            return base.GetCurrentOccurrencePerYearLocal() * groundwaterAmount / GroundwaterCapacity;
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as SinkholeAI != null;
        }

        public override string GetName()
        {
            return LocalizationService.GetDisasterName(DType);
        }

        public override float CalculateDestructionRadio(byte intensity)
        {
            var unitSize = 8;
            var unitsBase = 24; //24 + 4 Original, Distance Fix for proximity
            float unitCalculation;
            var intensityInt = intensity / 10;
            var intensityDec = intensity % 10;

            switch (intensity)
            {
                case byte n when n < 26:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f;
                    break;

                case byte n when n >= 26 && n < 101:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                                      (intensityInt - 2) * 0.4f;
                    break;

                case byte n when n >= 101 && n < 126:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f - 3f;
                    break;

                case byte n when n >= 126 && n < 151:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                                      (intensityInt - 2) * 0.3f - (intensityDec - 5) * 0.04f -
                                      0.5f * (intensityInt - 12);
                    break;

                case byte n when n >= 151 && n < 176:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                                      (intensityInt - 2) * 0.4f;
                    break;

                case byte n when n >= 176 && n < 201:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                        0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                        (intensityInt - 2) * 0.4f + (intensityDec - 5) * 0.08f + (intensityInt - 17) * 0.8f;
                    break;

                case byte n when n >= 201 && n < 226:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                        0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                        (intensityInt - 2) * 0.3f - (intensityDec - 5) * 0.04f - 0.5f * (intensityInt - 12) + 4f;
                    break;

                case byte n when n >= 226 && n < 251:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                        0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                        (intensityInt - 2) * 0.4f + 1;
                    break;

                default:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      0.28f * intensityDec - intensityInt * 2.8f - (intensityDec - 5) * 0.04f -
                                      (intensityInt - 2) * 0.3f - (intensityDec - 5) * 0.04f -
                                      0.5f * (intensityInt - 12) + 5 +
                                      intensityDec * 0.36f;
                    break;
            }

            return (float)Math.Sqrt(unitCalculation / 2 * unitSize);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            var d = disaster as SinkholeModel;
            if (d != null) GroundwaterCapacity = d.GroundwaterCapacity;
        }
    }
}