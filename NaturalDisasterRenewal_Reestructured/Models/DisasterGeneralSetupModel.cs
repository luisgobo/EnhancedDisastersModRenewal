using ColossalFramework;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace NaturalDisasterRenewal_Reestructured.Models
{
    public class DisasterGeneralSetupModel : Singleton<DisasterGeneralSetupModel>
    {
        //public ForestFireService ForestFire;
        //public ThunderstormService Thunderstorm;
        //public SinkholeService Sinkhole;
        //public TsunamiService Tsunami;
        //public TornadoService Tornado;
        //public EarthquakeService Earthquake;
        //public MeteorStrikeService MeteorStrike;

        //General options
        public bool DisableDisasterFocus = true;

        public bool PauseOnDisasterStarts = true;
        public float PartialEvacuationRadius = 1000f;

        public bool ScaleMaxIntensityWithPopulation = true;
        public bool RecordDisasterEvents = false;
        public bool ShowDisasterPanelButton = true;
        public Vector3 ToggleButtonPos = new Vector3(90, 62);

        [XmlIgnore]
        public List<DisasterBaseModel> AllDisasters = new List<DisasterBaseModel>();
    }
}