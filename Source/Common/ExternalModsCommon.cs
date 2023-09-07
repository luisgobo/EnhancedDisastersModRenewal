using ColossalFramework;
using ColossalFramework.Plugins;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using NaturalDisastersRenewal.Models.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NaturalDisastersRenewal.Common
{
    public class ExternalModsCommon
    {
        static readonly List<string> assemblyList = new List<string>();
        DisasterSetupModel disasterContainer = Singleton<NaturalDisasterHandler>.instance.container;

        public ExternalModsCommon()
        {
            GetAssemblyNamesList();
        }

        /// <summary>
        /// Get the assembly of another game mod
        /// </summary>
        /// <param name="modName">The IUserMod class name (lowercase)</param>
        /// <param name="assemblyName">The assembly name (lowercase)</param>
        /// <param name="assNameExcept">An assembly name exception to skip even if matches previous parameter</param>
        /// <param name="onlyEnabled">Limit result to enabled mods?</param>
        /// <returns>Game mod's assembly</returns>
        public static Assembly GetAssembly(string modName, string assemblyName, string assNameExcept = "", bool onlyEnabled = true)
        {
            foreach (PluginManager.PluginInfo pluginInfo in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                try
                {
                    if (pluginInfo.userModInstance?.GetType().Name.ToLower() == modName.ToLower() && (!onlyEnabled || pluginInfo.isEnabled))
                    {
                        if (assNameExcept.Length > 0)
                        {
                            if (pluginInfo.GetAssemblies().Any(mod => mod.GetName().Name.ToLower() == assNameExcept.ToLower()))
                            {
                                break;
                            }
                        }
                        foreach (Assembly assembly in pluginInfo.GetAssemblies())
                        {
                            if (assembly.GetName().Name.ToLower() == assemblyName.ToLower())
                            {
                                return assembly;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.Log(ex.Message);
                } // If the plugin fails to process, move on to next plugin
            }

            return null;
        }

        static List<string> GetAssemblyNamesList()
        {
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (!assemblyList.Contains(assembly.GetName().Name.ToLower()))
                    {
                        assemblyList.Add(assembly.GetName().Name.ToLower());
                    }
                }
            }

            return assemblyList;
        }

        public static bool CheckAssemblyExistence(string assemblyName)
        {
            var existsAssembly = assemblyList.Contains(assemblyName.ToLower());
            DebugLogger.EnabledModsLog($"Date & time:{DateTime.Now.ToString("yyyy-MM-d H:m:s.fff")}. {assemblyName} found: {existsAssembly}");
            return existsAssembly;
        }

        public static bool CheckAssemblyExistence(string modName, string assemblyName, string assNameExcept = "", bool onlyEnabled = true)
        {
            foreach (PluginManager.PluginInfo pluginInfo in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                try
                {
                    if (pluginInfo.userModInstance?.GetType().Name.ToLower() == modName.ToLower() && (!onlyEnabled || pluginInfo.isEnabled))
                    {
                        if (assNameExcept.Length > 0)
                        {
                            if (pluginInfo.GetAssemblies().Any(mod => mod.GetName().Name.ToLower() == assNameExcept.ToLower()))
                            {
                                DebugLogger.EnabledModsLog(assNameExcept + " found");
                                return true;
                            }
                        }
                        foreach (Assembly assembly in pluginInfo.GetAssemblies())
                        {
                            if (assembly.GetName().Name.ToLower() == assemblyName.ToLower())
                            {
                                DebugLogger.EnabledModsLog(assembly.GetName().Name.ToLower() + " found");
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.Log(ex.ToString());
                }
            }

            return false;
        }
    }
}