using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Disaster;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using UnityEngine;

namespace NaturalDisastersRenewal.Models.Setup
{
    public class DisasterSetupModel
    {
        [XmlIgnore] public readonly List<DisasterBaseModel> AllDisasters = new List<DisasterBaseModel>();

        //Disaster list
        //[XmlIgnore] //for now it's needed to read it when on load and save game
        public List<DisasterInfoModel> ActiveDisasters = new List<DisasterInfoModel>();

        //General options
        public bool DisableDisasterFocus = true;
        public Vector3 DPanelPos = new Vector3(90, 40);
        public EarthquakeModel Earthquake;
        public ForestFireModel ForestFire;
        public ModLanguage Language = ModLanguage.English;
        public float MaxPopulationToTriggerHigherDisasters = 200000;
        public MeteorStrikeModel MeteorStrike;
        public float PartialEvacuationRadius = 900f;

        public bool PauseOnDisasterStarts = false;
        public bool RecordDisasterEvents = false;

        public bool ScaleMaxIntensityWithPopulation = false;
        public bool ShowDisasterPanelButton = true;
        public SinkholeModel Sinkhole;
        public ThunderstormModel Thunderstorm;
        public Vector3 ToggleButtonPos = new Vector3(90, 62);
        [XmlIgnore] public KeyCode TogglePanelHotkey = KeyCode.D;
        [XmlIgnore] public EventModifiers TogglePanelHotkeyModifiers = EventModifiers.Shift;
        public TornadoModel Tornado;
        public TsunamiModel Tsunami;

        [XmlElement("TogglePanelHotkey")]
        public string TogglePanelHotkeySerialized
        {
            get => TogglePanelHotkey.ToString();
            set
            {
                if (string.IsNullOrEmpty(value) || value.Trim().Length == 0)
                {
                    TogglePanelHotkey = KeyCode.D;
                    return;
                }

                try
                {
                    TogglePanelHotkey = (KeyCode)Enum.Parse(typeof(KeyCode), value, true);
                }
                catch
                {
                    TogglePanelHotkey = KeyCode.D;
                }
            }
        }

        [XmlElement("TogglePanelHotkeyModifiers")]
        public string TogglePanelHotkeyModifiersSerialized
        {
            get => TogglePanelHotkeyModifiers.ToString();
            set
            {
                if (string.IsNullOrEmpty(value) || value.Trim().Length == 0)
                {
                    TogglePanelHotkeyModifiers = EventModifiers.Shift;
                    return;
                }

                try
                {
                    TogglePanelHotkeyModifiers = HotkeyHelper.GetSupportedHotkeyModifiers(
                        (EventModifiers)Enum.Parse(typeof(EventModifiers), value, true));
                }
                catch
                {
                    TogglePanelHotkeyModifiers = EventModifiers.Shift;
                }
            }
        }

        public void Save()
        {
            var ser = new XmlSerializer(typeof(DisasterSetupModel));
            TextWriter writer = new StreamWriter(CommonProperties.GetOptionsFilePath(CommonProperties.XmlFilename));
            ser.Serialize(writer, this);
            writer.Close();
        }

        public void CheckObjects()
        {
            if (ForestFire == null) ForestFire = new ForestFireModel();
            if (Thunderstorm == null) Thunderstorm = new ThunderstormModel();
            if (Sinkhole == null) Sinkhole = new SinkholeModel();
            if (Tsunami == null) Tsunami = new TsunamiModel();
            if (Tornado == null) Tornado = new TornadoModel();
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

        public static DisasterSetupModel CreateFromFile()
        {
            var path = CommonProperties.GetOptionsFilePath(CommonProperties.XmlFilename);

            if (!File.Exists(path)) return null;

            try
            {
                var ser = new XmlSerializer(typeof(DisasterSetupModel));
                TextReader reader = new StreamReader(path);
                var instance = (DisasterSetupModel)ser.Deserialize(reader);
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