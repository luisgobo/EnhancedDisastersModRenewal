using ColossalFramework;
using ColossalFramework.Plugins;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Setup;

namespace NaturalDisastersRenewal.Common
{
    /// <summary>
    /// Centralized service locator for accessing game services and mod singletons.
    /// Provides cached access and cleaner API than direct Singleton&lt;T&gt; calls.
    /// </summary>
    public static class Services
    {
        // Cached instances
        private static NaturalDisasterHandler _disasterHandler;
        private static DisasterManager _disasterManager;
        private static SimulationManager _simulationManager;
        private static BuildingManager _buildingManager;
        private static VehicleManager _vehicleManager;
        private static TerrainManager _terrainManager;
        private static DistrictManager _districtManager;
        private static WaterSimulation _waterSimulation;
        private static PluginManager _pluginManager;
        private static GameAreaManager _gameAreaManager;
        private static WeatherManager _weatherManager;
        private static UnlockManager _unlockManager;

        #region Mod Services

        /// <summary>
        /// Main disaster handler for the mod
        /// </summary>
        public static NaturalDisasterHandler DisasterHandler
        {
            get
            {
                if (!_disasterHandler)
                    _disasterHandler = Singleton<NaturalDisasterHandler>.instance;
                return _disasterHandler;
            }
        }

        /// <summary>
        /// Disaster configuration container (shortcut)
        /// </summary>
        public static DisasterSetupModel DisasterSetup => DisasterHandler ? DisasterHandler.Container : null;

        #endregion

        #region Game Services

        public static DisasterManager Disasters
        {
            get
            {
                if (!_disasterManager)
                    _disasterManager = Singleton<DisasterManager>.instance;
                return _disasterManager;
            }
        }

        public static SimulationManager Simulation
        {
            get
            {
                if (!_simulationManager)
                    _simulationManager = Singleton<SimulationManager>.instance;
                return _simulationManager;
            }
        }

        public static BuildingManager Buildings
        {
            get
            {
                if (!_buildingManager)
                    _buildingManager = Singleton<BuildingManager>.instance;
                return _buildingManager;
            }
        }

        public static VehicleManager Vehicles
        {
            get
            {
                if (!_vehicleManager)
                    _vehicleManager = Singleton<VehicleManager>.instance;
                return _vehicleManager;
            }
        }

        public static TerrainManager Terrain
        {
            get
            {
                if (!_terrainManager)
                    _terrainManager = Singleton<TerrainManager>.instance;
                return _terrainManager;
            }
        }

        public static DistrictManager Districts
        {
            get
            {
                if (!_districtManager)
                    _districtManager = Singleton<DistrictManager>.instance;
                return _districtManager;
            }
        }

        public static WaterSimulation Water
        {
            get
            {
                if (!_waterSimulation)
                    _waterSimulation = Singleton<WaterSimulation>.instance;
                return _waterSimulation;
            }
        }

        public static PluginManager Plugins
        {
            get
            {
                if (!_pluginManager)
                    _pluginManager = Singleton<PluginManager>.instance;
                return _pluginManager;
            }
        }

        public static GameAreaManager GameArea
        {
            get
            {
                if (!_gameAreaManager)
                    _gameAreaManager = Singleton<GameAreaManager>.instance;
                return _gameAreaManager;
            }
        }

        public static WeatherManager Weather
        {
            get
            {
                if (!_weatherManager)
                    _weatherManager = Singleton<WeatherManager>.instance;
                return _weatherManager;
            }
        }

        public static UnlockManager Unlocks
        {
            get
            {
                if (_unlockManager == null)
                    _unlockManager = Singleton<UnlockManager>.instance;
                return _unlockManager;
            }
        }

        #endregion

        /// <summary>
        /// Reset all cached services (call on level unload)
        /// </summary>
        public static void ResetCache()
        {
            _disasterHandler = null;
            _disasterManager = null;
            _simulationManager = null;
            _buildingManager = null;
            _vehicleManager = null;
            _terrainManager = null;
            _districtManager = null;
            _waterSimulation = null;
            _pluginManager = null;
            _gameAreaManager = null;
            _weatherManager = null;
            _unlockManager = null;
        }
    }
}

