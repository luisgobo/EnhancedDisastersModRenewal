using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.BaseGameExtensions;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Setup;
using NaturalDisastersRenewal.UI.ComponentHelper;
using NaturalDisastersRenewal.UI.Extensions;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace NaturalDisastersRenewal.UI
{
    public class SettingsScreen
    {
        private bool freezeUI;
        private UIComponent rootComponent;
        private UIHelper rootHelper;

        private string EvacuationModeText => LocalizationService.Get("settings.evacuation_mode");

        #region UI Components

        //General
        private UIDropDown UI_General_Language;
        private UICheckBox UI_General_DisableDisasterFocus;

        private UICheckBox UI_General_PauseOnDisasterStarts;
        private UISlider UI_General_PartialEvacuationRadius;
        private UISlider UI_General_MaxPopulationToTrigguerHigherDisasters;
        private UICheckBox UI_General_ScaleMaxIntensityWithPopulation;
        private UICheckBox UI_General_RecordDisasterEventsChkBox;
        private UICheckBox UI_General_ShowDisasterPanelButton;
        private UITextField UI_General_TogglePanelHotkeyField;

        //Forest Fire
        private UICheckBox UI_ForestFire_Enabled;

        private UISlider UI_ForestFireMaxProbability;
        private UISlider UI_ForestFire_WarmupDays;
        private UIDropDown UI_ForestFire_EvacuationMode;

        //Thunderstorm
        private UICheckBox UI_Thunderstorm_Enabled;

        private UISlider UI_Thunderstorm_MaxProbability;
        private UIDropDown UI_Thunderstorm_MaxProbabilityMonth;
        private UISlider UI_Thunderstorm_RainFactor;
        private UIDropDown UI_Thunderstorm_EvacuationMode;

        //Sunkhole
        private UICheckBox UI_Sinkhole_Enabled;

        private UISlider UI_Sinkhole_MaxProbability;
        private UISlider UI_Sinkhole_GroundwaterCapacity;
        private UIDropDown UI_Sinkhole_EvacuationMode;

        //Tornado
        private UICheckBox UI_Tornado_Enabled;

        private UISlider UI_Tornado_MaxProbability;
        private UIDropDown UI_Tornado_MaxProbabilityMonth;
        private UICheckBox UI_Tornado_NoDuringFog;
        private UIDropDown UI_Tornado_EvacuationMode;
        private UICheckBox UI_Tornado_EnableDestruction;
        private UISlider UI_Tornado_IntensityDestructionStart;

        //Tsunami
        private UICheckBox UI_Tsunami_Enabled;

        private UISlider UI_Tsunami_MaxProbability;
        private UISlider UI_Tsunami_WarmupYears;
        private UIDropDown UI_Tsunami_EvacuationMode;

        //Earthquake
        private UICheckBox UI_Earthquake_Enabled;

        private UISlider UI_Earthquake_MinIntensityToCrack;
        private UISlider UI_Earthquake_MaxProbability;
        private UISlider UI_Earthquake_WarmupYears;
        private UICheckBox UI_Earthquake_AftershocksEnabled;

        //UICheckBox UI_Earthquake_NoCrack;
        private UIDropDown UI_Earthquake_CrackMode;

        private UIDropDown UI_Earthquake_EvacuationMode;

        //Meteor Strike
        private UICheckBox UI_MeteorStrike_Enabled;

        private UISlider UI_MeteorStrike_MaxProbability;
        private UICheckBox UI_MeteorStrike_MeteorLongPeriodEnabled;
        private UICheckBox UI_MeteorStrike_MeteorMediumPeriodEnabled;
        private UICheckBox UI_MeteorStrike_MeteorShortPeriodEnabled;
        private UIDropDown UI_MeteorStrike_EvacuationMode;

        #endregion UI Components

        #region Options UI

        private bool hotkeyCaptureHandlerRegistered;
        private UIButton[] settingsSectionButtons;
        private UIPanel[] settingsSectionPages;

        private const int nextCheckboxSpacing = 0;

        public static bool IsCapturingHotkey { get; private set; }

        public static void UpdateUISettingsOptions()
        {
            foreach (var current in Services.Plugins.GetPluginsInfo())
                if (current.isEnabled)
                {
                    var instances = current.GetInstances<IUserMod>();

                    var method = instances[0].GetType().GetMethod("EnhancedDisastersOptionsUpdateUI",
                        BindingFlags.Instance | BindingFlags.Public);

                    if (method != null)
                    {
                        method.Invoke(instances[0], new object[] { });
                        return;
                    }
                }
        }

        private static void RebuildUISettingsOptions()
        {
            foreach (var current in Services.Plugins.GetPluginsInfo())
                if (current.isEnabled)
                {
                    var instances = current.GetInstances<IUserMod>();

                    var method = instances[0].GetType().GetMethod("EnhancedDisastersOptionsRebuildUI",
                        BindingFlags.Instance | BindingFlags.Public);

                    if (method == null) continue;
                    
                    method.Invoke(instances[0], new object[] { });
                    return;
                }
        }

        public void UpdateSetupContentUI()
        {
            if (UI_ForestFire_Enabled == null)
                return;

            var disasterSetupModel = Services.DisasterSetup;
            freezeUI = true;

            UI_General_Language.selectedIndex = (int)disasterSetupModel.Language;
            UI_General_DisableDisasterFocus.isChecked = disasterSetupModel.DisableDisasterFocus;
            UI_General_PauseOnDisasterStarts.isChecked = disasterSetupModel.PauseOnDisasterStarts;
            UI_General_PartialEvacuationRadius.value = disasterSetupModel.PartialEvacuationRadius;
            UI_General_MaxPopulationToTrigguerHigherDisasters.value =
                disasterSetupModel.MaxPopulationToTriggerHigherDisasters;

            UI_General_ScaleMaxIntensityWithPopulation.isChecked = disasterSetupModel.ScaleMaxIntensityWithPopulation;
            UI_General_RecordDisasterEventsChkBox.isChecked = disasterSetupModel.RecordDisasterEvents;
            UI_General_ShowDisasterPanelButton.isChecked = disasterSetupModel.ShowDisasterPanelButton;
            RefreshHotkeyFieldText();

            UI_ForestFire_Enabled.isChecked = disasterSetupModel.ForestFire.Enabled;
            UI_ForestFire_EvacuationMode.selectedIndex = (int)disasterSetupModel.ForestFire.EvacuationMode;
            UI_ForestFireMaxProbability.value = disasterSetupModel.ForestFire.BaseOccurrencePerYear;
            UI_ForestFire_WarmupDays.value = disasterSetupModel.ForestFire.WarmupDays;

            UI_Thunderstorm_Enabled.isChecked = disasterSetupModel.Thunderstorm.Enabled;
            UI_Thunderstorm_EvacuationMode.selectedIndex = (int)disasterSetupModel.Thunderstorm.EvacuationMode;
            UI_Thunderstorm_MaxProbability.value = disasterSetupModel.Thunderstorm.BaseOccurrencePerYear;
            UI_Thunderstorm_MaxProbabilityMonth.selectedIndex = disasterSetupModel.Thunderstorm.MaxProbabilityMonth - 1;
            UI_Thunderstorm_RainFactor.value = disasterSetupModel.Thunderstorm.RainFactor;

            UI_Sinkhole_Enabled.isChecked = disasterSetupModel.Sinkhole.Enabled;
            UI_Sinkhole_EvacuationMode.selectedIndex = (int)disasterSetupModel.Sinkhole.EvacuationMode;
            UI_Sinkhole_MaxProbability.value = disasterSetupModel.Sinkhole.BaseOccurrencePerYear;
            UI_Sinkhole_GroundwaterCapacity.value = disasterSetupModel.Sinkhole.GroundwaterCapacity;

            UI_Tornado_Enabled.isChecked = disasterSetupModel.Tornado.Enabled;
            UI_Tornado_EvacuationMode.selectedIndex = (int)disasterSetupModel.Tornado.EvacuationMode;
            UI_Tornado_MaxProbability.value = disasterSetupModel.Tornado.BaseOccurrencePerYear;
            UI_Tornado_MaxProbabilityMonth.selectedIndex = disasterSetupModel.Tornado.MaxProbabilityMonth - 1;
            UI_Tornado_NoDuringFog.isChecked = disasterSetupModel.Tornado.NoTornadoDuringFog;
            UI_Tornado_EnableDestruction.isChecked = disasterSetupModel.Tornado.EnableTornadoDestruction;
            UI_Tornado_IntensityDestructionStart.value = disasterSetupModel.Tornado.MinimalIntensityForDestruction;

            UI_Tsunami_Enabled.isChecked = disasterSetupModel.Tsunami.Enabled;
            UI_Tsunami_EvacuationMode.selectedIndex = (int)disasterSetupModel.Tsunami.EvacuationMode;
            UI_Tsunami_MaxProbability.value = disasterSetupModel.Tsunami.BaseOccurrencePerYear;
            UI_Tsunami_WarmupYears.value = disasterSetupModel.Tsunami.WarmupYears;

            UI_Earthquake_Enabled.isChecked = disasterSetupModel.Earthquake.Enabled;
            UI_Earthquake_EvacuationMode.selectedIndex = (int)disasterSetupModel.Earthquake.EvacuationMode;
            UI_Earthquake_MinIntensityToCrack.value = (int)disasterSetupModel.Earthquake.MinimalIntensityForCracks;
            UI_Earthquake_MaxProbability.value = disasterSetupModel.Earthquake.BaseOccurrencePerYear;
            UI_Earthquake_WarmupYears.value = disasterSetupModel.Earthquake.WarmupYears;
            UI_Earthquake_AftershocksEnabled.isChecked = disasterSetupModel.Earthquake.AftershocksEnabled;
            UI_Earthquake_CrackMode.selectedIndex = (int)disasterSetupModel.Earthquake.EarthquakeCrackMode;

            UI_MeteorStrike_Enabled.isChecked = disasterSetupModel.MeteorStrike.Enabled;
            UI_MeteorStrike_EvacuationMode.selectedIndex = (int)disasterSetupModel.MeteorStrike.EvacuationMode;
            UI_MeteorStrike_MaxProbability.value = disasterSetupModel.MeteorStrike.BaseOccurrencePerYear;
            UI_MeteorStrike_MeteorLongPeriodEnabled.isChecked = disasterSetupModel.MeteorStrike.GetEnabled(0);
            UI_MeteorStrike_MeteorMediumPeriodEnabled.isChecked = disasterSetupModel.MeteorStrike.GetEnabled(1);
            UI_MeteorStrike_MeteorShortPeriodEnabled.isChecked = disasterSetupModel.MeteorStrike.GetEnabled(2);

            freezeUI = false;
        }

        public void BuildSettingsMenu(UIHelper helper)
        {
            rootHelper = helper;
            rootComponent = helper.self as UIComponent;
            EnsureHotkeyCaptureRegistered();
            BuildSettingsContent(helper);
        }

        public void RebuildSettingsMenu()
        {
            if (rootHelper == null || rootComponent == null)
                return;

            freezeUI = true;

            foreach (var child in rootComponent.components.OfType<UIComponent>().ToArray())
                UnityObject.Destroy(child.gameObject);

            ResetUIReferences();
            BuildSettingsContent(rootHelper);
            UpdateSetupContentUI();

            freezeUI = false;
        }

        private void BuildSettingsContent(UIHelper helper)
        {
            var disasterContainer = Services.DisasterSetup;
            var settingsRoot = helper.self as UIComponent;
            if (settingsRoot == null)
                return;

            const float navigationWidth = 190f;
            const float footerHeight = 154f;
            const float panelGap = 10f;
            const float panelHeightGap = 15f;
            const float panelWidthGap = 13f;

            var availableWidth = settingsRoot.width > 0f ? settingsRoot.width - panelWidthGap : 980f;
            var availableHeight = settingsRoot.height > 0f ? settingsRoot.height - panelHeightGap : 720f;
            var contentHeight = Mathf.Max(240f, availableHeight - footerHeight - panelGap);
            var contentWidth = Mathf.Max(320f, availableWidth - navigationWidth - panelGap);

            var settingsCanvas = settingsRoot.AddUIComponent<UIPanel>();
            settingsCanvas.name = "SettingsCanvas";
            settingsCanvas.relativePosition = Vector3.zero;
            settingsCanvas.size = new Vector2(availableWidth, availableHeight);
            settingsCanvas.autoLayout = false;
            settingsCanvas.clipChildren = false;
            settingsCanvas.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;
            settingsCanvas.isInteractive = false;

            var navigationPanel = settingsCanvas.AddUIComponent<UIPanel>();
            navigationPanel.name = "SettingsNavigationPanel";
            navigationPanel.relativePosition = Vector3.zero;
            navigationPanel.size = new Vector2(navigationWidth, contentHeight);
            navigationPanel.backgroundSprite = "SubcategoriesPanel";
            navigationPanel.autoLayout = false;

            var contentHostPanel = settingsCanvas.AddUIComponent<UIPanel>();
            contentHostPanel.name = "SettingsContentHostPanel";
            contentHostPanel.relativePosition = new Vector3(navigationWidth + panelGap, 0f);
            contentHostPanel.size = new Vector2(contentWidth, contentHeight);
            contentHostPanel.backgroundSprite = "SubcategoriesPanel";
            contentHostPanel.autoLayout = false;

            var footerPanel = settingsCanvas.AddUIComponent<UIPanel>();
            footerPanel.name = "SettingsFooterPanel";
            footerPanel.relativePosition = new Vector3(0f, contentHeight + panelGap);
            footerPanel.size = new Vector2(availableWidth, footerHeight);
            footerPanel.backgroundSprite = "SubcategoriesPanel";
            footerPanel.autoLayout = false;

            var sectionButtons = new List<UIButton>();
            var sectionPages = new List<UIPanel>();
            var buttonY = 10f;

            AddSettingsSection(navigationPanel, contentHostPanel, ref buttonY,
                LocalizationService.Get("settings.general"), disasterContainer, SetupGeneralTab, sectionButtons,
                sectionPages);
            AddSettingsSection(navigationPanel, contentHostPanel, ref buttonY,
                LocalizationService.Get("settings.group.dependencies"), disasterContainer, SetupDependenciesTab,
                sectionButtons, sectionPages);
            AddSettingsSection(navigationPanel, contentHostPanel, ref buttonY,
                LocalizationService.GetDisasterName(disasterContainer.ForestFire.GetDisasterType()), disasterContainer,
                SetupForestFire, sectionButtons, sectionPages);
            AddSettingsSection(navigationPanel, contentHostPanel, ref buttonY,
                LocalizationService.GetDisasterName(disasterContainer.Thunderstorm.GetDisasterType()),
                disasterContainer, SetupThunderstorm, sectionButtons, sectionPages);
            AddSettingsSection(navigationPanel, contentHostPanel, ref buttonY,
                LocalizationService.GetDisasterName(disasterContainer.Sinkhole.GetDisasterType()), disasterContainer,
                SetupSinkhole, sectionButtons, sectionPages);
            AddSettingsSection(navigationPanel, contentHostPanel, ref buttonY,
                LocalizationService.GetDisasterName(disasterContainer.Tornado.GetDisasterType()), disasterContainer,
                SetupTornado, sectionButtons, sectionPages);
            AddSettingsSection(navigationPanel, contentHostPanel, ref buttonY,
                LocalizationService.GetDisasterName(disasterContainer.Tsunami.GetDisasterType()), disasterContainer,
                SetupTsunami, sectionButtons, sectionPages);
            AddSettingsSection(navigationPanel, contentHostPanel, ref buttonY,
                LocalizationService.GetDisasterName(disasterContainer.Earthquake.GetDisasterType()), disasterContainer,
                SetupEarthquake, sectionButtons, sectionPages);
            AddSettingsSection(navigationPanel, contentHostPanel, ref buttonY,
                LocalizationService.GetDisasterName(disasterContainer.MeteorStrike.GetDisasterType()),
                disasterContainer, SetupMeteorStrike, sectionButtons, sectionPages);

            settingsSectionButtons = sectionButtons.ToArray();
            settingsSectionPages = sectionPages.ToArray();

            BuildSaveFooter(footerPanel);
            SelectSettingsSection(0);
        }

        private delegate void SettingsSectionBuilder(ref UIHelper helper, DisasterSetupModel disasterContainer);

        private void AddSettingsSection(
            UIPanel navigationPanel,
            UIPanel contentHostPanel,
            ref float buttonY,
            string sectionTitle,
            DisasterSetupModel disasterContainer,
            SettingsSectionBuilder builder,
            List<UIButton> sectionButtons,
            List<UIPanel> sectionPages)
        {
            var sectionIndex = sectionButtons.Count;

            var button = CreateSectionButton(navigationPanel, sectionTitle, buttonY);
            button.eventClick += delegate { SelectSettingsSection(sectionIndex); };
            sectionButtons.Add(button);

            var contentPage = contentHostPanel.AddUIComponent<UIPanel>();
            contentPage.name = "SettingsSectionPage" + sectionIndex;
            contentPage.relativePosition = Vector3.zero;
            contentPage.size = contentHostPanel.size;
            contentPage.isVisible = false;

            var scrollablePanel = CreateScrollableSettingsPanel(contentPage);
            var sectionHelper = new UIHelper(scrollablePanel);
            builder(ref sectionHelper, disasterContainer);

            sectionPages.Add(contentPage);
            buttonY += 34f;
        }

        private static UIButton CreateSectionButton(UIPanel parentPanel, string text, float yPosition)
        {
            var button = parentPanel.AddUIComponent<UIButton>();
            button.text = text;
            button.relativePosition = new Vector3(8f, yPosition);
            button.size = new Vector2(parentPanel.width - 16f, 30f);
            UIStyleHelper.ApplySectionButtonStyle(button);
            button.textPadding = new RectOffset(10, 10, 8, 6);
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            return button;
        }

        private static UIScrollablePanel CreateScrollableSettingsPanel(UIPanel parentPanel)
        {
            const float panelPadding = 8f;
            const float scrollbarWidth = 12f;
            const float scrollbarGap = 8f;

            var scrollablePanel = parentPanel.AddUIComponent<UIScrollablePanel>();
            var scrollbar = parentPanel.AddUIComponent<UIScrollbar>();
            var track = scrollbar.AddUIComponent<UISlicedSprite>();
            var thumb = track.AddUIComponent<UISlicedSprite>();

            scrollablePanel.relativePosition = new Vector2(panelPadding, panelPadding);
            scrollablePanel.size = new Vector2(
                parentPanel.width - panelPadding * 2f - scrollbarWidth - scrollbarGap,
                parentPanel.height - panelPadding * 2f);
            scrollablePanel.clipChildren = true;
            scrollablePanel.autoLayout = true;
            scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            scrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
            scrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 10);
            scrollablePanel.wrapLayout = false;
            scrollablePanel.scrollWheelAmount = 20;

            scrollbar.orientation = UIOrientation.Vertical;
            scrollbar.width = scrollbarWidth;
            scrollbar.relativePosition = new Vector2(
                parentPanel.width - panelPadding - scrollbarWidth,
                panelPadding);
            scrollbar.height = parentPanel.height - panelPadding * 2f;

            track.spriteName = "ScrollbarTrack";
            track.size = new Vector2(scrollbarWidth, scrollbar.height);
            track.relativePosition = Vector2.zero;
            scrollbar.trackObject = track;

            thumb.spriteName = "ScrollbarThumb";
            thumb.width = scrollbarWidth;
            thumb.height = 48f;
            scrollbar.thumbObject = thumb;
            UIStyleHelper.ApplyScrollbarStyle(track, thumb);

            scrollablePanel.verticalScrollbar = scrollbar;
            scrollablePanel.eventMouseWheel += delegate(UIComponent component, UIMouseEventParameter eventParam)
            {
                scrollablePanel.scrollPosition +=
                    new Vector2(0f, -eventParam.wheelDelta * scrollablePanel.scrollWheelAmount);
            };

            return scrollablePanel;
        }

        private void BuildSaveFooter(UIPanel footerPanel)
        {
            const float sidePadding = 10f;
            const float buttonTop = 40f;
            const float buttonHeight = 28f;
            const float buttonSpacing = 38f;


            var buttonWidth = Mathf.Min(420f, Mathf.Max(240f, footerPanel.width - 120f));
            var buttonX = Mathf.Max(sidePadding, (footerPanel.width - buttonWidth) * 0.5f);

            var titleLabel = footerPanel.AddUIComponent<UILabel>();
            titleLabel.text = LocalizationService.Get("settings.save_options");
            titleLabel.textScale = 0.9f;
            titleLabel.relativePosition = new Vector3(sidePadding, 15f);

            CreateFooterButton(
                footerPanel,
                LocalizationService.Get("settings.save_default"),
                new Vector3(buttonX, buttonTop),
                buttonWidth,
                buttonHeight,
                delegate { Services.DisasterSetup.Save(); });

            CreateFooterButton(
                footerPanel,
                LocalizationService.Get("settings.reset_saved"),
                new Vector3(buttonX, buttonTop + buttonSpacing),
                buttonWidth,
                buttonHeight,
                delegate
                {
                    Services.DisasterHandler.ReadValuesFromFile();
                    UpdateSetupContentUI();
                });

            CreateFooterButton(
                footerPanel,
                LocalizationService.Get("settings.reset_defaults"),
                new Vector3(buttonX, buttonTop + buttonSpacing * 2f),
                buttonWidth,
                buttonHeight,
                delegate
                {
                    Services.DisasterHandler.ResetToDefaultValues();
                    UpdateSetupContentUI();
                });
        }

        private static void CreateFooterButton(
            UIPanel footerPanel,
            string text,
            Vector3 position,
            float width,
            float height,
            MouseEventHandler clickHandler)
        {
            var button = footerPanel.AddUIComponent<UIButton>();
            button.text = text;
            button.relativePosition = position;
            button.size = new Vector2(width, height);
            UIStyleHelper.ApplyActionButtonStyle(button);
            button.eventClick += clickHandler;
        }

        private void SelectSettingsSection(int selectedIndex)
        {
            if (settingsSectionButtons == null || settingsSectionPages == null)
                return;

            for (var i = 0; i < settingsSectionButtons.Length; i++)
            {
                var isSelected = i == selectedIndex;
                settingsSectionButtons[i].isEnabled = !isSelected;
                settingsSectionPages[i].isVisible = isSelected;
            }
        }

        private void EnsureHotkeyCaptureRegistered()
        {
            if (hotkeyCaptureHandlerRegistered)
                return;

            UIInput.eventProcessKeyEvent += HotkeyCapture_eventProcessKeyEvent;
            hotkeyCaptureHandlerRegistered = true;
        }

        private void HotkeyCapture_eventProcessKeyEvent(EventType eventType, KeyCode keyCode, EventModifiers modifiers)
        {
            if (!IsCapturingHotkey || eventType != EventType.KeyDown)
                return;

            if (HotkeyHelper.IsModifierKey(keyCode))
                return;

            if (keyCode == KeyCode.Escape)
            {
                SetHotkeyCaptureState(false);
                RefreshHotkeyFieldText();
                return;
            }

            if ((keyCode == KeyCode.Backspace || keyCode == KeyCode.Delete) && modifiers == EventModifiers.None)
            {
                Services.DisasterSetup.TogglePanelHotkey = KeyCode.None;
                Services.DisasterSetup.TogglePanelHotkeyModifiers = EventModifiers.None;
                SetHotkeyCaptureState(false);
                RefreshHotkeyFieldText();
                return;
            }

            if (keyCode == KeyCode.None || keyCode == KeyCode.Escape)
                return;

            var normalizedModifiers = HotkeyHelper.GetSupportedHotkeyModifiers(modifiers);
            if (HotkeyHelper.CountHotkeyModifiers(normalizedModifiers) > 2)
                return;

            Services.DisasterSetup.TogglePanelHotkey = keyCode;
            Services.DisasterSetup.TogglePanelHotkeyModifiers = normalizedModifiers;
            SetHotkeyCaptureState(false);
            RefreshHotkeyFieldText();
        }

        private void BeginHotkeyCapture()
        {
            SetHotkeyCaptureState(true);
            RefreshHotkeyFieldText();
        }

        private void SetHotkeyCaptureState(bool isCapturing)
        {
            IsCapturingHotkey = isCapturing;
        }

        private void RefreshHotkeyFieldText()
        {
            if (UI_General_TogglePanelHotkeyField == null)
                return;

            UI_General_TogglePanelHotkeyField.text = IsCapturingHotkey
                ? LocalizationService.Get("settings.hotkey.capture")
                : HotkeyHelper.FormatHotkey(
                    Services.DisasterSetup.TogglePanelHotkey,
                    Services.DisasterSetup.TogglePanelHotkeyModifiers);
        }

        private void ResetUIReferences()
        {
            SetHotkeyCaptureState(false);
            UI_General_Language = null;
            UI_General_DisableDisasterFocus = null;
            UI_General_PauseOnDisasterStarts = null;
            UI_General_PartialEvacuationRadius = null;
            UI_General_MaxPopulationToTrigguerHigherDisasters = null;
            UI_General_ScaleMaxIntensityWithPopulation = null;
            UI_General_RecordDisasterEventsChkBox = null;
            UI_General_ShowDisasterPanelButton = null;
            UI_General_TogglePanelHotkeyField = null;
            UI_ForestFire_Enabled = null;
            UI_ForestFireMaxProbability = null;
            UI_ForestFire_WarmupDays = null;
            UI_ForestFire_EvacuationMode = null;
            UI_Thunderstorm_Enabled = null;
            UI_Thunderstorm_MaxProbability = null;
            UI_Thunderstorm_MaxProbabilityMonth = null;
            UI_Thunderstorm_RainFactor = null;
            UI_Thunderstorm_EvacuationMode = null;
            UI_Sinkhole_Enabled = null;
            UI_Sinkhole_MaxProbability = null;
            UI_Sinkhole_GroundwaterCapacity = null;
            UI_Sinkhole_EvacuationMode = null;
            UI_Tornado_Enabled = null;
            UI_Tornado_MaxProbability = null;
            UI_Tornado_MaxProbabilityMonth = null;
            UI_Tornado_NoDuringFog = null;
            UI_Tornado_EvacuationMode = null;
            UI_Tornado_EnableDestruction = null;
            UI_Tornado_IntensityDestructionStart = null;
            UI_Tsunami_Enabled = null;
            UI_Tsunami_MaxProbability = null;
            UI_Tsunami_WarmupYears = null;
            UI_Tsunami_EvacuationMode = null;
            UI_Earthquake_Enabled = null;
            UI_Earthquake_MinIntensityToCrack = null;
            UI_Earthquake_MaxProbability = null;
            UI_Earthquake_WarmupYears = null;
            UI_Earthquake_AftershocksEnabled = null;
            UI_Earthquake_CrackMode = null;
            UI_Earthquake_EvacuationMode = null;
            UI_MeteorStrike_Enabled = null;
            UI_MeteorStrike_MaxProbability = null;
            UI_MeteorStrike_MeteorLongPeriodEnabled = null;
            UI_MeteorStrike_MeteorMediumPeriodEnabled = null;
            UI_MeteorStrike_MeteorShortPeriodEnabled = null;
            UI_MeteorStrike_EvacuationMode = null;
            settingsSectionButtons = null;
            settingsSectionPages = null;
        }

        private void SetupGeneralTab(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var generalGroup = helper.AddGroup(LocalizationService.Get("settings.general"));

            UI_General_Language = (UIDropDown)generalGroup.AddDropdown(LocalizationService.Get("settings.language"),
                LocalizationService.GetLanguageDisplayNames(), (int)disasterContainer.Language, delegate(int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.Language = (ModLanguage)selection;
                        Services.DisasterHandler.RefreshLocalizedUI();
                        RebuildUISettingsOptions();
                    }
                });
            UIStyleHelper.ApplyDropDownStyle(UI_General_Language);
            UI_General_Language.tooltip = LocalizationService.Get("settings.language.tooltip");

            UI_General_DisableDisasterFocus = CheckboxHelper.AddCheckbox(
                ref generalGroup,
                LocalizationService.Get("settings.disable_follow"), disasterContainer.DisableDisasterFocus,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.DisableDisasterFocus = isChecked;
                        DisasterExtension.SetDisableDisasterFocus(disasterContainer.DisableDisasterFocus);
                    }
                }, spacing: nextCheckboxSpacing);

            UI_General_PauseOnDisasterStarts = CheckboxHelper.AddCheckbox(
                ref generalGroup,
                LocalizationService.Get("settings.pause_on_start"), disasterContainer.PauseOnDisasterStarts,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.PauseOnDisasterStarts = isChecked;
                });

            generalGroup.AddSpacing();

            UI_General_PartialEvacuationRadius = SliderHelper.AddSlider(
                ref generalGroup,
                LocalizationService.Get("settings.focused_radius"), 300f, 4200f, 100f,
                disasterContainer.PartialEvacuationRadius, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.PartialEvacuationRadius = val;
                }, tooltip: LocalizationService.Get("settings.focused_radius.tooltip"));

            UI_General_MaxPopulationToTrigguerHigherDisasters = SliderHelper.AddSlider(
                ref generalGroup,
                LocalizationService.Get("settings.max_population"), 20000f, 800000f, 1000f,
                disasterContainer.MaxPopulationToTriggerHigherDisasters, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.MaxPopulationToTriggerHigherDisasters = val;
                }, tooltip: LocalizationService.Get("settings.max_population.tooltip"));

            UI_General_ScaleMaxIntensityWithPopulation = CheckboxHelper.AddCheckbox(
                ref generalGroup,
                LocalizationService.Get("settings.scale_intensity"), disasterContainer.ScaleMaxIntensityWithPopulation,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.ScaleMaxIntensityWithPopulation = isChecked;
                },
                LocalizationService.Get("settings.scale_intensity.tooltip"),
                nextCheckboxSpacing);

            UI_General_RecordDisasterEventsChkBox = CheckboxHelper.AddCheckbox(
                ref generalGroup,
                LocalizationService.Get("settings.record_events"), disasterContainer.RecordDisasterEvents,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.RecordDisasterEvents = isChecked;
                },
                LocalizationService.Get("settings.record_events.tooltip"),
                nextCheckboxSpacing);

            UI_General_ShowDisasterPanelButton = CheckboxHelper.AddCheckbox(
                ref generalGroup,
                LocalizationService.Get("settings.show_panel_button"), disasterContainer.ShowDisasterPanelButton,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.ShowDisasterPanelButton = isChecked;

                    Services.DisasterHandler.UpdateDisastersPanelToggleBtn();
                    Services.DisasterHandler.UpdateDisastersDPanel();
                });

            generalGroup.AddSpacing();
            AddHotkeyContent(generalGroup);

            generalGroup.AddSpacing();

            var elementPositionsGroup = generalGroup.AddGroup(LocalizationService.Get("settings.positions"));

            elementPositionsGroup.AddButton(LocalizationService.Get("settings.reset_button_position"), delegate
            {
                Services.DisasterHandler.ResetToDefaultValues(true, false);
                UpdateSetupContentUI();
            });

            elementPositionsGroup.AddButton(LocalizationService.Get("settings.reset_panel_position"), delegate
            {
                Services.DisasterHandler.ResetToDefaultValues(false, true);
                UpdateSetupContentUI();
            });

            generalGroup.AddSpacing();

            var disastersGroup = generalGroup.AddGroup(LocalizationService.Get("settings.enable_disasters"));

            UI_ForestFire_Enabled = CheckboxHelper.AddCheckbox(ref disastersGroup,
                disasterContainer.ForestFire.GetName(),
                disasterContainer.ForestFire.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.ForestFire.Enabled = isChecked;
                }, spacing: nextCheckboxSpacing);

            UI_Thunderstorm_Enabled = CheckboxHelper.AddCheckbox(ref disastersGroup,
                disasterContainer.Thunderstorm.GetName(),
                disasterContainer.Thunderstorm.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Thunderstorm.Enabled = isChecked;
                }, spacing: nextCheckboxSpacing);

            UI_Sinkhole_Enabled = CheckboxHelper.AddCheckbox(ref disastersGroup, disasterContainer.Sinkhole.GetName(),
                disasterContainer.Sinkhole.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Sinkhole.Enabled = isChecked;
                }, spacing: nextCheckboxSpacing);

            UI_Tornado_Enabled = CheckboxHelper.AddCheckbox(ref disastersGroup, disasterContainer.Tornado.GetName(),
                disasterContainer.Tornado.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.Enabled = isChecked;
                }, spacing: nextCheckboxSpacing);

            UI_Tsunami_Enabled = CheckboxHelper.AddCheckbox(ref disastersGroup, disasterContainer.Tsunami.GetName(),
                disasterContainer.Tsunami.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Tsunami.Enabled = isChecked;
                }, spacing: nextCheckboxSpacing);

            UI_Earthquake_Enabled = CheckboxHelper.AddCheckbox(ref disastersGroup,
                disasterContainer.Earthquake.GetName(),
                disasterContainer.Earthquake.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.Enabled = isChecked;
                }, spacing: nextCheckboxSpacing);

            UI_MeteorStrike_Enabled = CheckboxHelper.AddCheckbox(ref disastersGroup,
                disasterContainer.MeteorStrike.GetName(),
                disasterContainer.MeteorStrike.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.MeteorStrike.Enabled = isChecked;
                }, spacing: nextCheckboxSpacing);
        }

        private void SetupForestFire(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var forestFireGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.ForestFire.GetDisasterType()));

            UI_ForestFireMaxProbability = SliderHelper.AddSlider(
                ref forestFireGroup,
                LocalizationService.Get("settings.max_probability"), 1, 50, 1,
                disasterContainer.ForestFire.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.ForestFire.BaseOccurrencePerYear = val;
                }, LocalizationService.Get("settings.times_per_year"),
                LocalizationService.Get("settings.forest_fire.max_probability.tooltip"));

            UI_ForestFire_WarmupDays = SliderHelper.AddSlider(
                ref forestFireGroup,
                LocalizationService.Get("settings.warmup_period"), 0, 360, 10, disasterContainer.ForestFire.WarmupDays,
                delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.ForestFire.WarmupDays = (int)val;
                }, LocalizationService.Get("settings.warmup_period.days"),
                LocalizationService.Get("settings.forest_fire.warmup.tooltip"));

            DropDownHelper.AddDropDown(
                ref UI_ForestFire_EvacuationMode,
                ref forestFireGroup,
                EvacuationModeText,
                DisasterSimulationUtils.GetManualAndFocusedEvacuationOptions(),
                ref disasterContainer.ForestFire.EvacuationMode,
                delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.ForestFire.EvacuationMode = (EvacuationOptions)(selection * 2);
                }
            );

            helper.AddSpacing(20);
        }

        private void SetupThunderstorm(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var thunderstormGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.Thunderstorm.GetDisasterType()));

            UI_Thunderstorm_MaxProbability = SliderHelper.AddSlider(
                ref thunderstormGroup,
                LocalizationService.Get("settings.max_probability"), 0.1f, 10f, 0.1f,
                disasterContainer.Thunderstorm.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Thunderstorm.BaseOccurrencePerYear = val;
                }, LocalizationService.Get("settings.times_per_year"),
                LocalizationService.Get("settings.thunderstorm.max_probability.tooltip"));

            DropDownHelper.AddDropDown(
                ref UI_Thunderstorm_MaxProbabilityMonth,
                ref thunderstormGroup,
                LocalizationService.Get("settings.season_peak.thunderstorm"),
                DisasterSimulationUtils.GetMonths(),
                ref disasterContainer.Thunderstorm.MaxProbabilityMonth,
                delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Thunderstorm.MaxProbabilityMonth = selection + 1;
                }
            );

            helper.AddSpacing();

            UI_Thunderstorm_RainFactor = SliderHelper.AddSlider(
                ref thunderstormGroup,
                LocalizationService.Get("settings.rain_factor"), 1f, 5f, 0.1f,
                disasterContainer.Thunderstorm.RainFactor, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Thunderstorm.RainFactor = val;
                }, tooltip: LocalizationService.Get("settings.rain_factor.tooltip"));

            DropDownHelper.AddDropDown(
                ref UI_Thunderstorm_EvacuationMode,
                ref thunderstormGroup,
                EvacuationModeText,
                DisasterSimulationUtils.GetAllEvacuationOptions(),
                ref disasterContainer.Thunderstorm.EvacuationMode,
                delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Thunderstorm.EvacuationMode = (EvacuationOptions)selection;
                }
            );

            helper.AddSpacing(20);
        }

        private void SetupSinkhole(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var sinkholeGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.Sinkhole.GetDisasterType()));

            UI_Sinkhole_MaxProbability = SliderHelper.AddSlider(
                ref sinkholeGroup,
                LocalizationService.Get("settings.max_probability"), 0.1f, 10, 0.1f,
                disasterContainer.Sinkhole.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Sinkhole.BaseOccurrencePerYear = val;
                }, LocalizationService.Get("settings.times_per_year"),
                LocalizationService.Get("settings.sinkhole.max_probability.tooltip"));

            helper.AddSpacing();

            UI_Sinkhole_GroundwaterCapacity = SliderHelper.AddSlider(
                ref sinkholeGroup,
                LocalizationService.Get("settings.groundwater_capacity"), 1, 100, 1,
                disasterContainer.Sinkhole.GroundwaterCapacity, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Sinkhole.GroundwaterCapacity = val;
                }, tooltip: LocalizationService.Get("settings.groundwater_capacity.tooltip"));

            DropDownHelper.AddDropDown(
                ref UI_Sinkhole_EvacuationMode,
                ref sinkholeGroup,
                EvacuationModeText,
                DisasterSimulationUtils.GetAllEvacuationOptions(true),
                ref disasterContainer.Sinkhole.EvacuationMode,
                delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Sinkhole.EvacuationMode = (EvacuationOptions)selection;
                });

            helper.AddSpacing(20);
        }

        private void SetupTornado(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var tornadoGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.Tornado.GetDisasterType()));

            UI_Tornado_MaxProbability = SliderHelper.AddSlider(
                ref tornadoGroup,
                LocalizationService.Get("settings.max_probability"), 0.1f, 10f, 0.1f,
                disasterContainer.Tornado.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.BaseOccurrencePerYear = val;
                }, LocalizationService.Get("settings.times_per_year"),
                LocalizationService.Get("settings.tornado.max_probability.tooltip"));

            UI_Tornado_MaxProbabilityMonth = (UIDropDown)tornadoGroup.AddDropdown(
                LocalizationService.Get("settings.season_peak.tornado"),
                DisasterSimulationUtils.GetMonths(),
                disasterContainer.Tornado.MaxProbabilityMonth - 1,
                delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.MaxProbabilityMonth = selection + 1;
                });
            UIStyleHelper.ApplyDropDownStyle(UI_Tornado_MaxProbabilityMonth);

            UI_Tornado_NoDuringFog = CheckboxHelper.AddCheckbox(
                ref tornadoGroup,
                LocalizationService.Get("settings.no_tornado_fog"), disasterContainer.Tornado.NoTornadoDuringFog,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.NoTornadoDuringFog = isChecked;
                },
                LocalizationService.Get("settings.no_tornado_fog.tooltip"),
                nextCheckboxSpacing);

            UI_Tornado_EnableDestruction = CheckboxHelper.AddCheckbox(
                ref tornadoGroup,
                LocalizationService.Get("settings.enable_tornado_destruction"),
                disasterContainer.Tornado.EnableTornadoDestruction, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.EnableTornadoDestruction = isChecked;

                    UI_Tornado_IntensityDestructionStart.enabled = isChecked;
                });

            UI_Tornado_IntensityDestructionStart = SliderHelper.AddSlider(
                ref tornadoGroup,
                LocalizationService.Get("settings.min_tornado_destruction"), 0.1f, 25.5f, 0.1f,
                disasterContainer.Tornado.MinimalIntensityForDestruction, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.MinimalIntensityForDestruction = (byte)val;
                }, LocalizationService.Get("settings.min_tornado_destruction.suffix"));

            tornadoGroup.AddSpacing();

            DropDownHelper.AddDropDown(
                ref UI_Tornado_EvacuationMode,
                ref tornadoGroup,
                EvacuationModeText,
                DisasterSimulationUtils.GetAllEvacuationOptions(true),
                ref disasterContainer.Tornado.EvacuationMode, delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.EvacuationMode = (EvacuationOptions)selection;
                }
            );

            helper.AddSpacing(20);
        }

        private void SetupTsunami(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var tsunamiGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.Tsunami.GetDisasterType()));

            UI_Tsunami_MaxProbability = SliderHelper.AddSlider(
                ref tsunamiGroup,
                LocalizationService.Get("settings.max_probability"), 0.1f, 10, 0.1f,
                disasterContainer.Tsunami.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Tsunami.BaseOccurrencePerYear = val;
                }, LocalizationService.Get("settings.times_per_year"),
                LocalizationService.Get("settings.tsunami.max_probability.tooltip"));

            UI_Tsunami_WarmupYears = SliderHelper.AddSlider(ref tsunamiGroup,
                LocalizationService.Get("settings.charge_period"),
                0, 20, 0.5f, disasterContainer.Tsunami.WarmupYears, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Tsunami.WarmupYears = val;
                }, LocalizationService.Get("settings.charge_period.years"),
                LocalizationService.Get("settings.tsunami.warmup.tooltip"));

            DropDownHelper.AddDropDown(
                ref UI_Tsunami_EvacuationMode,
                ref tsunamiGroup,
                EvacuationModeText,
                DisasterSimulationUtils.GetAllEvacuationOptions(),
                ref disasterContainer.Tsunami.EvacuationMode, delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Tsunami.EvacuationMode = (EvacuationOptions)selection;
                }
            );

            helper.AddSpacing(20);
        }

        private void SetupEarthquake(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var earthquakeGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.Earthquake.GetDisasterType()));

            UI_Earthquake_MaxProbability = SliderHelper.AddSlider(
                ref earthquakeGroup,
                LocalizationService.Get("settings.max_probability"), 0.1f, 10, 0.1f,
                disasterContainer.Earthquake.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.BaseOccurrencePerYear = val;
                }, LocalizationService.Get("settings.times_per_year"),
                LocalizationService.Get("settings.earthquake.max_probability.tooltip"));

            UI_Earthquake_WarmupYears = SliderHelper.AddSlider(
                ref earthquakeGroup,
                LocalizationService.Get("settings.charge_period"), 0, 20, 0.5f,
                disasterContainer.Earthquake.WarmupYears, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.WarmupYears = val;
                }, LocalizationService.Get("settings.charge_period.years"),
                LocalizationService.Get("settings.earthquake.warmup.tooltip"));

            UI_Earthquake_AftershocksEnabled = CheckboxHelper.AddCheckbox(
                ref earthquakeGroup,
                LocalizationService.Get("settings.enable_aftershocks"), disasterContainer.Earthquake.AftershocksEnabled,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.AftershocksEnabled = isChecked;
                },
                LocalizationService.Get("settings.enable_aftershocks.tooltip"));

            DropDownHelper.AddDropDown(
                ref UI_Earthquake_CrackMode,
                ref earthquakeGroup,
                LocalizationService.Get("settings.ground_cracks"),
                DisasterSimulationUtils.GetCrackModes(),
                ref disasterContainer.Earthquake.EarthquakeCrackMode,
                delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.EarthquakeCrackMode = (EarthquakeCrackOptions)selection;
                }
            );
            UI_Earthquake_CrackMode.tooltip = LocalizationService.Get("settings.ground_cracks.tooltip");

            UI_Earthquake_MinIntensityToCrack = SliderHelper.AddSlider(
                ref earthquakeGroup,
                LocalizationService.Get("settings.min_intensity_cracks"), 10f, 25.5f, 0.1f,
                disasterContainer.Earthquake.MinimalIntensityForCracks, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.MinimalIntensityForCracks = (byte)val;
                }, LocalizationService.Get("settings.min_intensity_cracks.suffix"));
            UI_Earthquake_MinIntensityToCrack.tooltip =
                LocalizationService.Get("settings.min_intensity_cracks.tooltip");
            earthquakeGroup.AddSpacing(15);

            DropDownHelper.AddDropDown(
                ref UI_Earthquake_EvacuationMode,
                ref earthquakeGroup,
                EvacuationModeText,
                DisasterSimulationUtils.GetAllEvacuationOptions(),
                ref disasterContainer.Earthquake.EvacuationMode,
                delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.EvacuationMode = (EvacuationOptions)selection;
                }
            );

            helper.AddSpacing(20);
        }

        private void SetupMeteorStrike(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var meteorStrikeGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.MeteorStrike.GetDisasterType()));

            UI_MeteorStrike_MaxProbability = SliderHelper.AddSlider(
                ref meteorStrikeGroup,
                LocalizationService.Get("settings.max_probability"), 1f, 50, 1f,
                disasterContainer.MeteorStrike.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.MeteorStrike.BaseOccurrencePerYear = val;
                }, LocalizationService.Get("settings.times_per_year"),
                LocalizationService.Get("settings.meteor.max_probability.tooltip"));

            UI_MeteorStrike_MeteorLongPeriodEnabled = CheckboxHelper.AddCheckbox(
                ref meteorStrikeGroup,
                LocalizationService.Get("settings.enable_long_meteor"), disasterContainer.MeteorStrike.GetEnabled(0),
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.MeteorStrike.SetEnabled(0, isChecked);
                },
                spacing: nextCheckboxSpacing);

            UI_MeteorStrike_MeteorMediumPeriodEnabled = CheckboxHelper.AddCheckbox(
                ref meteorStrikeGroup,
                LocalizationService.Get("settings.enable_medium_meteor"), disasterContainer.MeteorStrike.GetEnabled(1),
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.MeteorStrike.SetEnabled(1, isChecked);
                }, spacing: nextCheckboxSpacing);

            UI_MeteorStrike_MeteorShortPeriodEnabled = CheckboxHelper.AddCheckbox(
                ref meteorStrikeGroup,
                LocalizationService.Get("settings.enable_short_meteor"), disasterContainer.MeteorStrike.GetEnabled(2),
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.MeteorStrike.SetEnabled(2, isChecked);
                });

            DropDownHelper.AddDropDown(
                ref UI_MeteorStrike_EvacuationMode,
                ref meteorStrikeGroup,
                EvacuationModeText,
                DisasterSimulationUtils.GetAllEvacuationOptions(true),
                ref disasterContainer.MeteorStrike.EvacuationMode,
                delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.MeteorStrike.EvacuationMode = (EvacuationOptions)selection;
                }
            );

            helper.AddSpacing(20);
        }

        private void SetupHotkeySetup(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            AddHotkeyContent(helper);
        }

        private void SetupDependenciesTab(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var dependenciesGroup = helper.AddGroup(LocalizationService.Get("settings.group.dependencies"));
            helper.AddSpacing();

            if (!(dependenciesGroup is UIHelper dependenciesUiHelper) || !(dependenciesUiHelper.self is UIPanel dependenciesPanel))
                return;

            var realTimeLabel = dependenciesPanel.AddUIComponent<UILabel>();
            var isRealTimeActive = IsRealTimeModActive();

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

        private void AddHotkeyContent(UIHelperBase helper)
        {
            var hotkeyGroup = helper.AddGroup(LocalizationService.Get("settings.group.hotkey"));

            if (!(hotkeyGroup is UIHelper hotkeyUiHelper) || !(hotkeyUiHelper.self is UIPanel hotkeyPanel))
                return;

            var hotkeyInfoLabel = hotkeyPanel.AddUIComponent<UILabel>();
            hotkeyInfoLabel.text = LocalizationService.Get("settings.hotkey.info");
            hotkeyInfoLabel.textScale = 0.9f;
            hotkeyInfoLabel.wordWrap = true;
            hotkeyInfoLabel.autoHeight = true;
            hotkeyInfoLabel.width = hotkeyPanel.width > 0 ? hotkeyPanel.width - 20f : 700f;

            UI_General_TogglePanelHotkeyField = HotkeyFieldHelper.AddHotkeyField(
                hotkeyPanel,
                LocalizationService.Get("settings.hotkey.field_label"),
                HotkeyHelper.FormatHotkey(
                    Services.DisasterSetup.TogglePanelHotkey,
                    Services.DisasterSetup.TogglePanelHotkeyModifiers),
                LocalizationService.Get("settings.hotkey.tooltip"),
                BeginHotkeyCapture,
                EndHotkeyCapture);
            RefreshHotkeyFieldText();
        }

        private void EndHotkeyCapture()
        {
            if (!IsCapturingHotkey)
                return;

            SetHotkeyCaptureState(false);
            RefreshHotkeyFieldText();
        }

        private static bool IsRealTimeModActive()
        {
            const string realTimeModName = "Real Time";
            const ulong realTimeWorkshopId = 1420955187;
            const ulong realTimeWorkshopId26 = 3059406297;

            foreach (var plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin?.userModInstance == null || !plugin.isEnabled)
                    continue;

                var userMod = plugin.userModInstance as IUserMod;
                var modName = userMod?.Name?.ToLowerInvariant() ?? string.Empty;
                var pluginName = plugin.name?.ToLowerInvariant() ?? string.Empty;
                var publishedFileId = plugin.publishedFileID.AsUInt64;

                if (modName.Contains(realTimeModName.ToLowerInvariant()) ||
                    pluginName.Contains(realTimeModName.ToLowerInvariant()) ||
                    publishedFileId == realTimeWorkshopId ||
                    publishedFileId == realTimeWorkshopId26)
                    return true;
            }

            return false;
        }

        #endregion Options UI
    }
}
