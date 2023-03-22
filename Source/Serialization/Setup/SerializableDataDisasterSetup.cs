using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Setup;
using UnityEngine;

namespace NaturalDisastersRenewal.Serialization.Setup
{
    public class SerializableDataDisasterSetup : IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            DisasterSetupModel disasterSetupmodel = Singleton<NaturalDisasterHandler>.instance.container;

            dataSerializer.WriteBool(disasterSetupmodel.ScaleMaxIntensityWithPopulation);
            dataSerializer.WriteBool(disasterSetupmodel.RecordDisasterEvents);
            dataSerializer.WriteBool(disasterSetupmodel.ShowDisasterPanelButton);

            dataSerializer.WriteBool(disasterSetupmodel.DisableDisasterFocus);
            dataSerializer.WriteBool(disasterSetupmodel.PauseOnDisasterStarts);
            dataSerializer.WriteFloat(disasterSetupmodel.PartialEvacuationRadius);

            dataSerializer.WriteFloat(disasterSetupmodel.ToggleButtonPos.x);
            dataSerializer.WriteFloat(disasterSetupmodel.ToggleButtonPos.y);

            dataSerializer.WriteFloat(disasterSetupmodel.DPanelPos.x);
            dataSerializer.WriteFloat(disasterSetupmodel.DPanelPos.y);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            DisasterSetupModel disasterSetupmodel = Singleton<NaturalDisasterHandler>.instance.container;

            disasterSetupmodel.ScaleMaxIntensityWithPopulation = dataSerializer.ReadBool();
            disasterSetupmodel.RecordDisasterEvents = dataSerializer.ReadBool();
            disasterSetupmodel.ShowDisasterPanelButton = dataSerializer.ReadBool();

            disasterSetupmodel.DisableDisasterFocus = dataSerializer.ReadBool();
            disasterSetupmodel.PauseOnDisasterStarts = dataSerializer.ReadBool();
            disasterSetupmodel.PartialEvacuationRadius = dataSerializer.ReadFloat();

            if (dataSerializer.version >= 1)
            {
                disasterSetupmodel.ToggleButtonPos = new Vector3(dataSerializer.ReadFloat(), dataSerializer.ReadFloat());
                disasterSetupmodel.DPanelPos = new Vector3(dataSerializer.ReadFloat(), dataSerializer.ReadFloat());
            }
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            Singleton<NaturalDisasterHandler>.instance.UpdateDisastersPanelToggleBtn();
            Singleton<NaturalDisasterHandler>.instance.UpdateDisastersDPanel();
        }
    }
}