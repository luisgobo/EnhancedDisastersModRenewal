using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using ColossalFramework.Plugins;
using System.Reflection;
using System;

namespace NaturalDisastersRenewal
{
    public class Mod : IUserMod
    {
        public static string ModNameEng = "Natural Disasters Overhaul Renewal";
        public static string LogMsgPrefix = ">>> " + ModNameEng + ": ";
        public static string Version = "2023";
        private bool freezeUI = false;

        //General
        UICheckBox UI_General_AutoFocusOnDisaster;
        UICheckBox UI_General_PauseOnDisasterStarts;
        //****************

        private UICheckBox UI_ScaleMaxIntensityWithPopulation;
        private UICheckBox UI_RecordDisasterEventsChkBox;
        private UICheckBox UI_ShowDisasterPanelButton;

        private UICheckBox UI_ForestFire_Enabled;
        private UISlider ForestFireMaxProbabilityUI;
        private UISlider UI_ForestFire_WarmupDays;

        private UICheckBox UI_Thunderstorm_Enabled;
        private UISlider UI_Thunderstorm_MaxProbability;
        private UIDropDown UI_Thunderstorm_MaxProbabilityMonth;
        private UISlider UI_Thunderstorm_RainFactor;

        private UICheckBox UI_Sinkhole_Enabled;
        private UISlider UI_Sinkhole_MaxProbability;
        private UISlider UI_Sinkhole_GroundwaterCapacity;

        private UICheckBox UI_Tornado_Enabled;
        private UISlider UI_Tornado_MaxProbability;
        private UIDropDown UI_Tornado_MaxProbabilityMonth;
        private UICheckBox UI_Tornado_NoDuringFog;

        private UICheckBox UI_Tsunami_Enabled;
        private UISlider UI_Tsunami_MaxProbability;
        private UISlider UI_Tsunami_WarmupYears;

        private UICheckBox UI_Earthquake_Enabled;
        private UISlider UI_Earthquake_MaxProbability;
        private UISlider UI_Earthquake_WarmupYears;
        private UICheckBox UI_Earthquake_AftershocksEnabled;
        private UICheckBox UI_Earthquake_NoCrack;

        private UICheckBox UI_MeteorStrike_Enabled;
        private UISlider UI_MeteorStrike_MaxProbability;
        private UICheckBox UI_MeteorStrike_Meteor1Enabled;
        private UICheckBox UI_MeteorStrike_Meteor2Enabled;
        private UICheckBox UI_MeteorStrike_Meteor3Enabled;

        //AutoEvacuateRelease options
        UIDropDown UI_Earthquake_AutoEvacuateRelease;
        UIDropDown UI_ForestFire_AutoEvacuateRelease;
        UIDropDown UI_MeteorStrike_AutoEvacuateRelease;
        UIDropDown UI_Sinkhole_AutoEvacuateRelease;
        UIDropDown UI_StructureCollapse_AutoEvacuateRelease;
        UIDropDown UI_StructureFire_AutoEvacuateRelease;
        UIDropDown UI_Thunderstorm_AutoEvacuateRelease;
        UIDropDown UI_Tornado_AutoEvacuateRelease;
        UIDropDown UI_Tsunami_AutoEvacuateRelease;

        public string Name
        {
            get { return ModNameEng; }
        }

        public string Description
        {
            get { return "More natural behavior of natural disasters (ver. " + Version + ")"; }
        }

        #region Options UI

        public static void UpdateUI()
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

