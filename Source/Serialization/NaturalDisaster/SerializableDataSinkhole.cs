using ColossalFramework;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
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
            dataSerializer.WriteInt32((int)sinkhole.RealTimeSinkholeFrequency);
            dataSerializer.WriteFloat(sinkhole.RealTimeMinutesUntilNextSinkhole);
            dataSerializer.WriteFloat(sinkhole.RealTimeCurrentWetPeriodMinutes);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            SinkholeModel sinkhole = Services.DisasterSetup.Sinkhole;
            DeserializeCommonParameters(dataSerializer, sinkhole);
            sinkhole.GroundwaterCapacity = dataSerializer.ReadFloat();
            sinkhole.groundwaterAmount = dataSerializer.ReadFloat();

            if (dataSerializer.version < 13) return;

            sinkhole.RealTimeSinkholeFrequency =
                (RealTimeDisasterFrequencyPreset)dataSerializer.ReadInt32();
            sinkhole.RealTimeMinutesUntilNextSinkhole = dataSerializer.ReadFloat();
            sinkhole.RealTimeCurrentWetPeriodMinutes = dataSerializer.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            AfterDeserializeLog("SinkholeModel");
        }
    }
}
