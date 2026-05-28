using System.Collections.Generic;
using DisasterCommandCenter.Common;
using DisasterCommandCenter.Compatibility;
using DisasterCommandCenter.Disasters;
using DisasterCommandCenter.Evacuation;
using DisasterCommandCenter.Localization;
using DisasterCommandCenter.Migration;
using DisasterCommandCenter.Recovery;
using DisasterCommandCenter.Serialization;
using DisasterCommandCenter.Settings;
using DisasterCommandCenter.UI.InGame;

namespace DisasterCommandCenter.Core
{
    public sealed class CommandCenterBootstrapper
    {
        private readonly List<ICommandCenterModule> _modules = new List<ICommandCenterModule>();
        private bool _initialized;

        public CommandCenterBootstrapper()
        {
            Register(new MigrationModule());
            Register(new LocalizationModule());
            Register(new CompatibilityModule());
            Register(new SettingsModule());
            Register(new SerializationModule());
            Register(new DisasterSimulationModule());
            Register(new EvacuationModule());
            Register(new RecoveryModule());
            Register(new CommandCenterPanelModule());
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            CommandCenterContext context = new CommandCenterContext(
                CommandCenterProperties.ModName,
                CommandCenterProperties.ModVersion);

            foreach (ICommandCenterModule module in _modules)
                module.Initialize(context);

            _initialized = true;
        }

        public void Shutdown()
        {
            if (!_initialized)
                return;

            for (int i = _modules.Count - 1; i >= 0; i--)
                _modules[i].Shutdown();

            _initialized = false;
        }

        private void Register(ICommandCenterModule module)
        {
            _modules.Add(module);
        }
    }
}
