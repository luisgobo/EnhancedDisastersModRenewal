using DisasterCommandCenter.Common;
using DisasterCommandCenter.Core;
using DisasterCommandCenter.UI;
using ICities;

namespace DisasterCommandCenter
{
    public sealed class Mod : IUserMod
    {
        private readonly CommandCenterBootstrapper _bootstrapper = new CommandCenterBootstrapper();
        private readonly CommandCenterSettingsScreen _settingsScreen = new CommandCenterSettingsScreen();

        public string Name
        {
            get { return CommandCenterProperties.ModName; }
        }

        public string Description
        {
            get { return CommandCenterProperties.GetModDescription(); }
        }

        public void OnSettingsUI(UIHelper helper)
        {
            _bootstrapper.Initialize();
            _settingsScreen.BuildSettingsMenu(helper);
        }
    }
}
