using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.BaseGameExtensions;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public abstract class DisasterBaseModel
    {
        // Constants
        private const byte IndexReferenceDisasterValue = 10;
        protected const uint randomizerRange = 67108864u;
        protected const byte baseIntensity = 255; //Base intensity is 100 for all Disasters

        //Using Services class for centralized singleton access and better performance
        private readonly bool _isRealTimeActive = CommonServices.DisasterHandler.CheckRealTimeModActive();

        // Cooldown variables (Not stored into XML)
        protected int CalmDays = 0;

        [XmlIgnore] public float CalmDaysLeft;
        [XmlIgnore] public float ProbabilityWarmupDaysLeft;
        [XmlIgnore] public float IntensityWarmupDaysLeft;
        [XmlIgnore] protected int IntensityWarmupDays = 0;
        protected int ProbabilityWarmupDays = 0;
        [XmlIgnore] private float _simulationDaysAccumulator;

        // Disaster properties
        protected DisasterType DType = DisasterType.Empty;

        protected ProbabilityDistributions ProbabilityDistribution = ProbabilityDistributions.Uniform;
        protected OccurrenceAreas OccurrenceAreaBeforeUnlock = OccurrenceAreas.Nowhere;
        protected OccurrenceAreas OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
        protected bool Unlocked;

        // Disaster public properties (to be saved in XML)
        public bool IsDisasterEnabled = true;

        public EvacuationOptions EvacuationMode = EvacuationOptions.ManualEvacuation;
        private const double SecondsBeforePausing = 3;

        private readonly HashSet<ushort> _manualReleaseDisasters = [];
        
        public float BaseOccurrencePerYear = 1.0f;        

        // Disaster services
        private FieldInfo _evacuatingField;
        private WarningPhasePanel _phasePanel;
        
        // Public
        public abstract string GetName();

        protected virtual TimeBehaviorMode CurrentTimeBehaviorMode =>
            _isRealTimeActive ? TimeBehaviorMode.RealTimeCompatible : TimeBehaviorMode.Original;
        protected virtual float TimeProgressMultiplier => 1f;
        protected virtual float SimulationCheckIntervalDays => 0.25f;

        protected virtual float GetCurrentOccurrencePerYear()
        {
            //Check here to set up real time occurrence per year
            if (CalmDaysLeft > 0) return 0f;

            var currentOccurrencePerYearLocal = GetBaseOccurrencePerYear();
            return ScaleProbabilityByWarmup(currentOccurrencePerYearLocal);
        }

        public string GetDisasterProbabilityPercentageValue()
        {
            return $"{GetDisasterProbability() * 100:00.00}%";
        }

        public virtual float GetDisasterProbability()
        {
            var currentOccurrencePerYear = GetCurrentOccurrencePerYear();

            if (currentOccurrencePerYear <= 0.1)
                return 0;
            if (currentOccurrencePerYear >= 10)
                return 1;

            //Returns a value between 0 and 1 in intervals of 0.1 units
            //based on the logarithmic scale of the occurrence per year
            return (1f + Mathf.Log10(currentOccurrencePerYear)) / 2f;
        }

        protected virtual float GetBaseOccurrencePerYear()
        {
            return BaseOccurrencePerYear;
        }

        private float ScaleProbabilityByWarmup(float probability)
        {
            if (!Unlocked && OccurrenceAreaBeforeUnlock == OccurrenceAreas.Nowhere)
            {
                return 0;
            }

            if (ProbabilityWarmupDaysLeft > 0 && ProbabilityWarmupDays > 0)
            {
                if (ProbabilityWarmupDaysLeft >= ProbabilityWarmupDays)
                {
                    probability = 0;
                }
                else
                {
                    probability *= 1 - ProbabilityWarmupDaysLeft / ProbabilityWarmupDays;
                }
            }

            return probability;
        }
        
        public virtual void ResetDisasterProbabilities()
        {
            CalmDaysLeft = CalmDays;
            ProbabilityWarmupDaysLeft = ProbabilityWarmupDays;
            IntensityWarmupDaysLeft = IntensityWarmupDays;
            _simulationDaysAccumulator = 0f;
            ResetDisasterState();
        }

        protected virtual void ResetDisasterState()
        {
        }

        public virtual byte GetMaximumIntensity()
        {
            var intensity = baseIntensity;            
            intensity = ScaleIntensityByWarmup(intensity);
            intensity = ScaleIntensityByPopulation(intensity);
            return intensity;
        }

        public virtual void OnSimulationFrame()
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
                    {
                        ProbabilityWarmupDaysLeft = ProbabilityWarmupDays;
                    }

                    ProbabilityWarmupDaysLeft -= elapsedDays;
                }

                if (IntensityWarmupDaysLeft > 0)
                {
                    if (IntensityWarmupDaysLeft > IntensityWarmupDays) IntensityWarmupDaysLeft = IntensityWarmupDays;

                    IntensityWarmupDaysLeft -= elapsedDays;
                }

                var occurrencePerYear = GetCurrentOccurrencePerYear();

                if (occurrencePerYear == 0)
                {
                    continue;
                }

                var simulationManager = CommonServices.Simulation;
                var occurrencePerStep = occurrencePerYear / 365 * elapsedDays;
                if (simulationManager.m_randomizer.Int32(randomizerRange) < (uint)(randomizerRange * occurrencePerStep))
                {
                    var maxIntensity = GetMaximumIntensity();
                    var intensity = GetRandomIntensity(maxIntensity);

                    StartDisaster(intensity);
                }
            }
        }

        public virtual void OnSimulationFrame(ref List<DisasterInfoModel> activeDisasters)
        {
            OnSimulationFrame();
        }

        protected float GetSimulationDaysPerFrame()
        {
            return Helper.GetDaysPerFrame(CurrentTimeBehaviorMode) * Mathf.Max(1f, TimeProgressMultiplier);
        }

        public void Unlock()
        {
            Unlocked = true;
        }

        public virtual string GetTooltipInformation()
        {
            if (!Unlocked)
            {
                return LocalizationService.Get("tooltip.notUnlocked");
            }

            if (CalmDaysLeft > 0)
            {
                return LocalizationService.Format("tooltip.noDisasterForAnother", GetName(), Helper.FormatTimeSpan(CalmDaysLeft));
            }

            if (ProbabilityWarmupDaysLeft > 0)
            {
                return LocalizationService.Format("tooltip.recentlyOccurred", GetName());
            }

            return LocalizationService.Format("tooltip.probability", GetDisasterProbabilityPercentageValue());
        }

        public virtual string GetIntensityTooltip(float maxDisasterIntensity)
        {
            if (!Unlocked)
            {
                return LocalizationService.Get("tooltip.notUnlocked");
            }

            if (CalmDaysLeft > 0)
            {
                return LocalizationService.Format("tooltip.noDisasterForAnother", GetName(), Helper.FormatTimeSpan(CalmDaysLeft));
            }

            var result = LocalizationService.Format("tooltip.intensity", $"{maxDisasterIntensity * 25.5:#.##}");

            if (ProbabilityWarmupDaysLeft > 0)
            {
                result = LocalizationService.Format("tooltip.recentlyOccurred", GetName());
            }

            var naturalDisasterSetup = CommonServices.DisasterSetup;

            if (!(Helper.GetPopulation() < naturalDisasterSetup.MaxPopulationToTriggerHigherDisasters)) return result;

            if (result != "") result += CommonProperties.NewLine;
            result += LocalizationService.Get("tooltip.lowPopulation");

            return result;
        }

        public virtual void CopySettings(DisasterBaseModel disaster)
        {
            IsDisasterEnabled = disaster.IsDisasterEnabled;
            BaseOccurrencePerYear = disaster.BaseOccurrencePerYear;
            EvacuationMode = disaster.EvacuationMode;
        }

        public DisasterType GetDisasterType() => DType;

        // Utilities

        protected virtual byte GetRandomIntensity(byte maxIntensity)
        {
            byte intensity;            

            //if based on Gutenberg–Richter law
            if (ProbabilityDistribution == ProbabilityDistributions.PowerLow)
            {
                var randomValue =
                    Singleton<SimulationManager>.instance.m_randomizer.Int32(1000, 10000) /
                    10000.0f; // from range 0.1 - 1.0

                // See Gutenberg–Richter law.
                // a, b = 0.11
                intensity = (byte)(10 * (0.11 - Math.Log10(randomValue)) / 0.11);

                if (intensity > 100)
                {
                    intensity = 100;
                }
            }
            else
            {
                //Otherwise uniform increment based on random value between 10 and 100
                intensity = (byte)Singleton<SimulationManager>.instance.m_randomizer.Int32(10, 100);
            }

            if (maxIntensity < 100)
            {
                intensity = (byte)(10 + (((intensity - 10) * maxIntensity) / 100));
            }

            return intensity;
        }

        private byte ScaleIntensityByWarmup(byte intensity)
        {
            if (IntensityWarmupDaysLeft > 0 && IntensityWarmupDays > 0)
            {
                if (IntensityWarmupDaysLeft >= IntensityWarmupDays)
                {
                    intensity = IndexReferenceDisasterValue;
                }
                else
                {
                    intensity = (byte)(IndexReferenceDisasterValue + (intensity - IndexReferenceDisasterValue) * (1 - IntensityWarmupDaysLeft / IntensityWarmupDays));
                }
            }                       

            return intensity;
        }

        protected static byte ScaleIntensityByPopulation(byte intensity)
        {
            var naturalDisasterSetup = CommonServices.DisasterSetup;
            if (naturalDisasterSetup.ScaleMaxIntensityWithPopulation)
            {                
                int population = Helper.GetPopulation();
                if (population < naturalDisasterSetup.MaxPopulationToTriggerHigherDisasters)
                {
                    intensity = (byte)(IndexReferenceDisasterValue + (intensity - IndexReferenceDisasterValue) * population / naturalDisasterSetup.MaxPopulationToTriggerHigherDisasters);
                }
            }
            return intensity;
        }

        protected string GetDebugStr()
        {
            return DType + ", " + Singleton<SimulationManager>.instance.m_currentGameTime.ToShortDateString() + ", ";
        }

        // Disaster events

        protected virtual void OnSimulationFrameLocal(float elapsedDays)
        {
        }

        protected bool TryConsumeSimulationStep(out float elapsedDays)
        {
            _simulationDaysAccumulator += GetSimulationDaysPerFrame();

            if (_simulationDaysAccumulator < SimulationCheckIntervalDays)
            {
                elapsedDays = 0f;
                return false;
            }

            elapsedDays = _simulationDaysAccumulator;
            _simulationDaysAccumulator = 0f;
            return true;
        }

        public virtual void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId, ref List<DisasterInfoModel> activeDisaster)
        {
            var msg = string.Format("Disaster Activated. Id: {0}, Name: {1}, Type: {2}, Intensity: {3}",
                disasterId,
                disasterInfo.name,
                disasterInfo.type,
                disasterInfo.intensity);
            DebugLogger.Log(msg);

            var naturalDisasterSetup = CommonServices.DisasterSetup;
            DisasterExtension.SetPauseOnDisasterStarts(naturalDisasterSetup.PauseOnDisasterStarts, SecondsBeforePausing, disasterId, disasterInfo, IsDisasterEnabled);
        }

        public virtual void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            try
            {
                var msg = string.Format("Disaster Deactivated: Id: {0}, Name: {1}, Type: {2}, Intensity: {3}, EvacuationMode:{4}, FinishOnDeactivate:{5}",
                disasterInfoUnified.DisasterId,
                disasterInfoUnified.DisasterInfo.name,
                disasterInfoUnified.DisasterInfo.type,
                disasterInfoUnified.DisasterInfo.intensity,
                disasterInfoUnified.EvacuationMode,
                disasterInfoUnified.FinishOnDeactivate);

                DebugLogger.Log(msg);

                if (disasterInfoUnified.DisasterInfo.type == DisasterType.Empty)
                {
                    return;
                }

                if (!IsEvacuating())
                {
                    //Not evacuating. Clear list of active manual release disasters                    
                    _manualReleaseDisasters.Clear();
                    return;
                }

                //Evaluate Disaster finishing                
                var disasterFinishing = activeDisasters.Where(activeDisaster => activeDisaster.DisasterId == disasterInfoUnified.DisasterId).FirstOrDefault();
               
                if (disasterFinishing != null && disasterInfoUnified.FinishOnDeactivate)
                {
                    activeDisasters.Remove(disasterFinishing);

                    switch (disasterFinishing.EvacuationMode)
                    {
                        case EvacuationOptions.ManualEvacuation:
                            break;

                        case EvacuationOptions.AutoEvacuation:
                        case EvacuationOptions.FocusedAutoEvacuation:
                            //When all list empty, then release all using basegame code for it.
                            if (!_manualReleaseDisasters.Any() && !activeDisasters.Any())
                            {
                                //Auto releasing citizens
                                DisasterManager.instance.EvacuateAll(true);
                                break;
                            }

                            //Verify shelters when there are pending disasters
                            if (activeDisasters.Any())
                            {
                                var pendingShelters = new List<ushort>();
                                // //If pending disaster then get all pending shelters that souldnt be released
                                // foreach (var disaster in activeDisasters)
                                // {
                                //     //this is not being filled
                                //     foreach (var shelterId in disaster.ShelterList)
                                //     {
                                //         if (!pendingShelters.Contains(shelterId))
                                //             pendingShelters.Add(shelterId);
                                //     }
                                // }
                                
                                //If pending disaster then get all pending shelters that shouldn't be released
                                foreach (var shelterId in activeDisasters
                                             .SelectMany(disaster => disaster
                                                 .ShelterList.Where(shelterId => !pendingShelters.Contains(shelterId))))
                                {
                                    pendingShelters.Add(shelterId);
                                }

                                var sheltersToBeReleased =
                                    disasterFinishing.ShelterList.Where(disFinishing => pendingShelters.All(pendSh => pendSh != disFinishing))
                                        .ToList();

                                var buildingManager = Singleton<BuildingManager>.instance;

                                foreach (var shelterId in sheltersToBeReleased)
                                {
                                    var buildingInfo = buildingManager.m_buildings.m_buffer[shelterId];
                                    SetBuildingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, shelterId, ref buildingManager.m_buildings.m_buffer[shelterId], true);
                                }
                            }
                            break;
                    }
                }


            }
            catch (Exception ex)
            {
                DebugLogger.Log(ex.ToString());

                throw;
            }
        }

        public virtual void OnDisasterDetected(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            var msg = $"Disaster Detected. type: {disasterInfoUnified.DisasterInfo.type}, name:{disasterInfoUnified.DisasterInfo.name}, " +
                                  $"location => x:{disasterInfoUnified.DisasterInfo.targetX} y:{disasterInfoUnified.DisasterInfo.targetX} z:{disasterInfoUnified.DisasterInfo.targetZ}. " +
                                  $"Angle: {disasterInfoUnified.DisasterInfo.angle}, intensity: {disasterInfoUnified.DisasterInfo.intensity} " +
                                  $"EvacuationMode: {disasterInfoUnified.EvacuationMode}";
            DebugLogger.Log(msg);

            var naturalDisasterSetup = CommonServices.DisasterSetup;

            DisasterExtension.SetDisableDisasterFocus(naturalDisasterSetup.DisableDisasterFocus);

            switch (disasterInfoUnified.EvacuationMode)
            {
                case EvacuationOptions.ManualEvacuation:
                    SetupManualEvacuation(disasterInfoUnified.DisasterId);
                    break;

                case EvacuationOptions.AutoEvacuation: //Auto evacuate all shelters
                    SetupAutomaticEvacuation(disasterInfoUnified, ref activeDisasters);
                    break;

                case EvacuationOptions.FocusedAutoEvacuation:
                    SetupAutomaticFocusedEvacuation(disasterInfoUnified, naturalDisasterSetup.PartialEvacuationRadius, ref activeDisasters);
                    break;

                default:
                    break;
            }
        }
        
        public virtual void OnDisasterFinished(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            var msg = $"Disaster Finished: type: {disasterInfoUnified.DisasterInfo.type}, name:{disasterInfoUnified.DisasterInfo.name}, " +
                                  $"location => x:{disasterInfoUnified.DisasterInfo.targetX} y:{disasterInfoUnified.DisasterInfo.targetX} z:{disasterInfoUnified.DisasterInfo.targetZ}. " +
                                  $"Angle: {disasterInfoUnified.DisasterInfo.angle}, intensity: {disasterInfoUnified.DisasterInfo.intensity} " +
                                  $"EvacuationMode: {disasterInfoUnified.EvacuationMode}" +
                                  $"FinishOnDeactivate: {disasterInfoUnified.FinishOnDeactivate}";
            DebugLogger.Log(msg);
            
        }
        public virtual void OnDisasterStarted(byte intensity)
        {
            CalmDaysLeft = CalmDays * intensity / 100; // TO DO: May be defined the minimum CalmDays
            ProbabilityWarmupDaysLeft = ProbabilityWarmupDays;
            IntensityWarmupDaysLeft = IntensityWarmupDays;
        }

        protected virtual void DisasterStarting(DisasterInfo disasterInfo)
        {
        }

        public abstract bool CheckDisasterAIType(object disasterAI);

        protected void StartDisaster(byte intensity)
        {
            DisasterInfo disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);

            if (disasterInfo == null)
            {
                return;
            }

            bool targetFound = FindTarget(disasterInfo, out Vector3 targetPosition, out float angle);
            if (!targetFound)
                return;

            DisasterManager dm = Singleton<DisasterManager>.instance;

            bool disasterCreated = dm.CreateDisaster(out ushort disasterIndex, disasterInfo);
            if (!disasterCreated)
                DebugLogger.Log(GetDebugStr() + "could not create disaster");

            DisasterLogger.StartedByMod = true;

            DisasterStarting(disasterInfo);

            dm.m_disasters.m_buffer[(int)disasterIndex].m_targetPosition = targetPosition;
            dm.m_disasters.m_buffer[(int)disasterIndex].m_angle = angle;
            dm.m_disasters.m_buffer[(int)disasterIndex].m_intensity = intensity;
            DisasterData[] expr_98_cp_0 = dm.m_disasters.m_buffer;
            ushort expr_98_cp_1 = disasterIndex;
            expr_98_cp_0[(int)expr_98_cp_1].m_flags = (expr_98_cp_0[(int)expr_98_cp_1].m_flags | DisasterData.Flags.SelfTrigger);
            disasterInfo.m_disasterAI.StartNow(disasterIndex, ref dm.m_disasters.m_buffer[(int)disasterIndex]);

            DebugLogger.Log(GetDebugStr() + string.Format("disaster intensity: {0}, area: {1}", intensity, Unlocked ? OccurrenceAreaAfterUnlock : OccurrenceAreaBeforeUnlock));
        }

        protected virtual bool FindTarget(DisasterInfo disasterInfo, out Vector3 targetPosition, out float angle)
        {
            var area = Unlocked ? OccurrenceAreaAfterUnlock : OccurrenceAreaBeforeUnlock;

            switch (area)
            {
                case OccurrenceAreas.LockedAreas:
                    return FindRandomTargetInLockedAreas(out targetPosition, out angle);

                case OccurrenceAreas.Everywhere:
                    return FindRandomTargetEverywhere(out targetPosition, out angle);

                case OccurrenceAreas.UnlockedAreas: // Vanilla default
                    return disasterInfo.m_disasterAI.FindRandomTarget(out targetPosition, out angle);

                default:
                    targetPosition = new Vector3();
                    angle = 0;
                    return false;
            }
        }

        protected virtual float CalculateDestructionRadio(byte intensity)
        {
            return 5.656854249f; //min Value Radio
        }

        protected enum ImpactZone
        {
            None,
            Outer,
            Middle,
            Inner,
            Core
        }

        protected class ImpactAreaInfo
        {
            public ImpactZone Zone = ImpactZone.None;
            public bool IsInEvacuationArea;
            public bool IsInDestructionArea;
            public float Priority;
        }

        protected virtual ImpactAreaInfo EvaluateImpactArea(DisasterInfoModel disasterInfoModel, Vector3 pointPosition,
            float pointRadius, float? focusedRadius = null)
        {
            var disasterTargetPosition = new Vector3(
                disasterInfoModel.DisasterInfo.targetX,
                disasterInfoModel.DisasterInfo.targetY,
                disasterInfoModel.DisasterInfo.targetZ);

            var destructionRadius = CalculateDestructionRadio(disasterInfoModel.DisasterInfo.intensity);
            var evacuationRadius = destructionRadius;

            if (focusedRadius.HasValue)
            {
                evacuationRadius = Mathf.Max(evacuationRadius, Mathf.Sqrt(focusedRadius.Value));
            }

            return EvaluateCircularImpactArea(disasterTargetPosition, pointPosition, pointRadius, evacuationRadius, destructionRadius);
        }

        protected static ImpactAreaInfo EvaluateCircularImpactArea(Vector3 center, Vector3 pointPosition, float pointRadius,
            float evacuationRadius, float destructionRadius)
        {
            var planarDistance = Vector2.Distance(new Vector2(center.x, center.z), new Vector2(pointPosition.x, pointPosition.z));
            var evaluation = new ImpactAreaInfo
            {
                IsInEvacuationArea = planarDistance <= evacuationRadius + pointRadius,
                IsInDestructionArea = planarDistance <= destructionRadius + pointRadius
            };

            if (!evaluation.IsInEvacuationArea)
            {
                return evaluation;
            }

            var normalizedDistance = evacuationRadius > 0f
                ? Mathf.Clamp01(planarDistance / evacuationRadius)
                : 1f;

            evaluation.Priority = 1f - normalizedDistance;
            evaluation.Zone = GetImpactZoneFromNormalizedDistance(normalizedDistance);
            return evaluation;
        }

        protected static ImpactAreaInfo EvaluateDirectionalImpactArea(Vector3 center, float angle, Vector3 pointPosition,
            float pointRadius, float trailLength, float baseHalfWidth, float destructionLength, float destructionHalfWidth,
            float forwardPadding = 0f)
        {
            var offset = new Vector2(pointPosition.x - center.x, pointPosition.z - center.z);
            var direction = new Vector2(0f - Mathf.Sin(angle), Mathf.Cos(angle)).normalized;
            var perpendicular = new Vector2(direction.y, -direction.x);

            var forwardDistance = Vector2.Dot(offset, direction) + forwardPadding;
            var lateralDistance = Mathf.Abs(Vector2.Dot(offset, perpendicular));
            var maxForwardDistance = Mathf.Max(trailLength + pointRadius, 0f);

            var evaluation = new ImpactAreaInfo();

            if (forwardDistance < -pointRadius || forwardDistance > maxForwardDistance)
            {
                return evaluation;
            }

            var forwardRatio = maxForwardDistance > 0f ? Mathf.Clamp01(forwardDistance / maxForwardDistance) : 0f;
            var evacuationHalfWidth = Mathf.Lerp(baseHalfWidth, Mathf.Max(baseHalfWidth * 0.35f, pointRadius), forwardRatio);
            evaluation.IsInEvacuationArea = lateralDistance <= evacuationHalfWidth + pointRadius;

            if (!evaluation.IsInEvacuationArea)
            {
                return evaluation;
            }

            var destructionRatio = destructionLength > 0f ? Mathf.Clamp01(forwardDistance / destructionLength) : 1f;
            var destructionWidth = Mathf.Lerp(destructionHalfWidth, Mathf.Max(destructionHalfWidth * 0.35f, pointRadius), destructionRatio);
            evaluation.IsInDestructionArea = forwardDistance <= destructionLength + pointRadius &&
                                             lateralDistance <= destructionWidth + pointRadius;

            var lateralRatio = evacuationHalfWidth > 0f ? Mathf.Clamp01(lateralDistance / evacuationHalfWidth) : 1f;
            evaluation.Priority = 1f - Mathf.Clamp01(Mathf.Max(forwardRatio, lateralRatio));
            evaluation.Zone = GetImpactZoneFromNormalizedDistance(1f - evaluation.Priority);

            return evaluation;
        }

        protected static ImpactZone GetImpactZoneFromNormalizedDistance(float normalizedDistance)
        {
            if (normalizedDistance <= 0.2f)
                return ImpactZone.Core;
            if (normalizedDistance <= 0.45f)
                return ImpactZone.Inner;
            if (normalizedDistance <= 0.7f)
                return ImpactZone.Middle;
            if (normalizedDistance <= 1f)
                return ImpactZone.Outer;

            return ImpactZone.None;
        }

        protected virtual void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel, ref List<DisasterInfoModel> activeDisasters)
        {
            //Get disaster Info
            DisasterInfo disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);

            if (disasterInfo == null)
                return;

            //Identify Shelters
            var buildingManager = Singleton<BuildingManager>.instance;
            var serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);

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
                    var shelterPosition = buildingInfo.m_position;

                    if ((buildingInfo.Info.m_buildingAI as ShelterAI) != null)
                    {
                        float shelterRadius = ((buildingInfo.Length < buildingInfo.Width ? buildingInfo.Width : buildingInfo.Length) * 8) / 2;
                        var impactArea = EvaluateImpactArea(disasterInfoModel, shelterPosition, shelterRadius);

                        if (!impactArea.IsInEvacuationArea)
                            continue;

                        disasterInfoModel.ShelterList.Add(num);

                        //if Shelter will be destroyed, don't evacuate
                        if (!disasterInfoModel.IgnoreDestructionZone && impactArea.IsInDestructionArea)
                            DebugLogger.Log($"Shelter is located in Destruction Zone. Won't be avacuated");
                        else
                        {
                            DebugLogger.Log($"Shelter {num} mapped to {impactArea.Zone} zone.");
                            SetBuildingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num, ref buildingManager.m_buildings.m_buffer[num], false);
                        }
                    }
                }
            }

            activeDisasters.Add(disasterInfoModel);
        }

        bool FindRandomTargetEverywhere(out Vector3 target, out float angle)
        {
            GameAreaManager gam = Singleton<GameAreaManager>.instance;
            SimulationManager sm = Singleton<SimulationManager>.instance;
            int i = sm.m_randomizer.Int32(0, 4);
            int j = sm.m_randomizer.Int32(0, 4);
            float minX;
            float minZ;
            float maxX;
            float maxZ;
            gam.GetAreaBounds(i, j, out minX, out minZ, out maxX, out maxZ);

            float randX = (float)sm.m_randomizer.Int32(0, 10000) * 0.0001f;
            float randZ = (float)sm.m_randomizer.Int32(0, 10000) * 0.0001f;
            target.x = minX + (maxX - minX) * randX;
            target.y = 0f;
            target.z = minZ + (maxZ - minZ) * randZ;
            target.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(target, false, 0f);
            angle = (float)sm.m_randomizer.Int32(0, 10000) * 0.0006283185f;
            return true;
        }

        bool FindRandomTargetInLockedAreas(out Vector3 target, out float angle)
        {
            GameAreaManager gam = Singleton<GameAreaManager>.instance;
            SimulationManager sm = Singleton<SimulationManager>.instance;

            // No locked areas
            if (gam.m_areaCount >= 25)
            {
                target = Vector3.zero;
                angle = 0f;
                return false;
            }

            var lockedAreaCounter = sm.m_randomizer.Int32(1, 25 - gam.m_areaCount);

            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    if (IsUnlocked(i, j))
                    {
                        continue;
                    }

                    if (--lockedAreaCounter == 0)
                    {
                        float minX;
                        float minZ;
                        float maxX;
                        float maxZ;
                        gam.GetAreaBounds(j, i, out minX, out minZ, out maxX, out maxZ);

                        const float minimumEdgeDistance = 100f;
                        
                        if (IsUnlocked(j - 1, i))
                        {
                            minX += minimumEdgeDistance;
                        }
                        if (IsUnlocked(j, i - 1))
                        {
                            minZ += minimumEdgeDistance;
                        }
                        if (IsUnlocked(j + 1, i))
                        {
                            maxX -= minimumEdgeDistance;
                        }
                        if (IsUnlocked(j, i + 1))
                        {
                            maxZ -= minimumEdgeDistance;
                        }

                        var randX = sm.m_randomizer.Int32(0, 10000) * 0.0001f;
                        var randZ = sm.m_randomizer.Int32(0, 10000) * 0.0001f;
                        target.x = minX + (maxX - minX) * randX;
                        target.y = 0f;
                        target.z = minZ + (maxZ - minZ) * randZ;
                        target.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(target, false, 0f);
                        angle = sm.m_randomizer.Int32(0, 10000) * 0.0006283185f;
                        return true;
                    }
                }
            }

            target = Vector3.zero;
            angle = 0f;
            return false;
        }

        private static bool IsUnlocked(int x, int z)
        {
            return x >= 0 && z >= 0 && x < 5 && z < 5 && Singleton<GameAreaManager>.instance.m_areaGrid[z * 5 + x] != 0;
        }

        private void FindPhasePanel()
        {
            DebugLogger.Log("ES: Find Phase Panel");

            if (_phasePanel != null)
                return;

            _phasePanel = UnityObject.FindObjectOfType<WarningPhasePanel>();
            _evacuatingField = _phasePanel.GetType().GetField("m_isEvacuating", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        protected bool IsEvacuating()
        {
            FindPhasePanel();
            return (bool)_evacuatingField.GetValue(_phasePanel);
        }

        private void SetupManualEvacuation(ushort disasterId)
        {
            //Should be manually released
            _manualReleaseDisasters.Add(disasterId);
        }

        protected virtual void SetupAutomaticFocusedEvacuation(DisasterInfoModel disasterInfoModel, float disasterRadius, ref List<DisasterInfoModel> activeDisasters)
        {
            DebugLogger.Log("SetupAutomaticFocusedEvacuation");
            //Get disaster Info
            var disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);

            if (disasterInfo == null)
                return;

            //Identify Shelters
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            FastList<ushort> serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);

            DebugLogger.Log("serviceBuildings == null: " + (serviceBuildings == null));
            if (serviceBuildings == null)
                return;

            //Release Specific shelters based on disaster radius proximity
            for (var i = 0; i < serviceBuildings.m_size; i++)
            {
                var num = serviceBuildings.m_buffer[i];
                if (num == 0) continue;

                //here we got all shelter buildings
                var buildingInfo = buildingManager.m_buildings.m_buffer[num];
                var shelterPosition = buildingInfo.m_position;

                var shelterRadius =
                    (buildingInfo.Length < buildingInfo.Width ? buildingInfo.Width : buildingInfo.Length) * 8f / 2f;

                if (buildingInfo.Info.m_buildingAI as ShelterAI == null
                    ) continue;

                var impactArea = EvaluateImpactArea(disasterInfoModel, shelterPosition, shelterRadius, disasterRadius);
                if (!impactArea.IsInEvacuationArea)
                    continue;

                //Add Building/Shelter Data to disaster
                disasterInfoModel.ShelterList.Add(num);

                DebugLogger.Log(
                    $"Disaster intensity: {disasterInfoModel.DisasterInfo.intensity}. " +
                    $"Focused evacuation radius: {Mathf.Sqrt(disasterRadius)}. " +
                    $"Resolved zone: {impactArea.Zone}"
                );

                //if Shelter will be destroyed, don't evacuate
                if (!disasterInfoModel.IgnoreDestructionZone && impactArea.IsInDestructionArea)
                {
                    DebugLogger.Log("Shelter is located in Destruction Zone. Won't be evacuated");
                }
                else
                {
                    SetBuildingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num, ref buildingManager.m_buildings.m_buffer[num], false);
                }
            }

            activeDisasters.Add(disasterInfoModel);
        }

        protected static void SetBuildingEvacuationStatus(ShelterAI shelterAI, ushort num, ref Building buildingData, bool release)
        {
            shelterAI?.SetEmptying(num, ref buildingData, release);
        }

        protected virtual bool CanAffectAt(ushort disasterID, ref DisasterData data, Vector3 buildingPosition, Vector3 seasidePosition, out float priority)
        {
            priority = 0f;
            var canAffect = (data.m_flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active | DisasterData.Flags.Clearing)) != 0;            
            return canAffect;
        }

        public void DisableRain()
        {
            var weatherManager = Singleton<WeatherManager>.instance;
            if (weatherManager == null) return;

            // Stop current rain
            weatherManager.m_targetRain = 0f;
            weatherManager.m_currentRain = 0f;

            //Deactivate weather effects
            weatherManager.m_currentFog = 0f;
            weatherManager.m_targetFog = 0f;
        }
    }
}
