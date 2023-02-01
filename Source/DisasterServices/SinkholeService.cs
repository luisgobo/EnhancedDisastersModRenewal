using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Serialization;

namespace NaturalDisastersRenewal.DisasterServices
{
    public class SinkholeService : DisasterSerialization
    {
        public class Data : SerializableDataCommon, IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                SinkholeService d = Singleton<NaturalDisasterManager>.instance.container.Sinkhole;
                SerializeCommonParameters(s, d);
                s.WriteFloat(d.GroundwaterCapacity);
                s.WriteFloat(d.groundwaterAmount);
            }

            public void Deserialize(DataSerializer s)
            {
                SinkholeService d = Singleton<NaturalDisasterManager>.instance.container.Sinkhole;
                DeserializeCommonParameters(s, d);
                d.GroundwaterCapacity = s.ReadFloat();
                d.groundwaterAmount = s.ReadFloat();
            }

            public void AfterDeserialize(DataSerializer s)
            {
                AfterDeserializeLog("Sinkhole");
            }
        }

        public float GroundwaterCapacity = 50;
        float groundwaterAmount = 0; // groundwaterAmount=1 means rain of intensity 1 during 1 day

        public SinkholeService()
        {
            DType = DisasterType.Sinkhole;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
            BaseOccurrencePerYear = 1.5f; // When groundwater is full
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            calmDays = 30;
            probabilityWarmupDays = 0;
            intensityWarmupDays = 0;
            EvacuationMode = 0;
        }

        public override string GetProbabilityTooltip()
        {
            if (!unlocked)
            {
                return "Not unlocked yet";
            }

            if (calmDaysLeft <= 0)
            {
                int groundWaterPercent = (int)(100 * groundwaterAmount / GroundwaterCapacity);
                return "Ground water level " + groundWaterPercent.ToString() + "%";
            }

            return base.GetProbabilityTooltip();
        }

        public override void OnDisasterStarted(byte intensity)
        {
            groundwaterAmount = 0;

            base.OnDisasterStarted(intensity);
        }

        public override void OnDisasterDetected(DisasterSettings disasterInfo, ushort disasterID)
        {
            disasterInfo.type = DisasterType.Sinkhole;
            base.OnDisasterDetected(disasterInfo,disasterID);
        }

        protected override void OnSimulationFrameLocal()
        {
            float daysPerFrame = Helper.DaysPerFrame;

            WeatherManager wm = Singleton<WeatherManager>.instance;
            if (wm.m_currentRain > 0)
            {
                groundwaterAmount += wm.m_currentRain * daysPerFrame;
            }

            groundwaterAmount -= (groundwaterAmount / GroundwaterCapacity) * daysPerFrame;

            if (groundwaterAmount < 0)
            {
                groundwaterAmount = 0;
            }
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            return base.GetCurrentOccurrencePerYearLocal() * groundwaterAmount / GroundwaterCapacity;
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as SinkholeAI != null;
        }

        public override string GetName()
        {
            return "Sinkhole";
        }

        public override void CopySettings(DisasterSerialization disaster)
        {
            base.CopySettings(disaster);

            SinkholeService d = disaster as SinkholeService;
            if (d != null)
            {
                GroundwaterCapacity = d.GroundwaterCapacity;
            }
        }
    }
}