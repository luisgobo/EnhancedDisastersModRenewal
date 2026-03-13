using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataForestFire : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            var forestFire = CommonServices.DisasterSetup.ForestFire;
            SerializeCommonParameters(dataSerializer, forestFire);
            dataSerializer.WriteInt32(forestFire.WarmupDays);
            dataSerializer.WriteFloat(forestFire.NoRainDays);
        }

        public void Deserialize(DataSerializer dataSeralizer)
        {
            var forestFire = CommonServices.DisasterSetup.ForestFire;
            DeserializeCommonParameters(dataSeralizer, forestFire, 2);
            forestFire.WarmupDays = dataSeralizer.ReadInt32();
            if (dataSeralizer.version <= 2)
            {
                float daysPerFrame = Helper.DaysPerFrame;
                forestFire.NoRainDays = dataSeralizer.ReadInt32() * daysPerFrame;
            }
            else
            {
                forestFire.NoRainDays = dataSeralizer.ReadFloat();
            }
        }

        public void AfterDeserialize(DataSerializer dataSeralizer)
        {
            AfterDeserializeLog("ForestFireModel");
        }
    }
}