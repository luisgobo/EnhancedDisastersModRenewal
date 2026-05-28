namespace DisasterCommandCenter.Core
{
    public sealed class CommandCenterContext
    {
        public CommandCenterContext(string modName, string version)
        {
            ModName = modName;
            Version = version;
        }

        public string ModName { get; private set; }

        public string Version { get; private set; }
    }
}
