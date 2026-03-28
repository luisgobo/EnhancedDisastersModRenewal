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
        public ForestFireModel ForestFire;
        public ThunderstormModel Thunderstorm;
        public SinkholeModel Sinkhole;
        public TsunamiModel Tsunami;
        public TornadoModel Tornado;
        public EarthquakeModel Earthquake;
        public MeteorStrikeModel MeteorStrike;

        //General options
        public bool DisableDisasterFocus = true;

        public bool PauseOnDisasterStarts = false;
        public float PartialEvacuationRadius = 900f;
        public float MaxPopulationToTriggerHigherDisasters = 200000;

        public bool ScaleMaxIntensityWithPopulation = true;
        public bool RecordDisasterEvents = false;
        public bool ShowDisasterPanelButton = true;
        public ModLanguage Language = ModLanguage.English;
        public KeyCode TogglePanelHotkey = KeyCode.D;
        public EventModifiers TogglePanelHotkeyModifiers = EventModifiers.Shift;
        public Vector3 ToggleButtonPos = new Vector3(90, 62);
        public Vector3 DPanelPos = new Vector3(90, 40);

        //Disaster list
        //[XmlIgnore] //for now it's needed to read it when on load and save game
        public List<DisasterInfoModel> ActiveDisasters = new List<DisasterInfoModel>();

        [XmlIgnore] public readonly List<DisasterBaseModel> DisasterList = new List<DisasterBaseModel>();

        public void Save()
        {
            XmlSerializer ser = new XmlSerializer(typeof(DisasterSetupModel));
            using (TextWriter writer = new StreamWriter(CommonProperties.GetOptionsFilePath(CommonProperties.xmlFilename)))
            {
                ser.Serialize(writer, this);
            }
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

            DisasterList.Clear();
            DisasterList.Add(ForestFire);
            DisasterList.Add(Thunderstorm);
            DisasterList.Add(Sinkhole);
            DisasterList.Add(Tsunami);
            DisasterList.Add(Tornado);
            DisasterList.Add(Earthquake);
            DisasterList.Add(MeteorStrike);
        }

        public static DisasterSetupModel CreateFromFile()
        {
            var path = CommonProperties.GetOptionsFilePath(CommonProperties.xmlFilename);

            if (!File.Exists(path)) return null;

            try
            {
                var ser = new XmlSerializer(typeof(DisasterSetupModel));
                DisasterSetupModel instance;
                using (TextReader reader = new StreamReader(path))
                {
                    instance = (DisasterSetupModel)ser.Deserialize(reader);
                }

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
