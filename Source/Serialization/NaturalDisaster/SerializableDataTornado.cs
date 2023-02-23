using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataTornado : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer s)
        {
            TornadoModel d = Singleton<NaturalDisasterHandler>.instance.container.Tornado;
            SerializeCommonParameters(s, d);
            s.WriteInt32(d.MaxProbabilityMonth);
            s.WriteBool(d.NoTornadoDuringFog);
        }

        public void Deserialize(DataSerializer s)
        {
            TornadoModel d = Singleton<NaturalDisasterHandler>.instance.container.Tornado;
            DeserializeCommonParameters(s, d);

            if (s.version >= 3)
            {
                d.MaxProbabilityMonth = s.ReadInt32();
            }
            d.NoTornadoDuringFog = s.ReadBool();
        }

        public void AfterDeserialize(DataSerializer s)
        {
            AfterDeserializeLog("Tornado");
        }
    }
}