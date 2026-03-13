using ColossalFramework.IO;
using UnityEngine;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Serialization.Setup
{
    public class SerializableDataDisasterSetup : IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            var disasterSetupModel = CommonServices.DisasterSetup;

            dataSerializer.WriteBool(disasterSetupModel.ScaleMaxIntensityWithPopulation);
            dataSerializer.WriteBool(disasterSetupModel.RecordDisasterEvents);
            dataSerializer.WriteBool(disasterSetupModel.ShowDisasterPanelButton);

            dataSerializer.WriteBool(disasterSetupModel.DisableDisasterFocus);
            dataSerializer.WriteBool(disasterSetupModel.PauseOnDisasterStarts);
            dataSerializer.WriteFloat(disasterSetupModel.PartialEvacuationRadius);
            dataSerializer.WriteFloat(disasterSetupModel.MaxPopulationToTriggerHigherDisasters);

            dataSerializer.WriteFloat(disasterSetupModel.ToggleButtonPos.x);
            dataSerializer.WriteFloat(disasterSetupModel.ToggleButtonPos.y);

            dataSerializer.WriteFloat(disasterSetupModel.DPanelPos.x);
            dataSerializer.WriteFloat(disasterSetupModel.DPanelPos.y);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            var disasterSetupmodel = CommonServices.DisasterSetup;

            disasterSetupmodel.ScaleMaxIntensityWithPopulation = dataSerializer.ReadBool();
            disasterSetupmodel.RecordDisasterEvents = dataSerializer.ReadBool();
            disasterSetupmodel.ShowDisasterPanelButton = dataSerializer.ReadBool();

            disasterSetupmodel.DisableDisasterFocus = dataSerializer.ReadBool();
            disasterSetupmodel.PauseOnDisasterStarts = dataSerializer.ReadBool();
            disasterSetupmodel.PartialEvacuationRadius = dataSerializer.ReadFloat();
            disasterSetupmodel.MaxPopulationToTriggerHigherDisasters = dataSerializer.ReadFloat();

            if (dataSerializer.version >= 1)
            {
                disasterSetupmodel.ToggleButtonPos = new Vector3(dataSerializer.ReadFloat(), dataSerializer.ReadFloat());
                disasterSetupmodel.DPanelPos = new Vector3(dataSerializer.ReadFloat(), dataSerializer.ReadFloat());
            }
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            CommonServices.DisasterHandler.UpdateDisastersPanelToggleBtn();
            CommonServices.DisasterHandler.UpdateDisastersDPanel();
        }
    }
}