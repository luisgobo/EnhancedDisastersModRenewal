using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataTsunami : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            TsunamiModel tsunami = Singleton<NaturalDisasterHandler>.instance.container.Tsunami;
            SerializeCommonParameters(dataSerializer, tsunami);

            dataSerializer.WriteFloat(tsunami.WarmupYears);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            TsunamiModel tsunami = Singleton<NaturalDisasterHandler>.instance.container.Tsunami;
            DeserializeCommonParameters(dataSerializer, tsunami);

            tsunami.WarmupYears = dataSerializer.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            AfterDeserializeLog("TsunamiModel");
        }
    }
}