        private void EnhancedDisastersOptionsUpdateUI()
        {
            if (UI_ForestFire_Enabled == null) return;

            DisastersContainer c = Singleton<EnhancedDisastersManager>.instance.container;

            freezeUI = true;

            UI_General_AutoFocusOnDisaster.isChecked = c.AutoFocusOnDisaster;
            UI_General_PauseOnDisasterStarts.isChecked = c.PauseOnDisasterStarts;

            UI_ScaleMaxIntensityWithPopulation.isChecked = c.ScaleMaxIntensityWithPopilation;
            UI_RecordDisasterEventsChkBox.isChecked = c.RecordDisasterEvents;
            UI_ShowDisasterPanelButton.isChecked = c.ShowDisasterPanelButton;

            UI_ForestFire_Enabled.isChecked = c.ForestFire.Enabled;
            ForestFireMaxProbabilityUI.value = c.ForestFire.BaseOccurrencePerYear;
            UI_ForestFire_WarmupDays.value = c.ForestFire.WarmupDays;
            UI_ForestFire_AutoEvacuateRelease.selectedIndex = c.ForestFire.EvacuationMode;

            UI_Thunderstorm_Enabled.isChecked = c.Thunderstorm.Enabled;
            UI_Thunderstorm_MaxProbability.value = c.Thunderstorm.BaseOccurrencePerYear;
            UI_Thunderstorm_MaxProbabilityMonth.selectedIndex = c.Thunderstorm.MaxProbabilityMonth - 1;
            UI_Thunderstorm_RainFactor.value = c.Thunderstorm.RainFactor;
            UI_Thunderstorm_AutoEvacuateRelease.selectedIndex = c.Thunderstorm.EvacuationMode;

            UI_Sinkhole_Enabled.isChecked = c.Sinkhole.Enabled;
            UI_Sinkhole_MaxProbability.value = c.Sinkhole.BaseOccurrencePerYear;
            UI_Sinkhole_GroundwaterCapacity.value = c.Sinkhole.GroundwaterCapacity;
            UI_Sinkhole_AutoEvacuateRelease.selectedIndex = c.Sinkhole.EvacuationMode;

            UI_Tornado_Enabled.isChecked = c.Tornado.Enabled;
            UI_Tornado_MaxProbability.value = c.Tornado.BaseOccurrencePerYear;
            UI_Tornado_MaxProbabilityMonth.selectedIndex = c.Tornado.MaxProbabilityMonth - 1;
            UI_Tornado_NoDuringFog.isChecked = c.Tornado.NoTornadoDuringFog;
            UI_Tornado_AutoEvacuateRelease.selectedIndex = c.Tornado.EvacuationMode;

            UI_Tsunami_Enabled.isChecked = c.Tsunami.Enabled;
            UI_Tsunami_MaxProbability.value = c.Tsunami.BaseOccurrencePerYear;
            UI_Tsunami_WarmupYears.value = c.Tsunami.WarmupYears;
            UI_Tsunami_AutoEvacuateRelease.selectedIndex = c.Tsunami.EvacuationMode;

            UI_Earthquake_Enabled.isChecked = c.Earthquake.Enabled;
            UI_Earthquake_MaxProbability.value = c.Earthquake.BaseOccurrencePerYear;
            UI_Earthquake_WarmupYears.value = c.Earthquake.WarmupYears;
            UI_Earthquake_AftershocksEnabled.isChecked = c.Earthquake.AftershocksEnabled;
            UI_Earthquake_NoCrack.isChecked = c.Earthquake.NoCracks;
            UI_Earthquake_AutoEvacuateRelease.selectedIndex = c.Earthquake.EvacuationMode;

            UI_MeteorStrike_Enabled.isChecked = c.MeteorStrike.Enabled;
            UI_MeteorStrike_MaxProbability.value = c.MeteorStrike.BaseOccurrencePerYear;
            UI_MeteorStrike_Meteor1Enabled.isChecked = c.MeteorStrike.GetEnabled(0);
            UI_MeteorStrike_Meteor2Enabled.isChecked = c.MeteorStrike.GetEnabled(1);
            UI_MeteorStrike_Meteor3Enabled.isChecked = c.MeteorStrike.GetEnabled(2);
            UI_MeteorStrike_AutoEvacuateRelease.selectedIndex = c.MeteorStrike.EvacuationMode;

            //AutoEvacuateRelease options
            //UI_StructureCollapse_AutoEvacuateRelease.selectedIndex = c.AutoEvacuateSettings.EvacuationMode;
            //UI_StructureFire_AutoEvacuateRelease.selectedIndex = c.AutoEvacuateSettings.EvacuationMode;

            freezeUI = false;
        }

        private void addLabelToSlider(object obj)
        {
            addLabelToSlider(obj, "");
        }

        private void addLabelToSlider(object obj, string postfix)
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

        //public void OnSettingsUI(UIHelperBase helper)
        //{

        //    //Organize menu

        //    DisastersContainer c = Singleton<EnhancedDisastersManager>.instance.container;

        //    UI_ForestFire_Enabled = (UICheckBox)helper.AddCheckbox("Enable Forest Fire", c.ForestFire.Enabled, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.ForestFire.Enabled = isChecked;
        //    });
        //    UI_Thunderstorm_Enabled = (UICheckBox)helper.AddCheckbox("Enable Thunderstorm", c.Thunderstorm.Enabled, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.Thunderstorm.Enabled = isChecked;
        //    });
        //    UI_Sinkhole_Enabled = (UICheckBox)helper.AddCheckbox("Enable Sinkhole", c.Sinkhole.Enabled, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.Sinkhole.Enabled = isChecked;
        //    });
        //    UI_Tornado_Enabled = (UICheckBox)helper.AddCheckbox("Enable Tornado", c.Tornado.Enabled, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.Tornado.Enabled = isChecked;
        //    });
        //    UI_Tsunami_Enabled = (UICheckBox)helper.AddCheckbox("Enable Tsunami", c.Tsunami.Enabled, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.Tsunami.Enabled = isChecked;
        //    });
        //    UI_Earthquake_Enabled = (UICheckBox)helper.AddCheckbox("Enable Earthquake", c.Earthquake.Enabled, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.Earthquake.Enabled = isChecked;
        //    });
        //    UI_MeteorStrike_Enabled = (UICheckBox)helper.AddCheckbox("Enable Meteor Strike", c.MeteorStrike.Enabled, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.MeteorStrike.Enabled = isChecked;
        //    });

        //    helper.AddGroup(" "); // Adds horizontal line

        //    #region ForestFire

        //    UIHelperBase forestFireGroup = helper.AddGroup("Forest Fire disaster");

