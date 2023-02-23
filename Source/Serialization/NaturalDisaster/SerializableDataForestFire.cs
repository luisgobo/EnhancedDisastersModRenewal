using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataForestFire : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer s)
        {
            ForestFireModel d = Singleton<NaturalDisasterHandler>.instance.container.ForestFire;
            SerializeCommonParameters(s, d);
            s.WriteInt32(d.WarmupDays);
            s.WriteFloat(d.noRainDays);
        }

        public void Deserialize(DataSerializer s)
        {
            ForestFireModel d = Singleton<NaturalDisasterHandler>.instance.container.ForestFire;
            DeserializeCommonParameters(s, d);
            d.WarmupDays = s.ReadInt32();
            if (s.version <= 2)
            {
                float daysPerFrame = Helper.DaysPerFrame;
                d.noRainDays = s.ReadInt32() * daysPerFrame;
            }
            else
            {
                d.noRainDays = s.ReadFloat();
            }
        }

        public void AfterDeserialize(DataSerializer s)
        {
            AfterDeserializeLog("ForestFire");
        }
    }
}