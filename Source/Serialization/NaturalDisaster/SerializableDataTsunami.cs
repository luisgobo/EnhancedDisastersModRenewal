using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataTsunami : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer s)
        {
            TsunamiModel d = Singleton<NaturalDisasterHandler>.instance.container.Tsunami;
            SerializeCommonParameters(s, d);

            s.WriteFloat(d.WarmupYears);
        }

        public void Deserialize(DataSerializer s)
        {
            TsunamiModel d = Singleton<NaturalDisasterHandler>.instance.container.Tsunami;
            DeserializeCommonParameters(s, d);

            d.WarmupYears = s.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer s)
        {
            AfterDeserializeLog("Tsunami");
        }
    }
}