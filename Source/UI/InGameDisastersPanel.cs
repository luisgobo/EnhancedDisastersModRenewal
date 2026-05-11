using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using NaturalDisastersRenewal.UI.ComponentHelper;
using UnityEngine;
using UnityEngine.Serialization;

namespace NaturalDisastersRenewal.UI
{
    public class InGameDisastersPanel : UIPanel
    {
        private const float LabelTextScaleSmall = 0.7f;
        private const float LabelTextScaleNormal = 0.8f;
        private const string HelpTutorialKey = "NDR_TUTORIAL_PANEL_HELP";
        private const float PanelWidth = 430f;
        private const float PanelHeight = 335f;
        private const float ContentInset = 8f;
        private static readonly Color32 ActiveDependencyColor = new Color32(90, 200, 120, 255);
        private static readonly Color32 InactiveDependencyColor = new Color32(210, 120, 120, 255);
        private static readonly Color32 WarningDependencyColor = new Color32(230, 185, 70, 255);

        [FormerlySerializedAs("Counter")] public int counter;
        private DisasterRowHelper[] _disasterRows;
        private UILabel _populationLabel;
        private UILabel _extendedInfoPanel2StatusLabel;
        private UILabel[] _forestFireBehaviorModStatusLabels;
        private UILabel _realTimeStatusLabel;

        public override void Awake()
        {
            base.Awake();

            backgroundSprite = "MenuPanel";
            height = PanelHeight;
            width = PanelWidth;
            canFocus = true;
            isVisible = false;
        }

        public override void Start()
        {
            base.Start();
            RebuildLocalizedContent();
        }

        public override void Update()
        {
            base.Update();

            if (!isVisible) return;

            if (--counter > 0) return;
            counter = 10;

            UpdatePopulationLabel();
            UpdateDependencyLabels();

            if (_disasterRows == null) return;

            RefreshDisasterRow();
        }

        public void RebuildLocalizedContent()
        {
            var currentVisibility = isVisible;
            foreach (var child in components.OfType<UIComponent>().ToArray()) Destroy(child.gameObject);

            BuildInformationBar();
            BuildTabContainer();

            isVisible = currentVisibility;
            counter = 0;
        }

        private void BuildInformationBar()
        {
            var titleBar = AddUIComponent<UIPanel>();
            titleBar.name = "titleBar";
            titleBar.relativePosition = Vector3.zero;
            titleBar.size = new Vector2(PanelWidth - 80f, 40f);
            titleBar.tooltip = LocalizationService.Get("panel.drag_panel.tooltip");
            titleBar.isInteractive = true;

            var titleLabel = titleBar.AddUIComponent<UILabel>();
            titleLabel.relativePosition = new Vector3(10f, 15f);
            titleLabel.text = LocalizationService.Get("panel.title");
            titleLabel.tooltip = LocalizationService.Get("panel.drag_panel.tooltip");

            BuildPanelButtons(this);
        }

        private void BuildPanelButtons(UIPanel parentPanel)
        {
            ActionButtonHelper.CreateTextButton(
                parentPanel,
                "helpBtn",
                "?",
                new Vector3(PanelWidth - 70f, 8f),
                new Vector2(25f, 25f),
                LocalizationService.Get("panel.help.tooltip"),
                HelpBtn_eventClick,
                null,
                "OptionBase",
                "OptionBaseHovered",
                "OptionBasePressed",
                new RectOffset(1, 0, 5, 0));

            ActionButtonHelper.CreateTextButton(
                parentPanel,
                "closeBtn",
                "X",
                new Vector3(PanelWidth - 35f, 8f),
                new Vector2(25f, 25f),
                LocalizationService.Get("panel.close.tooltip"),
                ClosePanelBtn_eventClick,
                null,
                "OptionBase",
                "OptionBaseHovered",
                "OptionBasePressed",
                new RectOffset(2, 0, 5, 0));
        }

