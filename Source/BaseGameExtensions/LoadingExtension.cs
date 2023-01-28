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
                Singleton<DisasterServices.DisasterManager>.instance.CreateExtendedDisasterPanel();
                Singleton<DisasterServices.DisasterManager>.instance.CheckUnlocks();

                Singleton<DisasterServices.DisasterManager>.instance.container.Earthquake.UpdateDisasterProperties(true);
            }
        }

        public override void OnLevelUnloading()
        {
            Singleton<DisasterServices.DisasterManager>.instance.container.Earthquake.UpdateDisasterProperties(false);
        }
    }
}