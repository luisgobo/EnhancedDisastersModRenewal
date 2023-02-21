using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Services.Handlers;
using NaturalDisastersRenewal.Services.NaturalDisaster;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace NaturalDisastersRenewal.Services.Setup
{
    public class DisasterSetupService
    {
        public class Data : IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                DisasterSetupService c = Singleton<NaturalDisasterHandler>.instance.container;
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
                DisasterSetupService c = Singleton<NaturalDisasterHandler>.instance.container;
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

        public ForestFireService ForestFire;
        public ThunderstormService Thunderstorm;
        public SinkholeModel Sinkhole;
        public TsunamiService Tsunami;
        public TornadoService Tornado;
        public EarthquakeModel Earthquake;
        public MeteorStrikeModel MeteorStrike;

        //General options
        public bool DisableDisasterFocus = true;
        public bool PauseOnDisasterStarts = false;
        public float PartialEvacuationRadius = 900f;

        public bool ScaleMaxIntensityWithPopulation = true;
        public bool RecordDisasterEvents = false;
        public bool ShowDisasterPanelButton = true;
        public Vector3 ToggleButtonPos = new Vector3(90, 62);
        public Vector3 DPanelPos = new Vector3(90, 40);

        [XmlIgnore]
        public List<DisasterBaseModel> AllDisasters = new List<DisasterBaseModel>();

        public void Save()
        {
            XmlSerializer ser = new XmlSerializer(typeof(DisasterSetupService));
            TextWriter writer = new StreamWriter(CommonProperties.GetOptionsFilePath());
            ser.Serialize(writer, this);
            writer.Close();
        }

        public void CheckObjects()
        {
            if (ForestFire == null) ForestFire = new ForestFireService();
            if (Thunderstorm == null) Thunderstorm = new ThunderstormService();
            if (Sinkhole == null) Sinkhole = new SinkholeModel();
            if (Tsunami == null) Tsunami = new TsunamiService();
            if (Tornado == null) Tornado = new TornadoService();
            if (Earthquake == null) Earthquake = new EarthquakeModel();
            if (MeteorStrike == null) MeteorStrike = new MeteorStrikeModel();

            AllDisasters.Clear();
            AllDisasters.Add(ForestFire);
            AllDisasters.Add(Thunderstorm);
            AllDisasters.Add(Sinkhole);
            AllDisasters.Add(Tsunami);
            AllDisasters.Add(Tornado);
            AllDisasters.Add(Earthquake);
            AllDisasters.Add(MeteorStrike);
        }

        public static DisasterSetupService CreateFromFile()
        {
            string path = CommonProperties.GetOptionsFilePath();

            if (!File.Exists(path)) return null;

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(DisasterSetupService));
                TextReader reader = new StreamReader(path);
                DisasterSetupService instance = (DisasterSetupService)ser.Deserialize(reader);
                reader.Close();

                instance.CheckObjects();

                return instance;
            }
            catch
            {
                return null;
            }
        }
    }
}