        private void BuildTabContainer()
        {
            var tabContainer = AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(ContentInset, 70f);
            tabContainer.size = new Vector2(width - ContentInset * 2f, height - 65f - ContentInset);

            var tabStrip = AddUIComponent<UITabstrip>();
            tabStrip.relativePosition = new Vector3(0f, 40f);
            tabStrip.size = new Vector2(width, 30f);
            tabStrip.backgroundSprite = "SubcategoriesPanel";
            tabStrip.tabPages = tabContainer;

            var statisticsTab =
                TabHelper.CreateStyledTab(tabStrip, LocalizationService.Get("panel.tab.statistics"), 0f);
            TabHelper.CreateStyledTab(tabStrip, LocalizationService.Get("panel.tab.controls"), statisticsTab.width);

            if (tabContainer.components.Count < 2) return;

            var statisticsTabPanel = tabContainer.components[0] as UIPanel;
            var controlsTabPanel = tabContainer.components[1] as UIPanel;
            if (statisticsTabPanel == null || controlsTabPanel == null) return;

            var statisticsScrollablePanel = ScrollablePanelHelper.Create(statisticsTabPanel, 10f);
            var controlsScrollablePanel = ScrollablePanelHelper.Create(controlsTabPanel, 10f);

            BuildStatisticsInfoTabContent(statisticsScrollablePanel);
            BuildControlsTabContent(controlsScrollablePanel);

            statisticsTabPanel.isVisible = true;
            controlsTabPanel.isVisible = false;
        }

        private void BuildStatisticsInfoTabContent(UIScrollablePanel parentPanel)
        {
            const float itemSpacing = 4f;
            const float xPosition = 1f;
            var yPosition = 10f;
            const float probabilityBarStartX = xPosition + DisasterRowHelper.ProbabilityBarX;
            const float probabilityBarCenterX = probabilityBarStartX + DisasterRowHelper.BarWidth * 0.5f;
            const float probabilityBarEndX = probabilityBarStartX + DisasterRowHelper.BarWidth;
            const float intensityBarStartX = xPosition + DisasterRowHelper.IntensityBarX;
            const float intensityBarCenterX = intensityBarStartX + DisasterRowHelper.BarWidth * 0.5f;
            const float intensityBarEndX = intensityBarStartX + DisasterRowHelper.BarWidth;

            _populationLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal, string.Empty);
            UpdatePopulationLabel();

            yPosition += 22f;
            AddLabel(parentPanel, xPosition + 27f, yPosition, LabelTextScaleSmall,
                LocalizationService.Get("panel.header.disaster"));
            var probabilityHeader = AddCenteredLabel(parentPanel, probabilityBarCenterX - 3f, yPosition,
                LabelTextScaleSmall,
                LocalizationService.Get("panel.header.probability"));
            probabilityHeader.tooltip = LocalizationService.Get("panel.header.probability.tooltip");
            AddCenteredLabel(parentPanel, intensityBarCenterX, yPosition, LabelTextScaleSmall,
                LocalizationService.Get("panel.header.max_intensity"));

            yPosition += 15f;
            AddCenteredLabel(parentPanel, probabilityBarStartX + 2f, yPosition, LabelTextScaleSmall, "0");
            AddCenteredLabel(parentPanel, probabilityBarCenterX, yPosition, LabelTextScaleSmall, "50");
            AddCenteredLabel(parentPanel, probabilityBarEndX - 10f, yPosition, LabelTextScaleSmall, "100");
            AddCenteredLabel(parentPanel, intensityBarStartX + 10f, yPosition, LabelTextScaleSmall, "0.0");
            AddCenteredLabel(parentPanel, intensityBarEndX - 13f, yPosition, LabelTextScaleSmall, "25.5");

            yPosition += 15f;

            var disasterCount = Services.DisasterHandler.container.AllDisasters.Count;
            _disasterRows = new DisasterRowHelper[disasterCount];

            for (var i = 0; i < disasterCount; i++)
            {
                var disaster = Services.DisasterHandler.container.AllDisasters[i];
                var disasterRow = parentPanel.AddUIComponent<DisasterRowHelper>();
                disasterRow.Initialize(disaster, xPosition, yPosition, ToggleDisasterState);
                disasterRow.Refresh();
                _disasterRows[i] = disasterRow;
                yPosition += disasterRow.height + itemSpacing;
            }

            yPosition += 0f;
            BuildActionButtons(parentPanel, xPosition, yPosition);
            parentPanel.autoLayout = false;
            parentPanel.autoSize = false;
        }

