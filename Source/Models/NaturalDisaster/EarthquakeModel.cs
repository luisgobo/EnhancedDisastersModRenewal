using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class EarthquakeModel : DisasterBaseModel
    {
        public bool AftershocksEnabled = true;
        public EarthquakeCrackOptions EarthquakeCrackMode = EarthquakeCrackOptions.NoCracks;
        public float MinimalIntensityForCracks = 12.0f;

        [XmlIgnore] bool NoCracksInTheGroud = false;
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

            //setup Cracks based on intensity and game setup
            SetupCracksOnMap(disasterInfoUnified.DisasterInfo.intensity);

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

        public override float CalculateDestructionRadio(byte intensity)
        {
            int unitSize = 8;
            int unitsBase = 30; //24 Original, Distance Fix for proximity
            float unitCalculation;
            int intensityInt = intensity / 10;
            int intensityDec = intensity % 10;

            switch (intensity)
            {
                case byte n when (n < 25):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + (((intensityDec) * 0.24f) + (intensityInt * 10.4f));
                    break;

                case byte n when (n >= 25 && n <= 50):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 22f + (((intensityDec - 2f) * 11.6f) + ((intensityInt - 5f) * 0.36f));
                    break;

                case byte n when (n > 50 && n <= 75):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 55f + ((intensityInt - 5f) * 8f);
                    break;

                case byte n when (n > 75 && n <= 100):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 71f + +(((intensityInt - 7f) * 10.8f) + (((intensityDec - 5f) * 0.28f)));
                    break;

                case byte n when (n > 100 && n <= 125):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 102f + (((intensityInt - 10f) * 11.2f) + ((intensityDec * 0.32f)));
                    break;

                case byte n when (n > 125 && n <= 150):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 126f + ((intensityInt - 12f) * 8f);
                    break;

                case byte n when (n > 150 && n <= 175):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 150 + (intensityDec * 0.2f) + ((intensityInt - 15f) * 10f);
                    break;

                case byte n when (n > 175 && n <= 200):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 171 + ((intensityDec - 5) * 0.12f + ((intensityInt - 17) * 9.2f));
                    break;

                case byte n when (n > 200 && n <= 250):
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 198 + ((intensityDec * 0.2f) + ((intensityInt - 20f) * 10f));
                    break;

                default:
                    unitCalculation = ((((intensityInt - 5f) * -10f) + intensityDec) * 0.4f) + unitsBase + 248;
                    break;
            }

            return (float)Math.Sqrt((unitCalculation / 2) * unitSize);
        }

        public override void SetupAutomaticEvacuation(DisasterInfoModel disasterInfoModel, ref List<DisasterInfoModel> activeDisasters)
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
                        
                        bool IgnoreDestructionZoneForEarthquake;
                        switch (EarthquakeCrackMode)
                        {
                            case EarthquakeCrackOptions.NoCracks:
                                IgnoreDestructionZoneForEarthquake = true;
                                break;

                            case EarthquakeCrackOptions.AlwaysCracks:
                                IgnoreDestructionZoneForEarthquake = false;
                                break;

                            case EarthquakeCrackOptions eci when (eci == EarthquakeCrackOptions.CracksBasedOnIntensity && disasterInfoModel.DisasterInfo.intensity >= MinimalIntensityForCracks):
                                IgnoreDestructionZoneForEarthquake = false;
                                break;

                            default:
                                IgnoreDestructionZoneForEarthquake = true;
                                break;
                        }

                        //if Shelter will be destroyed, don't evacuate
                        if (base.IsShelterInDisasterZone(disasterTargetPosition, shelterPosition, shelterRadius, disasterDestructionRadius) && !IgnoreDestructionZoneForEarthquake)
                            DebugLogger.Log($"Shelter is located in Destruction Zone. Won't be avacuated");
                        else
                            base.SetBuidingEvacuationStatus(buildingInfo.Info.m_buildingAI as ShelterAI, num, ref buildingManager.m_buildings.m_buffer[num], false);
                    }
                }
            }

            activeDisasters.Add(disasterInfoModel);

        }

        void SetupCracksOnMap(byte intensity)
        {
            switch (EarthquakeCrackMode)
            {
                case EarthquakeCrackOptions.NoCracks:
                    NoCracksInTheGroud = true;
                    break;

                case EarthquakeCrackOptions.AlwaysCracks:
                    NoCracksInTheGroud = false;
                    break;

                case EarthquakeCrackOptions ecp when (ecp == EarthquakeCrackOptions.CracksBasedOnIntensity && intensity >= MinimalIntensityForCracks * 10):
                    NoCracksInTheGroud = false;
                    break;

                default:
                    NoCracksInTheGroud = true;
                    break;
            }

            UpdateDisasterProperties(true);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            if (disaster is EarthquakeModel earthquake)
            {
                AftershocksEnabled = earthquake.AftershocksEnabled;
                WarmupYears = earthquake.WarmupYears;
            }
        }

        public void UpdateDisasterProperties(bool isSet)
        {
            int prefabsCount = PrefabCollection<DisasterInfo>.PrefabCount();

            for (uint i = 0; i < prefabsCount; i++)
            {
                DisasterInfo disasterInfo = PrefabCollection<DisasterInfo>.GetPrefab(i);
                if (disasterInfo == null) continue;

                if (disasterInfo.m_disasterAI as EarthquakeAI != null)
                {                    
                    if (isSet && NoCracksInTheGroud)
                    {
                        ((EarthquakeAI)disasterInfo.m_disasterAI).m_crackLength = 0;
                        ((EarthquakeAI)disasterInfo.m_disasterAI).m_crackWidth = 0;
                    }
                    else
                    {
                        ((EarthquakeAI)disasterInfo.m_disasterAI).m_crackLength = 1000;
                        ((EarthquakeAI)disasterInfo.m_disasterAI).m_crackWidth = 100;
                    }
                }
            }
        }
    }
}