using ColossalFramework;
using ICities;
using UnityEngine;

namespace EnhancedDisastersMod
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame || mode == LoadMode.NewGameFromScenario)
            {
                Singleton<EnhancedDisastersManager>.instance.CreateExtendedDisasterPanel();
                Singleton<EnhancedDisastersManager>.instance.CheckUnlocks();

                Singleton<EnhancedDisastersManager>.instance.container.Earthquake.UpdateDisasterProperties(true);
            }
        }

        public override void OnLevelUnloading()
        {
            Singleton<EnhancedDisastersManager>.instance.container.Earthquake.UpdateDisasterProperties(false);
        }
    }
}
