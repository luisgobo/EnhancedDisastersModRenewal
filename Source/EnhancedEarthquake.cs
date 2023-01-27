using ICities;
using ColossalFramework;
using ColossalFramework.IO;
using UnityEngine;

namespace NaturalDisastersOverhaulRenewal
{
    public class EnhancedEarthquake : EnhancedDisaster
    {
        public class Data : SerializableDataCommon, IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                EnhancedEarthquake d = Singleton<EnhancedDisastersManager>.instance.container.Earthquake;
                serializeCommonParameters(s, d);

                s.WriteFloat(d.WarmupYears);
                s.WriteBool(d.NoCracks);

                s.WriteInt8(d.aftershocksCount);
                s.WriteInt8(d.aftershockMaxIntensity);
                s.WriteInt8(d.mainStrikeIntensity);

                s.WriteFloat(d.lastTargetPosition.x);
                s.WriteFloat(d.lastTargetPosition.y);
                s.WriteFloat(d.lastTargetPosition.z);
                s.WriteFloat(d.lastAngle);
            }

            public void Deserialize(DataSerializer s)
            {
                EnhancedEarthquake d = Singleton<EnhancedDisastersManager>.instance.container.Earthquake;
                deserializeCommonParameters(s, d);

                d.WarmupYears = s.ReadFloat();
                if (s.version >= 3)
                {
                    d.NoCracks = s.ReadBool();
                }

                d.aftershocksCount = (byte)s.ReadInt8();
                d.aftershockMaxIntensity = (byte)s.ReadInt8();
                if (s.version >= 2)
                {
                    d.mainStrikeIntensity = (byte)s.ReadInt8();
                }

                d.lastTargetPosition = new Vector3(s.ReadFloat(), s.ReadFloat(), s.ReadFloat());
                d.lastAngle = s.ReadFloat();
            }

            public void AfterDeserialize(DataSerializer s)
            {
                afterDeserializeLog("EnhancedEarthquake");
            }
        }

        public bool AftershocksEnabled = true;
        public bool NoCracks = true;
        byte aftershocksCount = 0;
        byte aftershockMaxIntensity = 0;
        byte mainStrikeIntensity = 0;
        Vector3 lastTargetPosition = new Vector3();
        float lastAngle = 0;

        public EnhancedEarthquake()
        {
            DType = DisasterType.Earthquake;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
            BaseOccurrencePerYear = 1.0f;
            ProbabilityDistribution = ProbabilityDistributions.PowerLow;

            WarmupYears = 3;
        }

        [System.Xml.Serialization.XmlElement]
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
                calmDays = probabilityWarmupDays / 2;
            }
        }

        public override string GetProbabilityTooltip()
        {
            if (aftershocksCount > 0)
            {
                return "Expect " + aftershocksCount.ToString() + " more aftershocks";
            }

            return base.GetProbabilityTooltip();
        }

        protected override float getCurrentOccurrencePerYear_local()
        {
            if (aftershocksCount > 0)
            {
                return 12 * aftershocksCount;
            }

            return base.getCurrentOccurrencePerYear_local();
        }

        public override void OnDisasterStarted(byte intensity)
        {
            if (!AftershocksEnabled)
            {
                aftershocksCount = 0;
                base.OnDisasterStarted(intensity);
                return;
            }

            if (aftershocksCount == 0)
            {
                mainStrikeIntensity = intensity;
                aftershockMaxIntensity = (byte)(10 + (intensity - 10) * 3 / 4);
                if (intensity > 20)
                {
                    aftershocksCount = (byte)(1 + Singleton<SimulationManager>.instance.m_randomizer.Int32(1 + (uint)intensity / 20));
                }
            }
            else
            {
                aftershocksCount--;
                aftershockMaxIntensity = (byte)(10 + (aftershockMaxIntensity - 10) * 3 / 4);
            }

            if (aftershocksCount > 0)
            {
                calmDays = 15;
                probabilityWarmupDaysLeft = 0;
                intensityWarmupDaysLeft = 0;

                Debug.Log(string.Format(Mod.LogMsgPrefix + "{0} aftershocks are still going to happen.", aftershocksCount));
            }
            else
            {
                base.OnDisasterStarted(mainStrikeIntensity);
            }
        }

        protected override bool findTarget(DisasterInfo disasterInfo, out Vector3 targetPosition, out float angle)
        {
            if (aftershocksCount == 0)
            {
                bool result = base.findTarget(disasterInfo, out targetPosition, out angle);
                lastTargetPosition = targetPosition;
                lastAngle = angle;
                return result;
            }
            else
            {
                targetPosition = lastTargetPosition;
                angle = lastAngle;
                return true;
            }
        }

        protected override byte getRandomIntensity(byte maxIntensity)
        {
            if (aftershocksCount > 0)
            {
                return (byte)Singleton<SimulationManager>.instance.m_randomizer.Int32(10, aftershockMaxIntensity);
            }
            else
            {
                return base.getRandomIntensity(maxIntensity);
            }
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as EarthquakeAI != null;
        }

        public override string GetName()
        {
            return "Earthquake";
        }

        public override void CopySettings(EnhancedDisaster disaster)
        {
            base.CopySettings(disaster);

            EnhancedEarthquake d = disaster as EnhancedEarthquake;
            if (d != null)
            {
                AftershocksEnabled = d.AftershocksEnabled;
                WarmupYears = d.WarmupYears;
            }
        }

        public void UpdateDisasterProperties(bool isSet)
        {
            int prefabsCount = PrefabCollection<DisasterInfo>.PrefabCount();

            for (uint i = 0; i < prefabsCount; i++)
            {
                DisasterInfo di = PrefabCollection<DisasterInfo>.GetPrefab(i);
                if (di == null) continue;

                if (di.m_disasterAI as EarthquakeAI != null)
                {
                    if (isSet && NoCracks)
                    {
                        ((EarthquakeAI)di.m_disasterAI).m_crackLength = 0;
                        ((EarthquakeAI)di.m_disasterAI).m_crackWidth = 0;
                    }
                    else
                    {
                        ((EarthquakeAI)di.m_disasterAI).m_crackLength = 1000;
                        ((EarthquakeAI)di.m_disasterAI).m_crackWidth = 100;
                    }
                }
            }
        }
    }
}
