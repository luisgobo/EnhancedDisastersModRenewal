using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class ForestFireModel : DisasterBaseModel
    {
        [XmlIgnore] public float noRainDays;
        public int WarmupDays = 180;

        public ForestFireModel()
        {
            DType = DisasterType.ForestFire;
            OccurrenceAreaBeforeUnlock = OccurrenceAreas.LockedAreas;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.Everywhere;
            BaseOccurrencePerYear = 10.0f; // In case of dry weather
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            calmDays = 7;
            probabilityWarmupDays = 0;
            intensityWarmupDays = 0;
        }

        protected override void OnSimulationFrameLocal()
        {
            var wm = Services.Weather;
            if (wm.m_currentRain > 0)
                noRainDays = 0;
            else
                noRainDays += DisasterSimulationUtils.DaysPerFrame;
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.ForestFire;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.ForestFire;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.ForestFire;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel,
            ref List<DisasterInfoModel> activeDisasters)
        {
            //Get disaster Info
            var disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);

            if (disasterInfo == null)
                return;

            //Identify Shelters
            var buildingManager = Services.Buildings;
            var serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);

            if (serviceBuildings == null)
                return;

            //Release all shelters but Potentyally destroyed
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
                        SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num,
                            ref buildingManager.m_buildings.m_buffer[num], false);
                    }
                }
            }

            activeDisasters.Add(disasterInfoModel);
        }

        public override string GetProbabilityTooltip(float value)
        {
            var tooltip = "";

            if (!unlocked) tooltip = LocalizationService.Get("tooltip.forest_fire.locked") + Environment.NewLine;

            if (calmDaysLeft == 0)
            {
                if (noRainDays <= 0)
                    return tooltip + LocalizationService.Format("tooltip.forest_fire.no_during_rain", GetName());

                if (noRainDays >= WarmupDays)
                    return tooltip + LocalizationService.Format("tooltip.forest_fire.maximum_no_rain", WarmupDays);

                return tooltip + LocalizationService.Format("tooltip.forest_fire.increasing_no_rain",
                    DisasterSimulationUtils.FormatTimeSpan(noRainDays));
            }

            return base.GetProbabilityTooltip(value);
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            return base.GetCurrentOccurrencePerYearLocal() * Math.Min(1f, noRainDays / WarmupDays);
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

            var d = disaster as ForestFireModel;
            if (d != null) WarmupDays = d.WarmupDays;
        }
    }
}