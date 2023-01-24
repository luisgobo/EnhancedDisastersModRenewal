using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.IO;
using UnityEngine;

namespace EnhancedDisastersMod
{
    public class DisastersContainer
    {
        public class Data : IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                DisastersContainer c = Singleton<EnhancedDisastersManager>.instance.container;
                s.WriteBool(c.ScaleMaxIntensityWithPopilation);
                s.WriteBool(c.RecordDisasterEvents);
                s.WriteBool(c.ShowDisasterPanelButton);
                s.WriteFloat(c.ToggleButtonPos.x);
                s.WriteFloat(c.ToggleButtonPos.y);
            }

            public void Deserialize(DataSerializer s)
            {
                DisastersContainer c = Singleton<EnhancedDisastersManager>.instance.container;
                c.ScaleMaxIntensityWithPopilation = s.ReadBool();
                c.RecordDisasterEvents = s.ReadBool();
                c.ShowDisasterPanelButton = s.ReadBool();

                if (s.version >= 1)
                {
                    c.ToggleButtonPos = new Vector3(s.ReadFloat(), s.ReadFloat());
                }
            }

            public void AfterDeserialize(DataSerializer s)
            {
                Singleton<EnhancedDisastersManager>.instance.UpdateDisastersPanelToggleBtn();
            }
        }


        private static string optionsFileName = "EnhancedDisastersModOptions.xml";

        public EnhancedForestFire ForestFire;
        public EnhancedThunderstorm Thunderstorm;
        public EnhancedSinkhole Sinkhole;
        public EnhancedTsunami Tsunami;
        public EnhancedTornado Tornado;
        public EnhancedEarthquake Earthquake;
        public EnhancedMeteorStrike MeteorStrike;

        public bool ScaleMaxIntensityWithPopilation = true;
        public bool RecordDisasterEvents = false;
        public bool ShowDisasterPanelButton = true;
        public Vector3 ToggleButtonPos = new Vector3(90, 62);

        [XmlIgnore]
        public List<EnhancedDisaster> AllDisasters = new List<EnhancedDisaster>();

        public void Save()
        {
            XmlSerializer ser = new XmlSerializer(typeof(DisastersContainer));
            TextWriter writer = new StreamWriter(getOptionsFilePath());
            ser.Serialize(writer, this);
            writer.Close();
        }

        public void CheckObjects()
        {
            if (ForestFire == null) ForestFire = new EnhancedForestFire();
            if (Thunderstorm == null) Thunderstorm = new EnhancedThunderstorm();
            if (Sinkhole == null) Sinkhole = new EnhancedSinkhole();
            if (Tsunami == null) Tsunami = new EnhancedTsunami();
            if (Tornado == null) Tornado = new EnhancedTornado();
            if (Earthquake == null) Earthquake = new EnhancedEarthquake();
            if (MeteorStrike == null) MeteorStrike = new EnhancedMeteorStrike();

            AllDisasters.Clear();
            AllDisasters.Add(ForestFire);
            AllDisasters.Add(Thunderstorm);
            AllDisasters.Add(Sinkhole);
            AllDisasters.Add(Tsunami);
            AllDisasters.Add(Tornado);
            AllDisasters.Add(Earthquake);
            AllDisasters.Add(MeteorStrike);
        }

        public static DisastersContainer CreateFromFile()
        {
            string path = getOptionsFilePath();

            if (!File.Exists(path)) return null;

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(DisastersContainer));
                TextReader reader = new StreamReader(path);
                DisastersContainer instance = (DisastersContainer)ser.Deserialize(reader);
                reader.Close();

                instance.CheckObjects();

                return instance;
            }
            catch
            {
                return null;
            }
        }

        private static string getOptionsFilePath()
        {
            //return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Colossal Order", "Cities_Skylines", optionsFileName);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, "Colossal Order");
            path = Path.Combine(path, "Cities_Skylines");
            path = Path.Combine(path, optionsFileName);
            return path;
        }
    }
}
