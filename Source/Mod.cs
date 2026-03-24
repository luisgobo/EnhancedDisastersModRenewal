using System.Reflection;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Models.Disaster;
using NaturalDisastersRenewal.UI;
using UnityEngine;

namespace NaturalDisastersRenewal
{
    public class Mod : IUserMod
    {
        private readonly SettingsScreen _settings = new SettingsScreen();

        public string Name => CommonProperties.modName;

        public string Description => CommonProperties.GetModDescription();

        public void OnSettingsUI(UIHelper helper)
        {
            _settings.BuildSettingsMenu(helper);
        }

        public void EnhancedDisastersOptionsUpdateUI()
        {
            _settings.UpdateSetupContentUI();
        }

        public void OnEnabled()
        {
            var assembly = Assembly.GetExecutingAssembly();
            DebugLogger.Log("[NDR DEBUG] Location: " + assembly.Location);
            DebugLogger.Log("[NDR DEBUG] CodeBase: " + assembly.CodeBase);
            Debug.Log("[NDR] BUILD TEST 001");
        }
    }
}