        //    ForestFireMaxProbabilityUI = (UISlider)forestFireGroup.AddSlider("Max probability", 1, 50, 1, c.ForestFire.BaseOccurrencePerYear, delegate (float val)
        //    {
        //        if (!freezeUI) c.ForestFire.BaseOccurrencePerYear = val;
        //    });
        //    addLabelToSlider(ForestFireMaxProbabilityUI, " times per year");
        //    ForestFireMaxProbabilityUI.tooltip = "Occurrence (per year) in case of a long period without rain";

        //    UI_ForestFire_WarmupDays = (UISlider)forestFireGroup.AddSlider("Warmup period", 0, 360, 10, c.ForestFire.WarmupDays, delegate (float val)
        //    {
        //        if (!freezeUI) c.ForestFire.WarmupDays = (int)val;
        //    });
        //    addLabelToSlider(UI_ForestFire_WarmupDays, " days");
        //    UI_ForestFire_WarmupDays.tooltip = "No-rain period during wich the probability of Forest Fire increases";

        //    helper.AddSpace(20);

        //    #endregion

        //    #region Thunderstorm

        //    UIHelperBase thunderstormGroup = helper.AddGroup("Thunderstorm disaster");

        //    UI_Thunderstorm_MaxProbability = (UISlider)thunderstormGroup.AddSlider("Max probability", 0.1f, 10f, 0.1f, c.Thunderstorm.BaseOccurrencePerYear, delegate (float val)
        //    {
        //        if (!freezeUI) c.Thunderstorm.BaseOccurrencePerYear = val;
        //    });
        //    addLabelToSlider(UI_Thunderstorm_MaxProbability, " times per year");
        //    UI_Thunderstorm_MaxProbability.tooltip = "Occurrence (per year) in thunderstorm season";

        //    UI_Thunderstorm_MaxProbabilityMonth = (UIDropDown)thunderstormGroup.AddDropdown("Thunderstorm season peak",
        //        Helper.GetMonths(),
        //        c.Thunderstorm.MaxProbabilityMonth - 1,
        //        delegate (int sel)
        //        {
        //            if (!freezeUI) c.Thunderstorm.MaxProbabilityMonth = sel + 1;
        //        });

        //    UI_Thunderstorm_RainFactor = (UISlider)thunderstormGroup.AddSlider("Rain factor", 1f, 5f, 0.1f, c.Thunderstorm.RainFactor, delegate (float val)
        //    {
        //        if (!freezeUI) c.Thunderstorm.RainFactor = val;
        //    });
        //    addLabelToSlider(UI_Thunderstorm_RainFactor);
        //    UI_Thunderstorm_RainFactor.tooltip = "Thunderstorm probability increases by this factor during rain.";

        //    helper.AddSpace(20);

        //    #endregion

        //    #region Sinkhole

        //    UIHelperBase sinkholeGroup = helper.AddGroup("Sinkhole disaster");

        //    UI_Sinkhole_MaxProbability = (UISlider)sinkholeGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, c.Sinkhole.BaseOccurrencePerYear, delegate (float val)
        //    {
        //        if (!freezeUI) c.Sinkhole.BaseOccurrencePerYear = val;
        //    });
        //    addLabelToSlider(UI_Sinkhole_MaxProbability, " times per year");
        //    UI_Sinkhole_MaxProbability.tooltip = "Occurrence (per year) in case of a long period of rain";

        //    UI_Sinkhole_GroundwaterCapacity = (UISlider)sinkholeGroup.AddSlider("Groundwater capacity", 1, 100, 1, c.Sinkhole.GroundwaterCapacity, delegate (float val)
        //    {
        //        if (!freezeUI) c.Sinkhole.GroundwaterCapacity = val;
        //    });
        //    addLabelToSlider(UI_Sinkhole_GroundwaterCapacity);
        //    UI_Sinkhole_GroundwaterCapacity.tooltip = "Set how fast groundwater fills up during rain and causes a sinkhole to appear.";

        //    helper.AddSpace(20);

        //    #endregion

        //    #region Tornado

        //    UIHelperBase tornadoGroup = helper.AddGroup("Tornado disaster");

        //    UI_Tornado_MaxProbability = (UISlider)tornadoGroup.AddSlider("Max probability", 0.1f, 10f, 0.1f, c.Tornado.BaseOccurrencePerYear, delegate (float val)
        //    {
        //        if (!freezeUI) c.Tornado.BaseOccurrencePerYear = val;
        //    });
        //    addLabelToSlider(UI_Tornado_MaxProbability, " times per year");
        //    UI_Tornado_MaxProbability.tooltip = "Occurrence (per year) in Tornado season";

        //    UI_Tornado_MaxProbabilityMonth = (UIDropDown)tornadoGroup.AddDropdown("Tornado season peak",
        //        Helper.GetMonths(),
        //        c.Tornado.MaxProbabilityMonth - 1,
        //        delegate (int sel)
        //        {
        //            if (!freezeUI) c.Tornado.MaxProbabilityMonth = sel + 1;
        //        });

        //    UI_Tornado_NoDuringFog = (UICheckBox)tornadoGroup.AddCheckbox("No Tornado during fog", c.Tornado.NoTornadoDuringFog, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.Tornado.NoTornadoDuringFog = isChecked;
        //    });
        //    UI_Tornado_NoDuringFog.tooltip = "Tornado does not occur during foggy weather";

