using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;
using DisasterType = ICities.DisasterType;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class ForestFireModel : DisasterBaseModel
    {
        public int WarmupDays = 180;
        [XmlIgnore] public float NoRainDays;

        public ForestFireModel()
        {
            DType = DisasterType.ForestFire;
            OccurrenceAreaBeforeUnlock = OccurrenceAreas.LockedAreas;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.Everywhere;
            BaseOccurrencePerYear = 10.0f; // In case of dry weather
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            CalmDays = 7;
            ProbabilityWarmupDays = 0;
            IntensityWarmupDays = 0;
        }

        public override void OnSimulationFrame()
        {
            if (!Enabled) return;

            if (!Unlocked && OccurrenceAreaBeforeUnlock == OccurrenceAreas.Nowhere) return;

            OnSimulationFrameLocal();

            var daysPerFrame = IsRealTimeActive ? Helper.DaysPerFrame * realTimeAccelerationIndex : Helper.DaysPerFrame;

            if (CalmDaysLeft > 0)
            {
                CalmDaysLeft -= daysPerFrame;
                return;
            }

            if (ProbabilityWarmupDaysLeft > 0)
            {
                if (ProbabilityWarmupDaysLeft > ProbabilityWarmupDays)
                    ProbabilityWarmupDaysLeft = ProbabilityWarmupDays;

                ProbabilityWarmupDaysLeft -= daysPerFrame;
            }

            if (IntensityWarmupDaysLeft > 0)
            {
                if (IntensityWarmupDaysLeft > IntensityWarmupDays) IntensityWarmupDaysLeft = IntensityWarmupDays;

                IntensityWarmupDaysLeft -= daysPerFrame;
            }

            var occurrencePerYear = GetCurrentOccurrencePerYear();

            if (occurrencePerYear == 0) return;

            var sm = Singleton<SimulationManager>.instance;
            var occurrencePerFrame = occurrencePerYear / 365 * daysPerFrame;

            var randomizedValue = sm.m_randomizer.Int32(randomizerRange);
            var randomizedOccurrence = (uint)(randomizerRange * occurrencePerFrame);
            // Debug.Log(
            //     $"occurrencePerYear: {occurrencePerYear}, daysPerFrame: {daysPerFrame}, occurrencePerFrame: {occurrencePerFrame}");
            // Debug.Log(
            //     $"Start Disaster? randomizedValue: {randomizedValue} < randomizedOccurrence {randomizedOccurrence}");

            // New Logarithmic activation based on occurrence per year
            var probabilityProgress = 0f;
            if (occurrencePerYear <= 0.1f)
                probabilityProgress = 0f;
            else if (occurrencePerYear >= 10f)
                probabilityProgress = 1f;
            else
                probabilityProgress = (1f + Mathf.Log10(occurrencePerYear)) / 2f;

            // Debug.Log($"ProbabilityProgress: {probabilityProgress}");

            if (occurrencePerYear > 0.1f && occurrencePerYear < 10f)
            {
                var threshold = probabilityProgress * randomizedOccurrence;
                // Debug.Log($"Start Disaster? randomizedValue: {randomizedValue} < threshold {threshold}");

                if (!(randomizedValue < threshold)) return;

                Debug.Log("Disaster Should be started!!!");
                var maxIntensity = GetMaximumIntensity();
                var intensity = GetRandomIntensity(maxIntensity);

                StartDisaster(intensity);
            }
            else if (occurrencePerYear >= 10f)
            {
                Debug.Log("OccurrencePerYear >= 10, disaster always starts.");
                var maxIntensity = GetMaximumIntensity();
                var intensity = GetRandomIntensity(maxIntensity);

                StartDisaster(intensity);
            }
        }

        protected override void OnSimulationFrameLocal()
        {
            WeatherManager wm = Singleton<WeatherManager>.instance;
            if (wm.m_currentRain > 0)
            {
                NoRainDays = 0;
            }
            else
            {
                NoRainDays += Helper.DaysPerFrame;
            }
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.ForestFire;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.ForestFire;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.ForestFire;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;


            DebugLogger.Log("ForestFireModel - OnDisasterDetected: Disaster Type set to ForestFire, EvacuationMode: " + EvacuationMode);
            
            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel, ref List<DisasterInfoModel> activeDisasters)
        {
            //Get disaster Info
            var disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);
            
            if (disasterInfo == null)
                return;

            //Identify Shelters
            var buildingManager = Singleton<BuildingManager>.instance;
            var serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);

            if (serviceBuildings == null)
                return;

            //Release all shelters but Potentially destroyed
            for (var i = 0; i < serviceBuildings.m_size; i++)
            {
                var num = serviceBuildings.m_buffer[i];
                if (num != 0)
                {
                    //here we got all shelter buildings
                    var buildingInfo = buildingManager.m_buildings.m_buffer[num];

                    if (buildingInfo.Info.m_buildingAI as ShelterAI != null)
                    {
                        //Add Building/Shelter Data to disaster
                        disasterInfoModel.ShelterList.Add(num);
                        SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num, ref buildingManager.m_buildings.m_buffer[num], false);
                    }
                }
            }

            activeDisasters.Add(disasterInfoModel);
        }

        public override string GetProbabilityTooltip(float value)
        {
            string tooltip = "";

            if (!Unlocked)
            {
                tooltip = "Not Unlocked yet (occurs only outside of your area)." + Environment.NewLine;
            }

            if (CalmDaysLeft == 0)
            {
                if (NoRainDays <= 0)
                {
                    return tooltip + "No " + GetName() + " during rain.";
                }
                else
                {
                    if (NoRainDays >= WarmupDays)
                    {
                        return tooltip + "Maximum because there was no rain for more than " + WarmupDays + " days.";
                    }

                    return tooltip + "Increasing because there was no rain for " + Helper.FormatTimeSpan(NoRainDays);
                }
            }

            return base.GetProbabilityTooltip(value);
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            return base.GetCurrentOccurrencePerYearLocal() * Math.Min(1f, NoRainDays / WarmupDays);
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as ForestFireAI != null;
        }

        public override string GetName()
        {
            return CommonProperties.forestFireName;
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