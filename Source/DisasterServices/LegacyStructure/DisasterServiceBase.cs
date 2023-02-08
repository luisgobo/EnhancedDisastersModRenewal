using ColossalFramework;
using ColossalFramework.IO;
using HarmonyLib;
using ICities;
using NaturalDisastersOverhaulRenewal.Models;
using NaturalDisastersRenewal.BaseGameExtensions;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static PathUnit;
using UnityObject = UnityEngine.Object;

namespace NaturalDisastersRenewal.DisasterServices.LegacyStructure

{
    public abstract class DisasterServiceBase
    {
        public class SerializableDataCommon
        {
            public void SerializeCommonParameters(DataSerializer s, DisasterServiceBase disaster)
            {
                s.WriteBool(disaster.Enabled);
                s.WriteFloat(disaster.BaseOccurrencePerYear);
                s.WriteFloat(disaster.calmDaysLeft);
                s.WriteFloat(disaster.probabilityWarmupDaysLeft);
                s.WriteFloat(disaster.intensityWarmupDaysLeft);
                s.WriteInt32(disaster.EvacuationMode);
            }

            public void DeserializeCommonParameters(DataSerializer s, DisasterServiceBase disaster)
            {
                disaster.Enabled = s.ReadBool();
                disaster.BaseOccurrencePerYear = s.ReadFloat();
                if (s.version <= 2)
                {
                    float daysPerFrame = 1f / 585f;
                    disaster.calmDaysLeft = s.ReadInt32() * daysPerFrame;
                    disaster.probabilityWarmupDaysLeft = s.ReadInt32() * daysPerFrame;
                    disaster.intensityWarmupDaysLeft = s.ReadInt32() * daysPerFrame;
                    disaster.EvacuationMode = s.ReadInt32();
                }
                else
                {
                    disaster.calmDaysLeft = s.ReadFloat();
                    disaster.probabilityWarmupDaysLeft = s.ReadFloat();
                    disaster.intensityWarmupDaysLeft = s.ReadFloat();
                    disaster.EvacuationMode = s.ReadInt32();
                }
            }

            public void AfterDeserializeLog(string className)
            {
                Debug.Log(CommonProperties.LogMsgPrefix + className + " data loaded.");
            }
        }

        // Constants
        protected const uint randomizerRange = 67108864u;

        // Cooldown variables
        protected int calmDays = 0;

        protected float calmDaysLeft = 0;
        protected int probabilityWarmupDays = 0;
        protected float probabilityWarmupDaysLeft = 0;
        protected int intensityWarmupDays = 0;
        protected float intensityWarmupDaysLeft = 0;

        // Disaster properties
        protected DisasterType DType = DisasterType.Empty;

        protected ProbabilityDistributions ProbabilityDistribution = ProbabilityDistributions.Uniform;
        protected int FullIntensityPopulation = 20000;
        protected OccurrenceAreas OccurrenceAreaBeforeUnlock = OccurrenceAreas.Nowhere;
        protected OccurrenceAreas OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
        protected bool unlocked = false;

        // Disaster public properties (to be saved in xml)
        public bool Enabled = true;

        public int EvacuationMode = 0;
        public float BaseOccurrencePerYear = 1.0f;


        // Disaster services        
        private FieldInfo evacuatingField;
        private WarningPhasePanel phasePanel;
        private readonly HashSet<ushort> manualReleaseDisasters = new HashSet<ushort>();


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
            byte intensity = 100;

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
                byte intensity = GetRandomIntensity(GetMaximumIntensity());

