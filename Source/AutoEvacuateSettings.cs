using ColossalFramework.IO;
using ColossalFramework;

namespace NaturalDisastersOverhaulRenewal
{
    public class AutoEvacuateSettings
    {
        public class Data
        {
            public void Serialize(DataSerializer dataSerializer)
            {
                AutoEvacuateSettings disastersContainer = Singleton<EnhancedDisastersManager>.instance.container.AutoEvacuateSettings;

                dataSerializer.WriteInt32(disastersContainer.AutoEvacuateEarthquake);
                dataSerializer.WriteInt32(disastersContainer.AutoEvacuateForestFire);
                dataSerializer.WriteInt32(disastersContainer.AutoEvacuateMeteorStrike);
                dataSerializer.WriteInt32(disastersContainer.AutoEvacuateSinkhole);
                dataSerializer.WriteInt32(disastersContainer.AutoEvacuateStructureCollapse);
                dataSerializer.WriteInt32(disastersContainer.AutoEvacuateStructureFire);
                dataSerializer.WriteInt32(disastersContainer.AutoEvacuateThunderstorm);
                dataSerializer.WriteInt32(disastersContainer.AutoEvacuateTornado);
                dataSerializer.WriteInt32(disastersContainer.AutoEvacuateTsunami);

            }

            public void Deserialize(DataSerializer dataSerializer)
            {
                AutoEvacuateSettings disastersContainer = Singleton<EnhancedDisastersManager>.instance.container.AutoEvacuateSettings;

                disastersContainer.AutoEvacuateEarthquake = dataSerializer.ReadInt32();
                disastersContainer.AutoEvacuateForestFire = dataSerializer.ReadInt32();
                disastersContainer.AutoEvacuateMeteorStrike = dataSerializer.ReadInt32();
                disastersContainer.AutoEvacuateSinkhole = dataSerializer.ReadInt32();
                disastersContainer.AutoEvacuateStructureCollapse = dataSerializer.ReadInt32();
                disastersContainer.AutoEvacuateStructureFire = dataSerializer.ReadInt32();
                disastersContainer.AutoEvacuateThunderstorm = dataSerializer.ReadInt32();
                disastersContainer.AutoEvacuateTornado = dataSerializer.ReadInt32();
                disastersContainer.AutoEvacuateTsunami = dataSerializer.ReadInt32();

            }

            public void AfterDeserialize(DataSerializer dataSerializer)
            {
                //Singleton<EnhancedDisastersManager>.instance.UpdateDisastersPanelToggleBtn();
            }
        }

        //Autoevacuate values
        public int AutoEvacuateEarthquake = 0;
        public int AutoEvacuateForestFire = 0;
        public int AutoEvacuateMeteorStrike = 0;
        public int AutoEvacuateSinkhole = 0;
        public int AutoEvacuateStructureCollapse = 0;
        public int AutoEvacuateStructureFire = 0;
        public int AutoEvacuateThunderstorm = 0;
        public int AutoEvacuateTornado = 0;
        public int AutoEvacuateTsunami = 0;

    }
}
