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
                Singleton<DisasterServices.NaturalDisasterManager>.instance.CreateExtendedDisasterPanel();
                Singleton<DisasterServices.NaturalDisasterManager>.instance.CheckUnlocks();

                Singleton<DisasterServices.NaturalDisasterManager>.instance.container.Earthquake.UpdateDisasterProperties(true);
            }
        }

        public override void OnLevelUnloading()
        {
            Singleton<DisasterServices.NaturalDisasterManager>.instance.container.Earthquake.UpdateDisasterProperties(false);
        }
    }
}