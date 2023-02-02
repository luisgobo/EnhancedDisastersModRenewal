using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.UI;

namespace NaturalDisastersRenewal
{
    public class Mod : IUserMod
    {
        readonly SettingsScreen settings = new SettingsScreen();

        public string Name
        {
            get { return CommonProperties.ModName; }
        }

        public string Description
        {
            get { return CommonProperties.getModDescription(); }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            settings.BuildSettingsMenu(helper);            
        }
    }
}