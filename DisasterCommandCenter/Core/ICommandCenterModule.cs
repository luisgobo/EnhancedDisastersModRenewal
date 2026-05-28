namespace DisasterCommandCenter.Core
{
    public interface ICommandCenterModule
    {
        string Name { get; }

        void Initialize(CommandCenterContext context);

        void Shutdown();
    }
}
