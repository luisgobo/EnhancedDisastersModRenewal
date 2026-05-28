using DisasterCommandCenter.Core;

namespace DisasterCommandCenter.Settings
{
    public sealed class SettingsModule : ICommandCenterModule
    {
        public string Name
        {
            get { return "Settings"; }
        }

        public void Initialize(CommandCenterContext context)
        {
            // Future home for option models, defaults, validation, and settings UI state.
        }

        public void Shutdown()
        {
        }
    }
}
