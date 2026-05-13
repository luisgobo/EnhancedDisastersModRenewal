using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataMeteorStrike : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            MeteorStrikeModel meteorStrike = Services.DisasterSetup.MeteorStrike;
            SerializeCommonParameters(dataSerializer, meteorStrike);
            dataSerializer.WriteFloat(meteorStrike.RealTimeFrequencyMultiplier);
            dataSerializer.WriteFloat(meteorStrike.RealTimePeriodDays);
            dataSerializer.WriteFloat(meteorStrike.RealTimeDaysUntilNextMeteor);
            dataSerializer.WriteInt32((int)meteorStrike.RealTimeMeteorFrequency);
            dataSerializer.WriteFloat(meteorStrike.RealTimeMinutesUntilNextMeteor);
            dataSerializer.WriteFloat(meteorStrike.RealTimeCurrentPeriodMinutes);

            for (int i = 0; i < meteorStrike.MeteorEvents.Length; i++)
            {
                dataSerializer.WriteBool(meteorStrike.MeteorEvents[i].Enabled);
                dataSerializer.WriteFloat(meteorStrike.MeteorEvents[i].PeriodDays);                
                dataSerializer.WriteInt16(meteorStrike.MeteorEvents[i].MaxIntensity);
                dataSerializer.WriteFloat(meteorStrike.MeteorEvents[i].DaysUntilNextEvent);
                dataSerializer.WriteInt32(meteorStrike.MeteorEvents[i].MeteorsFallen);
            }
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            MeteorStrikeModel meteorStrike = Services.DisasterSetup.MeteorStrike;
            DeserializeCommonParameters(dataSerializer, meteorStrike);

            if (dataSerializer.version >= 6)
                meteorStrike.RealTimeFrequencyMultiplier = dataSerializer.ReadFloat();

            if (dataSerializer.version >= 8)
                meteorStrike.RealTimePeriodDays = dataSerializer.ReadFloat();

            if (dataSerializer.version >= 7)
                meteorStrike.RealTimeDaysUntilNextMeteor = dataSerializer.ReadFloat();

            if (dataSerializer.version >= 9)
            {
                meteorStrike.RealTimeMeteorFrequency =
                    (RealTimeDisasterFrequencyPreset)dataSerializer.ReadInt32();
                meteorStrike.RealTimeMinutesUntilNextMeteor = dataSerializer.ReadFloat();
                meteorStrike.RealTimeCurrentPeriodMinutes = dataSerializer.ReadFloat();
            }

            if (dataSerializer.version <= 2)
            {
                float daysPerFrame = DisasterSimulationUtils.VanillaSimulationDaysPerFrame;
                for (int i = 0; i < meteorStrike.MeteorEvents.Length; i++)
                {
                    meteorStrike.MeteorEvents[i].Enabled = dataSerializer.ReadBool();
                    meteorStrike.MeteorEvents[i].PeriodDays = dataSerializer.ReadInt32() * daysPerFrame;
                    meteorStrike.MeteorEvents[i].MaxIntensity = (byte)dataSerializer.ReadInt16();
                    meteorStrike.MeteorEvents[i].DaysUntilNextEvent = dataSerializer.ReadInt32() * daysPerFrame;
                    meteorStrike.MeteorEvents[i].MeteorsFallen = dataSerializer.ReadInt32();
                }
            }
            else
            {
                for (int i = 0; i < meteorStrike.MeteorEvents.Length; i++)
                {
                    meteorStrike.MeteorEvents[i].Enabled = dataSerializer.ReadBool();
                    meteorStrike.MeteorEvents[i].PeriodDays = dataSerializer.ReadFloat();
                    meteorStrike.MeteorEvents[i].MaxIntensity = (byte)dataSerializer.ReadInt16();
                    meteorStrike.MeteorEvents[i].DaysUntilNextEvent = dataSerializer.ReadFloat();
                    meteorStrike.MeteorEvents[i].MeteorsFallen = dataSerializer.ReadInt32();
                }
            }
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            AfterDeserializeLog("MeteorStrikeModel");
        }
    }
}
