using ColossalFramework.UI;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.UI.Extensions;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.SettingsSections
{
    internal sealed class DependenciesSettingsSection
    {
        private static readonly Color32 ActiveColor = new Color32(90, 200, 120, 255);
        private static readonly Color32 InactiveColor = new Color32(210, 120, 120, 255);
        private static readonly Color32 WarningColor = new Color32(230, 185, 70, 255);

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

            var yPosition = 0f;
            yPosition = AddDependencyLabel(
                dependenciesPanel,
                "Real Time",
                DisasterSimulationUtils.IsRealTimeModActive(),
                LocalizationService.Get("settings.dependency.real_time_description"),
                yPosition);
            yPosition = AddDependencyLabel(dependenciesPanel, "Extended InfoPanel 2",
                DisasterSimulationUtils.IsExtendedInfoPanel2Active(),
                LocalizationService.Get("settings.dependency.extended_info_panel_2_description"),
                yPosition);

            yPosition += 8f;
            for (var i = 0; i < DisasterSimulationUtils.ForestFireBehaviorModNames.Length; i++)
                yPosition = AddForestFireBehaviorModLabel(
                    dependenciesPanel,
                    DisasterSimulationUtils.ForestFireBehaviorModNames[i],
                    DisasterSimulationUtils.IsForestFireBehaviorModActive(i),
                    yPosition);
        }

        private static float AddDependencyLabel(
            UIPanel parentPanel,
            string dependencyName,
            bool isActive,
            string description,
            float y)
        {
            var label = parentPanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(0f, y);
            label.text = dependencyName + ": " +
                         LocalizationService.Get(isActive
                             ? "settings.dependency.active"
                             : "settings.dependency.inactive");
            label.textScale = 1f;
            label.textColor = isActive ? ActiveColor : InactiveColor;
            label.autoSize = true;

            var descriptionLabel = parentPanel.AddUIComponent<UILabel>();
            descriptionLabel.relativePosition = new Vector3(12f, y + 18f);
            descriptionLabel.text = description;
            descriptionLabel.textScale = 0.85f;
            descriptionLabel.textColor = label.textColor;
            descriptionLabel.autoSize = true;

            return y + 44f;
        }

        private static float AddForestFireBehaviorModLabel(
            UIPanel parentPanel,
            string dependencyName,
            bool isActive,
            float y)
        {
            var label = parentPanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(0f, y);
            label.text = dependencyName + ": " + LocalizationService.Get(isActive
                ? "settings.dependency.forest_fire_warning"
                : "settings.dependency.inactive");
            label.textScale = 1f;
            label.textColor = isActive ? WarningColor : InactiveColor;
            label.autoSize = true;

            return y + 24f;
        }
    }
}
