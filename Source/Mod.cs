using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.UI;

namespace NaturalDisastersRenewal
{
    public class Mod : IUserMod
    {
        private readonly SettingsScreen _settings = new SettingsScreen();

        public string Name => CommonProperties.modName;

        public string Description => CommonProperties.GetModDescription();

        public void OnSettingsUI(UIHelper helper)
        {
            _settings.BuildSettingsMenu(helper);
        }

        public void EnhancedDisastersOptionsUpdateUI()
        {
            _settings.UpdateSetupContentUI();
        }
    }
}