using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.BaseGameExtensions;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Setup;
using NaturalDisastersRenewal.UI.ComponentHelper;
using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Helper = NaturalDisastersRenewal.Common.Helper;

namespace NaturalDisastersRenewal.UI
{
    public class SettingsScreen
    {
        bool freezeUI = false;
        readonly string evacuationModeText = "Evacuation Mode: ";

        #region UI Components

        //General
        UICheckBox UI_General_DisableDisasterFocus;

        UICheckBox UI_General_PauseOnDisasterStarts;
        UISlider UI_General_PartialEvacuationRadius;
        UISlider UI_General_MaxPopulationToTrigguerHigherDisasters;
        UICheckBox UI_General_ScaleMaxIntensityWithPopulation;
        UICheckBox UI_General_RecordDisasterEventsChkBox;
        UICheckBox UI_General_ShowDisasterPanelButton;

        //Forest Fire
        UICheckBox UI_ForestFire_Enabled;

        UISlider UI_ForestFireMaxProbability;
        UISlider UI_ForestFire_WarmupDays;
        UIDropDown UI_ForestFire_EvacuationMode;

        //Thunderstorm
        UICheckBox UI_Thunderstorm_Enabled;

        UISlider UI_Thunderstorm_MaxProbability;
        UIDropDown UI_Thunderstorm_MaxProbabilityMonth;
        UISlider UI_Thunderstorm_RainFactor;
        UIDropDown UI_Thunderstorm_EvacuationMode;

        //Sunkhole
        UICheckBox UI_Sinkhole_Enabled;

        UISlider UI_Sinkhole_MaxProbability;
        UISlider UI_Sinkhole_GroundwaterCapacity;
        UIDropDown UI_Sinkhole_EvacuationMode;

        //Tornado
        UICheckBox UI_Tornado_Enabled;

        UISlider UI_Tornado_MaxProbability;
        UIDropDown UI_Tornado_MaxProbabilityMonth;
        UICheckBox UI_Tornado_NoDuringFog;
        UIDropDown UI_Tornado_EvacuationMode;
        UICheckBox UI_Tornado_EnableDestruction;
        UISlider UI_Tornado_IntensityDestructionStart;

        //Tsunami
        UICheckBox UI_Tsunami_Enabled;

        UISlider UI_Tsunami_MaxProbability;
        UISlider UI_Tsunami_WarmupYears;
        UIDropDown UI_Tsunami_EvacuationMode;

        //Earthquake
        UICheckBox UI_Earthquake_Enabled;

        UISlider UI_Earthquake_MinIntensityToCrack;
        UISlider UI_Earthquake_MaxProbability;
        UISlider UI_Earthquake_WarmupYears;
        UICheckBox UI_Earthquake_AftershocksEnabled;

        //UICheckBox UI_Earthquake_NoCrack;
        UIDropDown UI_Earthquake_CrackMode;

        UIDropDown UI_Earthquake_EvacuationMode;

        //Meteor Strike
        UICheckBox UI_MeteorStrike_Enabled;

        UISlider UI_MeteorStrike_MaxProbability;
        UICheckBox UI_MeteorStrike_MeteorLongPeriodEnabled;
        UICheckBox UI_MeteorStrike_MeteorMediumPeriodEnabled;
        UICheckBox UI_MeteorStrike_MeteorShortPeriodEnabled;
        UIDropDown UI_MeteorStrike_EvacuationMode;

        #endregion UI Components

        #region Options UI

        public static void UpdateUISettingsOptions()
        {
            foreach (PluginManager.PluginInfo current in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if (current.isEnabled)
                {
                    IUserMod[] instances = current.GetInstances<IUserMod>();

                    MethodInfo method = instances[0].GetType().GetMethod("EnhancedDisastersOptionsUpdateUI", BindingFlags.Instance | BindingFlags.Public);

                    if (method != null)
                    {
                        method.Invoke(instances[0], new object[] { });
                        return;
                    }
                }
            }
        }

