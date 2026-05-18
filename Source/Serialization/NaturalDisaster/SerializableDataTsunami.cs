using ColossalFramework;
using NaturalDisastersRenewal.Common;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataTsunami : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            TsunamiModel tsunami = Services.DisasterSetup.Tsunami;
            SerializeCommonParameters(dataSerializer, tsunami);

            dataSerializer.WriteFloat(tsunami.WarmupYears);
            dataSerializer.WriteInt32((int)tsunami.RealTimeTsunamiFrequency);
            dataSerializer.WriteFloat(tsunami.RealTimeCurrentTsunamiPeriodMinutes);
            dataSerializer.WriteFloat(tsunami.RealTimeMinutesUntilNextTsunami);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            TsunamiModel tsunami = Services.DisasterSetup.Tsunami;
            DeserializeCommonParameters(dataSerializer, tsunami);

            tsunami.WarmupYears = dataSerializer.ReadFloat();

            if (dataSerializer.version >= 17)
            {
                tsunami.RealTimeTsunamiFrequency =
                    (RealTimeDisasterFrequencyPreset)dataSerializer.ReadInt32();
                tsunami.RealTimeCurrentTsunamiPeriodMinutes = dataSerializer.ReadFloat();
                tsunami.RealTimeMinutesUntilNextTsunami = dataSerializer.ReadFloat();
            }
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            AfterDeserializeLog("TsunamiModel");
        }
    }
}
