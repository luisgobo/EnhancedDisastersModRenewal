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
                Singleton<DisasterServices.NaturalDisasterHandler>.instance.CreateExtendedDisasterPanel();
                Singleton<DisasterServices.NaturalDisasterHandler>.instance.CheckUnlocks();

                Singleton<DisasterServices.NaturalDisasterHandler>.instance.container.Earthquake.UpdateDisasterProperties(true);
            }
        }

        public override void OnLevelUnloading()
        {
            Singleton<DisasterServices.NaturalDisasterHandler>.instance.container.Earthquake.UpdateDisasterProperties(false);
        }
    }
}