        //    helper.AddSpace(20);

        //    #endregion

        //    #region Tsunami

        //    UIHelperBase tsunamiGroup = helper.AddGroup("Tsunami disaster");

        //    UI_Tsunami_MaxProbability = (UISlider)tsunamiGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, c.Tsunami.BaseOccurrencePerYear, delegate (float val)
        //    {
        //        if (!freezeUI) c.Tsunami.BaseOccurrencePerYear = val;
        //    });
        //    addLabelToSlider(UI_Tsunami_MaxProbability, " times per year");
        //    UI_Tsunami_MaxProbability.tooltip = "Maximum occurrence (per year) after a long period without tsunamis";

        //    UI_Tsunami_WarmupYears = (UISlider)tsunamiGroup.AddSlider("Charge period", 0, 20, 0.5f, c.Tsunami.WarmupYears, delegate (float val)
        //    {
        //        if (!freezeUI) c.Tsunami.WarmupYears = val;
        //    });
        //    addLabelToSlider(UI_Tsunami_WarmupYears, " years");
        //    UI_Tsunami_WarmupYears.tooltip = "The probability of tsunami increases to the maximum during this period";

        //    helper.AddSpace(20);

        //    #endregion

        //    #region Earthquake

        //    UIHelperBase earthquakeGroup = helper.AddGroup("Earthquake disaster");

        //    UI_Earthquake_MaxProbability = (UISlider)earthquakeGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, c.Earthquake.BaseOccurrencePerYear, delegate (float val)
        //    {
        //        if (!freezeUI) c.Earthquake.BaseOccurrencePerYear = val;
        //    });
        //    addLabelToSlider(UI_Earthquake_MaxProbability, " times per year");
        //    UI_Earthquake_MaxProbability.tooltip = "Maximum occurrence (per year) after a long period without earthquakes";

        //    UI_Earthquake_WarmupYears = (UISlider)earthquakeGroup.AddSlider("Charge period", 0, 20, 0.5f, c.Earthquake.WarmupYears, delegate (float val)
        //    {
        //        if (!freezeUI) c.Earthquake.WarmupYears = val;
        //    });
        //    addLabelToSlider(UI_Earthquake_WarmupYears, " years");
        //    UI_Earthquake_WarmupYears.tooltip = "The probability of earthquake increases to the maximum during this period";

        //    UI_Earthquake_AftershocksEnabled = (UICheckBox)earthquakeGroup.AddCheckbox("Enable aftershocks", c.Earthquake.AftershocksEnabled, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.Earthquake.AftershocksEnabled = isChecked;
        //    });
        //    UI_Earthquake_AftershocksEnabled.tooltip = "Several aftershocks may occur after a big earthquake. Aftershocks strike the same place.";

        //    UI_Earthquake_NoCrack = (UICheckBox)earthquakeGroup.AddCheckbox("No cracks in the ground", c.Earthquake.NoCracks, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.Earthquake.NoCracks = isChecked;
        //        c.Earthquake.UpdateDisasterProperties(true);
        //    });
        //    UI_Earthquake_NoCrack.tooltip = "If checked, the earthquake does not put a crack in the ground.";

        //    helper.AddSpace(20);

        //    #endregion

        //    #region MeteorStrike

        //    UIHelperBase meteorStrikeGroup = helper.AddGroup("Meteor Strike disaster");

        //    UI_MeteorStrike_MaxProbability = (UISlider)meteorStrikeGroup.AddSlider("Max probability", 1f, 50, 1f, c.MeteorStrike.BaseOccurrencePerYear, delegate (float val)
        //    {
        //        if (!freezeUI) c.MeteorStrike.BaseOccurrencePerYear = val;
        //    });
        //    addLabelToSlider(UI_MeteorStrike_MaxProbability, " times per year");
        //    UI_MeteorStrike_MaxProbability.tooltip = "Maximum occurrence of meteor strike per year per one meteor when it approaches the Earth";

        //    UI_MeteorStrike_Meteor1Enabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable long period (9 years) meteor", c.MeteorStrike.GetEnabled(0), delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.MeteorStrike.SetEnabled(0, isChecked);
        //    });

        //    UI_MeteorStrike_Meteor2Enabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable medium period (5 years) meteor", c.MeteorStrike.GetEnabled(1), delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.MeteorStrike.SetEnabled(1, isChecked);
        //    });

        //    UI_MeteorStrike_Meteor3Enabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable short period (2 years) meteor", c.MeteorStrike.GetEnabled(2), delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.MeteorStrike.SetEnabled(2, isChecked);
        //    });

        //    helper.AddSpace(20);

        //    #endregion


        //    // Save buttons
        //    helper.AddButton("Save as default for new games", delegate ()
        //    {
        //        Singleton<EnhancedDisastersManager>.instance.container.Save();
        //    });
        //    helper.AddButton("Reset to the last saved values", delegate ()
        //    {
        //        Singleton<EnhancedDisastersManager>.instance.ReadValuesFromFile();
        //        EnhancedDisastersOptionsUpdateUI();
        //    });
        //    helper.AddButton("Reset to the mod default values", delegate ()
        //    {
        //        Singleton<EnhancedDisastersManager>.instance.ResetToDefaultValues();
        //        EnhancedDisastersOptionsUpdateUI();
        //    });
        //    helper.AddSpace(20);


