using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataTornado : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            TornadoModel tornado = Singleton<NaturalDisasterHandler>.instance.container.Tornado;
            SerializeCommonParameters(dataSerializer, tornado);
            dataSerializer.WriteInt32(tornado.MaxProbabilityMonth);
            dataSerializer.WriteBool(tornado.NoTornadoDuringFog);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            TornadoModel tornado = Singleton<NaturalDisasterHandler>.instance.container.Tornado;
            DeserializeCommonParameters(dataSerializer, tornado);

            if (dataSerializer.version >= 3)
            {
                tornado.MaxProbabilityMonth = dataSerializer.ReadInt32();
            }
            tornado.NoTornadoDuringFog = dataSerializer.ReadBool();
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            AfterDeserializeLog("TornadoModel");
        }
    }
}