        private void BuildControlsTabContent(UIScrollablePanel parentPanel)
        {
            var yPosition = 10f;
            AddLabel(parentPanel, 0f, yPosition, LabelTextScaleNormal,
                LocalizationService.Get("settings.group.dependencies"));
            yPosition += 28f;

            _realTimeStatusLabel = AddWrappedLabel(parentPanel, 0f, yPosition, PanelWidth - ContentInset * 4f,
                string.Empty);
            yPosition += 42f;
            _extendedInfoPanel2StatusLabel = AddWrappedLabel(parentPanel, 0f, yPosition, PanelWidth - ContentInset * 4f,
                string.Empty);
            yPosition += 28f;

            _forestFireBehaviorModStatusLabels =
                new UILabel[DisasterSimulationUtils.ForestFireBehaviorModNames.Length];
            for (var i = 0; i < DisasterSimulationUtils.ForestFireBehaviorModNames.Length; i++)
            {
                _forestFireBehaviorModStatusLabels[i] =
                    AddWrappedLabel(parentPanel, 0f, yPosition, PanelWidth - ContentInset * 4f, string.Empty);
                yPosition += 36f;
            }

            // AddLabel(parentPanel, 0f, yPosition, LabelTextScaleNormal, LocalizationService.Get("panel.controls.title"));
            // yPosition += 28f;
            // var toggleLabel = AddWrappedLabel(parentPanel, 0f, yPosition, WrappedContentWidth,
            //     LocalizationService.Get("panel.controls.toggle"));
            // yPosition += toggleLabel.height + 10f;
            // var stopLabel = AddWrappedLabel(parentPanel, 0f, yPosition, WrappedContentWidth,
            //     LocalizationService.Get("panel.controls.stop"));
            // yPosition += stopLabel.height + 10f;
            // var resetLabel = AddWrappedLabel(parentPanel, 0f, yPosition, WrappedContentWidth,
            //     LocalizationService.Get("panel.controls.reset"));
            // yPosition += resetLabel.height + 10f;
            // var dragLabel = AddWrappedLabel(parentPanel, 0f, yPosition, WrappedContentWidth,
            //     LocalizationService.Get("panel.controls.drag"));
            // yPosition += dragLabel.height + 10f;
            // AddWrappedLabel(parentPanel, 0f, yPosition, WrappedContentWidth,
            //     LocalizationService.Get("panel.controls.hotkey"));
            parentPanel.autoLayout = false;
            parentPanel.autoSize = false;

            // UpdateRealTimeLabels();
        }

        private static void BuildActionButtons(UIComponent parentPanel, float xPosition, float yPosition)
        {
            const float actionButtonSize = 22f;
            var actionButtonPadding = new RectOffset(0, 0, 3, 0);

            ActionButtonHelper.CreateTextButton(
                parentPanel,
                "stopDisasterBtn",
                "\u25A0",
                new Vector3(xPosition, yPosition),
                new Vector2(actionButtonSize, actionButtonSize),
                LocalizationService.Get("panel.stop_all"),
                StopAllDisastersBtn_eventClick,
                Color.red,
                "ButtonMenu",
                "ButtonMenuHovered",
                "ButtonMenuHovered",
                actionButtonPadding);

            ActionButtonHelper.CreateTextButton(
                parentPanel,
                "resetDisasterBtn",
                "\u21BA",
                new Vector3(xPosition + actionButtonSize + 4f, yPosition),
                new Vector2(actionButtonSize, actionButtonSize),
                LocalizationService.Get("panel.reset_all"),
                ResetAllDisastersBtn_eventClick,
                Color.yellow,
                "ButtonMenu",
                "ButtonMenuHovered",
                "ButtonMenuHovered",
                actionButtonPadding);
        }

        private void UpdatePopulationLabel()
        {
            if (!_populationLabel) return;

            var numberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            numberFormatInfo.NumberGroupSeparator = ",";
            var formatted =
                ((int)Services.DisasterHandler.container.MaxPopulationToTriggerHigherDisasters).ToString("#,0",
                    numberFormatInfo);
            _populationLabel.text = LocalizationService.Format("panel.population_threshold.value", formatted);
            _populationLabel.tooltip = LocalizationService.Get("panel.population_threshold");
        }