        public void UpdateSetupContentUI()
        {
            if (UI_ForestFire_Enabled == null)
                return;

            DisasterSetupModel disasterSetupModel = Singleton<NaturalDisasterHandler>.instance.container;
            freezeUI = true;

            UI_General_DisableDisasterFocus.isChecked = disasterSetupModel.DisableDisasterFocus;
            UI_General_PauseOnDisasterStarts.isChecked = disasterSetupModel.PauseOnDisasterStarts;
            UI_General_PartialEvacuationRadius.value = disasterSetupModel.PartialEvacuationRadius;
            UI_General_MaxPopulationToTrigguerHigherDisasters.value = disasterSetupModel.MaxPopulationToTrigguerHigherDisasters;

            UI_General_ScaleMaxIntensityWithPopulation.isChecked = disasterSetupModel.ScaleMaxIntensityWithPopulation;
            UI_General_RecordDisasterEventsChkBox.isChecked = disasterSetupModel.RecordDisasterEvents;
            UI_General_ShowDisasterPanelButton.isChecked = disasterSetupModel.ShowDisasterPanelButton;

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

        void AddLabelToSlider(object obj, string postfix = "")
        {
            UISlider uISlider = obj as UISlider;
            if (uISlider == null) return;

            UILabel label = uISlider.parent.AddUIComponent<UILabel>();
            label.text = uISlider.value.ToString() + postfix;
            label.textScale = 1f;
            (uISlider.parent as UIPanel).autoLayout = false;
            label.position = new Vector3(uISlider.position.x + uISlider.width + 15, uISlider.position.y);

            UILabel titleLabel = (uISlider.parent as UIPanel).Find<UILabel>("Label");
            titleLabel.anchor = UIAnchorStyle.None;
            titleLabel.position = new Vector3(titleLabel.position.x, titleLabel.position.y + 3);

            uISlider.eventValueChanged += new PropertyChangedEventHandler<float>(delegate (UIComponent component, float value)
            {
                label.text = uISlider.value.ToString() + postfix;
            });
        }

        public void BuildSettingsMenu(UIHelper helper)
        {
            DisasterSetupModel disasterContainer = Singleton<NaturalDisasterHandler>.instance.container;

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

        void SetupGeneralTab(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            UIHelperBase generalGroup = helper.AddGroup("General");

            UI_General_DisableDisasterFocus = (UICheckBox)generalGroup.AddCheckbox("Disable automatic disaster follow when it starts.", disasterContainer.DisableDisasterFocus, delegate (bool isChecked)
            {
                if (!freezeUI)
                {
                    disasterContainer.DisableDisasterFocus = isChecked;
                    DisasterExtension.SetDisableDisasterFocus(disasterContainer.DisableDisasterFocus);
                }
            });

            UI_General_PauseOnDisasterStarts = (UICheckBox)generalGroup.AddCheckbox("Pause on disaster starts", disasterContainer.PauseOnDisasterStarts, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.PauseOnDisasterStarts = isChecked;
            });

            UI_General_PartialEvacuationRadius = (UISlider)generalGroup.AddSlider("Partial evacuation Radius (In meters)", 300f, 4200f, 100f, disasterContainer.PartialEvacuationRadius, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.PartialEvacuationRadius = val;
            });
            AddLabelToSlider(UI_General_PartialEvacuationRadius);
            UI_General_PartialEvacuationRadius.tooltip = "Select the Radius (In meters) for Focused evacuations.";

            generalGroup.AddSpace(5);
            UI_General_MaxPopulationToTrigguerHigherDisasters = (UISlider)generalGroup.AddSlider("Max population to trigguer higher disasters.", 20000f, 400000f, 1000f, disasterContainer.MaxPopulationToTrigguerHigherDisasters, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.MaxPopulationToTrigguerHigherDisasters = val;
            });
            AddLabelToSlider(UI_General_MaxPopulationToTrigguerHigherDisasters);
            UI_General_MaxPopulationToTrigguerHigherDisasters.tooltip = "Select the max population to trigger higher disaster intensity.";

            generalGroup.AddSpace(10);
            
