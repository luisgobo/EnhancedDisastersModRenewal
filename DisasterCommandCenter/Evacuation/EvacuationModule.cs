using DisasterCommandCenter.Core;

namespace DisasterCommandCenter.Evacuation
{
    public sealed class EvacuationModule : ICommandCenterModule
    {
        public string Name
        {
            get { return "Evacuation"; }
        }

        public void Initialize(CommandCenterContext context)
        {
            // Future home for manual, automatic, focused, coastal, and release behavior.
        }

        public void Shutdown()
        {
        }
    }
}
