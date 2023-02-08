using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ColossalFramework;
using ICities;
using System.Reflection;
using UnityEngine;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.UI.ComponentHelper;
using System;
using NaturalDisastersRenewal.Serialization;
using NaturalDisastersRenewal.BaseGameExtensions;
using NaturalDisastersRenewal.DisasterServices.LegacyStructure;

namespace NaturalDisastersRenewal.UI
{
    public class SettingsScreen
    {
        bool freezeUI = false;
        readonly string evacuationModeText = "Evacuation Mode: ";

        //General
        UICheckBox UI_General_DisableAutoFocusOnDisasterStarts;
        UICheckBox UI_General_PauseOnDisasterStarts;
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

        //Tsunami
        UICheckBox UI_Tsunami_Enabled;
        UISlider UI_Tsunami_MaxProbability;
        UISlider UI_Tsunami_WarmupYears;
        UIDropDown UI_Tsunami_EvacuationMode;

        //Earthquake
        UICheckBox UI_Earthquake_Enabled;
        UISlider UI_Earthquake_MaxProbability;
        UISlider UI_Earthquake_WarmupYears;
        UICheckBox UI_Earthquake_AftershocksEnabled;
        UICheckBox UI_Earthquake_NoCrack;
        UIDropDown UI_Earthquake_EvacuationMode;

        //Meteor Strike
        UICheckBox UI_MeteorStrike_Enabled;
        UISlider UI_MeteorStrike_MaxProbability;
        UICheckBox UI_MeteorStrike_MeteorLongPeriodEnabled;
        UICheckBox UI_MeteorStrike_MeteorMediumPeriodEnabled;
        UICheckBox UI_MeteorStrike_MeteorShortPeriodEnabled;
        UIDropDown UI_MeteorStrike_EvacuationMode;

        //Next enhancements
        //UIDropDown UI_StructureCollapse_AutoEvacuateRelease;
        //UIDropDown UI_StructureFire_AutoEvacuateRelease;        

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

