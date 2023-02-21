using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.Models;
using NaturalDisastersRenewal.Services.Handlers;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace NaturalDisastersRenewal.Services.NaturalDisaster
{
    public class EarthquakeModel : DisasterBaseModel
    {
        public bool AftershocksEnabled = true;        
        public EarthquakeCrackOptions EarthquakeCrackMode = EarthquakeCrackOptions.NoCracks;
        
        [XmlIgnore] public byte aftershocksCount = 0;
        [XmlIgnore] public byte aftershockMaxIntensity = 0;
        [XmlIgnore] public byte mainStrikeIntensity = 0;
        [XmlIgnore] public Vector3 lastTargetPosition = new Vector3();
        [XmlIgnore] public float lastAngle = 0;

        public EarthquakeModel()
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

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            if (aftershocksCount > 0)
            {
                return 12 * aftershocksCount;
            }

            return base.GetCurrentOccurrencePerYearLocal();
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.Earthquake;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Earthquake;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Earthquake;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
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

                Debug.Log(string.Format(CommonProperties.LogMsgPrefix + "{0} aftershocks are still going to happen.", aftershocksCount));
            }
            else
            {
                base.OnDisasterStarted(mainStrikeIntensity);
            }
        }

        protected override bool FindTarget(DisasterInfo disasterInfo, out Vector3 targetPosition, out float angle)
        {
            if (aftershocksCount == 0)
            {
                bool result = base.FindTarget(disasterInfo, out targetPosition, out angle);
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

        protected override byte GetRandomIntensity(byte maxIntensity)
        {
            if (aftershocksCount > 0)
            {
                return (byte)Singleton<SimulationManager>.instance.m_randomizer.Int32(10, aftershockMaxIntensity);
            }
            else
            {
                return base.GetRandomIntensity(maxIntensity);
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

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            EarthquakeModel d = disaster as EarthquakeModel;
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
                    DebugLogger.Log($"Eartquake Found");
                    if (isSet)
                    {
                        DebugLogger.Log($"IsSet: {isSet}");
                        switch (EarthquakeCrackMode)
                        {
                            case EarthquakeCrackOptions.NoCracks:
                                DebugLogger.Log($"NoCracks");
                                ((EarthquakeAI)di.m_disasterAI).m_crackLength = 0;
                                ((EarthquakeAI)di.m_disasterAI).m_crackWidth = 0;
                                break;
                            case EarthquakeCrackOptions.AlwaysCracks:
                                DebugLogger.Log($"AlwaysCracks");
                                ((EarthquakeAI)di.m_disasterAI).m_crackLength = 1000;
                                ((EarthquakeAI)di.m_disasterAI).m_crackWidth = 100;
                                break;                            
                            default:
                                DebugLogger.Log($"CracksBasedOnIntensity");
                                break;
                        }
                    }
                    //if (isSet && EarthquakeCrackMode == EarthquakeCrackOptions.NoCracks)
                    //{
                    //    ((EarthquakeAI)di.m_disasterAI).m_crackLength = 0;
                    //    ((EarthquakeAI)di.m_disasterAI).m_crackWidth = 0;
                    //}
                    //else
                    //{
                    //    ((EarthquakeAI)di.m_disasterAI).m_crackLength = 1000;
                    //    ((EarthquakeAI)di.m_disasterAI).m_crackWidth = 100;
                    //}
                }
            }
        }
    }
}