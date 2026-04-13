using ColossalFramework;
using NaturalDisastersRenewal.Common;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Setup;
using UnityEngine;

namespace NaturalDisastersRenewal.Serialization.Setup
{
    public class SerializableDataDisasterSetup : IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            DisasterSetupModel disasterSetupmodel = Services.DisasterSetup;

            dataSerializer.WriteBool(disasterSetupmodel.ScaleMaxIntensityWithPopulation);
            dataSerializer.WriteBool(disasterSetupmodel.RecordDisasterEvents);
            dataSerializer.WriteBool(disasterSetupmodel.ShowDisasterPanelButton);

            dataSerializer.WriteBool(disasterSetupmodel.DisableDisasterFocus);
            dataSerializer.WriteBool(disasterSetupmodel.PauseOnDisasterStarts);
            dataSerializer.WriteFloat(disasterSetupmodel.PartialEvacuationRadius);
            dataSerializer.WriteFloat(disasterSetupmodel.MaxPopulationToTrigguerHigherDisasters);
            dataSerializer.WriteInt32((int)disasterSetupmodel.Language);

            dataSerializer.WriteFloat(disasterSetupmodel.ToggleButtonPos.x);
            dataSerializer.WriteFloat(disasterSetupmodel.ToggleButtonPos.y);

            dataSerializer.WriteFloat(disasterSetupmodel.DPanelPos.x);
            dataSerializer.WriteFloat(disasterSetupmodel.DPanelPos.y);

            dataSerializer.WriteInt32((int)disasterSetupmodel.TogglePanelHotkey);
            dataSerializer.WriteInt32((int)disasterSetupmodel.TogglePanelHotkeyModifiers);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            DisasterSetupModel disasterSetupmodel = Services.DisasterSetup;

            disasterSetupmodel.ScaleMaxIntensityWithPopulation = dataSerializer.ReadBool();
            disasterSetupmodel.RecordDisasterEvents = dataSerializer.ReadBool();
            disasterSetupmodel.ShowDisasterPanelButton = dataSerializer.ReadBool();

            disasterSetupmodel.DisableDisasterFocus = dataSerializer.ReadBool();
            disasterSetupmodel.PauseOnDisasterStarts = dataSerializer.ReadBool();
            disasterSetupmodel.PartialEvacuationRadius = dataSerializer.ReadFloat();
            disasterSetupmodel.MaxPopulationToTrigguerHigherDisasters = dataSerializer.ReadFloat();

            if (dataSerializer.version >= 4)
            {
                disasterSetupmodel.Language = (ModLanguage)dataSerializer.ReadInt32();
            }
            else
            {
                disasterSetupmodel.Language = ModLanguage.English;
            }

            if (dataSerializer.version >= 1)
            {
                disasterSetupmodel.ToggleButtonPos = new Vector3(dataSerializer.ReadFloat(), dataSerializer.ReadFloat());
                disasterSetupmodel.DPanelPos = new Vector3(dataSerializer.ReadFloat(), dataSerializer.ReadFloat());
            }

            if (dataSerializer.version >= 5)
            {
                disasterSetupmodel.TogglePanelHotkey = (KeyCode)dataSerializer.ReadInt32();
                disasterSetupmodel.TogglePanelHotkeyModifiers = HotkeyHelper.GetSupportedHotkeyModifiers(
                    (EventModifiers)dataSerializer.ReadInt32());
            }
        }

        public void AfterDeserialize(DataSerializer dataSerializer)
        {
            Services.DisasterHandler.UpdateDisastersPanelToggleBtn();
            Services.DisasterHandler.UpdateDisastersDPanel();
        }
    }
}
