using DisasterCommandCenter.Core;

namespace DisasterCommandCenter.Disasters
{
    public sealed class DisasterSimulationModule : ICommandCenterModule
    {
        public string Name
        {
            get { return "Disaster Simulation"; }
        }

        public void Initialize(CommandCenterContext context)
        {
            // Future home for disaster recurrence, targeting, intensity, and environmental factors.
        }

        public void Shutdown()
        {
        }
    }
}
