using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.NaturalDisaster;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataForestFire : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            var forestFire = Services.DisasterSetup.ForestFire;
            SerializeCommonParameters(dataSerializer, forestFire);
            dataSerializer.WriteInt32(forestFire.WarmupDays);
            dataSerializer.WriteFloat(forestFire.NoRainDays);
            dataSerializer.WriteInt32((int)forestFire.RealTimeForestFireFrequency);
            dataSerializer.WriteFloat(forestFire.RealTimeMinutesUntilNextForestFire);
            dataSerializer.WriteFloat(forestFire.RealTimeCurrentDryPeriodMinutes);
            dataSerializer.WriteBool(forestFire.FogRetardsDryTime);
        }

        public void Deserialize(DataSerializer dataSeralizer)
        {
            var forestFire = Services.DisasterSetup.ForestFire;
            DeserializeCommonParameters(dataSeralizer, forestFire);
            NormalizeForestFireEvacuationMode(forestFire);
            forestFire.WarmupDays = dataSeralizer.ReadInt32();
            if (dataSeralizer.version <= 2)
            {
                float daysPerFrame = DisasterSimulationUtils.DaysPerFrame;
                forestFire.NoRainDays = dataSeralizer.ReadInt32() * daysPerFrame;
            }
            else
            {
                forestFire.NoRainDays = dataSeralizer.ReadFloat();
            }

            if (dataSeralizer.version < 10) return;
            
            forestFire.RealTimeForestFireFrequency =
                (RealTimeDisasterFrequencyPreset)dataSeralizer.ReadInt32();
            forestFire.RealTimeMinutesUntilNextForestFire = dataSeralizer.ReadFloat();
            forestFire.RealTimeCurrentDryPeriodMinutes = dataSeralizer.ReadFloat();

            if (dataSeralizer.version < 11) return;

            forestFire.FogRetardsDryTime = dataSeralizer.ReadBool();
        }

        private static void NormalizeForestFireEvacuationMode(ForestFireModel forestFire)
        {
            if (forestFire.EvacuationMode == EvacuationOptions.ManualEvacuation)
                return;

            forestFire.EvacuationMode = EvacuationOptions.FocusedAutoEvacuation;
        }

        public void AfterDeserialize(DataSerializer dataSeralizer)
        {
            AfterDeserializeLog("ForestFireModel");
        }
    }
}
