using DisasterCommandCenter.Core;

namespace DisasterCommandCenter.Localization
{
    public sealed class LocalizationModule : ICommandCenterModule
    {
        public string Name
        {
            get { return "Localization"; }
        }

        public void Initialize(CommandCenterContext context)
        {
            // Future home for English, Spanish, and reusable UI text keys.
        }

        public void Shutdown()
        {
        }
    }
}
