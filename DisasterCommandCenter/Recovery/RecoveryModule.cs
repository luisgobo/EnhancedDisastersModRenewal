using DisasterCommandCenter.Core;

namespace DisasterCommandCenter.Recovery
{
    public sealed class RecoveryModule : ICommandCenterModule
    {
        public string Name
        {
            get { return "Recovery"; }
        }

        public void Initialize(CommandCenterContext context)
        {
            // Future home for rebuildable buildings, damaged roads, camera navigation, and repair actions.
        }

        public void Shutdown()
        {
        }
    }
}
