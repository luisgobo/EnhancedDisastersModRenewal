﻿using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Serialization.NaturalDisaster;
using System;

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

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as ThunderStormAI != null;
        }

        public override string GetName()
        {
            return "Thunderstorm";
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