        //    UIHelperBase generalGroup = helper.AddGroup("General");

        //    UI_General_AutoFocusOnDisaster = (UICheckBox)generalGroup.AddCheckbox("Auto focus on disaster", c.AutoFocusOnDisaster, delegate (bool isChecked)
        //    {
        //        if (!freezeUI)
        //            c.AutoFocusOnDisaster = isChecked;
        //    });
        //    UI_General_AutoFocusOnDisaster.tooltip = "Auto focus on disaster";

        //    UI_General_PauseOnDisasterStarts = (UICheckBox)generalGroup.AddCheckbox("Pause on disaster starts", c.PauseOnDisasterStarts, delegate (bool isChecked)
        //    {
        //        if (!freezeUI)
        //            c.PauseOnDisasterStarts = isChecked;
        //    });
        //    UI_General_PauseOnDisasterStarts.tooltip = "Pause on disaster starts";

        //    generalGroup.AddSpace(10);

        //    UI_ScaleMaxIntensityWithPopilation = (UICheckBox)helper.AddCheckbox("Scale max intensity with population", c.ScaleMaxIntensityWithPopilation, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.ScaleMaxIntensityWithPopilation = isChecked;
        //    });
        //    UI_ScaleMaxIntensityWithPopilation.tooltip = "Maximum intensity for all disasters is set to the minimum at the beginning of the game and gradually increases as the city grows.";

        //    UI_RecordDisasterEventsChkBox = (UICheckBox)helper.AddCheckbox("Record disaster events", c.RecordDisasterEvents, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.RecordDisasterEvents = isChecked;
        //    });
        //    UI_RecordDisasterEventsChkBox.tooltip = "Write out disaster name, date of occurrence, and intencity into Disasters.csv file";

        //    UI_ShowDisasterPanelButton = (UICheckBox)helper.AddCheckbox("Show Disasters Panel toggle button", c.ShowDisasterPanelButton, delegate (bool isChecked)
        //    {
        //        if (!freezeUI) c.ShowDisasterPanelButton = isChecked;

        //        Singleton<EnhancedDisastersManager>.instance.UpdateDisastersPanelToggleBtn();
        //    });

        //    helper.AddSpace(20);

        //    UIHelperBase autoEvacuate_ReleaseGroup = helper.AddGroup("Auto Evacuate");

        //    UI_Earthquake_AutoEvacuateRelease = (UIDropDown)autoEvacuate_ReleaseGroup.AddDropdown(
        //        "Earthquake",
        //        Helper.GetEvacuationOptions(),
        //        c.AutoEvacuateSettings.AutoEvacuateEarthquake,
        //        delegate (int selection)
        //        {
        //            if (!freezeUI)
        //            {
        //                c.AutoEvacuateSettings.AutoEvacuateEarthquake = selection;
        //            }
        //        }
        //    );

        //    UI_ForestFire_AutoEvacuateRelease = (UIDropDown)autoEvacuate_ReleaseGroup.AddDropdown(
        //        "Forest Fire",
        //        Helper.GetEvacuationOptions(),
        //        c.AutoEvacuateSettings.AutoEvacuateForestFire,
        //        delegate (int selection)
        //        {
        //            if (!freezeUI)
        //            {
        //                c.AutoEvacuateSettings.AutoEvacuateForestFire = selection;
        //            }

        //        });

        //    UI_MeteorStrike_AutoEvacuateRelease = (UIDropDown)autoEvacuate_ReleaseGroup.AddDropdown(
        //        "Meteor Strike",
        //        Helper.GetEvacuationOptions(true),
        //        c.AutoEvacuateSettings.AutoEvacuateMeteorStrike,
        //        delegate (int selection)
        //        {
        //            if (!freezeUI)
        //            {
        //                c.AutoEvacuateSettings.AutoEvacuateMeteorStrike = selection;
        //            }
        //        });

        //    UI_Sinkhole_AutoEvacuateRelease = (UIDropDown)autoEvacuate_ReleaseGroup.AddDropdown(
        //        "Sinkhole",
        //        Helper.GetEvacuationOptions(true),
        //        c.AutoEvacuateSettings.AutoEvacuateSinkhole,
        //        delegate (int selection)
        //        {
        //            if (!freezeUI)
        //            {
        //                c.AutoEvacuateSettings.AutoEvacuateSinkhole = selection;
        //            }
        //        });

        //    UI_StructureCollapse_AutoEvacuateRelease = (UIDropDown)autoEvacuate_ReleaseGroup.AddDropdown(
        //        "Structure Collapse",
        //        Helper.GetEvacuationOptions(),
        //        c.AutoEvacuateSettings.AutoEvacuateStructureCollapse,
        //        delegate (int selection)
        //        {
        //            if (!freezeUI)
        //            {
        //                c.AutoEvacuateSettings.AutoEvacuateStructureCollapse = selection;
        //            }
        //        });

