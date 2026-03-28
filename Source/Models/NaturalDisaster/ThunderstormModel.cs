using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class ThunderstormModel : DisasterBaseModel
    {
        private const float MinimumSeasonFactor = 0.35f;
        private const float ChargeDaysToMax = 45f;
        private const float RainChargeThreshold = 0.35f;
        private const float RainActivationThreshold = 0.55f;
        private const float ChargeOccurrenceBoost = 0.5f;

        public float RainFactor = 2.0f;
        public int MaxProbabilityMonth = 7;
        [XmlIgnore] public float StormChargeDays;

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

            var seasonFactor = GetSeasonFactor();
            var chargeRatio = GetChargeRatio();
            if (IsRainActive() && RainFactor > 1)
            {
                return LocalizationService.Format("tooltip.thunderstorm.rainBoostCharge", (int)(chargeRatio * 100f));
            }

            if (seasonFactor <= 0f)
            {
                return LocalizationService.Get("tooltip.thunderstorm.outOfSeason");
            }

            if (chargeRatio < RainChargeThreshold)
            {
                return LocalizationService.Format("tooltip.thunderstorm.buildingCharge", (int)(chargeRatio * 100f));
            }

            return LocalizationService.Format("tooltip.thunderstorm.waitingForRain", (int)(seasonFactor * 100f));
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            var seasonFactor = GetSeasonFactor();
            var chargeRatio = GetChargeRatio();
            var effectiveSeasonFactor = Mathf.Max(seasonFactor, MinimumSeasonFactor * chargeRatio);
            var effectiveOccurrenceFactor = effectiveSeasonFactor + ChargeOccurrenceBoost * chargeRatio;

            float occurence = base.GetCurrentOccurrencePerYearLocal() * effectiveOccurrenceFactor;

            if (IsRainActive())
            {
                occurence *= RainFactor;
            }

            return occurence;
        }

        protected override void OnSimulationFrameLocal()
        {
            var daysPerFrame = Helper.GetDaysPerFrame(CurrentTimeBehaviorMode);
            var seasonFactor = GetSeasonFactor();

            if (seasonFactor > 0f)
            {
                StormChargeDays += daysPerFrame * (0.4f + seasonFactor);
            }
            else
            {
                StormChargeDays -= daysPerFrame * 0.75f;
            }

            if (IsRainActive())
            {
                StormChargeDays += daysPerFrame * 0.75f;
            }

            StormChargeDays = Mathf.Clamp(StormChargeDays, 0f, ChargeDaysToMax);
            PromoteRainIfNeeded(seasonFactor);
        }

        private static bool IsRainActive()
        {
            var weatherManager = CommonServices.Weather;
            return weatherManager is not null && (weatherManager.m_currentRain > 0.01f || weatherManager.m_targetRain > 0.01f);
        }

        private float GetSeasonFactor()
        {
            var dt = CommonServices.Simulation.m_currentGameTime;
            int deltaMonth = Math.Abs(dt.Month - MaxProbabilityMonth);
            if (deltaMonth > 6) deltaMonth = 12 - deltaMonth;
            return 1f - deltaMonth / 6f;
        }

        private float GetChargeRatio()
        {
            return Mathf.Clamp01(StormChargeDays / ChargeDaysToMax);
        }

        private void PromoteRainIfNeeded(float seasonFactor)
        {
            if (seasonFactor <= 0f || IsRainActive())
            {
                return;
            }

            var weatherManager = CommonServices.Weather;
            if (weatherManager == null)
            {
                return;
            }

            var chargeRatio = GetChargeRatio();
            if (chargeRatio < RainChargeThreshold)
            {
                return;
            }

            var desiredRain = Mathf.Clamp01(0.08f + seasonFactor * chargeRatio * 0.85f);
            if (chargeRatio >= RainActivationThreshold && desiredRain > weatherManager.m_targetRain)
            {
                weatherManager.m_targetRain = desiredRain;
            }
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
            StormChargeDays = 0f;
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

        protected override void ResetDisasterState()
        {
            StormChargeDays = 0f;
        }
    }
}
