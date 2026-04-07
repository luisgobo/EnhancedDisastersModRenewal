using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataMeteorStrike : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            var meteorStrike = CommonServices.DisasterSetup.MeteorStrike;
            SerializeCommonParameters(dataSerializer, meteorStrike);
            dataSerializer.WriteFloat(meteorStrike.RealTimeFrequencyMultiplier);

            for (var i = 0; i < meteorStrike.MeteorEvents.Length; i++)
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
            var meteorStrike = CommonServices.DisasterSetup.MeteorStrike;
            DeserializeCommonParameters(dataSerializer, meteorStrike);
            if (dataSerializer.version >= 4)
            {
                meteorStrike.RealTimeFrequencyMultiplier = dataSerializer.ReadFloat();
            }

            if (dataSerializer.version <= 2)
            {
                float daysPerFrame = Helper.DaysPerFrame;
                for (var i = 0; i < meteorStrike.MeteorEvents.Length; i++)
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
                for (var i = 0; i < meteorStrike.MeteorEvents.Length; i++)
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
