using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.DisasterServices;
using NaturalDisastersRenewal.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace NaturalDisastersRenewal
{
    public class DisastersServiceBase
    {
        public class Data : IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                DisastersServiceBase c = Singleton<DisasterServices.DisasterManager>.instance.container;
                s.WriteBool(c.ScaleMaxIntensityWithPopilation);
                s.WriteBool(c.RecordDisasterEvents);
                s.WriteBool(c.ShowDisasterPanelButton);

                s.WriteBool(c.AutoFocusOnDisasterStarts);
                s.WriteBool(c.PauseOnDisasterStarts);

                s.WriteFloat(c.ToggleButtonPos.x);
                s.WriteFloat(c.ToggleButtonPos.y);
            }

            public void Deserialize(DataSerializer s)
            {
                DisastersServiceBase c = Singleton<DisasterServices.DisasterManager>.instance.container;
                c.ScaleMaxIntensityWithPopilation = s.ReadBool();
                c.RecordDisasterEvents = s.ReadBool();
                c.ShowDisasterPanelButton = s.ReadBool();

                c.PauseOnDisasterStarts = s.ReadBool();
                c.AutoFocusOnDisasterStarts = s.ReadBool();

                if (s.version >= 1)
                {
                    c.ToggleButtonPos = new Vector3(s.ReadFloat(), s.ReadFloat());
                }
            }

            public void AfterDeserialize(DataSerializer s)
            {
                Singleton<DisasterServices.DisasterManager>.instance.UpdateDisastersPanelToggleBtn();
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
        public bool AutoFocusOnDisasterStarts = true;

        public bool PauseOnDisasterStarts = true;

        public bool ScaleMaxIntensityWithPopilation = true;
        public bool RecordDisasterEvents = false;
        public bool ShowDisasterPanelButton = true;
        public Vector3 ToggleButtonPos = new Vector3(90, 62);

        [XmlIgnore]
        public List<DisasterSerialization> AllDisasters = new List<DisasterSerialization>();

        public void Save()
        {
            XmlSerializer ser = new XmlSerializer(typeof(DisastersServiceBase));
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

        public static DisastersServiceBase CreateFromFile()
        {
            string path = CommonProperties.GetOptionsFilePath();

            if (!File.Exists(path)) return null;

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(DisastersServiceBase));
                TextReader reader = new StreamReader(path);
                DisastersServiceBase instance = (DisastersServiceBase)ser.Deserialize(reader);
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