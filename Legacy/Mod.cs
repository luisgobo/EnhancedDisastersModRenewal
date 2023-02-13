using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System.Reflection;
using UnityEngine;

namespace EnhancedDisastersMod
{
    public class Mod : IUserMod
    {
        public static string ModNameEng = "Natural Disasters Overhaul";
        public static string LogMsgPrefix = ">>> " + ModNameEng + ": ";
        public static string Version = "2020/5/18";
        private bool freezeUI = false;

        private UICheckBox UI_ScaleMaxIntensityWithPopilation;
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

            UI_ScaleMaxIntensityWithPopilation.isChecked = c.ScaleMaxIntensityWithPopilation;
            UI_RecordDisasterEventsChkBox.isChecked = c.RecordDisasterEvents;
            UI_ShowDisasterPanelButton.isChecked = c.ShowDisasterPanelButton;

            UI_ForestFire_Enabled.isChecked = c.ForestFire.Enabled;
            ForestFireMaxProbabilityUI.value = c.ForestFire.BaseOccurrencePerYear;
            UI_ForestFire_WarmupDays.value = c.ForestFire.WarmupDays;

            UI_Thunderstorm_Enabled.isChecked = c.Thunderstorm.Enabled;
            UI_Thunderstorm_MaxProbability.value = c.Thunderstorm.BaseOccurrencePerYear;
            UI_Thunderstorm_MaxProbabilityMonth.selectedIndex = c.Thunderstorm.MaxProbabilityMonth - 1;
            UI_Thunderstorm_RainFactor.value = c.Thunderstorm.RainFactor;

            UI_Sinkhole_Enabled.isChecked = c.Sinkhole.Enabled;
            UI_Sinkhole_MaxProbability.value = c.Sinkhole.BaseOccurrencePerYear;
            UI_Sinkhole_GroundwaterCapacity.value = c.Sinkhole.GroundwaterCapacity;

            UI_Tornado_Enabled.isChecked = c.Tornado.Enabled;
            UI_Tornado_MaxProbability.value = c.Tornado.BaseOccurrencePerYear;
            UI_Tornado_MaxProbabilityMonth.selectedIndex = c.Tornado.MaxProbabilityMonth - 1;
            UI_Tornado_NoDuringFog.isChecked = c.Tornado.NoTornadoDuringFog;

            UI_Tsunami_Enabled.isChecked = c.Tsunami.Enabled;
            UI_Tsunami_MaxProbability.value = c.Tsunami.BaseOccurrencePerYear;
            UI_Tsunami_WarmupYears.value = c.Tsunami.WarmupYears;

            UI_Earthquake_Enabled.isChecked = c.Earthquake.Enabled;
            UI_Earthquake_MaxProbability.value = c.Earthquake.BaseOccurrencePerYear;
            UI_Earthquake_WarmupYears.value = c.Earthquake.WarmupYears;
            UI_Earthquake_AftershocksEnabled.isChecked = c.Earthquake.AftershocksEnabled;
            UI_Earthquake_NoCrack.isChecked = c.Earthquake.NoCracks;

            UI_MeteorStrike_Enabled.isChecked = c.MeteorStrike.Enabled;
            UI_MeteorStrike_MaxProbability.value = c.MeteorStrike.BaseOccurrencePerYear;
            UI_MeteorStrike_Meteor1Enabled.isChecked = c.MeteorStrike.GetEnabled(0);
            UI_MeteorStrike_Meteor2Enabled.isChecked = c.MeteorStrike.GetEnabled(1);
            UI_MeteorStrike_Meteor3Enabled.isChecked = c.MeteorStrike.GetEnabled(2);

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