            UI_General_ScaleMaxIntensityWithPopulation = (UICheckBox)generalGroup.AddCheckbox("Scale max intensity with population", disasterContainer.ScaleMaxIntensityWithPopulation, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.ScaleMaxIntensityWithPopulation = isChecked;
            });
            UI_General_ScaleMaxIntensityWithPopulation.tooltip = "Maximum intensity for all disasters is set to the minimum at the beginning of the game and gradually increases as the city grows.";

            UI_General_RecordDisasterEventsChkBox = (UICheckBox)generalGroup.AddCheckbox("Record disaster events", disasterContainer.RecordDisasterEvents, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.RecordDisasterEvents = isChecked;
            });
            UI_General_RecordDisasterEventsChkBox.tooltip = "Write out disaster name, date of occurrence, and intencity into Disasters.csv file";

            UI_General_ShowDisasterPanelButton = (UICheckBox)generalGroup.AddCheckbox("Show Disasters Panel toggle button", disasterContainer.ShowDisasterPanelButton, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.ShowDisasterPanelButton = isChecked;

                Singleton<NaturalDisasterHandler>.instance.UpdateDisastersPanelToggleBtn();
                Singleton<NaturalDisasterHandler>.instance.UpdateDisastersDPanel();
            });

            generalGroup.AddSpace(10);

            UIHelperBase elementPositionsGroup = generalGroup.AddGroup("Button/Panel positions:");

            elementPositionsGroup.AddButton("Reset Button Position", delegate ()
            {
                Singleton<NaturalDisasterHandler>.instance.ResetToDefaultValues(true, false);
                UpdateSetupContentUI();
            });

            elementPositionsGroup.AddButton("Reset Panel Position", delegate ()
            {
                Singleton<NaturalDisasterHandler>.instance.ResetToDefaultValues(false, true);
                UpdateSetupContentUI();
            });
            
            generalGroup.AddGroup("Hotkey to display/hide panel info: Shift + D (will be configurable soon).");
            generalGroup.AddSpace(10);

            generalGroup.AddSpace(10);
            UIHelperBase disastersGroup = generalGroup.AddGroup("Enable Disasters:");

            UI_ForestFire_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.ForestFire.GetName(), disasterContainer.ForestFire.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.ForestFire.Enabled = isChecked;
            });
            UI_Thunderstorm_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Thunderstorm.GetName(), disasterContainer.Thunderstorm.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Thunderstorm.Enabled = isChecked;
            });
            UI_Sinkhole_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Sinkhole.GetName(), disasterContainer.Sinkhole.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Sinkhole.Enabled = isChecked;
            });
            UI_Tornado_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Tornado.GetName(), disasterContainer.Tornado.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Tornado.Enabled = isChecked;
            });
            UI_Tsunami_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Tsunami.GetName(), disasterContainer.Tsunami.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Tsunami.Enabled = isChecked;
            });
            UI_Earthquake_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.Earthquake.GetName(), disasterContainer.Earthquake.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Earthquake.Enabled = isChecked;
            });
            UI_MeteorStrike_Enabled = (UICheckBox)disastersGroup.AddCheckbox(disasterContainer.MeteorStrike.GetName(), disasterContainer.MeteorStrike.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.MeteorStrike.Enabled = isChecked;
            });
        }

        void SetupForestFire(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            UIHelperBase forestFireGroup = helper.AddGroup("Forest Fire disaster");

            UI_ForestFireMaxProbability = (UISlider)forestFireGroup.AddSlider("Max probability", 1, 50, 1, disasterContainer.ForestFire.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.ForestFire.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(UI_ForestFireMaxProbability, " times per year");
            UI_ForestFireMaxProbability.tooltip = "Occurrence (per year) in case of a long period without rain";

            UI_ForestFire_WarmupDays = (UISlider)forestFireGroup.AddSlider("Warmup period", 0, 360, 10, disasterContainer.ForestFire.WarmupDays, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.ForestFire.WarmupDays = (int)val;
            });
            AddLabelToSlider(UI_ForestFire_WarmupDays, " days");
            UI_ForestFire_WarmupDays.tooltip = "No-rain period during wich the probability of Forest Fire increases";

            ComponentHelpers.AddDropDown(
                ref UI_ForestFire_EvacuationMode,
                ref forestFireGroup,
                evacuationModeText,
                Helper.GetManualAndFocusedEvacuationOptions(),
                ref disasterContainer.ForestFire.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
                        disasterContainer.ForestFire.EvacuationMode = (EvacuationOptions)(selection * 2);
                }
            );

            helper.AddSpace(20);            
        }

        void SetupThunderstorm(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            UIHelperBase thunderstormGroup = helper.AddGroup("Thunderstorm disaster");

            UI_Thunderstorm_MaxProbability = (UISlider)thunderstormGroup.AddSlider("Max probability", 0.1f, 10f, 0.1f, disasterContainer.Thunderstorm.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Thunderstorm.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(UI_Thunderstorm_MaxProbability, " times per year");
            UI_Thunderstorm_MaxProbability.tooltip = "Occurrence (per year) in thunderstorm season";

            UI_Thunderstorm_MaxProbabilityMonth = (UIDropDown)thunderstormGroup.AddDropdown("Thunderstorm season peak",
                Helper.GetMonths(),
                disasterContainer.Thunderstorm.MaxProbabilityMonth - 1,
                delegate (int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Thunderstorm.MaxProbabilityMonth = selection + 1;
                });

            UI_Thunderstorm_RainFactor = (UISlider)thunderstormGroup.AddSlider("Rain factor", 1f, 5f, 0.1f, disasterContainer.Thunderstorm.RainFactor, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Thunderstorm.RainFactor = val;
            });
            AddLabelToSlider(UI_Thunderstorm_RainFactor);
            UI_Thunderstorm_RainFactor.tooltip = "Thunderstorm probability increases by this factor during rain.";

            ComponentHelpers.AddDropDown(
                ref UI_Thunderstorm_EvacuationMode,
                ref thunderstormGroup,
                evacuationModeText,
                Helper.GetAllEvacuationOptions(),
                ref disasterContainer.Thunderstorm.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Thunderstorm.EvacuationMode = (EvacuationOptions)selection;
                }
            );

            helper.AddSpace(20);            
        }

        void SetupSinkhole(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            UIHelperBase sinkholeGroup = helper.AddGroup("Sinkhole disaster");

            UI_Sinkhole_MaxProbability = (UISlider)sinkholeGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, disasterContainer.Sinkhole.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Sinkhole.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(UI_Sinkhole_MaxProbability, " times per year");
            UI_Sinkhole_MaxProbability.tooltip = "Occurrence (per year) in case of a long period of rain";

            UI_Sinkhole_GroundwaterCapacity = (UISlider)sinkholeGroup.AddSlider("Groundwater capacity", 1, 100, 1, disasterContainer.Sinkhole.GroundwaterCapacity, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Sinkhole.GroundwaterCapacity = val;
            });
            AddLabelToSlider(UI_Sinkhole_GroundwaterCapacity);
            UI_Sinkhole_GroundwaterCapacity.tooltip = "Set how fast groundwater fills up during rain and causes a sinkhole to appear.";

            ComponentHelpers.AddDropDown(
                ref UI_Sinkhole_EvacuationMode,
                ref sinkholeGroup,
                evacuationModeText,
                Helper.GetAllEvacuationOptions(true),
                ref disasterContainer.Sinkhole.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Sinkhole.EvacuationMode = (EvacuationOptions)selection;
                });

            helper.AddSpace(20);

        }

        void SetupTornado(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            UIHelperBase tornadoGroup = helper.AddGroup("Tornado disaster");

            UI_Tornado_MaxProbability = (UISlider)tornadoGroup.AddSlider("Max probability", 0.1f, 10f, 0.1f, disasterContainer.Tornado.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Tornado.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(UI_Tornado_MaxProbability, " times per year");
            UI_Tornado_MaxProbability.tooltip = "Occurrence (per year) in Tornado season";

            UI_Tornado_MaxProbabilityMonth = (UIDropDown)tornadoGroup.AddDropdown("Tornado season peak",
                Helper.GetMonths(),
                disasterContainer.Tornado.MaxProbabilityMonth - 1,
                delegate (int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.MaxProbabilityMonth = selection + 1;
                });

            UI_Tornado_NoDuringFog = (UICheckBox)tornadoGroup.AddCheckbox("No Tornado during fog", disasterContainer.Tornado.NoTornadoDuringFog, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Tornado.NoTornadoDuringFog = isChecked;
            });
            UI_Tornado_NoDuringFog.tooltip = "Tornado does not occur during foggy weather";

            UI_Tornado_EnableDestruction = (UICheckBox)tornadoGroup.AddCheckbox("Enable tornado destruction", disasterContainer.Tornado.EnableTornadoDestruction, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Tornado.EnableTornadoDestruction = isChecked;

                UI_Tornado_IntensityDestructionStart.enabled = isChecked;
            });

            UI_Tornado_IntensityDestructionStart = (UISlider)tornadoGroup.AddSlider("Minimal intensity for tornado destruction:", 0.1f, 25.5f, 0.1f, disasterContainer.Tornado.MinimalIntensityForDestruction, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Tornado.MinimalIntensityForDestruction = (byte)val;
            });
            AddLabelToSlider(UI_Tornado_IntensityDestructionStart, " Intensity to start destruction");

            tornadoGroup.AddSpace(10);

            ComponentHelpers.AddDropDown(
                ref UI_Tornado_EvacuationMode,
                ref tornadoGroup,
                evacuationModeText,
                Helper.GetAllEvacuationOptions(true),
                ref disasterContainer.Tornado.EvacuationMode, delegate (int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Tornado.EvacuationMode = (EvacuationOptions)selection;
                }
            );

            helper.AddSpace(20);
        }

        void SetupTsunami(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            UIHelperBase tsunamiGroup = helper.AddGroup("Tsunami disaster");

            UI_Tsunami_MaxProbability = (UISlider)tsunamiGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, disasterContainer.Tsunami.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Tsunami.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(UI_Tsunami_MaxProbability, " times per year");
            UI_Tsunami_MaxProbability.tooltip = "Maximum occurrence (per year) after a long period without tsunamis";

            UI_Tsunami_WarmupYears = (UISlider)tsunamiGroup.AddSlider("Charge period", 0, 20, 0.5f, disasterContainer.Tsunami.WarmupYears, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Tsunami.WarmupYears = val;
            });
            AddLabelToSlider(UI_Tsunami_WarmupYears, " years");
            UI_Tsunami_WarmupYears.tooltip = "The probability of tsunami increases to the maximum during this period";

            ComponentHelpers.AddDropDown(
                ref UI_Tsunami_EvacuationMode,
                ref tsunamiGroup,
                evacuationModeText,
                Helper.GetAllEvacuationOptions(),
                ref disasterContainer.Tsunami.EvacuationMode, delegate (int selection)
                {
                    if (!freezeUI)
                        disasterContainer.Tsunami.EvacuationMode = (EvacuationOptions)selection;
                }
           );

            helper.AddSpace(20);

        }

        void SetupEarthquake(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            UIHelperBase earthquakeGroup = helper.AddGroup("Earthquake disaster");

            UI_Earthquake_MaxProbability = (UISlider)earthquakeGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, disasterContainer.Earthquake.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Earthquake.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(UI_Earthquake_MaxProbability, " times per year");
            UI_Earthquake_MaxProbability.tooltip = "Maximum occurrence (per year) after a long period without earthquakes";

            UI_Earthquake_WarmupYears = (UISlider)earthquakeGroup.AddSlider("Charge period", 0, 20, 0.5f, disasterContainer.Earthquake.WarmupYears, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Earthquake.WarmupYears = val;
            });
            AddLabelToSlider(UI_Earthquake_WarmupYears, " years");
            UI_Earthquake_WarmupYears.tooltip = "The probability of earthquake increases to the maximum during this period";

            UI_Earthquake_AftershocksEnabled = (UICheckBox)earthquakeGroup.AddCheckbox("Enable aftershocks", disasterContainer.Earthquake.AftershocksEnabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Earthquake.AftershocksEnabled = isChecked;
            });
            UI_Earthquake_AftershocksEnabled.tooltip = "Several aftershocks may occur after a big earthquake. Aftershocks strike the same place.";

            ComponentHelpers.AddDropDown(
                 ref UI_Earthquake_CrackMode,
                 ref earthquakeGroup,
                 "Cracks in the ground:",
                 Helper.GetCrackModes(),
                 ref disasterContainer.Earthquake.EarthquakeCrackMode,
                 delegate (int selection)
                 {
                     if (!freezeUI)
                         disasterContainer.Earthquake.EarthquakeCrackMode = (EarthquakeCrackOptions)selection;
                 }
             );
            UI_Earthquake_CrackMode.tooltip = "Based on selection you can put a crack in the ground, ignoring it or put it based on intensity.";

            UI_Earthquake_MinIntensityToCrack = (UISlider)earthquakeGroup.AddSlider("Min. intensity for cracks", 10f, 25.5f, 0.1f, disasterContainer.Earthquake.MinimalIntensityForCracks, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Earthquake.MinimalIntensityForCracks = (byte)val;
            });
            AddLabelToSlider(UI_Earthquake_MinIntensityToCrack, " minimal Intensity");
            UI_Earthquake_MinIntensityToCrack.tooltip = "Minimal intensity to see cracks on the ground";
            earthquakeGroup.AddSpace(15);

            ComponentHelpers.AddDropDown(
                 ref UI_Earthquake_EvacuationMode,
                 ref earthquakeGroup,
                 evacuationModeText,
                 Helper.GetAllEvacuationOptions(),
                 ref disasterContainer.Earthquake.EvacuationMode,
                 delegate (int selection)
                 {
                     if (!freezeUI)
                         disasterContainer.Earthquake.EvacuationMode = (EvacuationOptions)selection;
                 }
             );

            helper.AddSpace(20);

        }

        void SetupMeteorStrike(ref UIHelper helper, DisasterSetupModel disasterContainer)
        {
            UIHelperBase meteorStrikeGroup = helper.AddGroup("Meteor Strike disaster");

            UI_MeteorStrike_MaxProbability = (UISlider)meteorStrikeGroup.AddSlider("Max probability", 1f, 50, 1f, disasterContainer.MeteorStrike.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.MeteorStrike.BaseOccurrencePerYear = val;
            });
            AddLabelToSlider(UI_MeteorStrike_MaxProbability, " times per year");
            UI_MeteorStrike_MaxProbability.tooltip = "Maximum occurrence of meteor strike per year per one meteor when it approaches the Earth";

            UI_MeteorStrike_MeteorLongPeriodEnabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable long period (9 years) meteor", disasterContainer.MeteorStrike.GetEnabled(0), delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.MeteorStrike.SetEnabled(0, isChecked);
            });

            UI_MeteorStrike_MeteorMediumPeriodEnabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable medium period (5 years) meteor", disasterContainer.MeteorStrike.GetEnabled(1), delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.MeteorStrike.SetEnabled(1, isChecked);
            });

            UI_MeteorStrike_MeteorShortPeriodEnabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable short period (2 years) meteor", disasterContainer.MeteorStrike.GetEnabled(2), delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.MeteorStrike.SetEnabled(2, isChecked);
            });

            ComponentHelpers.AddDropDown(
                ref UI_MeteorStrike_EvacuationMode,
                ref meteorStrikeGroup,
                evacuationModeText,
                Helper.GetAllEvacuationOptions(true),
                ref disasterContainer.MeteorStrike.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
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
            UIHelperBase saveOptionsGroup = helper.AddGroup("Save options");

            saveOptionsGroup.AddButton("Save as default for new games", delegate ()
            {
                Singleton<NaturalDisasterHandler>.instance.container.Save();
            });
            saveOptionsGroup.AddButton("Reset to the last saved values", delegate ()
            {
                Singleton<NaturalDisasterHandler>.instance.ReadValuesFromFile();
                UpdateSetupContentUI();
            });
            saveOptionsGroup.AddButton("Reset to the mod default values", delegate ()
            {
                Singleton<NaturalDisasterHandler>.instance.ResetToDefaultValues();
                UpdateSetupContentUI();
            });
        }

        #endregion Options UI
    }
}