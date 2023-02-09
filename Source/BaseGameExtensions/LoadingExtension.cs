using ColossalFramework;
using ICities;

namespace NaturalDisastersRenewal.BaseGameExtensions
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame || mode == LoadMode.NewGameFromScenario)
            {
                Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.CreateExtendedDisasterPanel();
                Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.CheckUnlocks();

                Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.container.Earthquake.UpdateDisasterProperties(true);
            }
        }

        public override void OnLevelUnloading()
        {
            Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.container.Earthquake.UpdateDisasterProperties(false);
        }
    }
}