        public void OnSettingsUI(UIHelperBase helper)
        {
            DisastersContainer c = Singleton<EnhancedDisastersManager>.instance.container;

            UI_ForestFire_Enabled = (UICheckBox)helper.AddCheckbox("Enable Forest Fire", c.ForestFire.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI) c.ForestFire.Enabled = isChecked;
            });
            UI_Thunderstorm_Enabled = (UICheckBox)helper.AddCheckbox("Enable Thunderstorm", c.Thunderstorm.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI) c.Thunderstorm.Enabled = isChecked;
            });
            UI_Sinkhole_Enabled = (UICheckBox)helper.AddCheckbox("Enable Sinkhole", c.Sinkhole.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI) c.Sinkhole.Enabled = isChecked;
            });
            UI_Tornado_Enabled = (UICheckBox)helper.AddCheckbox("Enable Tornado", c.Tornado.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI) c.Tornado.Enabled = isChecked;
            });
            UI_Tsunami_Enabled = (UICheckBox)helper.AddCheckbox("Enable Tsunami", c.Tsunami.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI) c.Tsunami.Enabled = isChecked;
            });
            UI_Earthquake_Enabled = (UICheckBox)helper.AddCheckbox("Enable Earthquake", c.Earthquake.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI) c.Earthquake.Enabled = isChecked;
            });
            UI_MeteorStrike_Enabled = (UICheckBox)helper.AddCheckbox("Enable Meteor Strike", c.MeteorStrike.Enabled, delegate (bool isChecked)
            {
                if (!freezeUI) c.MeteorStrike.Enabled = isChecked;
            });

            helper.AddGroup(" "); // Adds horizontal line

            #region ForestFire

            UIHelperBase forestFireGroup = helper.AddGroup("Forest Fire disaster");

            ForestFireMaxProbabilityUI = (UISlider)forestFireGroup.AddSlider("Max probability", 1, 50, 1, c.ForestFire.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI) c.ForestFire.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(ForestFireMaxProbabilityUI, " times per year");
            ForestFireMaxProbabilityUI.tooltip = "Occurrence (per year) in case of a long period without rain";

            UI_ForestFire_WarmupDays = (UISlider)forestFireGroup.AddSlider("Warmup period", 0, 360, 10, c.ForestFire.WarmupDays, delegate (float val)
            {
                if (!freezeUI) c.ForestFire.WarmupDays = (int)val;
            });
            addLabelToSlider(UI_ForestFire_WarmupDays, " days");
            UI_ForestFire_WarmupDays.tooltip = "No-rain period during wich the probability of Forest Fire increases";

            helper.AddSpace(20);

            #endregion ForestFire

            #region Thunderstorm

            UIHelperBase thunderstormGroup = helper.AddGroup("Thunderstorm disaster");

            UI_Thunderstorm_MaxProbability = (UISlider)thunderstormGroup.AddSlider("Max probability", 0.1f, 10f, 0.1f, c.Thunderstorm.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI) c.Thunderstorm.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_Thunderstorm_MaxProbability, " times per year");
            UI_Thunderstorm_MaxProbability.tooltip = "Occurrence (per year) in thunderstorm season";

            UI_Thunderstorm_MaxProbabilityMonth = (UIDropDown)thunderstormGroup.AddDropdown("Thunderstorm season peak",
                Helper.GetMonths(),
                c.Thunderstorm.MaxProbabilityMonth - 1,
                delegate (int sel)
                {
                    if (!freezeUI) c.Thunderstorm.MaxProbabilityMonth = sel + 1;
                });

            UI_Thunderstorm_RainFactor = (UISlider)thunderstormGroup.AddSlider("Rain factor", 1f, 5f, 0.1f, c.Thunderstorm.RainFactor, delegate (float val)
            {
                if (!freezeUI) c.Thunderstorm.RainFactor = val;
            });
            addLabelToSlider(UI_Thunderstorm_RainFactor);
            UI_Thunderstorm_RainFactor.tooltip = "Thunderstorm probability increases by this factor during rain.";

            helper.AddSpace(20);

            #endregion Thunderstorm

            #region Sinkhole

            UIHelperBase sinkholeGroup = helper.AddGroup("Sinkhole disaster");

            UI_Sinkhole_MaxProbability = (UISlider)sinkholeGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, c.Sinkhole.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI) c.Sinkhole.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_Sinkhole_MaxProbability, " times per year");
            UI_Sinkhole_MaxProbability.tooltip = "Occurrence (per year) in case of a long period of rain";

            UI_Sinkhole_GroundwaterCapacity = (UISlider)sinkholeGroup.AddSlider("Groundwater capacity", 1, 100, 1, c.Sinkhole.GroundwaterCapacity, delegate (float val)
            {
                if (!freezeUI) c.Sinkhole.GroundwaterCapacity = val;
            });
            addLabelToSlider(UI_Sinkhole_GroundwaterCapacity);
            UI_Sinkhole_GroundwaterCapacity.tooltip = "Set how fast groundwater fills up during rain and causes a sinkhole to appear.";

            helper.AddSpace(20);

            #endregion Sinkhole

            #region Tornado

            UIHelperBase tornadoGroup = helper.AddGroup("Tornado disaster");

            UI_Tornado_MaxProbability = (UISlider)tornadoGroup.AddSlider("Max probability", 0.1f, 10f, 0.1f, c.Tornado.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI) c.Tornado.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_Tornado_MaxProbability, " times per year");
            UI_Tornado_MaxProbability.tooltip = "Occurrence (per year) in Tornado season";

            UI_Tornado_MaxProbabilityMonth = (UIDropDown)tornadoGroup.AddDropdown("Tornado season peak",
                Helper.GetMonths(),
                c.Tornado.MaxProbabilityMonth - 1,
                delegate (int sel)
                {
                    if (!freezeUI) c.Tornado.MaxProbabilityMonth = sel + 1;
                });

            UI_Tornado_NoDuringFog = (UICheckBox)tornadoGroup.AddCheckbox("No Tornado during fog", c.Tornado.NoTornadoDuringFog, delegate (bool isChecked)
            {
                if (!freezeUI) c.Tornado.NoTornadoDuringFog = isChecked;
            });
            UI_Tornado_NoDuringFog.tooltip = "Tornado does not occur during foggy weather";

            helper.AddSpace(20);

            #endregion Tornado

            #region Tsunami

            UIHelperBase tsunamiGroup = helper.AddGroup("Tsunami disaster");

            UI_Tsunami_MaxProbability = (UISlider)tsunamiGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, c.Tsunami.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI) c.Tsunami.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_Tsunami_MaxProbability, " times per year");
            UI_Tsunami_MaxProbability.tooltip = "Maximum occurrence (per year) after a long period without tsunamis";

            UI_Tsunami_WarmupYears = (UISlider)tsunamiGroup.AddSlider("Charge period", 0, 20, 0.5f, c.Tsunami.WarmupYears, delegate (float val)
            {
                if (!freezeUI) c.Tsunami.WarmupYears = val;
            });
            addLabelToSlider(UI_Tsunami_WarmupYears, " years");
            UI_Tsunami_WarmupYears.tooltip = "The probability of tsunami increases to the maximum during this period";

            helper.AddSpace(20);

            #endregion Tsunami

            #region Earthquake

            UIHelperBase earthquakeGroup = helper.AddGroup("Earthquake disaster");

            UI_Earthquake_MaxProbability = (UISlider)earthquakeGroup.AddSlider("Max probability", 0.1f, 10, 0.1f, c.Earthquake.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI) c.Earthquake.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_Earthquake_MaxProbability, " times per year");
            UI_Earthquake_MaxProbability.tooltip = "Maximum occurrence (per year) after a long period without earthquakes";

            UI_Earthquake_WarmupYears = (UISlider)earthquakeGroup.AddSlider("Charge period", 0, 20, 0.5f, c.Earthquake.WarmupYears, delegate (float val)
            {
                if (!freezeUI) c.Earthquake.WarmupYears = val;
            });
            addLabelToSlider(UI_Earthquake_WarmupYears, " years");
            UI_Earthquake_WarmupYears.tooltip = "The probability of earthquake increases to the maximum during this period";

            UI_Earthquake_AftershocksEnabled = (UICheckBox)earthquakeGroup.AddCheckbox("Enable aftershocks", c.Earthquake.AftershocksEnabled, delegate (bool isChecked)
            {
                if (!freezeUI) c.Earthquake.AftershocksEnabled = isChecked;
            });
            UI_Earthquake_AftershocksEnabled.tooltip = "Several aftershocks may occur after a big earthquake. Aftershocks strike the same place.";

            UI_Earthquake_NoCrack = (UICheckBox)earthquakeGroup.AddCheckbox("No cracks in the ground", c.Earthquake.NoCracks, delegate (bool isChecked)
            {
                if (!freezeUI) c.Earthquake.NoCracks = isChecked;
                c.Earthquake.UpdateDisasterProperties(true);
            });
            UI_Earthquake_NoCrack.tooltip = "If checked, the earthquake does not put a crack in the ground.";

            helper.AddSpace(20);

            #endregion Earthquake

            #region MeteorStrike

            UIHelperBase meteorStrikeGroup = helper.AddGroup("Meteor Strike disaster");

            UI_MeteorStrike_MaxProbability = (UISlider)meteorStrikeGroup.AddSlider("Max probability", 1f, 50, 1f, c.MeteorStrike.BaseOccurrencePerYear, delegate (float val)
            {
                if (!freezeUI) c.MeteorStrike.BaseOccurrencePerYear = val;
            });
            addLabelToSlider(UI_MeteorStrike_MaxProbability, " times per year");
            UI_MeteorStrike_MaxProbability.tooltip = "Maximum occurrence of meteor strike per year per one meteor when it approaches the Earth";

            UI_MeteorStrike_Meteor1Enabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable long period (9 years) meteor", c.MeteorStrike.GetEnabled(0), delegate (bool isChecked)
            {
                if (!freezeUI) c.MeteorStrike.SetEnabled(0, isChecked);
            });

            UI_MeteorStrike_Meteor2Enabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable medium period (5 years) meteor", c.MeteorStrike.GetEnabled(1), delegate (bool isChecked)
            {
                if (!freezeUI) c.MeteorStrike.SetEnabled(1, isChecked);
            });

            UI_MeteorStrike_Meteor3Enabled = (UICheckBox)meteorStrikeGroup.AddCheckbox("Enable short period (2 years) meteor", c.MeteorStrike.GetEnabled(2), delegate (bool isChecked)
            {
                if (!freezeUI) c.MeteorStrike.SetEnabled(2, isChecked);
            });

            helper.AddSpace(20);

            #endregion MeteorStrike

            // Save buttons
            helper.AddButton("Save as default for new games", delegate ()
            {
                Singleton<EnhancedDisastersManager>.instance.container.Save();
            });
            helper.AddButton("Reset to the last saved values", delegate ()
            {
                Singleton<EnhancedDisastersManager>.instance.ReadValuesFromFile();
                EnhancedDisastersOptionsUpdateUI();
            });
            helper.AddButton("Reset to the mod default values", delegate ()
            {
                Singleton<EnhancedDisastersManager>.instance.ResetToDefaultValues();
                EnhancedDisastersOptionsUpdateUI();
            });
            helper.AddSpace(20);

            UI_ScaleMaxIntensityWithPopilation = (UICheckBox)helper.AddCheckbox("Scale max intensity with population", c.ScaleMaxIntensityWithPopilation, delegate (bool isChecked)
            {
                if (!freezeUI) c.ScaleMaxIntensityWithPopilation = isChecked;
            });
            UI_ScaleMaxIntensityWithPopilation.tooltip = "Maximum intensity for all disasters is set to the minimum at the beginning of the game and gradually increases as the city grows.";

            UI_RecordDisasterEventsChkBox = (UICheckBox)helper.AddCheckbox("Record disaster events", c.RecordDisasterEvents, delegate (bool isChecked)
            {
                if (!freezeUI) c.RecordDisasterEvents = isChecked;
            });
            UI_RecordDisasterEventsChkBox.tooltip = "Write out disaster name, date of occurrence, and intencity into Disasters.csv file";

            UI_ShowDisasterPanelButton = (UICheckBox)helper.AddCheckbox("Show Disasters Panel toggle button", c.ShowDisasterPanelButton, delegate (bool isChecked)
            {
                if (!freezeUI) c.ShowDisasterPanelButton = isChecked;

                Singleton<EnhancedDisastersManager>.instance.UpdateDisastersPanelToggleBtn();
            });

            helper.AddSpace(20);
        }

        #endregion Options UI
    }
}