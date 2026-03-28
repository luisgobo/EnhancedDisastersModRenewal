using System;
using System.Collections.Generic;
using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;

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
            get => ProbabilityWarmupDays / 360f;

            set
            {
                ProbabilityWarmupDays = (int)(360 * value);
                IntensityWarmupDays = ProbabilityWarmupDays / 2;
                CalmDays = ProbabilityWarmupDays;
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

        protected override void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel, ref List<DisasterInfoModel> activeDisasters)
        {
            SetupTsunamiEvacuation(disasterInfoModel, null, ref activeDisasters);
        }

        protected override void SetupAutomaticFocusedEvacuation(DisasterInfoModel disasterInfoModel, float disasterRadius, ref List<DisasterInfoModel> activeDisasters)
        {
            SetupTsunamiEvacuation(disasterInfoModel, disasterRadius, ref activeDisasters);
        }
                
        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as TsunamiAI != null;
        }

        public override string GetName()
        {
            return LocalizationService.GetDisasterName(DType);
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

        protected override bool CanAffectAt(ushort disasterID, ref DisasterData disasterData, Vector3 buildingPosition, Vector3 closestShelter, out float priority)
        {
            string nl = $"{Environment.NewLine}";
            DebugLogger.Log($"DisasterId: {disasterID}");

            bool itCanAffect = base.CanAffectAt(disasterID, ref disasterData, buildingPosition, new Vector3(), out priority);
            if (!itCanAffect)
            {
                return false;
            }
            var simulationFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            var disasterStartFrame = disasterData.m_startFrame;

            var dot1 = 0f - Mathf.Sin(disasterData.m_angle);
            const float dot2 = 0f;
            var dot3 = Mathf.Cos(disasterData.m_angle);
            var rhsVector = new Vector3(dot1, dot2, dot3);
            var lhsVector = buildingPosition - closestShelter;

            //DOT => lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z
            var num = Vector3.Dot(lhsVector, rhsVector);
            var num2 = simulationFrame - disasterStartFrame;
            var num3 = num2 * 0.125f - 3000f;
            var num4 = num2 * 0.125f;
            priority = Mathf.Clamp01(Mathf.Min((num4 - num) * 0.01f, (num - num3) * 0.0005f));
            var calculation = num >= num3 && num <= num4;
             
            return calculation;
        }

        private void SetupTsunamiEvacuation(DisasterInfoModel disasterInfoModel, float? focusedRadius, ref List<DisasterInfoModel> activeDisasters)
        {
            if (NaturalDisasterHandler.GetDisasterInfo(DType) == null)
                return;

            var disasterTargetPosition = new Vector3(
                disasterInfoModel.DisasterInfo.targetX,
                disasterInfoModel.DisasterInfo.targetY,
                disasterInfoModel.DisasterInfo.targetZ);

            var buildingManager = Singleton<BuildingManager>.instance;
            var serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);
            if (serviceBuildings == null)
                return;

            var focusedEvacuationRadius = focusedRadius.HasValue ? (float)Math.Sqrt(focusedRadius.Value) : 0f;

            for (var i = 0; i < serviceBuildings.m_size; i++)
            {
                var shelterId = serviceBuildings.m_buffer[i];
                if (shelterId == 0)
                    continue;

                var buildingInfo = buildingManager.m_buildings.m_buffer[shelterId];
                if (buildingInfo.Info == null || buildingInfo.Info.m_buildingAI is not ShelterAI shelterAI)
                    continue;

                var shelterPosition = buildingInfo.m_position;
                if (!CanTsunamiAffectShelter(disasterInfoModel.DisasterId, shelterPosition, disasterTargetPosition, out var priority))
                    continue;

                var shelterRadius = (buildingInfo.Length < buildingInfo.Width ? buildingInfo.Width : buildingInfo.Length) * 8 / 2f;
                if (focusedRadius.HasValue &&
                    !IsShelterInDisasterZone(disasterTargetPosition, shelterPosition, shelterRadius, focusedEvacuationRadius))
                    continue;

                disasterInfoModel.ShelterList.Add(shelterId);
                SetBuildingEvacuationStatus(shelterAI, shelterId, ref buildingManager.m_buildings.m_buffer[shelterId], false);

                DebugLogger.Log(
                    $"Tsunami evacuation enabled for shelter {shelterId}. Priority: {priority:0.000}, " +
                    $"Focused radius active: {focusedRadius.HasValue}");
            }

            if (disasterInfoModel.ShelterList.Count == 0)
            {
                DebugLogger.Log(
                    $"No shelters found inside tsunami affectation range. DisasterId: {disasterInfoModel.DisasterId}");
                return;
            }

            activeDisasters.Add(disasterInfoModel);
        }

        private bool CanTsunamiAffectShelter(ushort disasterId, Vector3 shelterPosition, Vector3 disasterTargetPosition, out float priority)
        {
            priority = 0f;

            var disasterManager = Singleton<DisasterManager>.instance;
            if (disasterManager == null || disasterId >= disasterManager.m_disasters.m_buffer.Length)
                return false;

            var disasterData = disasterManager.m_disasters.m_buffer[disasterId];
            return CanAffectAt(disasterId, ref disasterData, shelterPosition, disasterTargetPosition, out priority);
        }
    }
}
