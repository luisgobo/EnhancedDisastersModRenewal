using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.Services.LegacyStructure.Handlers;
using NaturalDisastersRenewal.Services.LegacyStructure.NaturalDisaster;

namespace NaturalDisastersRenewal.BaseGameExtensions
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame || mode == LoadMode.NewGameFromScenario)
            {
                Singleton<NaturalDisasterHandler>.instance.CreateExtendedDisasterPanel();
                Singleton<NaturalDisasterHandler>.instance.CheckUnlocks();

                Singleton<NaturalDisasterHandler>.instance.container.Earthquake.UpdateDisasterProperties(true);

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