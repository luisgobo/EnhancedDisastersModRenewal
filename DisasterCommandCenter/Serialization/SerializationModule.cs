using DisasterCommandCenter.Core;

namespace DisasterCommandCenter.Serialization
{
    public sealed class SerializationModule : ICommandCenterModule
    {
        public string Name
        {
            get { return "Serialization"; }
        }

        public void Initialize(CommandCenterContext context)
        {
            // Future home for save-game data, options persistence, and migration-safe serializers.
        }

        public void Shutdown()
        {
        }
    }
}
