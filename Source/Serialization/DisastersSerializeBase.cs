using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.DisasterServices.LegacyStructure;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace NaturalDisastersRenewal.Serialization
{
    public class DisastersSerializeBase
    {
        public class Data : IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                DisastersSerializeBase c = Singleton<NaturalDisasterHandler>.instance.container;
                s.WriteBool(c.ScaleMaxIntensityWithPopulation);
                s.WriteBool(c.RecordDisasterEvents);
                s.WriteBool(c.ShowDisasterPanelButton);

                s.WriteBool(c.DisableDisasterFocus);
                s.WriteBool(c.PauseOnDisasterStarts);
                s.WriteFloat(c.PartialEvacuationRadius);

                s.WriteFloat(c.ToggleButtonPos.x);
                s.WriteFloat(c.ToggleButtonPos.y);
            }

            public void Deserialize(DataSerializer s)
            {
                DisastersSerializeBase c = Singleton<NaturalDisasterHandler>.instance.container;
                c.ScaleMaxIntensityWithPopulation = s.ReadBool();
                c.RecordDisasterEvents = s.ReadBool();
                c.ShowDisasterPanelButton = s.ReadBool();

                c.PauseOnDisasterStarts = s.ReadBool();
                c.DisableDisasterFocus = s.ReadBool();
                c.PartialEvacuationRadius = s.ReadFloat();

                if (s.version >= 1)
                {
                    c.ToggleButtonPos = new Vector3(s.ReadFloat(), s.ReadFloat());
                }
            }

            public void AfterDeserialize(DataSerializer s)
            {
                Singleton<NaturalDisasterHandler>.instance.UpdateDisastersPanelToggleBtn();
            }
        }

        public ForestFireService ForestFire;
        public ThunderstormService Thunderstorm;
        public SinkholeService Sinkhole;
        public TsunamiService Tsunami;
        public TornadoService Tornado;
        public EarthquakeService Earthquake;
        public MeteorStrikeService MeteorStrike;

        //General options
        public bool DisableDisasterFocus = true;
        public bool PauseOnDisasterStarts = true;
        public float PartialEvacuationRadius = 1000f;

        public bool ScaleMaxIntensityWithPopulation = true;
        public bool RecordDisasterEvents = false;
        public bool ShowDisasterPanelButton = true;
        public Vector3 ToggleButtonPos = new Vector3(90, 62);

        [XmlIgnore]
        public List<DisasterServiceBase> AllDisasters = new List<DisasterServiceBase>();

        public void Save()
        {
            XmlSerializer ser = new XmlSerializer(typeof(DisastersSerializeBase));
            TextWriter writer = new StreamWriter(CommonProperties.GetOptionsFilePath());
            ser.Serialize(writer, this);
            writer.Close();
        }

        public void CheckObjects()
        {
            if (ForestFire == null) ForestFire = new ForestFireService();
            if (Thunderstorm == null) Thunderstorm = new ThunderstormService();
            if (Sinkhole == null) Sinkhole = new SinkholeService();
            if (Tsunami == null) Tsunami = new TsunamiService();
            if (Tornado == null) Tornado = new TornadoService();
            if (Earthquake == null) Earthquake = new EarthquakeService();
            if (MeteorStrike == null) MeteorStrike = new MeteorStrikeService();

            AllDisasters.Clear();
            AllDisasters.Add(ForestFire);
            AllDisasters.Add(Thunderstorm);
            AllDisasters.Add(Sinkhole);
            AllDisasters.Add(Tsunami);
            AllDisasters.Add(Tornado);
            AllDisasters.Add(Earthquake);
            AllDisasters.Add(MeteorStrike);
        }

        public static DisastersSerializeBase CreateFromFile()
        {
            string path = CommonProperties.GetOptionsFilePath();

            if (!File.Exists(path)) return null;

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(DisastersSerializeBase));
                TextReader reader = new StreamReader(path);
                DisastersSerializeBase instance = (DisastersSerializeBase)ser.Deserialize(reader);
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