using ColossalFramework.IO;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataTsunami : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            var tsunami = CommonServices.DisasterSetup.Tsunami;
            SerializeCommonParameters(dataSerializer, tsunami);

            dataSerializer.WriteFloat(tsunami.WarmupYears);
            dataSerializer.WriteFloat(tsunami.RealTimeProgressMultiplier);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            var tsunami = CommonServices.DisasterSetup.Tsunami;
            DeserializeCommonParameters(dataSerializer, tsunami);

            tsunami.WarmupYears = dataSerializer.ReadFloat();
            if (dataSerializer.version >= 5)
            {
                tsunami.RealTimeProgressMultiplier = dataSerializer.ReadFloat();
            }
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            AfterDeserializeLog("TsunamiModel");
        }
    }
}
