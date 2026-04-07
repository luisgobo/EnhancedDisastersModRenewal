using ColossalFramework.IO;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataSinkhole : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            var sinkhole = CommonServices.DisasterSetup.Sinkhole;
            SerializeCommonParameters(dataSerializer, sinkhole);
            dataSerializer.WriteFloat(sinkhole.GroundwaterCapacity);
            dataSerializer.WriteFloat(sinkhole.groundwaterAmount);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            var sinkhole = CommonServices.DisasterSetup.Sinkhole;
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