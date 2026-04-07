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
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class ForestFireModel : DisasterBaseModel
    {
        private const float ForestFireDefaultBaseOccurrencePerYear = 0.9f;
        
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
            if (!IsDisasterEnabled) return;

            if (!Unlocked && OccurrenceAreaBeforeUnlock == OccurrenceAreas.Nowhere) return;

            while (TryConsumeSimulationStep(out var elapsedDays))
            {
                OnSimulationFrameLocal(elapsedDays);

                if (CalmDaysLeft > 0)
                {
                    CalmDaysLeft -= elapsedDays;
                    continue;
                }

                if (ProbabilityWarmupDaysLeft > 0)
                {
                    if (ProbabilityWarmupDaysLeft > ProbabilityWarmupDays)
                        ProbabilityWarmupDaysLeft = ProbabilityWarmupDays;

                    ProbabilityWarmupDaysLeft -= elapsedDays;
                }

                if (IntensityWarmupDaysLeft > 0)
                {
                    if (IntensityWarmupDaysLeft > IntensityWarmupDays) IntensityWarmupDaysLeft = IntensityWarmupDays;

                    IntensityWarmupDaysLeft -= elapsedDays;
                }

                var occurrencePerYear = GetCurrentOccurrencePerYear();

                if (occurrencePerYear == 0) continue;

                var simulationManager = CommonServices.Simulation;
                var occurrencePerStep = occurrencePerYear / 365 * elapsedDays;

                var randomizedValue = simulationManager.m_randomizer.Int32(randomizerRange);
                var randomizedOccurrence = (uint)(randomizerRange * occurrencePerStep);

                // New Logarithmic activation based on occurrence per year
                var probabilityProgress = occurrencePerYear switch
                {
                    <= 0.1f => 0f,
                    >= 10f => 1f,
                    _ => (1f + Mathf.Log10(occurrencePerYear)) / 2f
                };

                switch (occurrencePerYear)
                {
                    case > 0.1f and < 10f:
                    {
                        var threshold = probabilityProgress * randomizedOccurrence;

                        if (!(randomizedValue < threshold)) continue;

                        var maxIntensity = GetMaximumIntensity();
                        var intensity = GetRandomIntensity(maxIntensity);

                        StartDisaster(intensity);
                        break;
                    }
                    case >= 10f:
                    {
                        var maxIntensity = GetMaximumIntensity();
                        var intensity = GetRandomIntensity(maxIntensity);

                        StartDisaster(intensity);
                        break;
                    }
                }
            }
        }

        protected override void OnSimulationFrameLocal(float elapsedDays)
        {
            var wm = Singleton<WeatherManager>.instance;
            if (wm.m_currentRain > 0)
            {
                NoRainDays = 0;
            }
            else
            {
                NoRainDays += elapsedDays;
            }
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type = DisasterType.ForestFire;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type = DisasterType.ForestFire;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type = DisasterType.ForestFire;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;


            DebugLogger.Log("ForestFireModel - OnDisasterDetected: Disaster Type set to ForestFire, EvacuationMode: " + EvacuationMode);
            
            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        protected override void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel, ref List<DisasterInfoModel> activeDisasters)
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
                        SetBuildingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num, ref buildingManager.m_buildings.m_buffer[num], false);
                    }
                }
            }

            activeDisasters.Add(disasterInfoModel);
        }

        public override string GetTooltipInformation()
        {
            var tooltip = "";

            if (!Unlocked)
            {
                tooltip = LocalizationService.Get("tooltip.notUnlockedOutsideArea") + Environment.NewLine;
            }

            if (CalmDaysLeft != 0) return base.GetTooltipInformation();

            if (NoRainDays <= 0)
            {
                return tooltip + LocalizationService.Get("tooltip.forestFire.noDuringRain");
            }
            else
            {
                if (NoRainDays >= WarmupDays)
                {
                    return tooltip + LocalizationService.Format("tooltip.forestFire.maxNoRain", WarmupDays);
                }

                return tooltip + LocalizationService.Format("tooltip.forestFire.increasingNoRain", Helper.FormatTimeSpan(NoRainDays));
            }

        }

        protected override float GetBaseOccurrencePerYear()
        {
            return ForestFireDefaultBaseOccurrencePerYear * Math.Min(1f, NoRainDays / WarmupDays);
        }

        protected override void ResetDisasterState()
        {
            NoRainDays = 0;
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as ForestFireAI != null;
        }

        public override string GetName()
        {
            return LocalizationService.GetDisasterName(DType);
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

        public override float GetDisasterProbability()
        {
            return CalmDaysLeft > 0 ? 0f : base.GetDisasterProbability();
        }

        public override void ResetDisasterProbabilities()
        {
            BaseOccurrencePerYear = ForestFireDefaultBaseOccurrencePerYear;
            base.ResetDisasterProbabilities();
        }
        
    }
}
