using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataThunderstorm : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            ThunderstormModel thunderstorm = Singleton<NaturalDisasterHandler>.instance.container.Thunderstorm;
            SerializeCommonParameters(dataSerializer, thunderstorm);
            dataSerializer.WriteInt32(thunderstorm.MaxProbabilityMonth);
            dataSerializer.WriteFloat(thunderstorm.RainFactor);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            ThunderstormModel thunderstorm = Singleton<NaturalDisasterHandler>.instance.container.Thunderstorm;
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