        void EnhancedDisastersOptionsUpdateUI()
        {
            if (UI_ForestFire_Enabled == null)
                return;

            DisastersSerializeBase c = Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.container;

            freezeUI = true;

            UI_General_DisableAutoFocusOnDisasterStarts.isChecked = c.DisableAutoFocusOnDisasterStarts;
            UI_General_PauseOnDisasterStarts.isChecked = c.PauseOnDisasterStarts;
            UI_General_ScaleMaxIntensityWithPopulation.isChecked = c.ScaleMaxIntensityWithPopulation;
            UI_General_RecordDisasterEventsChkBox.isChecked = c.RecordDisasterEvents;
            UI_General_ShowDisasterPanelButton.isChecked = c.ShowDisasterPanelButton;

            UI_ForestFire_Enabled.isChecked = c.ForestFire.Enabled;
            UI_ForestFire_EvacuationMode.selectedIndex = c.ForestFire.EvacuationMode;
            UI_ForestFireMaxProbability.value = c.ForestFire.BaseOccurrencePerYear;
            UI_ForestFire_WarmupDays.value = c.ForestFire.WarmupDays;

            UI_Thunderstorm_Enabled.isChecked = c.Thunderstorm.Enabled;
            UI_Thunderstorm_EvacuationMode.selectedIndex = c.Thunderstorm.EvacuationMode;
            UI_Thunderstorm_MaxProbability.value = c.Thunderstorm.BaseOccurrencePerYear;
            UI_Thunderstorm_MaxProbabilityMonth.selectedIndex = c.Thunderstorm.MaxProbabilityMonth - 1;
            UI_Thunderstorm_RainFactor.value = c.Thunderstorm.RainFactor;

            UI_Sinkhole_Enabled.isChecked = c.Sinkhole.Enabled;
            UI_Sinkhole_EvacuationMode.selectedIndex = c.Sinkhole.EvacuationMode;
            UI_Sinkhole_MaxProbability.value = c.Sinkhole.BaseOccurrencePerYear;
            UI_Sinkhole_GroundwaterCapacity.value = c.Sinkhole.GroundwaterCapacity;

            UI_Tornado_Enabled.isChecked = c.Tornado.Enabled;
            UI_Tornado_EvacuationMode.selectedIndex = c.Tornado.EvacuationMode;
            UI_Tornado_MaxProbability.value = c.Tornado.BaseOccurrencePerYear;
            UI_Tornado_MaxProbabilityMonth.selectedIndex = c.Tornado.MaxProbabilityMonth - 1;
            UI_Tornado_NoDuringFog.isChecked = c.Tornado.NoTornadoDuringFog;

            UI_Tsunami_Enabled.isChecked = c.Tsunami.Enabled;
            UI_Tsunami_EvacuationMode.selectedIndex = c.Tsunami.EvacuationMode;
            UI_Tsunami_MaxProbability.value = c.Tsunami.BaseOccurrencePerYear;
            UI_Tsunami_WarmupYears.value = c.Tsunami.WarmupYears;

            UI_Earthquake_Enabled.isChecked = c.Earthquake.Enabled;
            UI_Earthquake_EvacuationMode.selectedIndex = c.Earthquake.EvacuationMode;
            UI_Earthquake_MaxProbability.value = c.Earthquake.BaseOccurrencePerYear;
            UI_Earthquake_WarmupYears.value = c.Earthquake.WarmupYears;
            UI_Earthquake_AftershocksEnabled.isChecked = c.Earthquake.AftershocksEnabled;
            UI_Earthquake_NoCrack.isChecked = c.Earthquake.NoCracks;

            UI_MeteorStrike_Enabled.isChecked = c.MeteorStrike.Enabled;
            UI_MeteorStrike_EvacuationMode.selectedIndex = c.MeteorStrike.EvacuationMode;
            UI_MeteorStrike_MaxProbability.value = c.MeteorStrike.BaseOccurrencePerYear;
            UI_MeteorStrike_MeteorLongPeriodEnabled.isChecked = c.MeteorStrike.GetEnabled(0);
            UI_MeteorStrike_MeteorMediumPeriodEnabled.isChecked = c.MeteorStrike.GetEnabled(1);
            UI_MeteorStrike_MeteorShortPeriodEnabled.isChecked = c.MeteorStrike.GetEnabled(2);

            //AutoEvacuateRelease options
            //UI_StructureCollapse_AutoEvacuateRelease.selectedIndex = c.AutoEvacuateSettings.EvacuationMode;
            //UI_StructureFire_AutoEvacuateRelease.selectedIndex = c.AutoEvacuateSettings.EvacuationMode;

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

        public void BuildSettingsMenu(UIHelperBase helper)
        {
            DisastersSerializeBase disasterContainer = Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.container;

            #region Gegeral options

            UIHelperBase generalGroup = helper.AddGroup("General");

            UI_General_DisableAutoFocusOnDisasterStarts = (UICheckBox)generalGroup.AddCheckbox("Disable automatic disaster follow when it starts.", disasterContainer.DisableAutoFocusOnDisasterStarts, delegate (bool isChecked)
            {
                if (!freezeUI)
                {
                    disasterContainer.DisableAutoFocusOnDisasterStarts = isChecked;
                    DebugLogger.Log("000001-DisasterContainer.AutoFocusOnDisasterStarts changed: " + disasterContainer.DisableAutoFocusOnDisasterStarts);
                    DisasterExtension.SetAutoFocusOnDisasterBaseSettings(disasterContainer.DisableAutoFocusOnDisasterStarts);
                }
            });
            //UI_General_AutoFocusOnDisaster.tooltip = "Autofocus on disaster";

            UI_General_PauseOnDisasterStarts = (UICheckBox)generalGroup.AddCheckbox("Pause on disaster starts", disasterContainer.PauseOnDisasterStarts, delegate (bool isChecked)
            {
                if (!freezeUI)                
                    disasterContainer.PauseOnDisasterStarts = isChecked;
                                    
            });
            //UI_General_PauseOnDisasterStarts.tooltip = "Pause on disaster starts";

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

                Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.UpdateDisastersPanelToggleBtn();
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

            #endregion Gegeral options

            #region ForestFire

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
                freezeUI, 
                ref UI_ForestFire_EvacuationMode, 
                ref forestFireGroup, 
                evacuationModeText, 
                Helper.GetEvacuationOptions(), 
                ref disasterContainer.ForestFire.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.ForestFire.EvacuationMode = selection;                        
                    }
                }
            );

            helper.AddSpace(20);

            #endregion ForestFire

