using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using ICities;
using NaturalDisastersRenewal.BaseGameExtensions;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public abstract class DisasterBaseModel
    {
        // Constants
        protected const uint randomizerRange = 67108864u;
        protected const byte baseIntensity = 255; //Base intensity is 100 for all Disasters
        protected const byte indexReferenceDisasterValue = 10;
        private readonly HashSet<ushort> manualReleaseDisasters = new HashSet<ushort>();
        private readonly double secondsBeforePausing = 3;
        private bool debugForceOccurrence;
        private DateTime lastStartFailureLogUtc = DateTime.MinValue;
        public float BaseOccurrencePerYear = 1.0f;

        // Cooldown variables (Not stored into XML)
        protected int calmDays = 0;
        [XmlIgnore] public float calmDaysLeft;


        // Disaster properties
        protected DisasterType DType = DisasterType.Empty;

        // Disaster public properties (to be saved in xml)
        public bool Enabled = true;

        // Disaster services
        private FieldInfo evacuatingField;

        public EvacuationOptions EvacuationMode = EvacuationOptions.ManualEvacuation;
        [XmlIgnore] public int intensityWarmupDays = 0;
        [XmlIgnore] public float intensityWarmupDaysLeft;

        protected OccurrenceAreas OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;

        //protected int FullIntensityMaxLimitPopulation = 70000;//Original: 20000
        protected OccurrenceAreas OccurrenceAreaBeforeUnlock = OccurrenceAreas.Nowhere;

        private WarningPhasePanel phasePanel;

        protected ProbabilityDistributions ProbabilityDistribution = ProbabilityDistributions.Uniform;

        protected int probabilityWarmupDays = 0;
        [XmlIgnore] public float probabilityWarmupDaysLeft;
        protected bool unlocked;

        // Public
        public abstract string GetName();

        public float GetCurrentOccurrencePerYear()
        {
            if (calmDaysLeft > 0) return 0f;

            if (debugForceOccurrence)
                return 365f / GetSimulationDaysPerFrame() * 2f;

            return ScaleProbabilityByWarmup(GetCurrentOccurrencePerYearLocal());
        }

        public virtual byte GetMaximumIntensity()
        {
            var intensity = baseIntensity;
            intensity = ScaleIntensityByWarmup(intensity);
            intensity = ScaleIntensityByPopulation(intensity);
            return intensity;
        }

        public byte GetMaximumGeneratedIntensity()
        {
            return GetMaximumGeneratedIntensity(GetMaximumIntensity());
        }

        public void OnSimulationFrame()
        {
            if (!Enabled) return;

            if (!unlocked && OccurrenceAreaBeforeUnlock == OccurrenceAreas.Nowhere) return;

            OnSimulationFrameLocal();

            var daysPerFrame = GetSimulationDaysPerFrame();

            if (calmDaysLeft > 0)
            {
                calmDaysLeft -= daysPerFrame;
                return;
            }

            if (probabilityWarmupDaysLeft > 0)
            {
                if (probabilityWarmupDaysLeft > probabilityWarmupDays)
                    probabilityWarmupDaysLeft = probabilityWarmupDays;

                probabilityWarmupDaysLeft -= daysPerFrame;
            }

            if (intensityWarmupDaysLeft > 0)
            {
                if (intensityWarmupDaysLeft > intensityWarmupDays) intensityWarmupDaysLeft = intensityWarmupDays;

                intensityWarmupDaysLeft -= daysPerFrame;
            }

            var occurrencePerYear = GetCurrentOccurrencePerYear();

            if (occurrencePerYear == 0) return;

            var sm = Services.Simulation;
            var occurrencePerFrame = occurrencePerYear / 365 * daysPerFrame;
            if (sm.m_randomizer.Int32(randomizerRange) < (uint)(randomizerRange * occurrencePerFrame))
            {
                var maxIntensity = GetMaximumIntensity();
                var intensity = GetRandomIntensity(maxIntensity);

                StartDisaster(intensity);
            }
        }

        public void Unlock()
        {
            unlocked = true;
        }

        public virtual string GetProbabilityTooltip(float value)
        {
            if (!unlocked) return LocalizationService.Get("tooltip.not_unlocked");

            if (calmDaysLeft > 0)
                return LocalizationService.Format("tooltip.no_disaster_for_another", GetName(),
                    DisasterSimulationUtils.FormatTimeSpan(calmDaysLeft));

            if (probabilityWarmupDaysLeft > 0)
                return LocalizationService.Format("tooltip.recently_occurred", GetName());

            //return $"Probability: {value * 10:#.##}";
            return LocalizationService.Format("tooltip.probability", string.Format("{0:#.##}", value * 2));
        }

        public virtual string GetIntensityTooltip(float value)
        {
            if (!unlocked) return LocalizationService.Get("tooltip.not_unlocked");

            if (calmDaysLeft > 0)
                return LocalizationService.Format("tooltip.no_disaster_for_another", GetName(),
                    DisasterSimulationUtils.FormatTimeSpan(calmDaysLeft));

            var result = LocalizationService.Format("tooltip.intensity", string.Format("{0:#.##}", value * 25.5f));

            if (probabilityWarmupDaysLeft > 0)
                result = LocalizationService.Format("tooltip.recently_occurred", GetName());

            var naturalDisasterSetup = Services.DisasterSetup;
            if (naturalDisasterSetup.ScaleMaxIntensityWithPopulation &&
                DisasterSimulationUtils.GetPopulation() < naturalDisasterSetup.MaxPopulationToTriggerHigherDisasters)
            {
                if (result != "") result += CommonProperties.NewLine;
                result += LocalizationService.Get("tooltip.low_population");
            }

            return result;
        }

        public virtual string GetGeneratedIntensityTooltip(byte maxGeneratedIntensity)
        {
            return GetIntensityTooltip(maxGeneratedIntensity / 255f);
        }

        public virtual void CopySettings(DisasterBaseModel disaster)
        {
            Enabled = disaster.Enabled;
            BaseOccurrencePerYear = disaster.BaseOccurrencePerYear;
            EvacuationMode = disaster.EvacuationMode;
        }

        public void SetEnabled(bool enabled)
        {
            if (Enabled == enabled)
                return;

            Enabled = enabled;
            OnEnabledChanged(enabled);
        }

        public virtual void OnEnabledChanged(bool enabled)
        {
        }

        public virtual void SetDebugProbabilityProgress(float progress)
        {
            var clampedProgress = Mathf.Clamp01(progress);
            debugForceOccurrence = clampedProgress >= 0.999f;
            calmDaysLeft = 0f;

            if (probabilityWarmupDays > 0)
                probabilityWarmupDaysLeft = probabilityWarmupDays * (1f - clampedProgress);

            if (intensityWarmupDays > 0)
                intensityWarmupDaysLeft = intensityWarmupDays * (1f - clampedProgress);
        }

        public DisasterType GetDisasterType()
        {
            return DType;
        }

        // Utilities

        protected virtual float GetCurrentOccurrencePerYearLocal()
        {
            return BaseOccurrencePerYear;
        }

        protected virtual byte GetRandomIntensity(byte maxIntensity)
        {
            byte intensity;

            //if based on Gutenberg–Richter law
            if (ProbabilityDistribution == ProbabilityDistributions.PowerLow)
            {
                var randomValue =
                    Services.Simulation.m_randomizer.Int32(1000, 10000) / 10000.0f; // from range 0.1 - 1.0

                // See Gutenberg–Richter law.
                // a, b = 0.11
                intensity = (byte)(10 * (0.11 - Math.Log10(randomValue)) / 0.11);

                if (intensity > 100) intensity = 100;
            }
            else
            {
                //Otherwise uniform increment based on random value between 10 and 100
                intensity = (byte)Services.Simulation.m_randomizer.Int32(10, 100);
            }

            if (maxIntensity < 100) intensity = (byte)(10 + (intensity - 10) * maxIntensity / 100);

            return intensity;
        }

        protected byte GetMaximumGeneratedIntensity(byte maxIntensity)
        {
            if (maxIntensity >= 100)
                return 100;

            return (byte)(10 + (100 - 10) * maxIntensity / 100);
        }

        // TODO: Evaluate an advanced extreme-intensity system instead of changing all disasters globally.
        // Current automatic generation effectively caps most disasters at 10.0, even though the game can store
        // intensity up to 25.5 (byte.MaxValue / 10). Candidate design:
        // - Add a global advanced toggle such as "Allow extreme intensities up to 25.5".
        // - Add per-disaster maximum generated intensity so each disaster can opt into a different cap.
        // - Strong candidates: Meteor Strike, Earthquake, Tsunami.
        // - Optional/advanced candidates: Tornado and Sinkhole, with rarity controls and warnings.
        // - Lower-priority candidates: Thunderstorm and Forest Fire, where frequency/spread/secondary effects
        //   probably model severity better than raw 25.5 intensity.
        // - Keep default behavior conservative and preserve current 10.0 cap unless explicitly enabled.
        // - Review UI labels/tooltips so "max strength" reflects the real generated cap, not only byte storage.

        protected byte ScaleIntensityByWarmup(byte intensity)
        {
            if (intensityWarmupDaysLeft > 0 && intensityWarmupDays > 0)
            {
                if (intensityWarmupDaysLeft >= intensityWarmupDays)
                    intensity = indexReferenceDisasterValue;
                else
                    intensity = (byte)(indexReferenceDisasterValue + (intensity - indexReferenceDisasterValue) *
                        (1 - intensityWarmupDaysLeft / intensityWarmupDays));
            }

            return intensity;
        }

        protected byte ScaleIntensityByPopulation(byte intensity)
        {
            if (Services.DisasterSetup.ScaleMaxIntensityWithPopulation)
            {
                var population = DisasterSimulationUtils.GetPopulation();
                var naturalDisasterSetup = Services.DisasterSetup;
                if (population < naturalDisasterSetup.MaxPopulationToTriggerHigherDisasters)
                    intensity = (byte)(indexReferenceDisasterValue + (intensity - indexReferenceDisasterValue) *
                        population / naturalDisasterSetup.MaxPopulationToTriggerHigherDisasters);
            }

            return intensity;
        }

        private float ScaleProbabilityByWarmup(float probability)
        {
            if (!unlocked && OccurrenceAreaBeforeUnlock == OccurrenceAreas.Nowhere) return 0;

            if (probabilityWarmupDaysLeft > 0 && probabilityWarmupDays > 0)
            {
                if (probabilityWarmupDaysLeft >= probabilityWarmupDays)
                    probability = 0;
                else
                    probability *= 1 - probabilityWarmupDaysLeft / probabilityWarmupDays;
            }

            return probability;
        }

        protected string GetDebugStr()
        {
            return DType + ", " + Services.Simulation.m_currentGameTime.ToShortDateString() + ", ";
        }

        protected void LogDisasterStartFailure(string reason)
        {
            var now = DateTime.UtcNow;
            if ((now - lastStartFailureLogUtc).TotalSeconds < 5)
                return;

            lastStartFailureLogUtc = now;
            DebugLogger.Log(GetDebugStr() + reason);
        }

        // Disaster events

        protected virtual void OnSimulationFrameLocal()
        {
        }

        protected virtual float GetSimulationDaysPerFrame()
        {
            return DisasterSimulationUtils.DaysPerFrame;
        }

        public virtual void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId,
            ref List<DisasterInfoModel> activeDisaster)
        {
            var msg = string.Format("Disaster Activated. Id: {0}, Name: {1}, Type: {2}, Intensity: {3}",
                disasterId,
                disasterInfo.name,
                disasterInfo.type,
                disasterInfo.intensity);
            DebugLogger.Log(msg);

            var naturalDisasterSetup = Services.DisasterSetup;
            DebugLogger.Log(GetDebugStr() + string.Format("activation handling. Id: {0}, Enabled: {1}",
                disasterId, Enabled));
            DisasterExtension.SetPauseOnDisasterStarts(naturalDisasterSetup.PauseOnDisasterStarts, secondsBeforePausing,
                disasterId, disasterInfo, Enabled);
        }

        public virtual void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            try
            {
                var msg = string.Format(
                    "Disaster Deactivated: Id: {0}, Name: {1}, Type: {2}, Intensity: {3}, EvacuationMode:{4}, FinishOnDeactivate:{5}",
                    disasterInfoUnified.DisasterId,
                    disasterInfoUnified.DisasterInfo.name,
                    disasterInfoUnified.DisasterInfo.type,
                    disasterInfoUnified.DisasterInfo.intensity,
                    disasterInfoUnified.EvacuationMode,
                    disasterInfoUnified.FinishOnDeactivate);

                DebugLogger.Log(msg);

                if (disasterInfoUnified.DisasterInfo.type == DisasterType.Empty) return;

                if (!IsEvacuating())
                {
                    // Not evacuating globally. Manual evacuations no longer need to block automatic releases.
                    manualReleaseDisasters.Clear();
                }

                //Evaluate Disaster finishing                
                var disasterFinishing = activeDisasters
                    .Where(activeDisaster => activeDisaster.DisasterId == disasterInfoUnified.DisasterId)
                    .FirstOrDefault();

                if (disasterFinishing != null && disasterInfoUnified.FinishOnDeactivate)
                {
                    activeDisasters.RemoveAll(activeDisaster =>
                        activeDisaster.DisasterId == disasterInfoUnified.DisasterId);

                    switch (disasterFinishing.EvacuationMode)
                    {
                        case EvacuationOptions.ManualEvacuation:
                            break;

                        case EvacuationOptions.AutoEvacuation:
                        case EvacuationOptions.FocusedAutoEvacuation:
                            //When all list empty, then release all using basegame code for it.
                            if (!manualReleaseDisasters.Any() && !activeDisasters.Any())
                            {
                                //Auto releasing citizens
                                ReleaseAllShelters();
                                break;
                            }

                            //Verify shelters when there are pending disasters
                            if (activeDisasters.Any())
                            {
                                var pendingShelters = new List<ushort>();
                                //If pending disaster then get all pending shelters that souldnt be released
                                foreach (var disaster in activeDisasters)
                                {
                                    foreach (var shelterId in disaster.ShelterList)
                                    {
                                        if (!pendingShelters.Contains(shelterId))
                                            pendingShelters.Add(shelterId);
                                    }
                                }

                                var sheltersToBeReleased = disasterFinishing.ShelterList.Where(disFinishing =>
                                    pendingShelters.All(pendSh => pendSh != disFinishing)).ToList();
                                ReleaseShelters(sheltersToBeReleased);
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

        public virtual void OnDisasterDetected(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            var msg =
                $"Disaster Detected. type: {disasterInfoUnified.DisasterInfo.type}, name:{disasterInfoUnified.DisasterInfo.name}, " +
                $"location => x:{disasterInfoUnified.DisasterInfo.targetX} y:{disasterInfoUnified.DisasterInfo.targetX} z:{disasterInfoUnified.DisasterInfo.targetZ}. " +
                $"Angle: {disasterInfoUnified.DisasterInfo.angle}, intensity: {disasterInfoUnified.DisasterInfo.intensity} " +
                $"EvacuationMode: {disasterInfoUnified.EvacuationMode}";
            DebugLogger.Log(msg);

            var naturalDisasterSetup = Services.DisasterSetup;

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
                    SetupAutomaticFocusedEvacuation(disasterInfoUnified, naturalDisasterSetup.PartialEvacuationRadius,
                        ref activeDisasters);
                    break;
            }
        }

        public virtual void OnDisasterFinished(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            var msg =
                $"Disaster Finished: type: {disasterInfoUnified.DisasterInfo.type}, name:{disasterInfoUnified.DisasterInfo.name}, " +
                $"location => x:{disasterInfoUnified.DisasterInfo.targetX} y:{disasterInfoUnified.DisasterInfo.targetX} z:{disasterInfoUnified.DisasterInfo.targetZ}. " +
                $"Angle: {disasterInfoUnified.DisasterInfo.angle}, intensity: {disasterInfoUnified.DisasterInfo.intensity} " +
                $"EvacuationMode: {disasterInfoUnified.EvacuationMode}" +
                $"FinishOnDeactivate: {disasterInfoUnified.FinishOnDeactivate}";
            DebugLogger.Log(msg);
        }

        public virtual void OnDisasterStarted(byte intensity)
        {
            var framesPerDay = DisasterSimulationUtils.FramesPerDay;

            calmDaysLeft = calmDays * intensity / 100; // TO DO: May be define minimum calmDays
            probabilityWarmupDaysLeft = probabilityWarmupDays;
            intensityWarmupDaysLeft = intensityWarmupDays;
        }

        protected virtual void DisasterStarting(DisasterInfo disasterInfo)
        {
        }

        protected virtual void AfterStartDisaster(ushort disasterId)
        {
        }

        public abstract bool CheckDisasterAIType(object disasterAI);

        protected void StartDisaster(byte intensity)
        {
            var disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);

            if (disasterInfo == null)
            {
                LogDisasterStartFailure("could not start disaster: disaster info not found");
                return;
            }

            var targetFound = FindTarget(disasterInfo, out var targetPosition, out var angle);
            if (!targetFound)
            {
                LogDisasterStartFailure("could not start disaster: target not found");
                return;
            }

            var dm = Services.Disasters;

            var disasterCreated = dm.CreateDisaster(out var disasterIndex, disasterInfo);
            if (!disasterCreated)
            {
                LogDisasterStartFailure("could not start disaster: CreateDisaster returned false");
                return;
            }

            DisasterLogger.StartedByMod = true;

            DisasterStarting(disasterInfo);

            dm.m_disasters.m_buffer[disasterIndex].m_targetPosition = targetPosition;
            dm.m_disasters.m_buffer[disasterIndex].m_angle = angle;
            dm.m_disasters.m_buffer[disasterIndex].m_intensity = intensity;
            var expr_98_cp_0 = dm.m_disasters.m_buffer;
            var expr_98_cp_1 = disasterIndex;
            expr_98_cp_0[expr_98_cp_1].m_flags = expr_98_cp_0[expr_98_cp_1].m_flags | DisasterData.Flags.SelfTrigger;
            disasterInfo.m_disasterAI.StartNow(disasterIndex, ref dm.m_disasters.m_buffer[disasterIndex]);
            AfterStartDisaster(disasterIndex);
            debugForceOccurrence = false;

            DebugLogger.Log(GetDebugStr() + string.Format(
                "StartNow called. Id: {0}, Flags: {1}, Intensity: {2}, Area: {3}, Target: x:{4:#.##} y:{5:#.##} z:{6:#.##}",
                disasterIndex,
                dm.m_disasters.m_buffer[disasterIndex].m_flags,
                intensity,
                unlocked ? OccurrenceAreaAfterUnlock : OccurrenceAreaBeforeUnlock,
                targetPosition.x,
                targetPosition.y,
                targetPosition.z));
        }

        protected virtual bool FindTarget(DisasterInfo disasterInfo, out Vector3 targetPosition, out float angle)
        {
            var area = unlocked ? OccurrenceAreaAfterUnlock : OccurrenceAreaBeforeUnlock;

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

        public virtual float CalculateDestructionRadio(byte intensity)
        {
            return 5.656854249f; //min Value Radio
        }

        public virtual void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel,
            ref List<DisasterInfoModel> activeDisasters)
        {
            var disasterTargetPosition = new Vector3(disasterInfoModel.DisasterInfo.targetX,
                disasterInfoModel.DisasterInfo.targetY, disasterInfoModel.DisasterInfo.targetZ);

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
                    var shelterPosition = buildingInfo.m_position;

                    if (buildingInfo.Info.m_buildingAI as ShelterAI != null)
                    {
                        //Add Building/Shelter Data to disaster
                        disasterInfoModel.ShelterList.Add(num);

                        //Getting diaster core
                        var disasterDestructionRadius =
                            CalculateDestructionRadio(disasterInfoModel.DisasterInfo.intensity);

                        float shelterRadius = (buildingInfo.Length < buildingInfo.Width
                            ? buildingInfo.Width
                            : buildingInfo.Length) * 8 / 2;

                        //if Shelter will be destroyed, don't evacuate
                        if (!disasterInfoModel.IgnoreDestructionZone && IsShelterInDisasterZone(disasterTargetPosition,
                                shelterPosition, shelterRadius, disasterDestructionRadius))
                            DebugLogger.Log("Shelter is located in Destruction Zone. Won't be avacuated");
                        else
                            SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num,
                                ref buildingManager.m_buildings.m_buffer[num], false);
                    }
                }
            }

            AddOrReplaceActiveDisaster(disasterInfoModel, ref activeDisasters);
        }

        private bool FindRandomTargetEverywhere(out Vector3 target, out float angle)
        {
            var gam = Services.GameArea;
            var sm = Services.Simulation;
            var i = sm.m_randomizer.Int32(0, 4);
            var j = sm.m_randomizer.Int32(0, 4);
            float minX;
            float minZ;
            float maxX;
            float maxZ;
            gam.GetAreaBounds(i, j, out minX, out minZ, out maxX, out maxZ);

            var randX = sm.m_randomizer.Int32(0, 10000) * 0.0001f;
            var randZ = sm.m_randomizer.Int32(0, 10000) * 0.0001f;
            target.x = minX + (maxX - minX) * randX;
            target.y = 0f;
            target.z = minZ + (maxZ - minZ) * randZ;
            target.y = Services.Terrain.SampleRawHeightSmoothWithWater(target, false, 0f);
            angle = sm.m_randomizer.Int32(0, 10000) * 0.0006283185f;
            return true;
        }

        private bool FindRandomTargetInLockedAreas(out Vector3 target, out float angle)
        {
            var gam = Services.GameArea;
            var sm = Services.Simulation;

            // No locked areas
            if (gam.m_areaCount >= 25)
            {
                target = Vector3.zero;
                angle = 0f;
                return false;
            }

            var lockedAreaCounter = sm.m_randomizer.Int32(1, 25 - gam.m_areaCount);
            for (var i = 0; i < 5; i++)
            for (var j = 0; j < 5; j++)
            {
                if (IsUnlocked(i, j)) continue;

                if (--lockedAreaCounter == 0)
                {
                    float minX;
                    float minZ;
                    float maxX;
                    float maxZ;
                    gam.GetAreaBounds(j, i, out minX, out minZ, out maxX, out maxZ);
                    var minimumEdgeDistance = 100f;
                    if (IsUnlocked(j - 1, i)) minX += minimumEdgeDistance;
                    if (IsUnlocked(j, i - 1)) minZ += minimumEdgeDistance;
                    if (IsUnlocked(j + 1, i)) maxX -= minimumEdgeDistance;
                    if (IsUnlocked(j, i + 1)) maxZ -= minimumEdgeDistance;

                    var randX = sm.m_randomizer.Int32(0, 10000) * 0.0001f;
                    var randZ = sm.m_randomizer.Int32(0, 10000) * 0.0001f;
                    target.x = minX + (maxX - minX) * randX;
                    target.y = 0f;
                    target.z = minZ + (maxZ - minZ) * randZ;
                    target.y = Services.Terrain.SampleRawHeightSmoothWithWater(target, false, 0f);
                    angle = sm.m_randomizer.Int32(0, 10000) * 0.0006283185f;
                    return true;
                }
            }

            target = Vector3.zero;
            angle = 0f;
            return false;
        }

        private bool IsUnlocked(int x, int z)
        {
            return x >= 0 && z >= 0 && x < 5 && z < 5 && Services.GameArea.m_areaGrid[z * 5 + x] != 0;
        }

        private void FindPhasePanel()
        {
            DebugLogger.Log("ES: Find Phase Panel");

            if (phasePanel != null)
                return;

            phasePanel = UnityObject.FindObjectOfType<WarningPhasePanel>();
            evacuatingField = phasePanel.GetType()
                .GetField("m_isEvacuating", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        protected bool IsEvacuating()
        {
            FindPhasePanel();
            return (bool)evacuatingField.GetValue(phasePanel);
        }

        private void SetupManualEvacuation(ushort disasterId)
        {
            //Should be manually released
            manualReleaseDisasters.Add(disasterId);
        }

        private void SetupAutomaticFocusedEvacuation(DisasterInfoModel disasterInfoModel, float disasterRadius,
            ref List<DisasterInfoModel> activeDisasters)
        {
            var disasterTargetPosition = new Vector3(disasterInfoModel.DisasterInfo.targetX,
                disasterInfoModel.DisasterInfo.targetY, disasterInfoModel.DisasterInfo.targetZ);

            //Get disaster Info
            var disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);

            //Get Disaster Radio from Settings property
            var disasterRadioEvacuation = (float)Math.Sqrt(disasterRadius); //32f as default aprox;

            if (disasterInfo == null)
                return;

            //Identify Shelters
            var buildingManager = Services.Buildings;
            var serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);

            if (serviceBuildings == null)
                return;

            //Release Specific shelters based on disaster radius proximity
            for (var i = 0; i < serviceBuildings.m_size; i++)
            {
                var num = serviceBuildings.m_buffer[i];
                if (num != 0)
                {
                    //here we got all shelter buildings
                    var buildingInfo = buildingManager.m_buildings.m_buffer[num];
                    var shelterPosition = buildingInfo.m_position;

                    //Once this is located into Risk Zone, it's needed verify if building would be destroyed by Natural disaster (setup in each one)
                    //Getting diaster core
                    var disasterDestructionRadius = CalculateDestructionRadio(disasterInfoModel.DisasterInfo.intensity);

                    float shelterRadius =
                        (buildingInfo.Length < buildingInfo.Width ? buildingInfo.Width : buildingInfo.Length) * 8 / 2;

                    if (buildingInfo.Info.m_buildingAI as ShelterAI != null && IsShelterInDisasterZone(
                            disasterTargetPosition, shelterPosition, shelterRadius,
                            disasterDestructionRadius > disasterRadioEvacuation
                                ? disasterDestructionRadius
                                : disasterRadioEvacuation))
                    {
                        //Add Building/Shelter Data to disaster
                        disasterInfoModel.ShelterList.Add(num);

                        DebugLogger.Log(
                            $"Disaster intensity: {disasterInfoModel.DisasterInfo.intensity}. " +
                            $"DisasterRadioEvacuation: {disasterRadioEvacuation}. " +
                            $"DisasterRadioEvacuation into destruction (New Calculation): {disasterDestructionRadius}"
                        );

                        //if Shelter will be destroyed, don't evacuate
                        if (!disasterInfoModel.IgnoreDestructionZone && IsShelterInDisasterZone(disasterTargetPosition,
                                shelterPosition, shelterRadius, disasterDestructionRadius))
                            DebugLogger.Log("Shelter is located in Destruction Zone. Won't be avacuated");
                        else
                            SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num,
                                ref buildingManager.m_buildings.m_buffer[num], false);
                    }
                }
            }

            AddOrReplaceActiveDisaster(disasterInfoModel, ref activeDisasters);
        }

        protected void AddOrReplaceActiveDisaster(DisasterInfoModel disasterInfoModel,
            ref List<DisasterInfoModel> activeDisasters)
        {
            activeDisasters.RemoveAll(activeDisaster => activeDisaster.DisasterId == disasterInfoModel.DisasterId);
            activeDisasters.Add(disasterInfoModel);
        }

        private void ReleaseAllShelters()
        {
            var buildingManager = Services.Buildings;
            var serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);

            if (serviceBuildings == null)
                return;

            var shelterIds = new List<ushort>();
            for (var i = 0; i < serviceBuildings.m_size; i++)
            {
                var shelterId = serviceBuildings.m_buffer[i];
                if (shelterId != 0)
                    shelterIds.Add(shelterId);
            }

            ReleaseShelters(shelterIds);
        }

        private void ReleaseShelters(IEnumerable<ushort> shelterIds)
        {
            var buildingManager = Services.Buildings;

            foreach (var shelterId in shelterIds)
            {
                var buildingInfo = buildingManager.m_buildings.m_buffer[shelterId];
                SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, shelterId,
                    ref buildingManager.m_buildings.m_buffer[shelterId], true);
            }
        }

        protected void SetBuidingEvacuationStatus(ShelterAI shelterAI, ushort num, ref Building buildingData,
            bool release)
        {
            shelterAI?.SetEmptying(num, ref buildingData, release);
        }

        protected bool IsShelterInDisasterZone(Vector3 disasterPosition, Vector3 shelterPosition, float shelterRadius,
            float evacuationRadius)
        {
            //First Squared Is required for correct calculation
            evacuationRadius *= evacuationRadius;
            // Compare radius of circle with distance
            // of its center from given point

            var distanceBetweenTwoPoints =
                (float)Math.Sqrt((disasterPosition.x - shelterPosition.x) * (disasterPosition.x - shelterPosition.x) +
                                 (disasterPosition.z - shelterPosition.z) * (disasterPosition.z - shelterPosition.z));

            if (distanceBetweenTwoPoints <= evacuationRadius - shelterRadius)
                return true;

            if (distanceBetweenTwoPoints <= shelterRadius - evacuationRadius)
                return true;

            if (distanceBetweenTwoPoints < evacuationRadius + shelterRadius)
                return true;

            if (distanceBetweenTwoPoints == evacuationRadius + shelterRadius)
                return true;
            return false;
        }

        public virtual bool CanAffectAt(ushort disasterID, ref DisasterData data, Vector3 buildingPosition,
            Vector3 seasidePosition, out float priority)
        {
            priority = 0f;
            var canAffect = (data.m_flags &
                             (DisasterData.Flags.Emerging | DisasterData.Flags.Active | DisasterData.Flags.Clearing)) !=
                            0;
            return canAffect;
        }
    }
}
