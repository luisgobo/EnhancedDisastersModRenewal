using DisasterCommandCenter.Core;

namespace DisasterCommandCenter.UI.InGame
{
    public sealed class CommandCenterPanelModule : ICommandCenterModule
    {
        public string Name
        {
            get { return "In-Game Command Center Panel"; }
        }

        public void Initialize(CommandCenterContext context)
        {
            // Future home for the in-game control panel, tabs, action buttons, and progress indicators.
        }

        public void Shutdown()
        {
        }
    }
}
