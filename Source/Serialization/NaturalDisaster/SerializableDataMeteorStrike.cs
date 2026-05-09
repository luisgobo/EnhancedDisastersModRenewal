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
            dataSerializer.WriteFloat(meteorStrike.realTimeDaysUntilNextMeteor);
            dataSerializer.WriteInt32((int)meteorStrike.RealTimeMeteorFrequency);
            dataSerializer.WriteFloat(meteorStrike.realTimeMinutesUntilNextMeteor);
            dataSerializer.WriteFloat(meteorStrike.realTimeCurrentPeriodMinutes);

            for (int i = 0; i < meteorStrike.meteorEvents.Length; i++)
            {
                dataSerializer.WriteBool(meteorStrike.meteorEvents[i].Enabled);
                dataSerializer.WriteFloat(meteorStrike.meteorEvents[i].PeriodDays);                
                dataSerializer.WriteInt16(meteorStrike.meteorEvents[i].MaxIntensity);
                dataSerializer.WriteFloat(meteorStrike.meteorEvents[i].DaysUntilNextEvent);
                dataSerializer.WriteInt32(meteorStrike.meteorEvents[i].MeteorsFallen);
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
                meteorStrike.realTimeDaysUntilNextMeteor = dataSerializer.ReadFloat();

            if (dataSerializer.version >= 9)
            {
                meteorStrike.RealTimeMeteorFrequency =
                    (RealTimeMeteorFrequencyPreset)dataSerializer.ReadInt32();
                meteorStrike.realTimeMinutesUntilNextMeteor = dataSerializer.ReadFloat();
                meteorStrike.realTimeCurrentPeriodMinutes = dataSerializer.ReadFloat();
            }

            if (dataSerializer.version <= 2)
            {
                float daysPerFrame = DisasterSimulationUtils.VanillaSimulationDaysPerFrame;
                for (int i = 0; i < meteorStrike.meteorEvents.Length; i++)
                {
                    meteorStrike.meteorEvents[i].Enabled = dataSerializer.ReadBool();
                    meteorStrike.meteorEvents[i].PeriodDays = dataSerializer.ReadInt32() * daysPerFrame;
                    meteorStrike.meteorEvents[i].MaxIntensity = (byte)dataSerializer.ReadInt16();
                    meteorStrike.meteorEvents[i].DaysUntilNextEvent = dataSerializer.ReadInt32() * daysPerFrame;
                    meteorStrike.meteorEvents[i].MeteorsFallen = dataSerializer.ReadInt32();
                }
            }
            else
            {
                for (int i = 0; i < meteorStrike.meteorEvents.Length; i++)
                {
                    meteorStrike.meteorEvents[i].Enabled = dataSerializer.ReadBool();
                    meteorStrike.meteorEvents[i].PeriodDays = dataSerializer.ReadFloat();
                    meteorStrike.meteorEvents[i].MaxIntensity = (byte)dataSerializer.ReadInt16();
                    meteorStrike.meteorEvents[i].DaysUntilNextEvent = dataSerializer.ReadFloat();
                    meteorStrike.meteorEvents[i].MeteorsFallen = dataSerializer.ReadInt32();
                }
            }
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            AfterDeserializeLog("MeteorStrikeModel");
        }
    }
}