        private void UpdateDependencyLabels()
        {
            if (!_realTimeStatusLabel || !_extendedInfoPanel2StatusLabel)
                return;

            UpdateDependencyLabel(
                _realTimeStatusLabel,
                "Real Time",
                IsRealTimeModActive(),
                LocalizationService.Get("settings.dependency.real_time_description"));
            UpdateDependencyLabel(
                _extendedInfoPanel2StatusLabel,
                "Extended InfoPanel 2",
                DisasterSimulationUtils.IsExtendedInfoPanel2Active(),
                LocalizationService.Get("settings.dependency.extended_info_panel_2_description"));

            if (_forestFireBehaviorModStatusLabels == null)
                return;

            for (var i = 0; i < _forestFireBehaviorModStatusLabels.Length; i++)
                UpdateForestFireBehaviorDependencyLabel(
                    _forestFireBehaviorModStatusLabels[i],
                    DisasterSimulationUtils.ForestFireBehaviorModNames[i],
                    DisasterSimulationUtils.IsForestFireBehaviorModActive(i));
        }

        private static void UpdateDependencyLabel(UILabel label, string dependencyName, bool isActive, string description)
        {
            var status = LocalizationService.Get(isActive
                ? "settings.dependency.active"
                : "settings.dependency.inactive");

            label.text = dependencyName + ": " + status + CommonProperties.NewLine + description;
            label.textColor = isActive ? ActiveDependencyColor : InactiveDependencyColor;
        }

        private static void UpdateForestFireBehaviorDependencyLabel(UILabel label, string dependencyName, bool isActive)
        {
            if (!label) return;

            label.text = dependencyName + ": " + LocalizationService.Get(isActive
                ? "settings.dependency.forest_fire_warning"
                : "settings.dependency.inactive");
            label.textColor = isActive ? WarningDependencyColor : InactiveDependencyColor;
        }

        private static bool IsRealTimeModActive()
        {
            return DisasterSimulationUtils.IsRealTimeModActive();
        }

        private void ToggleDisasterState(DisasterBaseModel disaster)
        {
            disaster.Enabled = !disaster.Enabled;
            switch (disaster.GetDisasterType())
            {
                case DisasterType.Earthquake:
                    Services.DisasterSetup.Earthquake.Enabled = disaster.Enabled;
                    break;
                case DisasterType.ForestFire:
                    Services.DisasterSetup.ForestFire.Enabled = disaster.Enabled;
                    break;
                case DisasterType.MeteorStrike:
                    Services.DisasterSetup.MeteorStrike.Enabled = disaster.Enabled;
                    break;
                case DisasterType.Sinkhole:
                    Services.DisasterSetup.Sinkhole.Enabled = disaster.Enabled;
                    break;
                case DisasterType.ThunderStorm:
                    Services.DisasterSetup.Thunderstorm.Enabled = disaster.Enabled;
                    break;
                case DisasterType.Tornado:
                    Services.DisasterSetup.Tornado.Enabled = disaster.Enabled;
                    break;
                case DisasterType.Tsunami:
                    Services.DisasterSetup.Tsunami.Enabled = disaster.Enabled;
                    break;
            }

            if (_disasterRows != null) RefreshDisasterRow();

            ModSettingsScreen.UpdateUISettingsOptions();
        }

        private void RefreshDisasterRow()
        {
            foreach (var disasterRow in _disasterRows) disasterRow.Refresh();
        }

