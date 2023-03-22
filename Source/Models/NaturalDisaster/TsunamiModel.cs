using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Common.Types;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.DataBinding.BindPropertyByKey;
using static RenderManager;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class TsunamiModel : DisasterBaseModel
    {
        public TsunamiModel()
        {
            DType = DisasterType.Tsunami;
            BaseOccurrencePerYear = 1.0f;
            ProbabilityDistribution = ProbabilityDistributions.PowerLow;
            WarmupYears = 4;
        }

        public float WarmupYears
        {
            get
            {
                return probabilityWarmupDays / 360f;
            }

            set
            {
                probabilityWarmupDays = (int)(360 * value);
                intensityWarmupDays = probabilityWarmupDays / 2;
                calmDays = probabilityWarmupDays;
            }
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.Tsunami;            
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Tsunami;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.FinishOnDeactivate = false;
            disasterInfoUnified.IgnoreDestructionZone = true;

            if (!IsEvacuating())
            {
                base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);                
            }

        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Tsunami;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.FinishOnDeactivate = false;
            disasterInfoUnified.IgnoreDestructionZone = true;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterFinished(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Tsunami;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.FinishOnDeactivate = true;
            disasterInfoUnified.IgnoreDestructionZone = true;

            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);            
        }

        public override void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel, ref List<DisasterInfoModel> activeDisasters)
        {
            //Get disaster Info
            DisasterInfo disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);

            if (disasterInfo == null)
                return;

            //Identify Shelters
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;            
            FastList<ushort> serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);

            if (serviceBuildings == null)
                return;

            //Release all shelters but Potentyally destroyed
            for (int i = 0; i < serviceBuildings.m_size; i++)
            {
                ushort num = serviceBuildings.m_buffer[i];
                if (num != 0)
                {
                    //here we got all shelter buildings
                    var buildingInfo = buildingManager.m_buildings.m_buffer[num];

                    if ((buildingInfo.Info.m_buildingAI as ShelterAI) != null)
                    {                                                
                            disasterInfoModel.ShelterList.Add(num);
                            SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num, ref buildingManager.m_buildings.m_buffer[num], false);
                    }
                }
            }

            activeDisasters.Add(disasterInfoModel);
        }        
                
        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as TsunamiAI != null;
        }

        public override string GetName()
        {
            return "Tsunami";
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            TsunamiModel d = disaster as TsunamiModel;
            if (d != null)
            {
                WarmupYears = d.WarmupYears;
            }
        }

        public override bool CanAffectAt(ushort disasterID, ref DisasterData disasterData, Vector3 buildingPosition, Vector3 closestShelter, out float priority)
        {
            string nl = $"{Environment.NewLine}";
            DebugLogger.Log($"DisasterId: {disasterID}");

            bool itCanAffect = base.CanAffectAt(disasterID, ref disasterData, buildingPosition, new Vector3(), out priority);
            if (!itCanAffect)
            {
                DebugLogger.Log($"It can't affect, return false");
                return false;
            }

            DebugLogger.Log($"angle: {disasterData.m_angle + nl}" +
                $"Shelter position: ( {buildingPosition.x}, {buildingPosition.y} ,{buildingPosition.z} ) {nl}" +
                $"Disaster Position: ( {disasterData.m_targetPosition.x}, {disasterData.m_targetPosition.y}, {disasterData.m_targetPosition.z} )");            

            var simulationFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            var disasterStartFrame = disasterData.m_startFrame;

            var dot1 = 0f - Mathf.Sin(disasterData.m_angle);
            var dot2 = 0f;
            var dot3 = Mathf.Cos(disasterData.m_angle);
            Vector3 rhsVector = new Vector3(dot1, dot2, dot3);
            Vector3 lhsVector = buildingPosition - closestShelter;

            DebugLogger.Log($"Angle: {disasterData.m_angle + nl}" +
                $"dot1= 0f - Mathf.Sin(disasterData.m_angle) : {dot1 + nl}" +
                $"dot2={dot2 + nl}" +
                $"dot3= Mathf.Cos(disasterData.m_angle): {dot3 + nl}" +
                $"rhsVector= {rhsVector + nl}" +
                $"lhsVector= buildingPosition - closestShelter: {buildingPosition} - {closestShelter} = {lhsVector + nl}" +
                $"DOT = lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z: {lhsVector.x} * {rhsVector.x} + {lhsVector.y} * {rhsVector.y} + {lhsVector.z} * {rhsVector.z + nl} ");

            //DOT => lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z
            float num = Vector3.Dot(lhsVector, rhsVector);
            uint num2 = simulationFrame - disasterStartFrame;
            float num3 = (float)num2 * 0.125f - 3000f;
            float num4 = (float)num2 * 0.125f;
            priority = Mathf.Clamp01(Mathf.Min((num4 - num) * 0.01f, (num - num3) * 0.0005f));
            bool calculation = num >= num3 && num <= num4;
            

            DebugLogger.Log($"instance.m_currentFrameIndex: {simulationFrame + nl}" +
                $"data.m_startFrame: {disasterStartFrame + nl}"+ 
                $"num = {num + nl}" +
                $"num2 = {num2 + nl}" +
                $"num3 = {num3 + nl}" +
                $"num4 = {num4 + nl}" +
                $"num >= num3 = {num >= num3}" +
                $"{nl}num <= num4 = {num <= num4}" +
                $"{nl}Calculation = {calculation}" +
                $"{nl}");

            return calculation;
        }
    }
}