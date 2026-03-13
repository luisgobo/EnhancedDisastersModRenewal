using ColossalFramework.IO;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataThunderstorm : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            var thunderstorm = CommonServices.DisasterSetup.Thunderstorm;
            SerializeCommonParameters(dataSerializer, thunderstorm);
            dataSerializer.WriteInt32(thunderstorm.MaxProbabilityMonth);
            dataSerializer.WriteFloat(thunderstorm.RainFactor);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            var thunderstorm = CommonServices.DisasterSetup.Thunderstorm;
            DeserializeCommonParameters(dataSerializer, thunderstorm);
            thunderstorm.MaxProbabilityMonth = dataSerializer.ReadInt32();
            thunderstorm.RainFactor = dataSerializer.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            AfterDeserializeLog("ThunderstormModel");
        }
    }
}