using System;
using ICities;
using ColossalFramework;
using ColossalFramework.IO;
using UnityEngine;

namespace NaturalDisastersOverhaulRenewal
{
    public abstract class EnhancedDisaster
    {
        public class SerializableDataCommon
        {
            public void serializeCommonParameters(DataSerializer s, EnhancedDisaster disaster)
            {
                s.WriteBool(disaster.Enabled);
                s.WriteFloat(disaster.BaseOccurrencePerYear);
                s.WriteFloat(disaster.calmDaysLeft);
                s.WriteFloat(disaster.probabilityWarmupDaysLeft);
                s.WriteFloat(disaster.intensityWarmupDaysLeft);
            }

            public void deserializeCommonParameters(DataSerializer s, EnhancedDisaster disaster)
            {
                disaster.Enabled = s.ReadBool();
                disaster.BaseOccurrencePerYear = s.ReadFloat();
                if (s.version <= 2)
                {
                    float daysPerFrame = 1f / 585f;
                    disaster.calmDaysLeft = s.ReadInt32() * daysPerFrame;
                    disaster.probabilityWarmupDaysLeft = s.ReadInt32() * daysPerFrame;
                    disaster.intensityWarmupDaysLeft = s.ReadInt32() * daysPerFrame;
                }
                else
                {
                    disaster.calmDaysLeft = s.ReadFloat();
                    disaster.probabilityWarmupDaysLeft = s.ReadFloat();
                    disaster.intensityWarmupDaysLeft = s.ReadFloat();
                }
            }

            public void afterDeserializeLog(string className)
            {
                Debug.Log(Mod.LogMsgPrefix + className + " data loaded.");
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
        public float BaseOccurrencePerYear = 1.0f;


        // Public

        public abstract string GetName();

        public float GetCurrentOccurrencePerYear()
        {
            if (calmDaysLeft > 0)
            {
                return 0f;
            }

            return scaleProbabilityByWarmup(getCurrentOccurrencePerYear_local());
        }

        public virtual byte GetMaximumIntensity()
        {
            byte intensity = 100;

            intensity = scaleIntensityByWarmup(intensity);

            intensity = scaleIntensityByPopulation(intensity);

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

            onSimulationFrame_local();

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
                byte intensity = getRandomIntensity(GetMaximumIntensity());

                startDisaster(intensity);
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

        public virtual void CopySettings(EnhancedDisaster disaster)
        {
            Enabled = disaster.Enabled;
            BaseOccurrencePerYear = disaster.BaseOccurrencePerYear;
        }


        // Utilities

        protected virtual float getCurrentOccurrencePerYear_local()
        {
            return BaseOccurrencePerYear;
        }

        protected virtual byte getRandomIntensity(byte maxIntensity)
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

        protected byte scaleIntensityByWarmup(byte intensity)
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

        protected byte scaleIntensityByPopulation(byte intensity)
        {
            if (Singleton<EnhancedDisastersManager>.instance.container.ScaleMaxIntensityWithPopilation)
            {
                int population = Helper.GetPopulation();
                if (population < FullIntensityPopulation)
                {
                    intensity = (byte)(10 + (intensity - 10) * population / FullIntensityPopulation);
                }
            }

            return intensity;
        }

        private float scaleProbabilityByWarmup(float probability)
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

        protected string getDebugStr()
        {
            return DType.ToString() + ", " + Singleton<SimulationManager>.instance.m_currentGameTime.ToShortDateString() + ", ";
        }


        // Disaster

        protected virtual void onSimulationFrame_local()
        {

        }

        public virtual void OnDisasterStarted(byte intensity)
        {
            float framesPerDay = Helper.FramesPerDay;

            calmDaysLeft = calmDays * intensity / 100; // TO DO: May be define minimum calmDays
            probabilityWarmupDaysLeft = probabilityWarmupDays;
            intensityWarmupDaysLeft = intensityWarmupDays;
        }

        protected virtual void disasterStarting(DisasterInfo disasterInfo)
        {

        }

        public abstract bool CheckDisasterAIType(object disasterAI);

        protected void startDisaster(byte intensity)
        {
            DisasterInfo disasterInfo = EnhancedDisastersManager.GetDisasterInfo(DType);

            if (disasterInfo == null)
            {
                return;
            }

            Vector3 targetPosition;
            float angle;
            bool targetFound = findTarget(disasterInfo, out targetPosition, out angle);

            if (!targetFound)
            {
                DebugLogger.Log(getDebugStr() + "target not found");
                return;
            }

            DisasterManager dm = Singleton<DisasterManager>.instance;

            ushort disasterIndex;
            bool disasterCreated = dm.CreateDisaster(out disasterIndex, disasterInfo);
            if (!disasterCreated)
            {
                DebugLogger.Log(getDebugStr() + "could not create disaster");
                return;
            }

            DisasterLogger.StartedByMod = true;

            disasterStarting(disasterInfo);

            dm.m_disasters.m_buffer[(int)disasterIndex].m_targetPosition = targetPosition;
            dm.m_disasters.m_buffer[(int)disasterIndex].m_angle = angle;
            dm.m_disasters.m_buffer[(int)disasterIndex].m_intensity = intensity;
            DisasterData[] expr_98_cp_0 = dm.m_disasters.m_buffer;
            ushort expr_98_cp_1 = disasterIndex;
            expr_98_cp_0[(int)expr_98_cp_1].m_flags = (expr_98_cp_0[(int)expr_98_cp_1].m_flags | DisasterData.Flags.SelfTrigger);
            disasterInfo.m_disasterAI.StartNow(disasterIndex, ref dm.m_disasters.m_buffer[(int)disasterIndex]);

            DebugLogger.Log(getDebugStr() + string.Format("disaster intensity: {0}, area: {1}", intensity, unlocked ? OccurrenceAreaAfterUnlock : OccurrenceAreaBeforeUnlock));
        }

        protected virtual bool findTarget(DisasterInfo disasterInfo, out Vector3 targetPosition, out float angle)
        {
            OccurrenceAreas area = unlocked ? OccurrenceAreaAfterUnlock : OccurrenceAreaBeforeUnlock;
            switch (area)
            {
                case OccurrenceAreas.LockedAreas:
                    return findRandomTargetInLockedAreas(out targetPosition, out angle);
                case OccurrenceAreas.Everywhere:
                    return findRandomTargetEverywhere(out targetPosition, out angle);
                case OccurrenceAreas.UnlockedAreas: // Vanilla default
                    return disasterInfo.m_disasterAI.FindRandomTarget(out targetPosition, out angle);
                default:
                    targetPosition = new Vector3();
                    angle = 0;
                    return false;
            }
        }

        private bool findRandomTargetEverywhere(out Vector3 target, out float angle)
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

        private bool findRandomTargetInLockedAreas(out Vector3 target, out float angle)
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
                    if (isUnlocked(i, j))
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
                        if (isUnlocked(j - 1, i))
                        {
                            minX += minimumEdgeDistance;
                        }
                        if (isUnlocked(j, i - 1))
                        {
                            minZ += minimumEdgeDistance;
                        }
                        if (isUnlocked(j + 1, i))
                        {
                            maxX -= minimumEdgeDistance;
                        }
                        if (isUnlocked(j, i + 1))
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

        private bool isUnlocked(int x, int z)
        {
            return x >= 0 && z >= 0 && x < 5 && z < 5 && Singleton<GameAreaManager>.instance.m_areaGrid[z * 5 + x] != 0;
        }
    }
}
