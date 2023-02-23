using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataThunderstorm : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer s)
        {
            ThunderstormModel d = Singleton<NaturalDisasterHandler>.instance.container.Thunderstorm;
            SerializeCommonParameters(s, d);
            s.WriteInt32(d.MaxProbabilityMonth);
            s.WriteFloat(d.RainFactor);
        }

        public void Deserialize(DataSerializer s)
        {
            ThunderstormModel d = Singleton<NaturalDisasterHandler>.instance.container.Thunderstorm;
            DeserializeCommonParameters(s, d);
            d.MaxProbabilityMonth = s.ReadInt32();
            d.RainFactor = s.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer s)
        {
            AfterDeserializeLog("Thunderstorm");
        }
    }
}