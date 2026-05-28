using DisasterCommandCenter.Core;

namespace DisasterCommandCenter.Compatibility
{
    public sealed class CompatibilityModule : ICommandCenterModule
    {
        public string Name
        {
            get { return "Compatibility"; }
        }

        public void Initialize(CommandCenterContext context)
        {
            // Future home for Real Time, ACME, Extended Info Panel, and behavior-mod checks.
        }

        public void Shutdown()
        {
        }
    }
}
