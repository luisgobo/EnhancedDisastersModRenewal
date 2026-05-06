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

            var realTimeLabel = dependenciesPanel.AddUIComponent<UILabel>();
            var isRealTimeActive = DisasterSimulationUtils.IsRealTimeModActive();

            realTimeLabel.text = "Real Time: " +
                                 LocalizationService.Get(isRealTimeActive
                                     ? "settings.dependency.active"
                                     : "settings.dependency.inactive");
            realTimeLabel.textScale = 1f;
            realTimeLabel.textColor = isRealTimeActive
                ? new Color32(90, 200, 120, 255)
                : new Color32(210, 120, 120, 255);
            realTimeLabel.autoSize = true;
        }
    }
}