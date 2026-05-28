using ICities;

namespace DisasterCommandCenter.UI
{
    public sealed class CommandCenterSettingsScreen
    {
        public void BuildSettingsMenu(UIHelper helper)
        {
            if (helper == null)
                return;

            helper.AddGroup("Disaster Command Center");
        }
    }
}
