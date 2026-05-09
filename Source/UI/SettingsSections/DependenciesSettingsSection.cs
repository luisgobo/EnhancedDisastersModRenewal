using ColossalFramework.UI;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.UI.Extensions;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.SettingsSections
{
    internal sealed class DependenciesSettingsSection
    {
        public void Build(ref UIHelper helper)
        {
            var dependenciesGroup = helper.AddGroup(LocalizationService.Get("settings.group.dependencies"));
            helper.AddSpacing();

            var dependenciesUiHelper = dependenciesGroup as UIHelper;
            if (dependenciesUiHelper == null)
                return;

            var dependenciesPanel = dependenciesUiHelper.self as UIPanel;
            if (dependenciesPanel == null)
                return;

            var isRealTimeActive = DisasterSimulationUtils.IsRealTimeModActive();
            AddDependencyLabel(dependenciesPanel, "Real Time", isRealTimeActive, 0f);

            var isExtendedInfoPanel2Active = DisasterSimulationUtils.IsExtendedInfoPanel2Active();
            AddDependencyLabel(dependenciesPanel, "Extended InfoPanel 2", isExtendedInfoPanel2Active, 24f);
        }

        private static void AddDependencyLabel(UIPanel parentPanel, string dependencyName, bool isActive, float y)
        {
            var label = parentPanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(0f, y);
            label.text = dependencyName + ": " +
                         LocalizationService.Get(isActive
                             ? "settings.dependency.active"
                             : "settings.dependency.inactive");
            label.textScale = 1f;
            label.textColor = isActive
                ? new Color32(90, 200, 120, 255)
                : new Color32(210, 120, 120, 255);
            label.autoSize = true;
        }
    }
}
