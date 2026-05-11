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
            ForestFireModel forestFire = Services.DisasterSetup.ForestFire;
            SerializeCommonParameters(dataSerializer, forestFire);
            dataSerializer.WriteInt32(forestFire.WarmupDays);
            dataSerializer.WriteFloat(forestFire.noRainDays);
        }

        public void Deserialize(DataSerializer dataSeralizer)
        {
            ForestFireModel forestFire = Services.DisasterSetup.ForestFire;
            // TODO: Forest Fire only exposes Manual/Focused evacuation in the UI; normalize the save/load mapping so FocusedAutoEvacuation is not multiplied into an invalid enum value.
            DeserializeCommonParameters(dataSeralizer, forestFire, 2);
            forestFire.WarmupDays = dataSeralizer.ReadInt32();
            if (dataSeralizer.version <= 2)
            {
                float daysPerFrame = DisasterSimulationUtils.DaysPerFrame;
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
