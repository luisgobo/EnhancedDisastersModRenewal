﻿using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.BaseGameExtensions;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.Models.Disaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Xml.Serialization;
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

        // Cooldown variables (Not stored into XML)
        protected int calmDays = 0;

        protected int probabilityWarmupDays = 0;
        [XmlIgnore] public float calmDaysLeft = 0;
        [XmlIgnore] public float probabilityWarmupDaysLeft = 0;
        [XmlIgnore] public int intensityWarmupDays = 0;
        [XmlIgnore] public float intensityWarmupDaysLeft = 0;
        

        // Disaster properties
        protected DisasterType DType = DisasterType.Empty;

        protected ProbabilityDistributions ProbabilityDistribution = ProbabilityDistributions.Uniform;
        //protected int FullIntensityMaxLimitPopulation = 70000;//Original: 20000
        protected OccurrenceAreas OccurrenceAreaBeforeUnlock = OccurrenceAreas.Nowhere;
        protected OccurrenceAreas OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
        protected bool unlocked = false;

        // Disaster public properties (to be saved in xml)
        public bool Enabled = true;

        public EvacuationOptions EvacuationMode = EvacuationOptions.ManualEvacuation;
        public float BaseOccurrencePerYear = 1.0f;        

        // Disaster services
        FieldInfo evacuatingField;

        WarningPhasePanel phasePanel;
        readonly HashSet<ushort> manualReleaseDisasters = new HashSet<ushort>();
        readonly double secondsBeforePausing = 3;

        // Public
        public abstract string GetName();

        public float GetCurrentOccurrencePerYear()
        {
            if (calmDaysLeft > 0)
            {
                return 0f;
            }

            return ScaleProbabilityByWarmup(GetCurrentOccurrencePerYearLocal());
        }

        public virtual byte GetMaximumIntensity()
        {            
            byte intensity = baseIntensity;            
            intensity = ScaleIntensityByWarmup(intensity);
            intensity = ScaleIntensityByPopulation(intensity);
            return intensity;
        }

        public void OnSimulationFrame()
        {
            if (!Enabled)
            {
                return;
            }

            if (!unlocked && OccurrenceAreaBeforeUnlock == OccurrenceAreas.Nowhere)
            {
                return;
            }

            OnSimulationFrameLocal();

            float daysPerFrame = Helper.DaysPerFrame;

            if (calmDaysLeft > 0)
            {
                calmDaysLeft -= daysPerFrame;
                return;
            }

            if (probabilityWarmupDaysLeft > 0)
            {
                if (probabilityWarmupDaysLeft > probabilityWarmupDays)
                {
                    probabilityWarmupDaysLeft = probabilityWarmupDays;
                }

                probabilityWarmupDaysLeft -= daysPerFrame;
            }

            if (intensityWarmupDaysLeft > 0)
            {
                if (intensityWarmupDaysLeft > intensityWarmupDays)
                {
                    intensityWarmupDaysLeft = intensityWarmupDays;
                }

                intensityWarmupDaysLeft -= daysPerFrame;
            }

            float occurrencePerYear = GetCurrentOccurrencePerYear();

            if (occurrencePerYear == 0)
            {
                return;
            }

            SimulationManager sm = Singleton<SimulationManager>.instance;
            float occurrencePerFrame = (occurrencePerYear / 365) * daysPerFrame;
            if (sm.m_randomizer.Int32(randomizerRange) < (uint)(randomizerRange * occurrencePerFrame))
            {                
                var maxIntensity = GetMaximumIntensity();                
                byte intensity = GetRandomIntensity(maxIntensity);

                StartDisaster(intensity);
            }
        }

        public void Unlock()
        {
            unlocked = true;
        }

        public virtual string GetProbabilityTooltip(float value)
        {
            if (!unlocked)
            {
                return "Not unlocked yet";
            }

            if (calmDaysLeft > 0)
            {
                return "No " + GetName() + " for another " + Helper.FormatTimeSpan(calmDaysLeft);
            }

            if (probabilityWarmupDaysLeft > 0)
            {
                return "Decreased because " + GetName() + " occured recently.";
            }

            //return $"Probability: {value * 10:#.##}";
            return $"Probability: {(value*2):#.##}";
        }

        public virtual string GetIntensityTooltip(float value)
        {
            if (!unlocked)
            {
                return "Not unlocked yet";
            }

            if (calmDaysLeft > 0)
            {
                return "No " + GetName() + " for another " + Helper.FormatTimeSpan(calmDaysLeft);
            }

            string result = $"Intensity: {value * 25.5:#.##}";            

            if (probabilityWarmupDaysLeft > 0)
            {
                result = "Decreased because " + GetName() + " occured recently.";
            }

            var naturalDisasterSetup = Singleton<NaturalDisasterHandler>.instance.container;
            if (Helper.GetPopulation() < naturalDisasterSetup.MaxPopulationToTrigguerHigherDisasters)
            {
                if (result != "") result += CommonProperties.newLine;
                result += "Decreased because of low population.";
            }

            return result;
        }

        public virtual void CopySettings(DisasterBaseModel disaster)
        {
            Enabled = disaster.Enabled;
            BaseOccurrencePerYear = disaster.BaseOccurrencePerYear;
            EvacuationMode = disaster.EvacuationMode;
        }

        public DisasterType GetDisasterType() => DType;

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
                float randomValue = Singleton<SimulationManager>.instance.m_randomizer.Int32(1000, 10000) / 10000.0f; // from range 0.1 - 1.0

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

        protected byte ScaleIntensityByWarmup(byte intensity)
        {
            if (intensityWarmupDaysLeft > 0 && intensityWarmupDays > 0)
            {
                if (intensityWarmupDaysLeft >= intensityWarmupDays)
                {
                    intensity = indexReferenceDisasterValue;
                }
                else
                {
                    intensity = (byte)(indexReferenceDisasterValue + (intensity - indexReferenceDisasterValue) * (1 - (intensityWarmupDaysLeft / intensityWarmupDays)));
                }
            }                       

            return intensity;
        }

        protected byte ScaleIntensityByPopulation(byte intensity)
        {
            if (Singleton<NaturalDisasterHandler>.instance.container.ScaleMaxIntensityWithPopulation)
            {                
                int population = Helper.GetPopulation();
                var naturalDisasterSetup = Singleton<NaturalDisasterHandler>.instance.container;
                if (population < naturalDisasterSetup.MaxPopulationToTrigguerHigherDisasters)
                {
                    intensity = (byte)(indexReferenceDisasterValue + (((intensity - indexReferenceDisasterValue) * population) / naturalDisasterSetup.MaxPopulationToTrigguerHigherDisasters));

                }
            }
            return intensity;
        }

        float ScaleProbabilityByWarmup(float probability)
        {
            if (!unlocked && OccurrenceAreaBeforeUnlock == OccurrenceAreas.Nowhere)
            {
                return 0;
            }

            if (probabilityWarmupDaysLeft > 0 && probabilityWarmupDays > 0)
            {
                if (probabilityWarmupDaysLeft >= probabilityWarmupDays)
                {
                    probability = 0;
                }
                else
                {
                    probability *= 1 - probabilityWarmupDaysLeft / probabilityWarmupDays;
                }
            }

            return probability;
        }

        protected string GetDebugStr()
        {
            return DType.ToString() + ", " + Singleton<SimulationManager>.instance.m_currentGameTime.ToShortDateString() + ", ";
        }

        // Disaster events

        protected virtual void OnSimulationFrameLocal()
        {
        }

        public virtual void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId, ref List<DisasterInfoModel> activeDisaster)
        {
            var msg = string.Format("Disaster Activated. Id: {0}, Name: {1}, Type: {2}, Intensity: {3}",
                disasterId,
                disasterInfo.name,
                disasterInfo.type,
                disasterInfo.intensity);
            DebugLogger.Log(msg);

            var naturalDisasterSetup = Singleton<NaturalDisasterHandler>.instance.container;
            DisasterExtension.SetPauseOnDisasterStarts(naturalDisasterSetup.PauseOnDisasterStarts, secondsBeforePausing, disasterId, disasterInfo, Enabled);
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
                    manualReleaseDisasters.Clear();
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
                            if (!manualReleaseDisasters.Any() && !activeDisasters.Any())
                            {
                                //Auto releasing citizens
                                DisasterManager.instance.EvacuateAll(true);
                                break;
                            }

                            //Verify shelters when there are pending disasters
                            if (activeDisasters.Any())
                            {
                                var pendingShelters = new List<ushort>();
                                //If pending disaster then get all pending shelters that souldnt be released
                                foreach (var disaster in activeDisasters)
                                {
                                    //this is not being filled
                                    foreach (var shelterId in disaster.ShelterList)
                                    {
                                        if (!pendingShelters.Contains(shelterId))
                                            pendingShelters.Add(shelterId);
                                    }
                                }

                                var sheltersToBeReleased = disasterFinishing.ShelterList.Where(disFinishing => pendingShelters.All(pendSh => pendSh != disFinishing)).ToList();
                                BuildingManager buildingManager = Singleton<BuildingManager>.instance;

                                foreach (var shelterId in sheltersToBeReleased)
                                {
                                    var buildingInfo = buildingManager.m_buildings.m_buffer[shelterId];
                                    SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, shelterId, ref buildingManager.m_buildings.m_buffer[shelterId], true);
                                }
                            }
                            break;

                        default:
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

            var naturalDisasterSetup = Singleton<NaturalDisasterHandler>.instance.container;

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
            float framesPerDay = Helper.FramesPerDay;

            calmDaysLeft = calmDays * intensity / 100; // TO DO: May be define minimum calmDays
            probabilityWarmupDaysLeft = probabilityWarmupDays;
            intensityWarmupDaysLeft = intensityWarmupDays;
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

            DebugLogger.Log(GetDebugStr() + string.Format("disaster intensity: {0}, area: {1}", intensity, unlocked ? OccurrenceAreaAfterUnlock : OccurrenceAreaBeforeUnlock));
        }

        protected virtual bool FindTarget(DisasterInfo disasterInfo, out Vector3 targetPosition, out float angle)
        {
            OccurrenceAreas area = unlocked ? OccurrenceAreaAfterUnlock : OccurrenceAreaBeforeUnlock;

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

        public virtual void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel, ref List<DisasterInfoModel> activeDisasters)
        {
            var disasterTargetPosition = new Vector3(disasterInfoModel.DisasterInfo.targetX, disasterInfoModel.DisasterInfo.targetY, disasterInfoModel.DisasterInfo.targetZ);

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
                    var shelterPosition = buildingInfo.m_position;

                    if ((buildingInfo.Info.m_buildingAI as ShelterAI) != null)
                    {
                        //Add Building/Shelter Data to disaster
                        disasterInfoModel.ShelterList.Add(num);

                        //Getting diaster core
                        var disasterDestructionRadius = CalculateDestructionRadio(disasterInfoModel.DisasterInfo.intensity);

                        float shelterRadius = ((buildingInfo.Length < buildingInfo.Width ? buildingInfo.Width : buildingInfo.Length) * 8) / 2;

                        //if Shelter will be destroyed, don't evacuate
                        if (!disasterInfoModel.IgnoreDestructionZone && IsShelterInDisasterZone(disasterTargetPosition, shelterPosition, shelterRadius, disasterDestructionRadius))
                            DebugLogger.Log($"Shelter is located in Destruction Zone. Won't be avacuated");
                        else
                            SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num, ref buildingManager.m_buildings.m_buffer[num], false);
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

            int lockedAreaCounter = sm.m_randomizer.Int32(1, 25 - gam.m_areaCount);
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
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
                        float minimumEdgeDistance = 100f;
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

                        float randX = (float)sm.m_randomizer.Int32(0, 10000) * 0.0001f;
                        float randZ = (float)sm.m_randomizer.Int32(0, 10000) * 0.0001f;
                        target.x = minX + (maxX - minX) * randX;
                        target.y = 0f;
                        target.z = minZ + (maxZ - minZ) * randZ;
                        target.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(target, false, 0f);
                        angle = (float)sm.m_randomizer.Int32(0, 10000) * 0.0006283185f;
                        return true;
                    }
                }
            }

            target = Vector3.zero;
            angle = 0f;
            return false;
        }

        bool IsUnlocked(int x, int z)
        {
            return x >= 0 && z >= 0 && x < 5 && z < 5 && Singleton<GameAreaManager>.instance.m_areaGrid[z * 5 + x] != 0;
        }

        void FindPhasePanel()
        {
            DebugLogger.Log("ES: Find Phase Panel");

            if (phasePanel != null)
                return;

            phasePanel = UnityObject.FindObjectOfType<WarningPhasePanel>();
            evacuatingField = phasePanel.GetType().GetField("m_isEvacuating", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        protected bool IsEvacuating()
        {
            FindPhasePanel();
            return (bool)evacuatingField.GetValue(phasePanel);
        }

        void SetupManualEvacuation(ushort disasterId)
        {
            //Should be manually released
            manualReleaseDisasters.Add(disasterId);
        }

        void SetupAutomaticFocusedEvacuation(DisasterInfoModel disasterInfoModel, float disasterRadius, ref List<DisasterInfoModel> activeDisasters)
        {
            var disasterTargetPosition = new Vector3(disasterInfoModel.DisasterInfo.targetX, disasterInfoModel.DisasterInfo.targetY, disasterInfoModel.DisasterInfo.targetZ);

            //Get disaster Info
            DisasterInfo disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);

            //Get Disaster Radio from Settings property
            float disasterRadioEvacuation = (float)Math.Sqrt(disasterRadius); //32f as default aprox;

            if (disasterInfo == null)
                return;

            //Identify Shelters
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            FastList<ushort> serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Disaster);

            if (serviceBuildings == null)
                return;

            //Release Specific shelters based on disaster radius proximity
            for (int i = 0; i < serviceBuildings.m_size; i++)
            {
                ushort num = serviceBuildings.m_buffer[i];
                if (num != 0)
                {
                    //here we got all shelter buildings
                    var buildingInfo = buildingManager.m_buildings.m_buffer[num];
                    var shelterPosition = buildingInfo.m_position;

                    //Once this is located into Risk Zone, it's needed verify if building would be destroyed by Natural disaster (setup in each one)
                    //Getting diaster core
                    var disasterDestructionRadius = CalculateDestructionRadio(disasterInfoModel.DisasterInfo.intensity);

                    float shelterRadius = ((buildingInfo.Length < buildingInfo.Width ? buildingInfo.Width : buildingInfo.Length) * 8) / 2;

                    if ((buildingInfo.Info.m_buildingAI as ShelterAI) != null && IsShelterInDisasterZone(disasterTargetPosition, shelterPosition, shelterRadius, disasterDestructionRadius > disasterRadioEvacuation ? disasterDestructionRadius : disasterRadioEvacuation))
                    {
                        //Add Building/Shelter Data to disaster
                        disasterInfoModel.ShelterList.Add(num);

                        DebugLogger.Log(
                            $"Disaster intensity: {disasterInfoModel.DisasterInfo.intensity}. " +
                            $"DisasterRadioEvacuation: {disasterRadioEvacuation}. " +
                            $"DisasterRadioEvacuation into destruction (New Calculation): {disasterDestructionRadius}"
                        );

                        //if Shelter will be destroyed, don't evacuate
                        if (!disasterInfoModel.IgnoreDestructionZone && IsShelterInDisasterZone(disasterTargetPosition, shelterPosition, shelterRadius, disasterDestructionRadius))
                            DebugLogger.Log($"Shelter is located in Destruction Zone. Won't be avacuated");
                        else
                            SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num, ref buildingManager.m_buildings.m_buffer[num], false);
                    }
                }
            }

            activeDisasters.Add(disasterInfoModel);
        }

        protected void SetBuidingEvacuationStatus(ShelterAI shelterAI, ushort num, ref Building buildingData, bool release)
        {
            shelterAI?.SetEmptying(num, ref buildingData, release);
        }

        protected Boolean IsShelterInDisasterZone(Vector3 disasterPosition, Vector3 shelterPosition, float shelterRadius, float evacuationRadius)
        {

            //First Squared Is required for correct calculation
            evacuationRadius *= evacuationRadius;
            // Compare radius of circle with distance
            // of its center from given point
            
            float distanceBetweenTwoPoints = (float)Math.Sqrt((disasterPosition.x - shelterPosition.x) * (disasterPosition.x - shelterPosition.x) + (disasterPosition.z - shelterPosition.z) * (disasterPosition.z - shelterPosition.z));

            if (distanceBetweenTwoPoints <= evacuationRadius - shelterRadius)
                return true;

            if (distanceBetweenTwoPoints <= shelterRadius - evacuationRadius)
                return true;

            if (distanceBetweenTwoPoints < evacuationRadius + shelterRadius)
                return true;

            if (distanceBetweenTwoPoints == evacuationRadius + shelterRadius)
                return true;
            else
                return false;
        }

        public virtual bool CanAffectAt(ushort disasterID, ref DisasterData data, Vector3 buildingPosition, Vector3 seasidePosition, out float priority)
        {
            priority = 0f;            
            bool canAffect =  (data.m_flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active | DisasterData.Flags.Clearing)) != 0;            
            return canAffect;
        }
    }
}