using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Setup;
using UnityEngine;

namespace NaturalDisastersRenewal.Serialization
{
    public class SerializableDataDisasterSetup : IDataContainer
    {
        public void Serialize(DataSerializer s)
        {
            DisasterSetupModel c = Singleton<NaturalDisasterHandler>.instance.container;
            s.WriteBool(c.ScaleMaxIntensityWithPopulation);
            s.WriteBool(c.RecordDisasterEvents);
            s.WriteBool(c.ShowDisasterPanelButton);

            s.WriteBool(c.DisableDisasterFocus);
            s.WriteBool(c.PauseOnDisasterStarts);
            s.WriteFloat(c.PartialEvacuationRadius);

            s.WriteFloat(c.ToggleButtonPos.x);
            s.WriteFloat(c.ToggleButtonPos.y);

            s.WriteFloat(c.DPanelPos.x);
            s.WriteFloat(c.DPanelPos.y);
        }

        public void Deserialize(DataSerializer s)
        {
            DisasterSetupModel c = Singleton<NaturalDisasterHandler>.instance.container;
            c.ScaleMaxIntensityWithPopulation = s.ReadBool();
            c.RecordDisasterEvents = s.ReadBool();
            c.ShowDisasterPanelButton = s.ReadBool();

            c.PauseOnDisasterStarts = s.ReadBool();
            c.DisableDisasterFocus = s.ReadBool();
            c.PartialEvacuationRadius = s.ReadFloat();

            if (s.version >= 1)
            {
                c.ToggleButtonPos = new Vector3(s.ReadFloat(), s.ReadFloat());
                c.DPanelPos = new Vector3(s.ReadFloat(), s.ReadFloat());
            }
        }

        public void AfterDeserialize(DataSerializer s)
        {
            Singleton<NaturalDisasterHandler>.instance.UpdateDisastersPanelToggleBtn();
            Singleton<NaturalDisasterHandler>.instance.UpdateDisastersDPanel();
        }
    }
}