using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using System;
using UnityEngine;

namespace NaturalDisastersRenewal.BaseGameExtensions
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {            
            if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame || mode == LoadMode.NewGameFromScenario)
            {

                Singleton<NaturalDisasterHandler>.instance.CityName = SimulationManager.instance.m_metaData.m_CityName;
                DebugLogger.Log($"Charging City:{Singleton<NaturalDisasterHandler>.instance.CityName}. DateTime:{DateTime.Now}");

                Singleton<NaturalDisasterHandler>.instance.CreateExtendedDisasterPanel();
                Singleton<NaturalDisasterHandler>.instance.CheckUnlocks();

                Singleton<NaturalDisasterHandler>.instance.container.Earthquake.UpdateDisasterProperties(true);
                Singleton<NaturalDisasterHandler>.instance.RedefineDisasterMaxIntensity();
            }
        }        

        public override void OnLevelUnloading()
        {
            Singleton<NaturalDisasterHandler>.instance.container.Earthquake.UpdateDisasterProperties(false);
        }
    }
}