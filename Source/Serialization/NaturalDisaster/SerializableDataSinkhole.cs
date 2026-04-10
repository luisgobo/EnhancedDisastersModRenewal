using ColossalFramework;
using NaturalDisastersRenewal.Common;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataSinkhole : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            SinkholeModel sinkhole = Services.DisasterSetup.Sinkhole;
            SerializeCommonParameters(dataSerializer, sinkhole);
            dataSerializer.WriteFloat(sinkhole.GroundwaterCapacity);
            dataSerializer.WriteFloat(sinkhole.groundwaterAmount);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            SinkholeModel sinkhole = Services.DisasterSetup.Sinkhole;
            DeserializeCommonParameters(dataSerializer, sinkhole);
            sinkhole.GroundwaterCapacity = dataSerializer.ReadFloat();
            sinkhole.groundwaterAmount = dataSerializer.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            AfterDeserializeLog("SinkholeModel");
        }
    }
}