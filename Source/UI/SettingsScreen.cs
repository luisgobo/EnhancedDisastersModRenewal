using System.Linq;
using System.Reflection;
using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.BaseGameExtensions;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Setup;
using NaturalDisastersRenewal.UI.ComponentHelper;
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
        private UIButton UI_General_TogglePanelHotkeyButton;

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
        private bool isCapturingHotkey;

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

        public static void RebuildUISettingsOptions()
        {
            foreach (var current in Services.Plugins.GetPluginsInfo())
                if (current.isEnabled)
                {
                    var instances = current.GetInstances<IUserMod>();

                    var method = instances[0].GetType().GetMethod("EnhancedDisastersOptionsRebuildUI",
                        BindingFlags.Instance | BindingFlags.Public);

                    if (method != null)
                    {
                        method.Invoke(instances[0], new object[] { });
                        return;
                    }
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
                disasterSetupModel.MaxPopulationToTrigguerHigherDisasters;

            UI_General_ScaleMaxIntensityWithPopulation.isChecked = disasterSetupModel.ScaleMaxIntensityWithPopulation;
            UI_General_RecordDisasterEventsChkBox.isChecked = disasterSetupModel.RecordDisasterEvents;
            UI_General_ShowDisasterPanelButton.isChecked = disasterSetupModel.ShowDisasterPanelButton;
            RefreshHotkeyButtonText();

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

        private void AddLabelToSlider(object obj, string postfix = "")
        {
            var uISlider = obj as UISlider;
            if (uISlider == null) return;

            var label = uISlider.parent.AddUIComponent<UILabel>();
            label.text = uISlider.value + postfix;
            label.textScale = 1f;
            (uISlider.parent as UIPanel).autoLayout = false;
            label.position = new Vector3(uISlider.position.x + uISlider.width + 15, uISlider.position.y);

            var titleLabel = (uISlider.parent as UIPanel).Find<UILabel>("Label");
            titleLabel.anchor = UIAnchorStyle.None;
            titleLabel.position = new Vector3(titleLabel.position.x, titleLabel.position.y + 3);

            uISlider.eventValueChanged += delegate { label.text = uISlider.value + postfix; };
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

            SetupGeneralTab(ref helper, disasterContainer);
            SetupForestFire(ref helper, disasterContainer);
            SetupThunderstorm(ref helper, disasterContainer);
            SetupSinkhole(ref helper, disasterContainer);
            SetupTornado(ref helper, disasterContainer);
            SetupTsunami(ref helper, disasterContainer);
            SetupEarthquake(ref helper, disasterContainer);
            SetupMeteorStrike(ref helper, disasterContainer);
            SetupHotkeySetup(ref helper, disasterContainer);
            SetupSaveOptions(ref helper, disasterContainer);
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
            if (!isCapturingHotkey || eventType != EventType.KeyDown)
                return;

            if (HotkeyHelper.IsModifierKey(keyCode))
                return;

            if (keyCode == KeyCode.Escape && modifiers == EventModifiers.None)
            {
                isCapturingHotkey = false;
                RefreshHotkeyButtonText();
                return;
            }

            if ((keyCode == KeyCode.Backspace || keyCode == KeyCode.Delete) && modifiers == EventModifiers.None)
            {
                Services.DisasterSetup.TogglePanelHotkey = KeyCode.None;
                Services.DisasterSetup.TogglePanelHotkeyModifiers = EventModifiers.None;
                isCapturingHotkey = false;
                RefreshHotkeyButtonText();
                return;
            }

            if (keyCode == KeyCode.None || keyCode == KeyCode.Escape)
                return;

            EventModifiers normalizedModifiers = HotkeyHelper.GetSupportedHotkeyModifiers(modifiers);
            if (HotkeyHelper.CountHotkeyModifiers(normalizedModifiers) > 2)
                return;

            Services.DisasterSetup.TogglePanelHotkey = keyCode;
            Services.DisasterSetup.TogglePanelHotkeyModifiers = normalizedModifiers;
            isCapturingHotkey = false;
            RefreshHotkeyButtonText();
        }

        private void BeginHotkeyCapture()
        {
            isCapturingHotkey = true;
            RefreshHotkeyButtonText();
        }

        private void RefreshHotkeyButtonText()
        {
            if (UI_General_TogglePanelHotkeyButton == null)
                return;

            UI_General_TogglePanelHotkeyButton.text = isCapturingHotkey
                ? LocalizationService.Get("settings.hotkey.capture")
                : HotkeyHelper.FormatHotkey(
                    Services.DisasterSetup.TogglePanelHotkey,
                    Services.DisasterSetup.TogglePanelHotkeyModifiers);
        }

        private void ResetUIReferences()
        {
            UI_General_Language = null;
            UI_General_DisableDisasterFocus = null;
            UI_General_PauseOnDisasterStarts = null;
            UI_General_PartialEvacuationRadius = null;
            UI_General_MaxPopulationToTrigguerHigherDisasters = null;
            UI_General_ScaleMaxIntensityWithPopulation = null;
            UI_General_RecordDisasterEventsChkBox = null;
            UI_General_ShowDisasterPanelButton = null;
            UI_General_TogglePanelHotkeyButton = null;
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
            UI_General_Language.tooltip = LocalizationService.Get("settings.language.tooltip");
            UI_General_DisableDisasterFocus = (UICheckBox)generalGroup.AddCheckbox(
                LocalizationService.Get("settings.disable_follow"), disasterContainer.DisableDisasterFocus,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.DisableDisasterFocus = isChecked;
                        DisasterExtension.SetDisableDisasterFocus(disasterContainer.DisableDisasterFocus);
                    }
                });

            UI_General_PauseOnDisasterStarts = (UICheckBox)generalGroup.AddCheckbox(
                LocalizationService.Get("settings.pause_on_start"), disasterContainer.PauseOnDisasterStarts,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.PauseOnDisasterStarts = isChecked;
                });

            UI_General_PartialEvacuationRadius = (UISlider)generalGroup.AddSlider(
                LocalizationService.Get("settings.focused_radius"), 300f, 4200f, 100f,
                disasterContainer.PartialEvacuationRadius, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.PartialEvacuationRadius = val;
                });
            AddLabelToSlider(UI_General_PartialEvacuationRadius);
            UI_General_PartialEvacuationRadius.tooltip = LocalizationService.Get("settings.focused_radius.tooltip");

            generalGroup.AddSpace(5);
            UI_General_MaxPopulationToTrigguerHigherDisasters = (UISlider)generalGroup.AddSlider(
                LocalizationService.Get("settings.max_population"), 20000f, 800000f, 1000f,
                disasterContainer.MaxPopulationToTrigguerHigherDisasters, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.MaxPopulationToTrigguerHigherDisasters = val;
                });
            AddLabelToSlider(UI_General_MaxPopulationToTrigguerHigherDisasters);
            UI_General_MaxPopulationToTrigguerHigherDisasters.tooltip =
                LocalizationService.Get("settings.max_population.tooltip");

            generalGroup.AddSpace(10);

            UI_General_ScaleMaxIntensityWithPopulation = (UICheckBox)generalGroup.AddCheckbox(
                LocalizationService.Get("settings.scale_intensity"), disasterContainer.ScaleMaxIntensityWithPopulation,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.ScaleMaxIntensityWithPopulation = isChecked;
                });
            UI_General_ScaleMaxIntensityWithPopulation.tooltip =
                LocalizationService.Get("settings.scale_intensity.tooltip");

            UI_General_RecordDisasterEventsChkBox = (UICheckBox)generalGroup.AddCheckbox(
                LocalizationService.Get("settings.record_events"), disasterContainer.RecordDisasterEvents,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.RecordDisasterEvents = isChecked;
                });
            UI_General_RecordDisasterEventsChkBox.tooltip = LocalizationService.Get("settings.record_events.tooltip");

            UI_General_ShowDisasterPanelButton = (UICheckBox)generalGroup.AddCheckbox(
                LocalizationService.Get("settings.show_panel_button"), disasterContainer.ShowDisasterPanelButton,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.ShowDisasterPanelButton = isChecked;

                    Services.DisasterHandler.UpdateDisastersPanelToggleBtn();
                    Services.DisasterHandler.UpdateDisastersDPanel();
                });

            generalGroup.AddSpace(10);

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


            generalGroup.AddSpace(10);

            generalGroup.AddSpace(10);
            var disastersGroup = generalGroup.AddGroup(LocalizationService.Get("settings.enable_disasters"));

            UI_ForestFire_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.ForestFire.GetName(),
                disasterContainer.ForestFire.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.ForestFire.Enabled = isChecked;
                });
            UI_Thunderstorm_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Thunderstorm.GetName(),
                disasterContainer.Thunderstorm.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Thunderstorm.Enabled = isChecked;
                });
            UI_Sinkhole_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Sinkhole.GetName(),
                disasterContainer.Sinkhole.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Sinkhole.Enabled = isChecked;
                });
            UI_Tornado_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Tornado.GetName(),
                disasterContainer.Tornado.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.Enabled = isChecked;
                });
            UI_Tsunami_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Tsunami.GetName(),
                disasterContainer.Tsunami.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Tsunami.Enabled = isChecked;
                });
            UI_Earthquake_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Earthquake.GetName(),
                disasterContainer.Earthquake.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.Enabled = isChecked;
                });
            UI_MeteorStrike_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.MeteorStrike.GetName(),
                disasterContainer.MeteorStrike.Enabled, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.MeteorStrike.Enabled = isChecked;
                });
        }

        private void SetupForestFire(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var forestFireGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.ForestFire.GetDisasterType()));

            UI_ForestFireMaxProbability = (UISlider)forestFireGroup.AddSlider(
                LocalizationService.Get("settings.max_probability"), 1, 50, 1,
                disasterContainer.ForestFire.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.ForestFire.BaseOccurrencePerYear = val;
                });
            AddLabelToSlider(UI_ForestFireMaxProbability, LocalizationService.Get("settings.times_per_year"));
            UI_ForestFireMaxProbability.tooltip =
                LocalizationService.Get("settings.forest_fire.max_probability.tooltip");

            UI_ForestFire_WarmupDays = (UISlider)forestFireGroup.AddSlider(
                LocalizationService.Get("settings.warmup_period"), 0, 360, 10, disasterContainer.ForestFire.WarmupDays,
                delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.ForestFire.WarmupDays = (int)val;
                });
            AddLabelToSlider(UI_ForestFire_WarmupDays, LocalizationService.Get("settings.warmup_period.days"));
            UI_ForestFire_WarmupDays.tooltip = LocalizationService.Get("settings.forest_fire.warmup.tooltip");

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

            helper.AddSpace(20);
        }

        private void SetupThunderstorm(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var thunderstormGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.Thunderstorm.GetDisasterType()));

            UI_Thunderstorm_MaxProbability = (UISlider)thunderstormGroup.AddSlider(
                LocalizationService.Get("settings.max_probability"), 0.1f, 10f, 0.1f,
                disasterContainer.Thunderstorm.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Thunderstorm.BaseOccurrencePerYear = val;
                });
            AddLabelToSlider(UI_Thunderstorm_MaxProbability, LocalizationService.Get("settings.times_per_year"));
            UI_Thunderstorm_MaxProbability.tooltip =
                LocalizationService.Get("settings.thunderstorm.max_probability.tooltip");

            UI_Thunderstorm_MaxProbabilityMonth = (UIDropDown)thunderstormGroup.AddDropdown(
                LocalizationService.Get("settings.season_peak.thunderstorm"),
                DisasterSimulationUtils.GetMonths(),
                disasterContainer.Thunderstorm.MaxProbabilityMonth - 1,
                delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Thunderstorm.MaxProbabilityMonth = selection + 1;
                });

            UI_Thunderstorm_RainFactor = (UISlider)thunderstormGroup.AddSlider(
                LocalizationService.Get("settings.rain_factor"), 1f, 5f, 0.1f,
                disasterContainer.Thunderstorm.RainFactor, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Thunderstorm.RainFactor = val;
                });
            AddLabelToSlider(UI_Thunderstorm_RainFactor);
            UI_Thunderstorm_RainFactor.tooltip = LocalizationService.Get("settings.rain_factor.tooltip");

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

            helper.AddSpace(20);
        }

        private void SetupSinkhole(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var sinkholeGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.Sinkhole.GetDisasterType()));

            UI_Sinkhole_MaxProbability = (UISlider)sinkholeGroup.AddSlider(
                LocalizationService.Get("settings.max_probability"), 0.1f, 10, 0.1f,
                disasterContainer.Sinkhole.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Sinkhole.BaseOccurrencePerYear = val;
                });
            AddLabelToSlider(UI_Sinkhole_MaxProbability, LocalizationService.Get("settings.times_per_year"));
            UI_Sinkhole_MaxProbability.tooltip = LocalizationService.Get("settings.sinkhole.max_probability.tooltip");

            UI_Sinkhole_GroundwaterCapacity = (UISlider)sinkholeGroup.AddSlider(
                LocalizationService.Get("settings.groundwater_capacity"), 1, 100, 1,
                disasterContainer.Sinkhole.GroundwaterCapacity, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Sinkhole.GroundwaterCapacity = val;
                });
            AddLabelToSlider(UI_Sinkhole_GroundwaterCapacity);
            UI_Sinkhole_GroundwaterCapacity.tooltip = LocalizationService.Get("settings.groundwater_capacity.tooltip");

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

            helper.AddSpace(20);
        }

        private void SetupTornado(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var tornadoGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.Tornado.GetDisasterType()));

            UI_Tornado_MaxProbability = (UISlider)tornadoGroup.AddSlider(
                LocalizationService.Get("settings.max_probability"), 0.1f, 10f, 0.1f,
                disasterContainer.Tornado.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.BaseOccurrencePerYear = val;
                });
            AddLabelToSlider(UI_Tornado_MaxProbability, LocalizationService.Get("settings.times_per_year"));
            UI_Tornado_MaxProbability.tooltip = LocalizationService.Get("settings.tornado.max_probability.tooltip");

            UI_Tornado_MaxProbabilityMonth = (UIDropDown)tornadoGroup.AddDropdown(
                LocalizationService.Get("settings.season_peak.tornado"),
                DisasterSimulationUtils.GetMonths(),
                disasterContainer.Tornado.MaxProbabilityMonth - 1,
                delegate(int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.MaxProbabilityMonth = selection + 1;
                });

            UI_Tornado_NoDuringFog = (UICheckBox)tornadoGroup.AddCheckbox(
                LocalizationService.Get("settings.no_tornado_fog"), disasterContainer.Tornado.NoTornadoDuringFog,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.NoTornadoDuringFog = isChecked;
                });
            UI_Tornado_NoDuringFog.tooltip = LocalizationService.Get("settings.no_tornado_fog.tooltip");

            UI_Tornado_EnableDestruction = (UICheckBox)tornadoGroup.AddCheckbox(
                LocalizationService.Get("settings.enable_tornado_destruction"),
                disasterContainer.Tornado.EnableTornadoDestruction, delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.EnableTornadoDestruction = isChecked;

                    UI_Tornado_IntensityDestructionStart.enabled = isChecked;
                });

            UI_Tornado_IntensityDestructionStart = (UISlider)tornadoGroup.AddSlider(
                LocalizationService.Get("settings.min_tornado_destruction"), 0.1f, 25.5f, 0.1f,
                disasterContainer.Tornado.MinimalIntensityForDestruction, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.MinimalIntensityForDestruction = (byte)val;
                });
            AddLabelToSlider(UI_Tornado_IntensityDestructionStart,
                LocalizationService.Get("settings.min_tornado_destruction.suffix"));

            tornadoGroup.AddSpace(10);

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

            helper.AddSpace(20);
        }

        private void SetupTsunami(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var tsunamiGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.Tsunami.GetDisasterType()));

            UI_Tsunami_MaxProbability = (UISlider)tsunamiGroup.AddSlider(
                LocalizationService.Get("settings.max_probability"), 0.1f, 10, 0.1f,
                disasterContainer.Tsunami.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Tsunami.BaseOccurrencePerYear = val;
                });
            AddLabelToSlider(UI_Tsunami_MaxProbability, LocalizationService.Get("settings.times_per_year"));
            UI_Tsunami_MaxProbability.tooltip = LocalizationService.Get("settings.tsunami.max_probability.tooltip");

            UI_Tsunami_WarmupYears = (UISlider)tsunamiGroup.AddSlider(LocalizationService.Get("settings.charge_period"),
                0, 20, 0.5f, disasterContainer.Tsunami.WarmupYears, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Tsunami.WarmupYears = val;
                });
            AddLabelToSlider(UI_Tsunami_WarmupYears, LocalizationService.Get("settings.charge_period.years"));
            UI_Tsunami_WarmupYears.tooltip = LocalizationService.Get("settings.tsunami.warmup.tooltip");

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

            helper.AddSpace(20);
        }

        private void SetupEarthquake(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var earthquakeGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.Earthquake.GetDisasterType()));

            UI_Earthquake_MaxProbability = (UISlider)earthquakeGroup.AddSlider(
                LocalizationService.Get("settings.max_probability"), 0.1f, 10, 0.1f,
                disasterContainer.Earthquake.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.BaseOccurrencePerYear = val;
                });
            AddLabelToSlider(UI_Earthquake_MaxProbability, LocalizationService.Get("settings.times_per_year"));
            UI_Earthquake_MaxProbability.tooltip =
                LocalizationService.Get("settings.earthquake.max_probability.tooltip");

            UI_Earthquake_WarmupYears = (UISlider)earthquakeGroup.AddSlider(
                LocalizationService.Get("settings.charge_period"), 0, 20, 0.5f,
                disasterContainer.Earthquake.WarmupYears, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.WarmupYears = val;
                });
            AddLabelToSlider(UI_Earthquake_WarmupYears, LocalizationService.Get("settings.charge_period.years"));
            UI_Earthquake_WarmupYears.tooltip = LocalizationService.Get("settings.earthquake.warmup.tooltip");

            UI_Earthquake_AftershocksEnabled = (UICheckBox)earthquakeGroup.AddCheckbox(
                LocalizationService.Get("settings.enable_aftershocks"), disasterContainer.Earthquake.AftershocksEnabled,
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.AftershocksEnabled = isChecked;
                });
            UI_Earthquake_AftershocksEnabled.tooltip = LocalizationService.Get("settings.enable_aftershocks.tooltip");

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

            UI_Earthquake_MinIntensityToCrack = (UISlider)earthquakeGroup.AddSlider(
                LocalizationService.Get("settings.min_intensity_cracks"), 10f, 25.5f, 0.1f,
                disasterContainer.Earthquake.MinimalIntensityForCracks, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.Earthquake.MinimalIntensityForCracks = (byte)val;
                });
            AddLabelToSlider(UI_Earthquake_MinIntensityToCrack,
                LocalizationService.Get("settings.min_intensity_cracks.suffix"));
            UI_Earthquake_MinIntensityToCrack.tooltip =
                LocalizationService.Get("settings.min_intensity_cracks.tooltip");
            earthquakeGroup.AddSpace(15);

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

            helper.AddSpace(20);
        }

        private void SetupMeteorStrike(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var meteorStrikeGroup =
                helper.AddGroup(LocalizationService.GetDisasterName(disasterContainer.MeteorStrike.GetDisasterType()));

            UI_MeteorStrike_MaxProbability = (UISlider)meteorStrikeGroup.AddSlider(
                LocalizationService.Get("settings.max_probability"), 1f, 50, 1f,
                disasterContainer.MeteorStrike.BaseOccurrencePerYear, delegate(float val)
                {
                    if (!freezeUI)
                        disasterContainer.MeteorStrike.BaseOccurrencePerYear = val;
                });
            AddLabelToSlider(UI_MeteorStrike_MaxProbability, LocalizationService.Get("settings.times_per_year"));
            UI_MeteorStrike_MaxProbability.tooltip = LocalizationService.Get("settings.meteor.max_probability.tooltip");

            UI_MeteorStrike_MeteorLongPeriodEnabled = (UICheckBox)meteorStrikeGroup.AddCheckbox(
                LocalizationService.Get("settings.enable_long_meteor"), disasterContainer.MeteorStrike.GetEnabled(0),
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.MeteorStrike.SetEnabled(0, isChecked);
                });

            UI_MeteorStrike_MeteorMediumPeriodEnabled = (UICheckBox)meteorStrikeGroup.AddCheckbox(
                LocalizationService.Get("settings.enable_medium_meteor"), disasterContainer.MeteorStrike.GetEnabled(1),
                delegate(bool isChecked)
                {
                    if (!freezeUI)
                        disasterContainer.MeteorStrike.SetEnabled(1, isChecked);
                });

            UI_MeteorStrike_MeteorShortPeriodEnabled = (UICheckBox)meteorStrikeGroup.AddCheckbox(
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

            helper.AddSpace(20);
        }

        private void SetupHotkeySetup(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var hotkeyGroup = helper.AddGroup(LocalizationService.Get("settings.group.hotkey"));

            if (hotkeyGroup is UIHelper hotkeyUiHelper && hotkeyUiHelper.self is UIPanel hotkeyPanel)
            {
                UILabel hotkeyInfoLabel = hotkeyPanel.AddUIComponent<UILabel>();
                hotkeyInfoLabel.text = LocalizationService.Get("settings.hotkey.info");
                hotkeyInfoLabel.textScale = 0.9f;
                hotkeyInfoLabel.wordWrap = true;
                hotkeyInfoLabel.autoHeight = true;
                hotkeyInfoLabel.width = hotkeyPanel.width > 0 ? hotkeyPanel.width - 20f : 700f;
            }

            UI_General_TogglePanelHotkeyButton = (UIButton)hotkeyGroup.AddButton(
                LocalizationService.Get("settings.hotkey.capture"),
                delegate { BeginHotkeyCapture(); });
            UI_General_TogglePanelHotkeyButton.tooltip = LocalizationService.Get("settings.hotkey.tooltip");
            RefreshHotkeyButtonText();
        }

        private void SetupSaveOptions(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            // Save buttons
            var saveOptionsGroup = helper.AddGroup(LocalizationService.Get("settings.save_options"));

            saveOptionsGroup.AddButton(LocalizationService.Get("settings.save_default"),
                delegate { Services.DisasterSetup.Save(); });
            saveOptionsGroup.AddButton(LocalizationService.Get("settings.reset_saved"), delegate
            {
                Services.DisasterHandler.ReadValuesFromFile();
                UpdateSetupContentUI();
            });
            saveOptionsGroup.AddButton(LocalizationService.Get("settings.reset_defaults"), delegate
            {
                Services.DisasterHandler.ResetToDefaultValues();
                UpdateSetupContentUI();
            });
        }

        #endregion Options UI
    }
}
