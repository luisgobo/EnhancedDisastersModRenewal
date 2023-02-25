using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataForestFire : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            ForestFireModel forestFire = Singleton<NaturalDisasterHandler>.instance.container.ForestFire;
            SerializeCommonParameters(dataSerializer, forestFire);
            dataSerializer.WriteInt32(forestFire.WarmupDays);
            dataSerializer.WriteFloat(forestFire.noRainDays);
        }

        public void Deserialize(DataSerializer dataSeralizer)
        {
            ForestFireModel forestFire = Singleton<NaturalDisasterHandler>.instance.container.ForestFire;
            DeserializeCommonParameters(dataSeralizer, forestFire, 2);
            forestFire.WarmupDays = dataSeralizer.ReadInt32();
            if (dataSeralizer.version <= 2)
            {
                float daysPerFrame = Helper.DaysPerFrame;
                forestFire.noRainDays = dataSeralizer.ReadInt32() * daysPerFrame;
            }
            else
            {
                forestFire.noRainDays = dataSeralizer.ReadFloat();
            }
        }

        public void AfterDeserialize(DataSerializer dataSeralizer)
        {
            AfterDeserializeLog("ForestFireModel");
        }
    }
}