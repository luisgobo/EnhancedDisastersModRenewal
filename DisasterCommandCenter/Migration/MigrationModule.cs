using DisasterCommandCenter.Core;

namespace DisasterCommandCenter.Migration
{
    public sealed class MigrationModule : ICommandCenterModule
    {
        public string Name
        {
            get { return "Migration"; }
        }

        public void Initialize(CommandCenterContext context)
        {
            // Future home for importing Natural Disasters Renewal options and save data.
        }

        public void Shutdown()
        {
        }
    }
}
