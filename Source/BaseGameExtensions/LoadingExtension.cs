using ICities;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.BaseGameExtensions
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame || mode == LoadMode.NewGameFromScenario)
            {
                CommonServices.DisasterHandler.CreateExtendedDisasterPanel();
                CommonServices.DisasterHandler.CheckUnlocks();

                CommonServices.DisasterSetup.Earthquake.UpdateDisasterProperties(true);
                CommonServices.DisasterHandler.RedefineDisasterMaxIntensity();
            }
        }

        public override void OnLevelUnloading()
        {
            CommonServices.DisasterSetup.Earthquake.UpdateDisasterProperties(false);
            CommonServices.ResetCache();
        }
    }
}