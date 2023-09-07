using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Disaster;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
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
        //public PanelIconsVisivility ShowDisasterPanelButton = PanelIconsVisivility.Both;

        public bool PauseOnDisasterStarts = false;
        public float PartialEvacuationRadius = 900f;
        public float MaxPopulationToTrigguerHigherDisasters = 200000;

        public bool ScaleMaxIntensityWithPopulation = true;
        public bool RecordDisasterEvents = false;
        public bool ShowDisasterPanelButton = true;
        public Vector3 ToggleButtonPos = new Vector3(90, 62);
        public Vector3 DPanelPos = new Vector3(90, 40);

        //Disaster list
        public List<DisasterInfoModel> activeDisasters = new List<DisasterInfoModel>();

        [XmlIgnore] public List<DisasterBaseModel> AllDisasters = new List<DisasterBaseModel>();
        [XmlIgnore] public bool isAcmeModActive = false;
        [XmlIgnore] public bool isRealTimeModActive = false;
        [XmlIgnore] public bool isUnifiedUIActive = false;

        public void Save()
        {
            XmlSerializer ser = new XmlSerializer(typeof(DisasterSetupModel));
            TextWriter writer = new StreamWriter(CommonProperties.GetOptionsFilePath(CommonProperties.xmlFilename));
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
            string path = CommonProperties.GetOptionsFilePath(CommonProperties.xmlFilename);

            if (!File.Exists(path)) return null;

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(DisasterSetupModel));
                TextReader reader = new StreamReader(path);
                DisasterSetupModel instance = (DisasterSetupModel)ser.Deserialize(reader);
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