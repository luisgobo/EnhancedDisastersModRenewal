using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.UI;

namespace NaturalDisastersRenewal
{
    public class Mod : IUserMod
    {
        private readonly ModSettingsScreen _modSettings = new ModSettingsScreen();

        public string Name => CommonProperties.ModName;

        public string Description => CommonProperties.GetModDescription();

        public void OnSettingsUI(UIHelper helper)
        {
            _modSettings.BuildSettingsMenu(helper);
        }

        public void EnhancedDisastersOptionsUpdateUI()
        {
            _modSettings.UpdateSetupContentUI();
        }

        public void EnhancedDisastersOptionsRebuildUI()
        {
            _modSettings.RebuildSettingsMenu();
        }
    }
}