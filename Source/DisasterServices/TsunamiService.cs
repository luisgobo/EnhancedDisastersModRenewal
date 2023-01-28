using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Serialization;

namespace NaturalDisastersRenewal.DisasterServices
{
    public class TsunamiService : DisasterSerialization
    {
        public class Data : SerializableDataCommon, IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                TsunamiService d = Singleton<DisasterManager>.instance.container.Tsunami;
                serializeCommonParameters(s, d);

                s.WriteFloat(d.WarmupYears);
            }

            public void Deserialize(DataSerializer s)
            {
                TsunamiService d = Singleton<DisasterManager>.instance.container.Tsunami;
                deserializeCommonParameters(s, d);

                d.WarmupYears = s.ReadFloat();
            }

            public void AfterDeserialize(DataSerializer s)
            {
                afterDeserializeLog("Tsunami");
            }
        }

        public TsunamiService()
        {
            DType = DisasterType.Tsunami;
            BaseOccurrencePerYear = 1.0f;
            ProbabilityDistribution = ProbabilityDistributions.PowerLow;

            WarmupYears = 4;
            EvacuationMode = 0;
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

        public override void CopySettings(DisasterSerialization disaster)
        {
            base.CopySettings(disaster);

            TsunamiService d = disaster as TsunamiService;
            if (d != null)
            {
                WarmupYears = d.WarmupYears;
            }
        }
    }
}