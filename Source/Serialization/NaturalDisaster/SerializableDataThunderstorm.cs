using ColossalFramework;
using NaturalDisastersRenewal.Common;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataThunderstorm : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            ThunderstormModel thunderstorm = Services.DisasterSetup.Thunderstorm;
            SerializeCommonParameters(dataSerializer, thunderstorm);
            dataSerializer.WriteInt32(thunderstorm.MaxProbabilityMonth);
            dataSerializer.WriteFloat(thunderstorm.RainFactor);
            dataSerializer.WriteInt32((int)thunderstorm.RealTimeThunderstormFrequency);
            dataSerializer.WriteFloat(thunderstorm.RealTimeMinutesUntilNextThunderstorm);
            dataSerializer.WriteFloat(thunderstorm.RealTimeCurrentStormPeriodMinutes);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            ThunderstormModel thunderstorm = Services.DisasterSetup.Thunderstorm;
            DeserializeCommonParameters(dataSerializer, thunderstorm);
            thunderstorm.MaxProbabilityMonth = dataSerializer.ReadInt32();
            thunderstorm.RainFactor = dataSerializer.ReadFloat();

            if (dataSerializer.version < 14) return;

            thunderstorm.RealTimeThunderstormFrequency =
                (RealTimeDisasterFrequencyPreset)dataSerializer.ReadInt32();
            thunderstorm.RealTimeMinutesUntilNextThunderstorm = dataSerializer.ReadFloat();
            thunderstorm.RealTimeCurrentStormPeriodMinutes = dataSerializer.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            AfterDeserializeLog("ThunderstormModel");
        }
    }
}
