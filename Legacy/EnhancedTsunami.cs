using ColossalFramework;
using ColossalFramework.IO;
using ICities;

namespace EnhancedDisastersMod
{
    public class EnhancedTsunami : EnhancedDisaster
    {
        public class Data : SerializableDataCommon, IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                EnhancedTsunami d = Singleton<EnhancedDisastersManager>.instance.container.Tsunami;
                serializeCommonParameters(s, d);

                s.WriteFloat(d.WarmupYears);
            }

            public void Deserialize(DataSerializer s)
            {
                EnhancedTsunami d = Singleton<EnhancedDisastersManager>.instance.container.Tsunami;
                deserializeCommonParameters(s, d);

                d.WarmupYears = s.ReadFloat();
            }

            public void AfterDeserialize(DataSerializer s)
            {
                afterDeserializeLog("Tsunami");
            }
        }

        public EnhancedTsunami()
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

        public override void CopySettings(EnhancedDisaster disaster)
        {
            base.CopySettings(disaster);

            EnhancedTsunami d = disaster as EnhancedTsunami;
            if (d != null)
            {
                WarmupYears = d.WarmupYears;
            }
        }
    }
}