using System;
using System.Collections.Generic;
using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;
using System.Xml.Serialization;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class TsunamiModel : DisasterBaseModel
    {
        private const float DefaultRealTimeProgressMultiplier = 4f;
        private const uint ActiveShelterRefreshFrameInterval = 128u;
        private float _realTimeProgressMultiplier = DefaultRealTimeProgressMultiplier;
        private readonly bool _isRealTimeActive = CommonServices.DisasterHandler.CheckRealTimeModActive();

        protected override TimeBehaviorMode CurrentTimeBehaviorMode => TimeBehaviorMode.VanillaSimulationCompatible;
        protected override float TimeProgressMultiplier => _isRealTimeActive ? RealTimeProgressMultiplier : 1f;

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

        [XmlElement]
        public float RealTimeProgressMultiplier
        {
            get
            {
                return _realTimeProgressMultiplier;
            }

            set
            {
                _realTimeProgressMultiplier = Mathf.Max(1f, value);
            }
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.Tsunami;
            RefreshActiveTsunamiShelters(disasterInfo, disasterId, ref activeDisasters);
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

        public override void OnSimulationFrame(ref List<DisasterInfoModel> activeDisasters)
        {
            base.OnSimulationFrame(ref activeDisasters);

            if (activeDisasters == null || activeDisasters.Count == 0)
                return;

            var currentFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            if (currentFrame % ActiveShelterRefreshFrameInterval != 0)
                return;

            for (var i = 0; i < activeDisasters.Count; i++)
            {
                var activeDisaster = activeDisasters[i];
                if ((activeDisaster.DisasterInfo.type & DisasterType.Tsunami) == 0)
                    continue;

                RefreshActiveTsunamiShelters(activeDisaster.DisasterInfo, activeDisaster.DisasterId, ref activeDisasters, true);
            }
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
                RealTimeProgressMultiplier = d.RealTimeProgressMultiplier;
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

        protected override ImpactAreaInfo EvaluateImpactArea(DisasterInfoModel disasterInfoModel, Vector3 pointPosition,
            float pointRadius, float? focusedRadius = null)
        {
            var evaluation = new ImpactAreaInfo();

            var disasterManager = Singleton<DisasterManager>.instance;
            if (disasterManager == null || disasterInfoModel.DisasterId >= disasterManager.m_disasters.m_buffer.Length)
                return evaluation;

            var disasterData = disasterManager.m_disasters.m_buffer[disasterInfoModel.DisasterId];
            float priority;

            var center = new Vector3(
                disasterInfoModel.DisasterInfo.targetX,
                disasterInfoModel.DisasterInfo.targetY,
                disasterInfoModel.DisasterInfo.targetZ);

            if (!CanAffectAt(disasterInfoModel.DisasterId, ref disasterData, pointPosition, center, out priority))
                return evaluation;

            if (focusedRadius.HasValue && !IsInsideWaveCorridor(center, disasterInfoModel.DisasterInfo.angle, pointPosition, pointRadius, Mathf.Sqrt(focusedRadius.Value)))
                return evaluation;

            evaluation.IsInEvacuationArea = true;
            evaluation.Priority = Mathf.Clamp01(priority);
            evaluation.Zone = evaluation.Priority switch
            {
                >= 0.75f => ImpactZone.Core,
                >= 0.5f => ImpactZone.Inner,
                >= 0.25f => ImpactZone.Middle,
                _ => ImpactZone.Outer
            };

            return evaluation;
        }

        private void SetupTsunamiEvacuation(DisasterInfoModel disasterInfoModel, float? focusedRadius, ref List<DisasterInfoModel> activeDisasters,
            bool appendOnly = false)
        {
            if (NaturalDisasterHandler.GetDisasterInfo(DType) == null)
                return;

            var existingDisaster = activeDisasters.Find(disaster => disaster.DisasterId == disasterInfoModel.DisasterId);
            if (existingDisaster != null && !ReferenceEquals(existingDisaster, disasterInfoModel))
            {
                disasterInfoModel = existingDisaster;
            }

            disasterInfoModel.EvacuationMode = EvacuationMode;
            disasterInfoModel.FinishOnDeactivate = false;
            disasterInfoModel.IgnoreDestructionZone = true;
            if (!appendOnly)
                disasterInfoModel.ShelterList.Clear();

            var buildingManager = Singleton<BuildingManager>.instance;
            var serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);
            if (serviceBuildings == null)
                return;

            for (var i = 0; i < serviceBuildings.m_size; i++)
            {
                var shelterId = serviceBuildings.m_buffer[i];
                if (shelterId == 0)
                    continue;

                var buildingInfo = buildingManager.m_buildings.m_buffer[shelterId];
                if (buildingInfo.Info == null || buildingInfo.Info.m_buildingAI is not ShelterAI shelterAI)
                    continue;

                var shelterRadius = (buildingInfo.Length < buildingInfo.Width ? buildingInfo.Width : buildingInfo.Length) * 8 / 2f;
                var shelterPosition = buildingInfo.m_position;
                var impactArea = EvaluateImpactArea(disasterInfoModel, shelterPosition, shelterRadius, focusedRadius);
                if (!impactArea.IsInEvacuationArea)
                    continue;

                if (!disasterInfoModel.ShelterList.Contains(shelterId))
                    disasterInfoModel.ShelterList.Add(shelterId);
                SetBuildingEvacuationStatus(shelterAI, shelterId, ref buildingManager.m_buildings.m_buffer[shelterId], false);

                DebugLogger.Log(
                    $"Tsunami evacuation enabled for shelter {shelterId}. Priority: {impactArea.Priority:0.000}, " +
                    $"Zone: {impactArea.Zone}, " +
                    $"Focused radius active: {focusedRadius.HasValue}");
            }

            if (disasterInfoModel.ShelterList.Count == 0)
            {
                DebugLogger.Log(
                    $"No shelters found inside tsunami affectation range. DisasterId: {disasterInfoModel.DisasterId}");
                return;
            }

            if (existingDisaster == null)
                activeDisasters.Add(disasterInfoModel);
        }

        private void RefreshActiveTsunamiShelters(DisasterSettings disasterInfo, ushort disasterId,
            ref List<DisasterInfoModel> activeDisasters, bool appendOnly = false)
        {
            var disasterInfoModel = activeDisasters.Find(disaster => disaster.DisasterId == disasterId) ??
                                    new DisasterInfoModel
                                    {
                                        DisasterInfo = disasterInfo,
                                        DisasterId = disasterId
                                    };

            disasterInfoModel.DisasterInfo = disasterInfo;

            switch (EvacuationMode)
            {
                case EvacuationOptions.AutoEvacuation:
                    SetupTsunamiEvacuation(disasterInfoModel, null, ref activeDisasters, appendOnly);
                    break;
                case EvacuationOptions.FocusedAutoEvacuation:
                    SetupTsunamiEvacuation(disasterInfoModel, CommonServices.DisasterSetup.PartialEvacuationRadius, ref activeDisasters, appendOnly);
                    break;
            }
        }

        private static bool IsInsideWaveCorridor(Vector3 center, float angle, Vector3 pointPosition, float pointRadius, float halfWidth)
        {
            var direction = new Vector2(0f - Mathf.Sin(angle), Mathf.Cos(angle)).normalized;
            var perpendicular = new Vector2(direction.y, -direction.x);
            var offset = new Vector2(pointPosition.x - center.x, pointPosition.z - center.z);
            var lateralDistance = Mathf.Abs(Vector2.Dot(offset, perpendicular));
            return lateralDistance <= halfWidth + pointRadius;
        }
    }
}
