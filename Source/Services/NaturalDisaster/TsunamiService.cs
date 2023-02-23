using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Serialization;
using NaturalDisastersRenewal.Services.Handlers;

namespace NaturalDisastersRenewal.Services.NaturalDisaster
{
    public class TsunamiService : DisasterBaseModel
    {
        public class Data : SerializableDataDisasterBase, IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                TsunamiService d = Singleton<NaturalDisasterHandler>.instance.container.Tsunami;
                SerializeCommonParameters(s, d);

                s.WriteFloat(d.WarmupYears);
            }

            public void Deserialize(DataSerializer s)
            {
                TsunamiService d = Singleton<NaturalDisasterHandler>.instance.container.Tsunami;
                DeserializeCommonParameters(s, d);

                d.WarmupYears = s.ReadFloat();
            }

            public void AfterDeserialize(DataSerializer s)
            {
                AfterDeserializeLog("Tsunami");
            }
        }

        public TsunamiService()
        {
            DType = DisasterType.Tsunami;
            BaseOccurrencePerYear = 1.0f;
            ProbabilityDistribution = ProbabilityDistributions.PowerLow;
            WarmupYears = 4;            
        }

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
                calmDays = probabilityWarmupDays;
            }
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as TsunamiAI != null;
        }

        public override string GetName()
        {
            return "Tsunami";
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            TsunamiService d = disaster as TsunamiService;
            if (d != null)
            {
                WarmupYears = d.WarmupYears;
            }
        }

        //public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified)
        //{
        //    disasterInfoUnified.DisasterInfo.type = DisasterType.Tsunami;
        //    base.OnDisasterDetected(disasterInfoUnified);
        //}
    }
}