        //    UI_StructureFire_AutoEvacuateRelease = (UIDropDown)autoEvacuate_ReleaseGroup.AddDropdown(
        //        "Structure Fire",
        //        Helper.GetEvacuationOptions(),
        //        c.AutoEvacuateSettings.AutoEvacuateStructureFire,
        //        delegate (int selection)
        //        {
        //            if (!freezeUI)
        //            {
        //                c.AutoEvacuateSettings.AutoEvacuateStructureFire = selection;
        //            }
        //        });

        //    UI_Thunderstorm_AutoEvacuateRelease = (UIDropDown)autoEvacuate_ReleaseGroup.AddDropdown(
        //        "Thunderstorm",
        //        Helper.GetEvacuationOptions(),
        //        c.AutoEvacuateSettings.AutoEvacuateThunderstorm,
        //        delegate (int selection)
        //        {
        //            if (!freezeUI)
        //            {
        //                c.AutoEvacuateSettings.AutoEvacuateThunderstorm = selection;
        //            }
        //        });

        //    UI_Tornado_AutoEvacuateRelease = (UIDropDown)autoEvacuate_ReleaseGroup.AddDropdown(
        //        "Tornado",
        //        Helper.GetEvacuationOptions(true),
        //        c.AutoEvacuateSettings.AutoEvacuateTornado,
        //        delegate (int selection)
        //        {
        //            if (!freezeUI)
        //            {
        //                c.AutoEvacuateSettings.AutoEvacuateTornado = selection;
        //            }
        //        });


        //    UI_Tsunami_AutoEvacuateRelease = (UIDropDown)autoEvacuate_ReleaseGroup.AddDropdown(
        //        "Tsunami",
        //        Helper.GetEvacuationOptions(true),
        //        c.AutoEvacuateSettings.AutoEvacuateTsunami,
        //        delegate (int selection)
        //        {
        //            if (!freezeUI)
        //            {
        //                c.AutoEvacuateSettings.AutoEvacuateTsunami = selection;
        //            }
        //        });

        //    helper.AddSpace(20);

        //}

        public void OnSettingsUI(UIHelperBase helper)
        {
            DisastersContainer disasterContainer = Singleton<EnhancedDisastersManager>.instance.container;

            #region Gegeral options

            UIHelperBase generalGroup = helper.AddGroup("General");

            UI_General_AutoFocusOnDisaster = (UICheckBox)generalGroup.AddCheckbox("Auto focus on disaster", disasterContainer.AutoFocusOnDisaster, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.AutoFocusOnDisaster = isChecked;
            });
            UI_General_AutoFocusOnDisaster.tooltip = "Auto focus on disaster";

            UI_General_PauseOnDisasterStarts = (UICheckBox)generalGroup.AddCheckbox("Pause on disaster starts", disasterContainer.PauseOnDisasterStarts, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.PauseOnDisasterStarts = isChecked;
            });
            UI_General_PauseOnDisasterStarts.tooltip = "Pause on disaster starts";

            generalGroup.AddSpace(10);

