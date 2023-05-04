using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.UI;

namespace NaturalDisastersRenewal
{
    public class Mod : IUserMod
    {
        readonly SettingsScreen settings = new SettingsScreen();

        public string Name
        {
            get { return CommonProperties.modName; }
        }

        public string Description
        {
            get { return CommonProperties.GetModDescription(); }
        }

        public void OnSettingsUI(UIHelper helper)
        {
            settings.BuildSettingsMenu(helper);
        }

        public void EnhancedDisastersOptionsUpdateUI()
        {
            settings.UpdateSetupContentUI();
        }
    }
}