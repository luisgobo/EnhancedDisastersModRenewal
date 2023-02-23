using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataMeteorStrike : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer s)
        {
            MeteorStrikeModel d = Singleton<NaturalDisasterHandler>.instance.container.MeteorStrike;
            SerializeCommonParameters(s, d);

            for (int i = 0; i < d.meteorEvents.Length; i++)
            {
                s.WriteBool(d.meteorEvents[i].Enabled);
                s.WriteFloat(d.meteorEvents[i].PeriodDays);
                s.WriteInt8(d.meteorEvents[i].MaxIntensity);
                s.WriteFloat(d.meteorEvents[i].DaysUntilNextEvent);
                s.WriteInt32(d.meteorEvents[i].MeteorsFallen);
            }
        }

        public void Deserialize(DataSerializer s)
        {
            MeteorStrikeModel d = Singleton<NaturalDisasterHandler>.instance.container.MeteorStrike;
            DeserializeCommonParameters(s, d);

            if (s.version <= 2)
            {
                float daysPerFrame = Helper.DaysPerFrame;
                for (int i = 0; i < d.meteorEvents.Length; i++)
                {
                    d.meteorEvents[i].Enabled = s.ReadBool();
                    d.meteorEvents[i].PeriodDays = s.ReadInt32() * daysPerFrame;
                    d.meteorEvents[i].MaxIntensity = (byte)s.ReadInt8();
                    d.meteorEvents[i].DaysUntilNextEvent = s.ReadInt32() * daysPerFrame;
                    d.meteorEvents[i].MeteorsFallen = s.ReadInt32();
                }
            }
            else
            {
                for (int i = 0; i < d.meteorEvents.Length; i++)
                {
                    d.meteorEvents[i].Enabled = s.ReadBool();
                    d.meteorEvents[i].PeriodDays = s.ReadFloat();
                    d.meteorEvents[i].MaxIntensity = (byte)s.ReadInt8();
                    d.meteorEvents[i].DaysUntilNextEvent = s.ReadFloat();
                    d.meteorEvents[i].MeteorsFallen = s.ReadInt32();
                }
            }
        }

        public void AfterDeserialize(DataSerializer s)
        {
            AfterDeserializeLog("EnhancedMeteorStrike");
        }
    }
}