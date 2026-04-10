using ColossalFramework;
using NaturalDisastersRenewal.Common;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataTsunami : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            TsunamiModel tsunami = Services.DisasterSetup.Tsunami;
            SerializeCommonParameters(dataSerializer, tsunami);

            dataSerializer.WriteFloat(tsunami.WarmupYears);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            TsunamiModel tsunami = Services.DisasterSetup.Tsunami;
            DeserializeCommonParameters(dataSerializer, tsunami);

            tsunami.WarmupYears = dataSerializer.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            AfterDeserializeLog("TsunamiModel");
        }
    }
}