                StartDisaster(intensity);
            }
        }

        public void Unlock()
        {
            unlocked = true;
        }

        public virtual string GetProbabilityTooltip()
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

            return "";
        }

        public virtual string GetIntensityTooltip()
        {
            if (!unlocked)
            {
                return "Not unlocked yet";
            }

            if (calmDaysLeft > 0)
            {
                return "No " + GetName() + " for another " + Helper.FormatTimeSpan(calmDaysLeft);
            }

            string result = "";

            if (probabilityWarmupDaysLeft > 0)
            {
                result = "Decreased because " + GetName() + " occured recently.";
            }

            if (Helper.GetPopulation() < FullIntensityPopulation)
            {
                if (result != "") result += Environment.NewLine;
                result += "Decreased because of low population.";
            }

            return result;
        }

        public virtual void CopySettings(DisasterServiceBase disaster)
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
                intensity = (byte)Singleton<SimulationManager>.instance.m_randomizer.Int32(10, 100);
            }

            if (maxIntensity < 100)
            {
                intensity = (byte)(10 + (intensity - 10) * maxIntensity / 100);
            }

            return intensity;
        }

        protected byte ScaleIntensityByWarmup(byte intensity)
        {
            if (intensityWarmupDaysLeft > 0 && intensityWarmupDays > 0)
            {
                if (intensityWarmupDaysLeft >= intensityWarmupDays)
                {
                    intensity = 10;
                }
                else
                {
                    intensity = (byte)(10 + (intensity - 10) * (1 - intensityWarmupDaysLeft / intensityWarmupDays));
                }
            }

            return intensity;
        }

        protected byte ScaleIntensityByPopulation(byte intensity)
        {
            if (Singleton<NaturalDisasterHandler>.instance.container.ScaleMaxIntensityWithPopulation)
            {
                int population = Helper.GetPopulation();
                if (population < FullIntensityPopulation)
                {
                    intensity = (byte)(10 + (intensity - 10) * population / FullIntensityPopulation);
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

        public virtual void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId)
        {
            var msg = string.Format("EvacuationService.OnDisasterActivated. Id: {0}, Name: {1}, Type: {2}, Intensity: {3}",
                disasterId,
                disasterInfo.name,
                disasterInfo.type,
                disasterInfo.intensity);
            DebugLogger.Log(msg);

            ////Pause when disaster start
            //if (TryDisableDisaster(disasterId, info))
            //{
            //    return;
            //}

            //if (ModConfig.Instance.GetSetting<bool>(SettingKeys.PauseOnDisasterStart) && autoPauseDisasters.Contains(disasterType))
            //{
            //    new Thread(
            //        () =>
            //        {
            //            try
            //            {
            //                var pauseStart = DateTime.UtcNow + TimeSpan.FromSeconds(SecondsBeforePausing);

            //                while (DateTime.UtcNow < pauseStart)
            //                {
            //                }

            //                DebugLogger.Log("Pausing game");
            //                SimulationManager.instance.SimulationPaused = true;
            //            }
            //            catch (Exception ex)
            //            {
            //                DebugLogger.Log(ex.ToString());

            //                throw;
            //            }
            //        }).Start();
            //}

        }

        public virtual void OnDisasterDeactivated(DisasterSettings disasterInfo, ushort disasterId, int releaseType)
        {
            try
            {
                var msg = string.Format("EvacuationService.OnDisasterDeactivated. Id: {0}, Name: {1}, Type: {2}, Intensity: {3}",
                disasterId,
                disasterInfo.name,
                disasterInfo.type,
                disasterInfo.intensity);

                DebugLogger.Log(msg);

                if (disasterInfo.type == DisasterType.Empty)
                {
                    return;
                }

                if (!IsEvacuating())
                {
                    DebugLogger.Log("Not evacuating. Clear list of active manual release disasters");
                    manualReleaseDisasters.Clear();
                    return;
                }

                DebugLogger.Log("ShouldAutoRelease: " + ShouldAutoRelease(releaseType));
                DebugLogger.Log("Existmanual releases: " + !manualReleaseDisasters.Any());

                //based on dister setup and list of manual releases check if autorelease
                if (ShouldAutoRelease(releaseType) && !manualReleaseDisasters.Any())
                {
                    DebugLogger.Log("Auto releasing citizens");
                    DisasterManager.instance.EvacuateAll(true);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log(ex.ToString());

                throw;
            }
        }

        public virtual void OnDisasterDetected(DisasterInfoModel disasterInfoUnified)
        {
            DebugLogger.Log("Execute code to evacuate cims based on type of disaster and also the specification of localization");
            DebugLogger.Log($"DisasterSettings: Type-{disasterInfoUnified.DisasterInfo.type}");

            var msg = $"disasterInfo(Base): type: {disasterInfoUnified.DisasterInfo.type}, name:{disasterInfoUnified.DisasterInfo.name}, " +
                                  $"location => x:{disasterInfoUnified.DisasterInfo.targetX} y:{disasterInfoUnified.DisasterInfo.targetX} z:{disasterInfoUnified.DisasterInfo.targetZ}. " +
                                  $"Angle: {disasterInfoUnified.DisasterInfo.angle}, intensity: {disasterInfoUnified.DisasterInfo.intensity} " +
                                  $"EvacuationMode: {disasterInfoUnified.EvacuationMode}";
            DebugLogger.Log(msg);

            //Getting issue check Disaster extension and container object
            var naturalDisasterSetup = Singleton<NaturalDisasterHandler>.instance.container;            

            DebugLogger.Log("SetAutoFocusOnDisasterBaseSettings: " + naturalDisasterSetup.DisableAutoFocusOnDisasterStarts);
            DisasterExtension.SetAutoFocusOnDisasterBaseSettings(naturalDisasterSetup.DisableAutoFocusOnDisasterStarts);

            switch (disasterInfoUnified.EvacuationMode)
            {
                case 0:
                case 1:
                    SetupManualEvacuation(disasterInfoUnified.DisasterId);
                    break;
                case 2: //Auto evacuate all shelters
                    SetupAutomaticEvacuation();
                    break;
                case 3:
                    SetupAutomaticFocusedEvacuation(disasterInfoUnified.DisasterInfo, disasterInfoUnified.IgnoreDestructionZone, naturalDisasterSetup.PartialEvacuationRadius);
                    break;
                default:
                    break;
            }
        }

        protected virtual void DisasterStarting(DisasterInfo disasterInfo)
        {
        }

        public virtual void OnDisasterStarted(byte intensity)
        {
            float framesPerDay = Helper.FramesPerDay;

            calmDaysLeft = calmDays * intensity / 100; // TO DO: May be define minimum calmDays
            probabilityWarmupDaysLeft = probabilityWarmupDays;
            intensityWarmupDaysLeft = intensityWarmupDays;
        }

        public abstract bool CheckDisasterAIType(object disasterAI);

        protected void StartDisaster(byte intensity)
        {
            DisasterInfo disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);

            if (disasterInfo == null)
            {
                return;
            }

            Vector3 targetPosition;
            float angle;
            bool targetFound = FindTarget(disasterInfo, out targetPosition, out angle);

            if (!targetFound)
            {
                DebugLogger.Log(GetDebugStr() + "target not found");
                return;
            }

            DisasterManager dm = Singleton<DisasterManager>.instance;

            ushort disasterIndex;
            bool disasterCreated = dm.CreateDisaster(out disasterIndex, disasterInfo);
            if (!disasterCreated)
            {
                DebugLogger.Log(GetDebugStr() + "could not create disaster");
                return;
            }

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
            DebugLogger.Log($"area {area}");

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
        bool ShouldAutoRelease(int releaseType)
        {
            return releaseType == (2 | 3);
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
                        DebugLogger.Log(string.Format("findRandomTargetInLockedAreas, j = {0}, i = {1}, areaCount = {2}", j, i, gam.m_areaCount));
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
            {
                return;
            }

            phasePanel = UnityObject.FindObjectOfType<WarningPhasePanel>();
            evacuatingField = phasePanel.GetType().GetField("m_isEvacuating", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        bool IsEvacuating()
        {
            FindPhasePanel();

            var isEvacuating = (bool)evacuatingField.GetValue(phasePanel);

            DebugLogger.Log("Is evacuating: " + isEvacuating);

            return isEvacuating;
        }

        void SetupManualEvacuation(ushort disasterId)
        {
            //Setup autorelease
            DebugLogger.Log("Should be manually released");
            manualReleaseDisasters.Add(disasterId);
        }

        void SetupAutomaticEvacuation()
        {
            DebugLogger.Log("Is auto-evacuate disaster");
            if (!IsEvacuating())
            {
                DebugLogger.Log("Starting evacuation");
                DisasterManager.instance.EvacuateAll(false);
            }
            else
            {
                DebugLogger.Log("Already evacuating");
            }
        }

        void SetupAutomaticFocusedEvacuation(DisasterSettings disasterInfoSettings, bool ignoreDestructionZone, float disasterRadius, bool release = false)
        {
            var disasterTargetPosition = new Vector3(disasterInfoSettings.targetX, disasterInfoSettings.targetY, disasterInfoSettings.targetZ);

            //Get disaster Info
            DisasterInfo disasterInfo = NaturalDisasterHandler.GetDisasterInfo(DType);

            //Get Disaster Radio from Settings property
            //float disasterRadioEvacuation = 32f;
            float disasterRadioEvacuation = (float)Math.Sqrt(disasterRadius);

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
                    BuildingInfo info = buildingManager.m_buildings.m_buffer[num].Info;

                    var shelterPosition = buildingManager.m_buildings.m_buffer[num].m_position;
                    if ((info.m_buildingAI as ShelterAI) != null && IsShelterInDisasterZone(disasterTargetPosition, shelterPosition, disasterRadioEvacuation))
                    {
                        DebugLogger.Log($"Shelter is located in risk zone");
                        
                        //Once this is located into Risk Zone, it's needed verify if building would be ddestroyed by Natural disaster (setup in each one)                                                
                        //Getting diaster core 
                        var disasterRadioDestruction = disasterRadioEvacuation / 3f;
                        
                        //if Shelter will be destroyed, do not evacuate
                        if (IsShelterInDisasterZone(disasterTargetPosition, shelterPosition, disasterRadioDestruction) && !ignoreDestructionZone)
                            DebugLogger.Log($"Shelter is located in Destruction Zone. DON'T EVACUATE");
                        else
                            (info.m_buildingAI as ShelterAI)?.SetEmptying(num, ref buildingManager.m_buildings.m_buffer[num], release);

                    }
                }
            }

        }

        Boolean IsShelterInDisasterZone(Vector3 disasterPosition, Vector3 shelterPosition, float evacuationRadius)
        {
            //First Squared Is required for correct calculation
            evacuationRadius *= evacuationRadius;

            // Compare radius of circle with distance
            // of its center from given point
            return (
                (shelterPosition.x - disasterPosition.x) * (shelterPosition.x - disasterPosition.x) +
                (shelterPosition.z - disasterPosition.z) * (shelterPosition.z - disasterPosition.z) <= evacuationRadius * evacuationRadius
            );
        }

        //Boolean IsShelterInDestructionZone(Vector3 disasterPosition, Vector3 shelterPosition, float evacuationRadius)
        //{
        //    DebugLogger.Log($"EvacuationRadius: {evacuationRadius}");
        //    //First Squared Is required for correct calculation
        //    evacuationRadius *= evacuationRadius;
        //    DebugLogger.Log($"EvacuationRadius Squared: {evacuationRadius}");

        //    var formula = (shelterPosition.x - disasterPosition.x) * (shelterPosition.x - disasterPosition.x) +
        //        (shelterPosition.z - disasterPosition.z) * (shelterPosition.z - disasterPosition.z);
        //    var erSqr2 = evacuationRadius * evacuationRadius;

        //    DebugLogger.Log($"Formula: {formula}");
        //    DebugLogger.Log($"ErSqr: {erSqr2}");

        //    // Compare radius of circle with distance
        //    // of its center from given point
        //    return (
        //        (shelterPosition.x - disasterPosition.x) * (shelterPosition.x - disasterPosition.x) +
        //        (shelterPosition.z - disasterPosition.z) * (shelterPosition.z - disasterPosition.z) <= erSqr2
        //    );
        //}

    }
}