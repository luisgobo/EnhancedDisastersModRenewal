﻿using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models;
using NaturalDisastersRenewal.Services.LegacyStructure.Handlers;
using System;

namespace NaturalDisastersRenewal.Services.LegacyStructure.NaturalDisaster
{
    public class SinkholeService : DisasterBaseService
    {
        public class Data : SerializableDataCommon, IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                SinkholeService d = Singleton<NaturalDisasterHandler>.instance.container.Sinkhole;
                SerializeCommonParameters(s, d);
                s.WriteFloat(d.GroundwaterCapacity);
                s.WriteFloat(d.groundwaterAmount);
            }

            public void Deserialize(DataSerializer s)
            {
                SinkholeService d = Singleton<NaturalDisasterHandler>.instance.container.Sinkhole;
                DeserializeCommonParameters(s, d);
                d.GroundwaterCapacity = s.ReadFloat();
                d.groundwaterAmount = s.ReadFloat();
            }

            public void AfterDeserialize(DataSerializer s)
            {
                AfterDeserializeLog("Sinkhole");
            }
        }

        public float GroundwaterCapacity = 50;
        float groundwaterAmount = 0; // groundwaterAmount=1 means rain of intensity 1 during 1 day

        public SinkholeService()
        {
            DType = DisasterType.Sinkhole;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
            BaseOccurrencePerYear = 1.5f; // When groundwater is full
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            calmDays = 30;
            probabilityWarmupDays = 0;
            intensityWarmupDays = 0;            
        }

        public override string GetProbabilityTooltip()
        {
            if (!unlocked)
            {
                return "Not unlocked yet";
            }

            if (calmDaysLeft <= 0)
            {
                int groundWaterPercent = (int)(100 * groundwaterAmount / GroundwaterCapacity);
                return "Ground water level " + groundWaterPercent.ToString() + "%";
            }

            return base.GetProbabilityTooltip();
        }

        protected override void OnSimulationFrameLocal()
        {
            float daysPerFrame = Helper.DaysPerFrame;

            WeatherManager wm = Singleton<WeatherManager>.instance;
            if (wm.m_currentRain > 0)
            {
                groundwaterAmount += wm.m_currentRain * daysPerFrame;
            }

            groundwaterAmount -= (groundwaterAmount / GroundwaterCapacity) * daysPerFrame;

            if (groundwaterAmount < 0)
            {
                groundwaterAmount = 0;
            }
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId)
        {
            disasterInfo.type |= DisasterType.Sinkhole;
            base.OnDisasterActivated(disasterInfo, disasterId);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Sinkhole;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;
            base.OnDisasterDeactivated(disasterInfoUnified);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Sinkhole;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;

            base.OnDisasterDetected(disasterInfoUnified);
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
            return "Sinkhole";
        }

        public override float CalculateDestructionRadio(byte intensity)
        {
            int unitSize = 8;
            int unitsBase = 24; //24 + 4 Original, Distance Fix for proximity
            float unitCalculation;
            int intensityInt = intensity / 10;
            int intensityDec = intensity % 10;

            switch (intensity)
            {
                case byte n when (n < 26):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - (0.28f * intensityDec) - (intensityInt * 2.8f);
                    break;
                case byte n when (n >= 26 && n < 101):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - (0.28f * intensityDec) - (intensityInt * 2.8f) - ((intensityDec - 5) * 0.04f) - ((intensityInt - 2) * 0.4f);
                    break;
                case byte n when (n >= 101 && n < 126):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - (0.28f * intensityDec) - (intensityInt * 2.8f) - 3f;
                    break;
                case byte n when (n >= 126 && n <151):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - (0.28f * intensityDec) - (intensityInt * 2.8f) - ((intensityDec - 5) * 0.04f) - ((intensityInt - 2) * 0.3f) - ((intensityDec - 5) * 0.04f) - (0.5f * (intensityInt - 12));
                    break;
                case byte n when (n >= 151 && n < 176):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - (0.28f * intensityDec) - (intensityInt * 2.8f) - ((intensityDec - 5) * 0.04f) - ((intensityInt - 2) * 0.4f);
                    break;
                case byte n when (n >= 176 && n < 201):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - (0.28f * intensityDec) - (intensityInt * 2.8f) - ((intensityDec - 5) * 0.04f) - ((intensityInt - 2) * 0.4f) + ((intensityDec - 5) * 0.08f) + ((intensityInt - 17) * 0.8f);
                    break;
                case byte n when (n >= 201 && n < 226):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - (0.28f * intensityDec) - (intensityInt * 2.8f) - ((intensityDec - 5) * 0.04f) - ((intensityInt - 2) * 0.3f) - ((intensityDec - 5) * 0.04f) - (0.5f * (intensityInt - 12)) + 4f;
                    break;
                case byte n when (n >= 226 && n < 251):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - (0.28f * intensityDec) - (intensityInt * 2.8f) - ((intensityDec - 5) * 0.04f) - ((intensityInt - 2) * 0.4f) + 1;
                    break;
                default:
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - (0.28f * intensityDec) - (intensityInt * 2.8f) - ((intensityDec - 5) * 0.04f) - ((intensityInt - 2) * 0.3f) - ((intensityDec - 5) * 0.04f) - (0.5f * (intensityInt - 12)) + 5 + (intensityDec * 0.36f);
                    break;
            }

            return (float)Math.Sqrt((unitCalculation / 2) * unitSize);
        }

        public override void CopySettings(DisasterBaseService disaster)
        {
            base.CopySettings(disaster);

            SinkholeService d = disaster as SinkholeService;
            if (d != null)
            {
                GroundwaterCapacity = d.GroundwaterCapacity;
            }
        }
    }
}