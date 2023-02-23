using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataSinkhole : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer s)
        {
            SinkholeModel d = Singleton<NaturalDisasterHandler>.instance.container.Sinkhole;
            SerializeCommonParameters(s, d);
            s.WriteFloat(d.GroundwaterCapacity);
            s.WriteFloat(d.groundwaterAmount);
        }

        public void Deserialize(DataSerializer s)
        {
            SinkholeModel d = Singleton<NaturalDisasterHandler>.instance.container.Sinkhole;
            DeserializeCommonParameters(s, d);
            d.GroundwaterCapacity = s.ReadFloat();
            d.groundwaterAmount = s.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer s)
        {
            AfterDeserializeLog("Sinkhole");
        }
    }
}