        private static void HelpBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ShowHelpPanel();
        }

        private static void ShowHelpPanel()
        {
            var advisorPanel = ToolsModifierControl.advisorPanel;
            if (advisorPanel == null) return;

            RegisterHelpTutorialLocalization();
            advisorPanel.Show(HelpTutorialKey, "ToolbarIconZoomOutGlobe", string.Empty, 0f);
        }

        private static void RegisterHelpTutorialLocalization()
        {
            var locale = GetLocale();
            if (locale == null) return;

            SetLocalizedString(
                locale,
                new Locale.Key
                {
                    m_Identifier = "TUTORIAL_ADVISER_TITLE",
                    m_Key = HelpTutorialKey
                },
                LocalizationService.Get("panel.help.title"));

            SetLocalizedString(
                locale,
                new Locale.Key
                {
                    m_Identifier = "TUTORIAL_ADVISER",
                    m_Key = HelpTutorialKey
                },
                LocalizationService.Get("panel.help.content"));
        }

        private static void SetLocalizedString(Locale locale, Locale.Key key, string value)
        {
            try
            {
                locale.AddLocalizedString(key, value);
            }
            catch (ArgumentException)
            {
                TryUpdateLocalizedString(locale, key, value);
            }
        }

        private static void TryUpdateLocalizedString(Locale locale, Locale.Key key, string value)
        {
            var fields = locale.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (var i = 0; i < fields.Length; i++)
            {
                if (!typeof(IDictionary).IsAssignableFrom(fields[i].FieldType)) continue;

                var dictionary = fields[i].GetValue(locale) as IDictionary;
                if (dictionary == null) continue;

                dictionary[key] = value;
                return;
            }
        }

        private static Locale GetLocale()
        {
            var localeField =
                typeof(LocaleManager).GetField("m_Locale", BindingFlags.Instance | BindingFlags.NonPublic);
            if (localeField == null) return null;

            return localeField.GetValue(LocaleManager.instance) as Locale;
        }

        private static void StopAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            StopAllDisasters();
        }

        private static void ResetAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            StopAllDisasters();

            for (var i = 0; i < Services.DisasterHandler.container.AllDisasters.Count; i++)
            {
                var disaster = Services.DisasterHandler.container.AllDisasters[i];
                disaster.calmDaysLeft = 0f;
                disaster.probabilityWarmupDaysLeft = 0f;
                disaster.intensityWarmupDaysLeft = 0f;
            }
        }

        private static void StopAllDisasters()
        {
            var sb = new StringBuilder();
            var vm = Services.Vehicles;
            for (var i = 1; i < 16384; i++)
            {
                if ((vm.m_vehicles.m_buffer[i].m_flags & Vehicle.Flags.Created) == 0) continue;
                if (vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is MeteorAI ||
                    vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is VortexAI)
                    vm.ReleaseVehicle((ushort)i);
            }

            var ws = Services.Water;
            if (ws != null)
                for (var i = ws.m_waterWaves.m_size; i >= 1; i--)
                    Services.Terrain.WaterSimulation.ReleaseWaterWave((ushort)i);

            var dm = Services.Disasters;
            for (ushort i = 0; i < dm.m_disasterCount; i++)
            {
                sb.AppendLine(dm.m_disasters.m_buffer[i].Info.name + " flags: " + dm.m_disasters.m_buffer[i].m_flags);
                if ((dm.m_disasters.m_buffer[i].m_flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active |
                                                           DisasterData.Flags.Clearing)) == DisasterData.Flags.None)
                    continue;
                if (!IsStoppableDisaster(dm.m_disasters.m_buffer[i].Info.m_disasterAI))
                    continue;

                dm.m_disasters.m_buffer[i].m_flags =
                    (dm.m_disasters.m_buffer[i].m_flags & ~(DisasterData.Flags.Emerging | DisasterData.Flags.Active |
                                                            DisasterData.Flags.Clearing))
                    | DisasterData.Flags.Finished;
            }

            if (Services.DisasterHandler.container.ActiveDisasters != null)
                Services.DisasterHandler.container.ActiveDisasters.Clear();

            Debug.Log(sb.ToString());
        }

        private static bool IsStoppableDisaster(DisasterAI ai)
        {
            return ai as ThunderStormAI != null || ai as SinkholeAI != null || ai as TornadoAI != null ||
                   ai as EarthquakeAI != null || ai as MeteorStrikeAI != null || ai as ForestFireAI != null ||
                   ai as TsunamiAI != null;
        }

        private void ClosePanelBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }

        private static UILabel AddLabel(UIComponent parentPanel, float x, float y, float textScale, string text)
        {
            var label = parentPanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(x, y);
            label.textScale = textScale;
            label.text = text;
            return label;
        }

        private static UILabel AddCenteredLabel(UIComponent parentPanel, float centerX, float y, float textScale,
            string text)
        {
            var label = AddLabel(parentPanel, 0f, y, textScale, text);
            label.relativePosition = new Vector3(centerX - label.width * 0.5f, y);
            return label;
        }

        private static UILabel AddWrappedLabel(UIComponent parentPanel, float x, float y, float width, string text)
        {
            var label = parentPanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(x, y);
            label.autoSize = false;
            label.wordWrap = true;
            label.width = width;
            label.height = 36f;
            label.textScale = LabelTextScaleNormal;
            label.text = text;
            return label;
        }
    }
}