            UI_ScaleMaxIntensityWithPopulation = (UICheckBox)generalGroup.AddCheckbox("Scale max intensity with population", disasterContainer.ScaleMaxIntensityWithPopilation, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.ScaleMaxIntensityWithPopilation = isChecked;
            });
            UI_ScaleMaxIntensityWithPopulation.tooltip = "Maximum intensity for all disasters is set to the minimum at the beginning of the game and gradually increases as the city grows.";

            UI_RecordDisasterEventsChkBox = (UICheckBox)generalGroup.AddCheckbox("Record disaster events", disasterContainer.RecordDisasterEvents, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.RecordDisasterEvents = isChecked;
            });
            UI_RecordDisasterEventsChkBox.tooltip = "Write out disaster name, date of occurrence, and intencity into Disasters.csv file";

            UI_ShowDisasterPanelButton = (UICheckBox)generalGroup.AddCheckbox("Show Disasters Panel toggle button", disasterContainer.ShowDisasterPanelButton, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.ShowDisasterPanelButton = isChecked;

                Singleton<EnhancedDisastersManager>.instance.UpdateDisastersPanelToggleBtn();
            });

            generalGroup.AddSpace(10);


            UIHelperBase disastersGroup = generalGroup.AddGroup("Enable Disasters:");

            UI_ForestFire_Enabled = (UICheckBox)disastersGroup.AddCheckbox("Forest Fire", disasterContainer.ForestFire.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.ForestFire.Enabled = isChecked;
            });
            UI_Thunderstorm_Enabled = (UICheckBox)disastersGroup.AddCheckbox("Thunderstorm", disasterContainer.Thunderstorm.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Thunderstorm.Enabled = isChecked;
            });
            UI_Sinkhole_Enabled = (UICheckBox)disastersGroup.AddCheckbox("Sinkhole", disasterContainer.Sinkhole.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Sinkhole.Enabled = isChecked;
            });
            UI_Tornado_Enabled = (UICheckBox)disastersGroup.AddCheckbox("Tornado", disasterContainer.Tornado.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Tornado.Enabled = isChecked;
            });
            UI_Tsunami_Enabled = (UICheckBox)disastersGroup.AddCheckbox("Tsunami", disasterContainer.Tsunami.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Tsunami.Enabled = isChecked;
            });
            UI_Earthquake_Enabled = (UICheckBox)disastersGroup.AddCheckbox("Earthquake", disasterContainer.Earthquake.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Earthquake.Enabled = isChecked;
            });
            UI_MeteorStrike_Enabled = (UICheckBox)disastersGroup.AddCheckbox("Meteor Strike", disasterContainer.MeteorStrike.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.MeteorStrike.Enabled = isChecked;
            });

            #endregion

            #region ForestFire

            UIHelperBase forestFireGroup = helper.AddGroup("Forest Fire disaster");

            ForestFireMaxProbabilityUI = (UISlider)forestFireGroup.AddSlider("Max probability", 1, 50, 1, disasterContainer.ForestFire.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.ForestFire.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(ForestFireMaxProbabilityUI, " times per year");
            ForestFireMaxProbabilityUI.tooltip = "Occurrence (per year) in case of a long period without rain";

            UI_ForestFire_WarmupDays = (UISlider)forestFireGroup.AddSlider("Warmup period", 0, 360, 10, disasterContainer.ForestFire.WarmupDays, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.ForestFire.WarmupDays = (int)val;
            });
            addLabelToSlider(UI_ForestFire_WarmupDays, " days");
            UI_ForestFire_WarmupDays.tooltip = "No-rain period during wich the probability of Forest Fire increases";

            UI_ForestFire_AutoEvacuateRelease = (UIDropDown)forestFireGroup.AddDropdown(
                "Evacuation mode:",
                Helper.GetEvacuationOptions(),
                disasterContainer.ForestFire.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.ForestFire.EvacuationMode = selection;
                    }

                });

            helper.AddSpace(20);

            #endregion

            #region Thunderstorm

            UIHelperBase thunderstormGroup = helper.AddGroup("Thunderstorm disaster");

            UI_Thunderstorm_MaxProbability = (UISlider)thunderstormGroup.AddSlider("Max probability", 0.1f, 10f, 0.1f, disasterContainer.Thunderstorm.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Thunderstorm.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_Thunderstorm_MaxProbability, " times per year");
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
            addLabelToSlider(UI_Thunderstorm_RainFactor);
            UI_Thunderstorm_RainFactor.tooltip = "Thunderstorm probability increases by this factor during rain.";

            UI_Thunderstorm_AutoEvacuateRelease = (UIDropDown)thunderstormGroup.AddDropdown(
                "Evacuation mode:",
                Helper.GetEvacuationOptions(),
                disasterContainer.Thunderstorm.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.Thunderstorm.EvacuationMode = selection;
                    }
                });

            helper.AddSpace(20);

            #endregion

            #region Sinkhole

            UIHelperBase sinkholeGroup = helper.AddGroup("Sinkhole disaster");

            UI_Sinkhole_MaxProbability = (UISlider)sinkholeGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, disasterContainer.Sinkhole.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Sinkhole.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_Sinkhole_MaxProbability, " times per year");
            UI_Sinkhole_MaxProbability.tooltip = "Occurrence (per year) in case of a long period of rain";

            UI_Sinkhole_GroundwaterCapacity = (UISlider)sinkholeGroup.AddSlider("Groundwater capacity", 1, 100, 1, disasterContainer.Sinkhole.GroundwaterCapacity, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Sinkhole.GroundwaterCapacity = val;
            });
            addLabelToSlider(UI_Sinkhole_GroundwaterCapacity);
            UI_Sinkhole_GroundwaterCapacity.tooltip = "Set how fast groundwater fills up during rain and causes a sinkhole to appear.";

            UI_Sinkhole_AutoEvacuateRelease = (UIDropDown)sinkholeGroup.AddDropdown(
                "Evacuation mode:",
                Helper.GetEvacuationOptions(true),
                disasterContainer.Sinkhole.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.Sinkhole.EvacuationMode = selection;
                    }
                });

            helper.AddSpace(20);

            #endregion

            #region Tornado

            UIHelperBase tornadoGroup = helper.AddGroup("Tornado disaster");

            UI_Tornado_MaxProbability = (UISlider)tornadoGroup.AddSlider("Max probability", 0.1f, 10f, 0.1f, disasterContainer.Tornado.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI) disasterContainer.Tornado.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_Tornado_MaxProbability, " times per year");
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

            UI_Tornado_AutoEvacuateRelease = (UIDropDown)tornadoGroup.AddDropdown(
                "Evacuation mode:",
                Helper.GetEvacuationOptions(true),
                disasterContainer.Tornado.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.Tornado.EvacuationMode = selection;
                    }
                });

            helper.AddSpace(20);

            #endregion

            #region Tsunami

            UIHelperBase tsunamiGroup = helper.AddGroup("Tsunami disaster");

            UI_Tsunami_MaxProbability = (UISlider)tsunamiGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, disasterContainer.Tsunami.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Tsunami.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_Tsunami_MaxProbability, " times per year");
            UI_Tsunami_MaxProbability.tooltip = "Maximum occurrence (per year) after a long period without tsunamis";

            UI_Tsunami_WarmupYears = (UISlider)tsunamiGroup.AddSlider("Charge period", 0, 20, 0.5f, disasterContainer.Tsunami.WarmupYears, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Tsunami.WarmupYears = val;
            });
            addLabelToSlider(UI_Tsunami_WarmupYears, " years");
            UI_Tsunami_WarmupYears.tooltip = "The probability of tsunami increases to the maximum during this period";

            UI_Tsunami_AutoEvacuateRelease = (UIDropDown)tsunamiGroup.AddDropdown(
                "Evacuation mode:",
                Helper.GetEvacuationOptions(true),
                disasterContainer.Tsunami.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.Tsunami.EvacuationMode = selection;
                    }
                });

            helper.AddSpace(20);

            #endregion

            #region Earthquake

            UIHelperBase earthquakeGroup = helper.AddGroup("Earthquake disaster");

            UI_Earthquake_MaxProbability = (UISlider)earthquakeGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, disasterContainer.Earthquake.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI) disasterContainer.Earthquake.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_Earthquake_MaxProbability, " times per year");
            UI_Earthquake_MaxProbability.tooltip = "Maximum occurrence (per year) after a long period without earthquakes";

            UI_Earthquake_WarmupYears = (UISlider)earthquakeGroup.AddSlider("Charge period", 0, 20, 0.5f, disasterContainer.Earthquake.WarmupYears, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.Earthquake.WarmupYears = val;
            });
            addLabelToSlider(UI_Earthquake_WarmupYears, " years");
            UI_Earthquake_WarmupYears.tooltip = "The probability of earthquake increases to the maximum during this period";

            UI_Earthquake_AftershocksEnabled = (UICheckBox)earthquakeGroup.AddCheckbox("Enable aftershocks", disasterContainer.Earthquake.AftershocksEnabled, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Earthquake.AftershocksEnabled = isChecked;
            });
            UI_Earthquake_AftershocksEnabled.tooltip = "Several aftershocks may occur after a big earthquake. Aftershocks strike the same place.";

            UI_Earthquake_NoCrack = (UICheckBox)earthquakeGroup.AddCheckbox("No cracks in the ground", disasterContainer.Earthquake.NoCracks, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Earthquake.NoCracks = isChecked;
                disasterContainer.Earthquake.UpdateDisasterProperties(true);
            });
            UI_Earthquake_NoCrack.tooltip = "If checked, the earthquake does not put a crack in the ground.";

            UI_Earthquake_AutoEvacuateRelease = (UIDropDown)earthquakeGroup.AddDropdown(
               "Evacuation mode:",
               Helper.GetEvacuationOptions(),
               disasterContainer.Earthquake.EvacuationMode,
               delegate (int selection)
               {
                   if (!freezeUI)
                   {
                       disasterContainer.Earthquake.EvacuationMode = selection;
                   }
               }
           );

            helper.AddSpace(20);

            #endregion

            #region MeteorStrike

            UIHelperBase meteorStrikeGroup = helper.AddGroup("Meteor Strike disaster");

            UI_MeteorStrike_MaxProbability = (UISlider)meteorStrikeGroup.AddSlider("Max probability", 1f, 50, 1f, disasterContainer.MeteorStrike.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI)
                    disasterContainer.MeteorStrike.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_MeteorStrike_MaxProbability, " times per year");
            UI_MeteorStrike_MaxProbability.tooltip = "Maximum occurrence of meteor strike per year per one meteor when it approaches the Earth";

            UI_MeteorStrike_Meteor1Enabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable long period (9 years) meteor", disasterContainer.MeteorStrike.GetEnabled(0), delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.MeteorStrike.SetEnabled(0, isChecked);
            });

            UI_MeteorStrike_Meteor2Enabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable medium period (5 years) meteor", disasterContainer.MeteorStrike.GetEnabled(1), delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.MeteorStrike.SetEnabled(1, isChecked);
            });

            UI_MeteorStrike_Meteor3Enabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable short period (2 years) meteor", disasterContainer.MeteorStrike.GetEnabled(2), delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.MeteorStrike.SetEnabled(2, isChecked);
            });

            UI_MeteorStrike_AutoEvacuateRelease = (UIDropDown)meteorStrikeGroup.AddDropdown(
                "Evacuation mode:",
                Helper.GetEvacuationOptions(true),
                disasterContainer.MeteorStrike.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.MeteorStrike.EvacuationMode = selection;
                    }
                });

            helper.AddSpace(20);


            #endregion            

            #region Save_Options
            // Save buttons
            UIHelperBase saveOptionsGroup = helper.AddGroup("Save options");

            saveOptionsGroup.AddButton("Save as default for new games", delegate ()
            {
                Singleton<EnhancedDisastersManager>.instance.container.Save();
            });
            saveOptionsGroup.AddButton("Reset to the last saved values", delegate ()
            {
                Singleton<EnhancedDisastersManager>.instance.ReadValuesFromFile();
                EnhancedDisastersOptionsUpdateUI();
            });
            saveOptionsGroup.AddButton("Reset to the mod default values", delegate ()
            {
                Singleton<EnhancedDisastersManager>.instance.ResetToDefaultValues();
                EnhancedDisastersOptionsUpdateUI();
            });

            #endregion
        }

        #endregion
    }
}
