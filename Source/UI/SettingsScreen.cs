using System.Reflection;
using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.BaseGameExtensions;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Setup;
using NaturalDisastersRenewal.UI.ComponentHelper;
using UnityEngine;
using Helper = NaturalDisastersRenewal.Common.Helper;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.UI
{
    public class SettingsScreen
    {
        private bool _freezeUI;
        private string EvacuationModeText => LocalizationService.Get("settings.evacuationMode");
        private UIHelper _rootHelper;

        #region UI Components

        //General
        private UIDropDown _uiGeneralLanguage;
        private UICheckBox _uiGeneralDisableDisasterFocus;
        private UICheckBox _uiGeneralPauseOnDisasterStarts;
        private UISlider _uiGeneralPartialEvacuationRadius;
        private UISlider _uiGeneralMaxPopulationToTrigguerHigherDisasters;
        private UICheckBox _uiGeneralScaleMaxIntensityWithPopulation;
        private UICheckBox _uiGeneralRecordDisasterEventsChkBox;
        private UICheckBox _uiGeneralShowDisasterPanelButton;
        private UIButton _uiGeneralTogglePanelHotkeyButton;

        //Forest Fire
        private UICheckBox _uiForestFireEnabled;
        private UISlider _uiForestFireMaxProbability;
        private UISlider _uiForestFireWarmupDays;
        private UIDropDown _uiForestFireEvacuationMode;

        //Thunderstorm
        private UICheckBox _uiThunderstormEnabled;
        private UISlider _uiThunderstormMaxProbability;
        private UIDropDown _uiThunderstormMaxProbabilityMonth;
        private UISlider _uiThunderstormRainFactor;
        private UIDropDown _uiThunderstormEvacuationMode;

        //Sunkhole
        private UICheckBox _uiSinkholeEnabled;
        private UISlider _uiSinkholeMaxProbability;
        private UISlider _uiSinkholeGroundwaterCapacity;
        private UIDropDown _uiSinkholeEvacuationMode;

        //Tornado
        private UICheckBox _uiTornadoEnabled;
        private UISlider _uiTornadoMaxProbability;
        private UIDropDown _uiTornadoMaxProbabilityMonth;
        private UICheckBox _uiTornadoNoDuringFog;
        private UIDropDown _uiTornadoEvacuationMode;
        private UICheckBox _uiTornadoEnableDestruction;
        private UISlider _uiTornadoIntensityDestructionStart;

        //Tsunami
        private UICheckBox _uiTsunamiEnabled;
        private UISlider _uiTsunamiMaxProbability;
        private UISlider _uiTsunamiWarmupYears;
        private UISlider _uiTsunamiRealTimeProgressMultiplier;
        private UIDropDown _uiTsunamiEvacuationMode;

        //Earthquake
        private UICheckBox _uiEarthquakeEnabled;
        private UISlider _uiEarthquakeMinIntensityToCrack;
        private UISlider _uiEarthquakeMaxProbability;
        private UISlider _uiEarthquakeWarmupYears;
        private UICheckBox _uiEarthquakeAftershocksEnabled;

        //UICheckBox UI_Earthquake_NoCrack;
        private UIDropDown _uiEarthquakeCrackMode;
        private UIDropDown _uiEarthquakeEvacuationMode;

        //Meteor Strike
        private UICheckBox _uiMeteorStrikeEnabled;
        private UISlider _uiMeteorStrikeMaxProbability;
        private UISlider _uiMeteorStrikeRealTimeFrequencyMultiplier;
        private UICheckBox _uiMeteorStrikeMeteorLongPeriodEnabled;
        private UICheckBox _uiMeteorStrikeMeteorMediumPeriodEnabled;
        private UICheckBox _uiMeteorStrikeMeteorShortPeriodEnabled;
        private UIDropDown _uiMeteorStrikeEvacuationMode;

        #endregion UI Components

        #region Options UI

        private bool _hotkeyCaptureHandlerRegistered;
        private bool _isCapturingHotkey;

        public static void UpdateUISettingsOptions()
        {
            foreach (var current in CommonServices.Plugins.GetPluginsInfo())
            {
                if (!current.isEnabled) continue;

                var instances = current.GetInstances<IUserMod>();

                var method = instances[0].GetType().GetMethod("EnhancedDisastersOptionsUpdateUI", BindingFlags.Instance | BindingFlags.Public);

                if (method == null) continue;

                method.Invoke(instances[0], []);
                return;
            }
        }

        public void UpdateSetupContentUI()
        {
            if (_uiForestFireEnabled == null)
                return;

            var disasterSetupModel = CommonServices.DisasterSetup;
            _freezeUI = true;

            _uiGeneralLanguage.selectedIndex = (int)disasterSetupModel.Language;
            _uiGeneralDisableDisasterFocus.isChecked = disasterSetupModel.DisableDisasterFocus;
            _uiGeneralPauseOnDisasterStarts.isChecked = disasterSetupModel.PauseOnDisasterStarts;
            _uiGeneralPartialEvacuationRadius.value = disasterSetupModel.PartialEvacuationRadius;
            _uiGeneralMaxPopulationToTrigguerHigherDisasters.value = disasterSetupModel.MaxPopulationToTriggerHigherDisasters;

            _uiGeneralScaleMaxIntensityWithPopulation.isChecked = disasterSetupModel.ScaleMaxIntensityWithPopulation;
            _uiGeneralRecordDisasterEventsChkBox.isChecked = disasterSetupModel.RecordDisasterEvents;
            _uiGeneralShowDisasterPanelButton.isChecked = disasterSetupModel.ShowDisasterPanelButton;
            RefreshHotkeyButtonText();

            _uiForestFireEnabled.isChecked = disasterSetupModel.ForestFire.IsDisasterEnabled;
            _uiForestFireEvacuationMode.selectedIndex = (int)disasterSetupModel.ForestFire.EvacuationMode;
            _uiForestFireMaxProbability.value = disasterSetupModel.ForestFire.BaseOccurrencePerYear;
            _uiForestFireWarmupDays.value = disasterSetupModel.ForestFire.WarmupDays;

            _uiThunderstormEnabled.isChecked = disasterSetupModel.Thunderstorm.IsDisasterEnabled;
            _uiThunderstormEvacuationMode.selectedIndex = (int)disasterSetupModel.Thunderstorm.EvacuationMode;
            _uiThunderstormMaxProbability.value = disasterSetupModel.Thunderstorm.BaseOccurrencePerYear;
            _uiThunderstormMaxProbabilityMonth.selectedIndex = disasterSetupModel.Thunderstorm.MaxProbabilityMonth - 1;
            _uiThunderstormRainFactor.value = disasterSetupModel.Thunderstorm.RainFactor;

            _uiSinkholeEnabled.isChecked = disasterSetupModel.Sinkhole.IsDisasterEnabled;
            _uiSinkholeEvacuationMode.selectedIndex = (int)disasterSetupModel.Sinkhole.EvacuationMode;
            _uiSinkholeMaxProbability.value = disasterSetupModel.Sinkhole.BaseOccurrencePerYear;
            _uiSinkholeGroundwaterCapacity.value = disasterSetupModel.Sinkhole.GroundwaterCapacity;

            _uiTornadoEnabled.isChecked = disasterSetupModel.Tornado.IsDisasterEnabled;
            _uiTornadoEvacuationMode.selectedIndex = (int)disasterSetupModel.Tornado.EvacuationMode;
            _uiTornadoMaxProbability.value = disasterSetupModel.Tornado.BaseOccurrencePerYear;
            _uiTornadoMaxProbabilityMonth.selectedIndex = disasterSetupModel.Tornado.MaxProbabilityMonth - 1;
            _uiTornadoNoDuringFog.isChecked = disasterSetupModel.Tornado.NoTornadoDuringFog;
            _uiTornadoEnableDestruction.isChecked = disasterSetupModel.Tornado.EnableTornadoDestruction;
            _uiTornadoIntensityDestructionStart.value = disasterSetupModel.Tornado.MinimalIntensityForDestruction;

            _uiTsunamiEnabled.isChecked = disasterSetupModel.Tsunami.IsDisasterEnabled;
            _uiTsunamiEvacuationMode.selectedIndex = (int)disasterSetupModel.Tsunami.EvacuationMode;
            _uiTsunamiMaxProbability.value = disasterSetupModel.Tsunami.BaseOccurrencePerYear;
            _uiTsunamiWarmupYears.value = disasterSetupModel.Tsunami.WarmupYears;
            _uiTsunamiRealTimeProgressMultiplier.value = disasterSetupModel.Tsunami.RealTimeProgressMultiplier;
            _uiTsunamiRealTimeProgressMultiplier.enabled = CommonServices.DisasterHandler.CheckRealTimeModActive();

            _uiEarthquakeEnabled.isChecked = disasterSetupModel.Earthquake.IsDisasterEnabled;
            _uiEarthquakeEvacuationMode.selectedIndex = (int)disasterSetupModel.Earthquake.EvacuationMode;
            _uiEarthquakeMinIntensityToCrack.value = (int)disasterSetupModel.Earthquake.MinimalIntensityForCracks;
            _uiEarthquakeMaxProbability.value = disasterSetupModel.Earthquake.BaseOccurrencePerYear;
            _uiEarthquakeWarmupYears.value = disasterSetupModel.Earthquake.WarmupYears;
            _uiEarthquakeAftershocksEnabled.isChecked = disasterSetupModel.Earthquake.AftershocksEnabled;
            _uiEarthquakeCrackMode.selectedIndex = (int)disasterSetupModel.Earthquake.EarthquakeCrackMode;

            _uiMeteorStrikeEnabled.isChecked = disasterSetupModel.MeteorStrike.IsDisasterEnabled;
            _uiMeteorStrikeEvacuationMode.selectedIndex = (int)disasterSetupModel.MeteorStrike.EvacuationMode;
            _uiMeteorStrikeMaxProbability.value = disasterSetupModel.MeteorStrike.BaseOccurrencePerYear;
            _uiMeteorStrikeRealTimeFrequencyMultiplier.value = disasterSetupModel.MeteorStrike.RealTimeFrequencyMultiplier;
            _uiMeteorStrikeRealTimeFrequencyMultiplier.enabled = CommonServices.DisasterHandler.CheckRealTimeModActive();
            _uiMeteorStrikeMeteorLongPeriodEnabled.isChecked = disasterSetupModel.MeteorStrike.GetEnabled(0);
            _uiMeteorStrikeMeteorMediumPeriodEnabled.isChecked = disasterSetupModel.MeteorStrike.GetEnabled(1);
            _uiMeteorStrikeMeteorShortPeriodEnabled.isChecked = disasterSetupModel.MeteorStrike.GetEnabled(2);

            _freezeUI = false;
        }

        void AddLabelToSlider(object obj, string postfix = "")
        {
            var uISlider = obj as UISlider;
            
            if (uISlider == null) return;

            var sliderPanel = uISlider.parent as UIPanel;
            if (sliderPanel == null) return;

            sliderPanel.autoLayout = false;

            UILabel titleLabel = sliderPanel.Find<UILabel>("Label");
            if (titleLabel != null)
            {
                titleLabel.anchor = UIAnchorStyle.None;
                titleLabel.wordWrap = true;
                titleLabel.autoHeight = true;
                titleLabel.position = new Vector3(titleLabel.position.x, titleLabel.position.y + 3);

                // Expand slider rows when the title wraps to multiple lines.
                const float singleLineTitleHeight = 22f;
                var wrappedExtraHeight = Mathf.Max(0f, titleLabel.height - singleLineTitleHeight);
                if (wrappedExtraHeight > 0f)
                {
                    uISlider.position = new Vector3(uISlider.position.x, uISlider.position.y + wrappedExtraHeight, uISlider.position.z);
                    sliderPanel.height += wrappedExtraHeight;
                }
            }

            UILabel label = sliderPanel.AddUIComponent<UILabel>();
            label.text = uISlider.value.ToString() + postfix;
            label.textScale = 1f;
            label.position = new Vector3(uISlider.position.x + uISlider.width + 15, uISlider.position.y);

            uISlider.eventValueChanged += new PropertyChangedEventHandler<float>(delegate (UIComponent component, float value)
            {
                label.text = uISlider.value.ToString() + postfix;
            });
        }

        public void BuildSettingsMenu(UIHelper helper)
        {
            _rootHelper = helper;
            EnsureHotkeyCaptureRegistered();
            var disasterContainer = CommonServices.DisasterSetup;

            SetupGeneralTab(ref helper, disasterContainer);
            SetupForestFire(ref helper, disasterContainer);
            SetupThunderstorm(ref helper, disasterContainer);
            SetupSinkhole(ref helper, disasterContainer);
            SetupTornado(ref helper, disasterContainer);
            SetupTsunami(ref helper, disasterContainer);
            SetupEarthquake(ref helper, disasterContainer);
            SetupMeteorStrike(ref helper, disasterContainer);
            //SetupHotkeySetup(ref helper, disasterContainer);
            SetupSaveOptions(ref helper, disasterContainer);

        }

        private void RebuildSettingsMenu()
        {
            if (_rootHelper?.self is not UIComponent rootComponent)
                return;

            _freezeUI = true;
            _isCapturingHotkey = false;

            while (rootComponent.components.Count > 0)
            {
                var child = rootComponent.components[0];
                if (child == null)
                {
                    break;
                }

                Object.DestroyImmediate(child.gameObject);
            }

            BuildSettingsMenu(_rootHelper);
            _freezeUI = false;
        }

        private void EnsureHotkeyCaptureRegistered()
        {
            if (_hotkeyCaptureHandlerRegistered)
                return;

            UIInput.eventProcessKeyEvent += HotkeyCapture_eventProcessKeyEvent;
            _hotkeyCaptureHandlerRegistered = true;
        }

        private void HotkeyCapture_eventProcessKeyEvent(EventType eventType, KeyCode keyCode, EventModifiers modifiers)
        {
            if (!_isCapturingHotkey || eventType != EventType.KeyDown)
                return;

            if (Helper.IsModifierKey(keyCode))
                return;

            if (keyCode == KeyCode.Escape && modifiers == EventModifiers.None)
            {
                _isCapturingHotkey = false;
                RefreshHotkeyButtonText();
                return;
            }

            if ((keyCode == KeyCode.Backspace || keyCode == KeyCode.Delete) && modifiers == EventModifiers.None)
            {
                CommonServices.DisasterSetup.TogglePanelHotkey = KeyCode.None;
                CommonServices.DisasterSetup.TogglePanelHotkeyModifiers = EventModifiers.None;
                _isCapturingHotkey = false;
                RefreshHotkeyButtonText();
                return;
            }

            if (keyCode == KeyCode.None || keyCode == KeyCode.Escape)
                return;

            var normalizedModifiers = Helper.GetSupportedHotkeyModifiers(modifiers);
            if (Helper.CountHotkeyModifiers(normalizedModifiers) > 2)
                return;

            CommonServices.DisasterSetup.TogglePanelHotkey = keyCode;
            CommonServices.DisasterSetup.TogglePanelHotkeyModifiers = normalizedModifiers;
            _isCapturingHotkey = false;
            RefreshHotkeyButtonText();
        }

        private void BeginHotkeyCapture()
        {
            _isCapturingHotkey = true;
            RefreshHotkeyButtonText();
        }

        private void RefreshHotkeyButtonText()
        {
            if (_uiGeneralTogglePanelHotkeyButton == null)
                return;

            _uiGeneralTogglePanelHotkeyButton.text = _isCapturingHotkey
                ? LocalizationService.Get("settings.hotkey.capture")
                : Helper.FormatHotkey(
                    CommonServices.DisasterSetup.TogglePanelHotkey,
                    CommonServices.DisasterSetup.TogglePanelHotkeyModifiers);
        }

        void SetupGeneralTab(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var generalGroup = helper.AddGroup(LocalizationService.Get("settings.group.general"));

            _uiGeneralLanguage = (UIDropDown)generalGroup.AddDropdown(
                LocalizationService.Get("settings.language"),
                LocalizationService.GetLanguageDisplayNames(),
                (int)disasterContainer.Language,
                delegate(int selection)
                {
                    if (_freezeUI)
                        return;

                    disasterContainer.Language = (ModLanguage)selection;
                    CommonServices.DisasterHandler.RefreshLocalizedUI();
                    RebuildSettingsMenu();
                });
            _uiGeneralLanguage.tooltip = LocalizationService.Get("settings.language.tooltip");

            generalGroup.AddGroup(LocalizationService.Get("settings.tooltip.timeFlow"));

            _uiGeneralDisableDisasterFocus = (UICheckBox)generalGroup.AddCheckbox(LocalizationService.Get("settings.disableFollow"), disasterContainer.DisableDisasterFocus, delegate(bool isChecked)
            {
                if (!_freezeUI)
                {
                    disasterContainer.DisableDisasterFocus = isChecked;
                    DisasterExtension.SetDisableDisasterFocus(disasterContainer.DisableDisasterFocus);
                }
            });

            _uiGeneralPauseOnDisasterStarts = (UICheckBox)generalGroup.AddCheckbox(LocalizationService.Get("settings.pauseOnStart"), disasterContainer.PauseOnDisasterStarts, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.PauseOnDisasterStarts = isChecked;
            });

            _uiGeneralPartialEvacuationRadius = (UISlider)generalGroup.AddSlider(LocalizationService.Get("settings.focusedRadius"), 300f, 4200f, 100f, disasterContainer.PartialEvacuationRadius, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.PartialEvacuationRadius = val;
            });
            AddLabelToSlider(_uiGeneralPartialEvacuationRadius);
            _uiGeneralPartialEvacuationRadius.tooltip = LocalizationService.Get("settings.tooltip.focusedRadius");

            generalGroup.AddSpace(5);
            _uiGeneralMaxPopulationToTrigguerHigherDisasters = (UISlider)generalGroup.AddSlider(LocalizationService.Get("settings.maxPopulation"), 20000f, 800000f, 1000f, disasterContainer.MaxPopulationToTriggerHigherDisasters, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.MaxPopulationToTriggerHigherDisasters = val;
            });
            AddLabelToSlider(_uiGeneralMaxPopulationToTrigguerHigherDisasters);
            _uiGeneralMaxPopulationToTrigguerHigherDisasters.tooltip = LocalizationService.Get("settings.tooltip.maxPopulation");

            generalGroup.AddSpace(10);

            _uiGeneralScaleMaxIntensityWithPopulation = (UICheckBox)generalGroup.AddCheckbox(LocalizationService.Get("settings.scaleIntensity"), disasterContainer.ScaleMaxIntensityWithPopulation, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.ScaleMaxIntensityWithPopulation = isChecked;
            });
            _uiGeneralScaleMaxIntensityWithPopulation.tooltip = LocalizationService.Get("settings.tooltip.scaleIntensity");

            _uiGeneralRecordDisasterEventsChkBox = (UICheckBox)generalGroup.AddCheckbox(LocalizationService.Get("settings.recordEvents"), disasterContainer.RecordDisasterEvents, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.RecordDisasterEvents = isChecked;
            });
            _uiGeneralRecordDisasterEventsChkBox.tooltip = LocalizationService.Get("settings.tooltip.recordEvents");

            _uiGeneralShowDisasterPanelButton = (UICheckBox)generalGroup.AddCheckbox(LocalizationService.Get("settings.showPanelButton"), disasterContainer.ShowDisasterPanelButton, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.ShowDisasterPanelButton = isChecked;

                CommonServices.DisasterHandler.UpdateDisastersPanelToggleBtn();
                CommonServices.DisasterHandler.UpdateDisastersDPanel();
            });
            _uiGeneralShowDisasterPanelButton.tooltip = LocalizationService.Get("settings.tooltip.showPanelButton");

            generalGroup.AddSpace(10);

            var elementPositionsGroup = generalGroup.AddGroup(LocalizationService.Get("settings.group.positions"));

            elementPositionsGroup.AddButton(LocalizationService.Get("settings.resetButtonPosition"), delegate
            {
                CommonServices.DisasterHandler.ResetToDefaultValues(true, false);
                UpdateSetupContentUI();
            });

            elementPositionsGroup.AddButton(LocalizationService.Get("settings.resetPanelPosition"), delegate
            {
                CommonServices.DisasterHandler.ResetToDefaultValues(false, true);
                UpdateSetupContentUI();
            });

            var hotkeyGroup = generalGroup.AddGroup(LocalizationService.Get("settings.togglePanelHotkey"));
            if (hotkeyGroup is UIHelper hotkeyUiHelper && hotkeyUiHelper.self is UIPanel hotkeyPanel)
            {
                var hotkeyInfoLabel = hotkeyPanel.AddUIComponent<UILabel>();
                hotkeyInfoLabel.text = LocalizationService.Get("settings.hotkeyInfo");
                hotkeyInfoLabel.textScale = 0.9f;
                hotkeyInfoLabel.wordWrap = true;
                hotkeyInfoLabel.autoHeight = true;
                hotkeyInfoLabel.width = hotkeyPanel.width > 0 ? hotkeyPanel.width - 20f : 700f;
            }

            _uiGeneralTogglePanelHotkeyButton = (UIButton)hotkeyGroup.AddButton(
                LocalizationService.Get("settings.hotkey.capture"),
                delegate
                {
                    BeginHotkeyCapture();
                });
            _uiGeneralTogglePanelHotkeyButton.tooltip = LocalizationService.Get("settings.hotkey.tooltip");
            RefreshHotkeyButtonText();
            generalGroup.AddSpace(10);
            
            var disastersGroup = generalGroup.AddGroup(LocalizationService.Get("settings.group.enableDisasters"));

            _uiForestFireEnabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.ForestFire.GetName(), disasterContainer.ForestFire.IsDisasterEnabled, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.ForestFire.IsDisasterEnabled = isChecked;
            });
            _uiThunderstormEnabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Thunderstorm.GetName(), disasterContainer.Thunderstorm.IsDisasterEnabled, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.Thunderstorm.IsDisasterEnabled = isChecked;
            });
            _uiSinkholeEnabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Sinkhole.GetName(), disasterContainer.Sinkhole.IsDisasterEnabled, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.Sinkhole.IsDisasterEnabled = isChecked;
            });
            _uiTornadoEnabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Tornado.GetName(), disasterContainer.Tornado.IsDisasterEnabled, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.Tornado.IsDisasterEnabled = isChecked;
            });
            _uiTsunamiEnabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Tsunami.GetName(), disasterContainer.Tsunami.IsDisasterEnabled, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.Tsunami.IsDisasterEnabled = isChecked;
            });
            _uiEarthquakeEnabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Earthquake.GetName(), disasterContainer.Earthquake.IsDisasterEnabled, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.Earthquake.IsDisasterEnabled = isChecked;
            });
            _uiMeteorStrikeEnabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.MeteorStrike.GetName(), disasterContainer.MeteorStrike.IsDisasterEnabled, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.MeteorStrike.IsDisasterEnabled = isChecked;
            });
        }

        void SetupForestFire(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var forestFireGroup = helper.AddGroup(LocalizationService.Format("settings.group.disaster", disasterContainer.ForestFire.GetName()));

            _uiForestFireMaxProbability = (UISlider)forestFireGroup.AddSlider(LocalizationService.Get("settings.howOften"), 1, 50, 1, disasterContainer.ForestFire.BaseOccurrencePerYear, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.ForestFire.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(_uiForestFireMaxProbability);
            _uiForestFireMaxProbability.tooltip = LocalizationService.Get("settings.tooltip.howOften");

            _uiForestFireWarmupDays = (UISlider)forestFireGroup.AddSlider(LocalizationService.Get("settings.buildUpTime"), 0, 360, 10, disasterContainer.ForestFire.WarmupDays, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.ForestFire.WarmupDays = (int)val;
            });
            AddLabelToSlider(_uiForestFireWarmupDays, " " + LocalizationService.Get("time.day.plural"));
            _uiForestFireWarmupDays.tooltip = LocalizationService.Get("settings.tooltip.buildUpTime");

            ComponentHelpers.AddDropDown(
                ref _uiForestFireEvacuationMode,
                ref forestFireGroup,
                EvacuationModeText,
                Helper.GetManualAndFocusedEvacuationOptions(),
                ref disasterContainer.ForestFire.EvacuationMode,
                delegate (int selection)
                {
                    if (!_freezeUI)
                        disasterContainer.ForestFire.EvacuationMode = (EvacuationOptions)(selection * 2);
                }
            );

            helper.AddSpace(20);            
        }

        void SetupThunderstorm(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var thunderstormGroup = helper.AddGroup(LocalizationService.Format("settings.group.disaster", disasterContainer.Thunderstorm.GetName()));

            _uiThunderstormMaxProbability = (UISlider)thunderstormGroup.AddSlider(LocalizationService.Get("settings.howOften"), 0.1f, 10f, 0.1f, disasterContainer.Thunderstorm.BaseOccurrencePerYear, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Thunderstorm.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(_uiThunderstormMaxProbability);
            _uiThunderstormMaxProbability.tooltip = LocalizationService.Get("settings.tooltip.howOften");

            _uiThunderstormMaxProbabilityMonth = (UIDropDown)thunderstormGroup.AddDropdown(LocalizationService.Get("settings.seasonPeak"),
                Helper.GetMonths(),
                disasterContainer.Thunderstorm.MaxProbabilityMonth - 1,
                delegate (int selection)
                {
                    if (!_freezeUI)
                        disasterContainer.Thunderstorm.MaxProbabilityMonth = selection + 1;
                });

            _uiThunderstormRainFactor = (UISlider)thunderstormGroup.AddSlider(LocalizationService.Get("settings.rainBoost"), 1f, 5f, 0.1f, disasterContainer.Thunderstorm.RainFactor, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Thunderstorm.RainFactor = val;
            });
            AddLabelToSlider(_uiThunderstormRainFactor);
            _uiThunderstormRainFactor.tooltip = LocalizationService.Get("settings.tooltip.rainBoost");

            ComponentHelpers.AddDropDown(
                ref _uiThunderstormEvacuationMode,
                ref thunderstormGroup,
                EvacuationModeText,
                Helper.GetAllEvacuationOptions(),
                ref disasterContainer.Thunderstorm.EvacuationMode,
                delegate (int selection)
                {
                    if (!_freezeUI)
                        disasterContainer.Thunderstorm.EvacuationMode = (EvacuationOptions)selection;
                }
            );

            helper.AddSpace(20);            
        }

        void SetupSinkhole(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var sinkholeGroup = helper.AddGroup(LocalizationService.Format("settings.group.disaster", disasterContainer.Sinkhole.GetName()));

            _uiSinkholeMaxProbability = (UISlider)sinkholeGroup.AddSlider(LocalizationService.Get("settings.howOften"), 0.1f, 10, 0.1f, disasterContainer.Sinkhole.BaseOccurrencePerYear, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Sinkhole.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(_uiSinkholeMaxProbability);
            _uiSinkholeMaxProbability.tooltip = LocalizationService.Get("settings.tooltip.howOften");

            _uiSinkholeGroundwaterCapacity = (UISlider)sinkholeGroup.AddSlider(LocalizationService.Get("settings.groundwaterCapacity"), 1, 100, 1, disasterContainer.Sinkhole.GroundwaterCapacity, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Sinkhole.GroundwaterCapacity = val;
            });
            AddLabelToSlider(_uiSinkholeGroundwaterCapacity);
            _uiSinkholeGroundwaterCapacity.tooltip = LocalizationService.Get("settings.tooltip.groundwaterCapacity");

            ComponentHelpers.AddDropDown(
                ref _uiSinkholeEvacuationMode,
                ref sinkholeGroup,
                EvacuationModeText,
                Helper.GetAllEvacuationOptions(true),
                ref disasterContainer.Sinkhole.EvacuationMode,
                delegate (int selection)
                {
                    if (!_freezeUI)
                        disasterContainer.Sinkhole.EvacuationMode = (EvacuationOptions)selection;
                });

            helper.AddSpace(20);

        }

        void SetupTornado(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var tornadoGroup = helper.AddGroup(LocalizationService.Format("settings.group.disaster", disasterContainer.Tornado.GetName()));

            _uiTornadoMaxProbability = (UISlider)tornadoGroup.AddSlider(LocalizationService.Get("settings.howOften"), 0.1f, 10f, 0.1f, disasterContainer.Tornado.BaseOccurrencePerYear, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Tornado.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(_uiTornadoMaxProbability);
            _uiTornadoMaxProbability.tooltip = LocalizationService.Get("settings.tooltip.howOften");

            _uiTornadoMaxProbabilityMonth = (UIDropDown)tornadoGroup.AddDropdown(LocalizationService.Get("settings.seasonPeak"),
                Helper.GetMonths(),
                disasterContainer.Tornado.MaxProbabilityMonth - 1,
                delegate (int selection)
                {
                    if (!_freezeUI)
                        disasterContainer.Tornado.MaxProbabilityMonth = selection + 1;
                });

            _uiTornadoNoDuringFog = (UICheckBox)tornadoGroup.AddCheckbox(LocalizationService.Get("settings.noTornadoDuringFog"), disasterContainer.Tornado.NoTornadoDuringFog, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.Tornado.NoTornadoDuringFog = isChecked;
            });
            _uiTornadoNoDuringFog.tooltip = LocalizationService.Get("settings.tooltip.noTornadoDuringFog");

            _uiTornadoEnableDestruction = (UICheckBox)tornadoGroup.AddCheckbox(LocalizationService.Get("settings.enableTornadoDestruction"), disasterContainer.Tornado.EnableTornadoDestruction, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.Tornado.EnableTornadoDestruction = isChecked;

                _uiTornadoIntensityDestructionStart.enabled = isChecked;
            });
            _uiTornadoEnableDestruction.tooltip = LocalizationService.Get("settings.tooltip.enableTornadoDestruction");

            _uiTornadoIntensityDestructionStart = (UISlider)tornadoGroup.AddSlider(LocalizationService.Get("settings.minTornadoDestruction"), 0.1f, 25.5f, 0.1f, disasterContainer.Tornado.MinimalIntensityForDestruction, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Tornado.MinimalIntensityForDestruction = (byte)val;
            });
            AddLabelToSlider(_uiTornadoIntensityDestructionStart);
            _uiTornadoIntensityDestructionStart.tooltip = LocalizationService.Get("settings.tooltip.minTornadoDestruction");

            tornadoGroup.AddSpace(10);

            ComponentHelpers.AddDropDown(
                ref _uiTornadoEvacuationMode,
                ref tornadoGroup,
                EvacuationModeText,
                Helper.GetAllEvacuationOptions(true),
                ref disasterContainer.Tornado.EvacuationMode, delegate (int selection)
                {
                    if (!_freezeUI)
                        disasterContainer.Tornado.EvacuationMode = (EvacuationOptions)selection;
                }
            );

            helper.AddSpace(20);
        }

        void SetupTsunami(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var tsunamiGroup = helper.AddGroup(LocalizationService.Format("settings.group.disaster", disasterContainer.Tsunami.GetName()));
            var isRealTimeActive = CommonServices.DisasterHandler.CheckRealTimeModActive();

            _uiTsunamiMaxProbability = (UISlider)tsunamiGroup.AddSlider(LocalizationService.Get("settings.howOften"), 0.1f, 10, 0.1f, disasterContainer.Tsunami.BaseOccurrencePerYear, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Tsunami.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(_uiTsunamiMaxProbability);
            _uiTsunamiMaxProbability.tooltip = LocalizationService.Get("settings.tooltip.howOften");

            _uiTsunamiWarmupYears = (UISlider)tsunamiGroup.AddSlider(LocalizationService.Get("settings.buildUpTime"), 0, 20, 0.5f, disasterContainer.Tsunami.WarmupYears, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Tsunami.WarmupYears = val;
            });
            AddLabelToSlider(_uiTsunamiWarmupYears, " " + LocalizationService.Get("time.year.plural"));
            _uiTsunamiWarmupYears.tooltip = LocalizationService.Get("settings.tooltip.buildUpTime");

            _uiTsunamiRealTimeProgressMultiplier = (UISlider)tsunamiGroup.AddSlider(LocalizationService.Get("settings.realTimeTsunamiSpeed"), 1f, 12f, 0.5f, disasterContainer.Tsunami.RealTimeProgressMultiplier, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Tsunami.RealTimeProgressMultiplier = val;
            });
            AddLabelToSlider(_uiTsunamiRealTimeProgressMultiplier, "x");
            _uiTsunamiRealTimeProgressMultiplier.tooltip = LocalizationService.Get("settings.tooltip.realTimeTsunamiSpeed");
            _uiTsunamiRealTimeProgressMultiplier.enabled = isRealTimeActive;

            ComponentHelpers.AddDropDown(
                ref _uiTsunamiEvacuationMode,
                ref tsunamiGroup,
                EvacuationModeText,
                Helper.GetAllEvacuationOptions(),
                ref disasterContainer.Tsunami.EvacuationMode, delegate (int selection)
                {
                    if (!_freezeUI)
                        disasterContainer.Tsunami.EvacuationMode = (EvacuationOptions)selection;
                }
           );

            helper.AddSpace(20);

        }

        void SetupEarthquake(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var earthquakeGroup = helper.AddGroup(LocalizationService.Format("settings.group.disaster", disasterContainer.Earthquake.GetName()));

            _uiEarthquakeMaxProbability = (UISlider)earthquakeGroup.AddSlider(LocalizationService.Get("settings.howOften"), 0.1f, 10, 0.1f, disasterContainer.Earthquake.BaseOccurrencePerYear, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Earthquake.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(_uiEarthquakeMaxProbability);
            _uiEarthquakeMaxProbability.tooltip = LocalizationService.Get("settings.tooltip.howOften");

            _uiEarthquakeWarmupYears = (UISlider)earthquakeGroup.AddSlider(LocalizationService.Get("settings.buildUpTime"), 0, 20, 0.5f, disasterContainer.Earthquake.WarmupYears, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Earthquake.WarmupYears = val;
            });
            AddLabelToSlider(_uiEarthquakeWarmupYears, " " + LocalizationService.Get("time.year.plural"));
            _uiEarthquakeWarmupYears.tooltip = LocalizationService.Get("settings.tooltip.buildUpTime");

            _uiEarthquakeAftershocksEnabled = (UICheckBox)earthquakeGroup.AddCheckbox(LocalizationService.Get("settings.enableAftershocks"), disasterContainer.Earthquake.AftershocksEnabled, delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.Earthquake.AftershocksEnabled = isChecked;
            });
            _uiEarthquakeAftershocksEnabled.tooltip = LocalizationService.Get("settings.tooltip.enableAftershocks");

            ComponentHelpers.AddDropDown(
                ref _uiEarthquakeCrackMode,
                 ref earthquakeGroup,
                 LocalizationService.Get("settings.groundCracks"),
                 Helper.GetCrackModes(),
                 ref disasterContainer.Earthquake.EarthquakeCrackMode,
                 delegate (int selection)
                 {
                     if (!_freezeUI)
                         disasterContainer.Earthquake.EarthquakeCrackMode = (EarthquakeCrackOptions)selection;
                 }
             );
            _uiEarthquakeCrackMode.tooltip = LocalizationService.Get("settings.tooltip.groundCracks");

            _uiEarthquakeMinIntensityToCrack = (UISlider)earthquakeGroup.AddSlider(LocalizationService.Get("settings.minCrackStrength"), 10f, 25.5f, 0.1f, disasterContainer.Earthquake.MinimalIntensityForCracks, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.Earthquake.MinimalIntensityForCracks = (byte)val;
            });
            AddLabelToSlider(_uiEarthquakeMinIntensityToCrack);
            _uiEarthquakeMinIntensityToCrack.tooltip = LocalizationService.Get("settings.tooltip.minCrackStrength");
            earthquakeGroup.AddSpace(15);

            ComponentHelpers.AddDropDown(
                ref _uiEarthquakeEvacuationMode,
                 ref earthquakeGroup,
                 EvacuationModeText,
                 Helper.GetAllEvacuationOptions(),
                 ref disasterContainer.Earthquake.EvacuationMode,
                 delegate (int selection)
                 {
                     if (!_freezeUI)
                         disasterContainer.Earthquake.EvacuationMode = (EvacuationOptions)selection;
                 }
             );

            helper.AddSpace(20);

        }

        void SetupMeteorStrike(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            var meteorStrikeGroup = helper.AddGroup(LocalizationService.Format("settings.group.disaster", disasterContainer.MeteorStrike.GetName()));
            var isRealTimeActive = CommonServices.DisasterHandler.CheckRealTimeModActive();

            _uiMeteorStrikeMaxProbability = (UISlider)meteorStrikeGroup.AddSlider(LocalizationService.Get("settings.howOften"), 1f, 50, 1f, disasterContainer.MeteorStrike.BaseOccurrencePerYear, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.MeteorStrike.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(_uiMeteorStrikeMaxProbability);
            _uiMeteorStrikeMaxProbability.tooltip = LocalizationService.Get("settings.tooltip.howOften");

            meteorStrikeGroup.AddSpace(15);

            _uiMeteorStrikeRealTimeFrequencyMultiplier = (UISlider)meteorStrikeGroup.AddSlider(LocalizationService.Get("settings.realTimeMeteorFrequency"), 1f, 12f, 0.5f, disasterContainer.MeteorStrike.RealTimeFrequencyMultiplier, delegate(float val)
            {
                if (!_freezeUI)
                    disasterContainer.MeteorStrike.RealTimeFrequencyMultiplier = val;
            });
            AddLabelToSlider(_uiMeteorStrikeRealTimeFrequencyMultiplier, "x");
            _uiMeteorStrikeRealTimeFrequencyMultiplier.tooltip = LocalizationService.Get("settings.tooltip.realTimeMeteorFrequency");
            _uiMeteorStrikeRealTimeFrequencyMultiplier.enabled = isRealTimeActive;
            
            _uiMeteorStrikeMeteorLongPeriodEnabled = (UICheckBox)meteorStrikeGroup.AddCheckbox(LocalizationService.Get("settings.enableLongMeteor"), disasterContainer.MeteorStrike.GetEnabled(0), delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.MeteorStrike.SetEnabled(0, isChecked);
            });
            _uiMeteorStrikeMeteorLongPeriodEnabled.tooltip = LocalizationService.Get("settings.tooltip.enableLongMeteor");

            _uiMeteorStrikeMeteorMediumPeriodEnabled = (UICheckBox)meteorStrikeGroup.AddCheckbox(LocalizationService.Get("settings.enableMediumMeteor"), disasterContainer.MeteorStrike.GetEnabled(1), delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.MeteorStrike.SetEnabled(1, isChecked);
            });
            _uiMeteorStrikeMeteorMediumPeriodEnabled.tooltip = LocalizationService.Get("settings.tooltip.enableMediumMeteor");

            _uiMeteorStrikeMeteorShortPeriodEnabled = (UICheckBox)meteorStrikeGroup.AddCheckbox(LocalizationService.Get("settings.enableShortMeteor"), disasterContainer.MeteorStrike.GetEnabled(2), delegate(bool isChecked)
            {
                if (!_freezeUI)
                    disasterContainer.MeteorStrike.SetEnabled(2, isChecked);
            });
            _uiMeteorStrikeMeteorShortPeriodEnabled.tooltip = LocalizationService.Get("settings.tooltip.enableShortMeteor");

            ComponentHelpers.AddDropDown(
                ref _uiMeteorStrikeEvacuationMode,
                ref meteorStrikeGroup,
                EvacuationModeText,
                Helper.GetAllEvacuationOptions(true),
                ref disasterContainer.MeteorStrike.EvacuationMode,
                delegate (int selection)
                {
                    if (!_freezeUI)
                        disasterContainer.MeteorStrike.EvacuationMode = (EvacuationOptions)selection;
                }
            );

            helper.AddSpace(20);

        }

        //Next enhancement
        void SetupHotkeySetup(ref UIHelper helper, DisasterSetupModel disasterContainer) {

            
        }

        void SetupSaveOptions(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            // Save buttons
            var saveOptionsGroup = helper.AddGroup(LocalizationService.Get("settings.group.save"));

            saveOptionsGroup.AddButton(LocalizationService.Get("settings.saveDefault"), delegate
            {
                CommonServices.DisasterSetup.Save();
            });
            saveOptionsGroup.AddButton(LocalizationService.Get("settings.resetSaved"), delegate
            {
                CommonServices.DisasterHandler.ReadValuesFromFile();
                UpdateSetupContentUI();
            });
            saveOptionsGroup.AddButton(LocalizationService.Get("settings.resetModDefault"), delegate
            {
                CommonServices.DisasterHandler.ResetToDefaultValues();
                UpdateSetupContentUI();
            });
        }

        #endregion Options UI
    }
}