            #region Thunderstorm

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
                freezeUI, 
                ref UI_Thunderstorm_EvacuationMode, 
                ref thunderstormGroup, 
                evacuationModeText, 
                Helper.GetEvacuationOptions(), 
                ref disasterContainer.Thunderstorm.EvacuationMode,
                delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.Thunderstorm.EvacuationMode = selection;
                    }
                }
            );

            helper.AddSpace(20);

            #endregion Thunderstorm

            #region Sinkhole

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
                freezeUI, 
                ref UI_Sinkhole_EvacuationMode, 
                ref sinkholeGroup, 
                evacuationModeText, 
                Helper.GetEvacuationOptions(true), 
                ref disasterContainer.Sinkhole.EvacuationMode, 
                delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.Sinkhole.EvacuationMode = selection;
                    }
                });

            helper.AddSpace(20);

            #endregion Sinkhole

            #region Tornado

            UIHelperBase tornadoGroup = helper.AddGroup("Tornado disaster");

            UI_Tornado_MaxProbability = (UISlider)tornadoGroup.AddSlider("Max probability", 0.1f, 10f, 0.1f, disasterContainer.Tornado.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI) disasterContainer.Tornado.BaseOccurrencePerYear = val;
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

            ComponentHelpers.AddDropDown(
                freezeUI, 
                ref UI_Tornado_EvacuationMode, 
                ref tornadoGroup, 
                evacuationModeText, 
                Helper.GetEvacuationOptions(true), 
                ref disasterContainer.Tornado.EvacuationMode, delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.Tornado.EvacuationMode = selection;
                    }
                }
            );

            helper.AddSpace(20);

            #endregion Tornado

            #region Tsunami

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
                freezeUI, 
                ref UI_Tsunami_EvacuationMode, 
                ref tsunamiGroup, 
                evacuationModeText, 
                Helper.GetEvacuationOptions(true), 
                ref disasterContainer.Tsunami.EvacuationMode, delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.Tsunami.EvacuationMode = selection;
                    }
                }
           );

            helper.AddSpace(20);

            #endregion Tsunami

            #region Earthquake

            UIHelperBase earthquakeGroup = helper.AddGroup("Earthquake disaster");

            UI_Earthquake_MaxProbability = (UISlider)earthquakeGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, disasterContainer.Earthquake.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI) disasterContainer.Earthquake.BaseOccurrencePerYear = val;
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

            UI_Earthquake_NoCrack = (UICheckBox)earthquakeGroup.AddCheckbox("No cracks in the ground", disasterContainer.Earthquake.NoCracks, delegate (bool isChecked)
            {
                if (!freezeUI)
                    disasterContainer.Earthquake.NoCracks = isChecked;
                
                disasterContainer.Earthquake.UpdateDisasterProperties(true);
            });
            UI_Earthquake_NoCrack.tooltip = "If checked, the earthquake does not put a crack in the ground.";

           ComponentHelpers.AddDropDown(
                freezeUI, 
                ref UI_Earthquake_EvacuationMode, 
                ref earthquakeGroup, 
                evacuationModeText, 
                Helper.GetEvacuationOptions(), 
                ref disasterContainer.Earthquake.EvacuationMode, 
                delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.Earthquake.EvacuationMode = selection;
                    }
                }
            );

            helper.AddSpace(20);

            #endregion Earthquake

            #region MeteorStrike

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
                freezeUI, 
                ref UI_MeteorStrike_EvacuationMode,
                ref meteorStrikeGroup, 
                evacuationModeText, 
                Helper.GetEvacuationOptions(true), 
                ref disasterContainer.MeteorStrike.EvacuationMode, 
                delegate (int selection)
                {
                    if (!freezeUI)
                    {
                        disasterContainer.MeteorStrike.EvacuationMode = selection;
                    }
                }
            );

            helper.AddSpace(20);

            #endregion MeteorStrike

            #region SaveOptions

            // Save buttons
            UIHelperBase saveOptionsGroup = helper.AddGroup("Save options");

            saveOptionsGroup.AddButton("Save as default for new games", delegate ()
            {
                Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.container.Save();
            });
            saveOptionsGroup.AddButton("Reset to the last saved values", delegate ()
            {
                Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.ReadValuesFromFile();
                EnhancedDisastersOptionsUpdateUI();
            });
            saveOptionsGroup.AddButton("Reset to the mod default values", delegate ()
            {
                Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.ResetToDefaultValues();
                EnhancedDisastersOptionsUpdateUI();
            });

            #endregion SaveOptions
        }

        #endregion Options UI
    }
}