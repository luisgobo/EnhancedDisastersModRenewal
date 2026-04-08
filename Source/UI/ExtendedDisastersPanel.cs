using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using NaturalDisastersRenewal.UI.ComponentHelper;
using UnityEngine;
using UnityEngine.Serialization;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.UI
{
    public class ExtendedDisastersPanel : UIPanel
    {
        private const float LabelTextScaleSmall = 0.7f;
        private const float LabelTextScaleNormal = 0.8f;
        private const string HelpTutorialKey = "NDR_TUTORIAL_PANEL_HELP";

        private const float PanelWidth = 414f;
        private const float PanelHeight = 320f;

        [FormerlySerializedAs("Counter")] public int counter;
        private readonly NaturalDisasterHandler _disasterHandler = CommonServices.DisasterHandler;
        private uint _dayTimeframes;
        private uint _dayTimeOffsetFrames;

        private DisasterRowHelper[] _disasterRows;
        private UILabel _populationLabel;
        private UILabel _realTimeDayTimeFramesLabel;
        private UILabel _realTimeDayTimeOffsetFramesLabel;
        private UILabel _realTimeStatusLabel;
        private UILabel _realTimeTimeOffsetTicksLabel;
        private UIPanel _selectedRadioPanel;

        private bool _realTimeStatus;
        private long _timeOffsetTicks;

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

            BuildInformationBar();
            BuildTabContainer();

            //Get List 
            _disasterHandler.GetSpriteNames();
        }

        public override void Update()
        {
            base.Update();

            if (!isVisible) return;

            if (--counter > 0) return;
            counter = 10;

            DefineMinPopulationLabelContent();

            var disasterCount = _disasterHandler.Container.DisasterList.Count;

            for (var i = 0; i < disasterCount; i++)
            {
                _disasterRows[i].Refresh();
            }

            GetRealTimeModValues();

            var statusText = LocalizationService.Get(_realTimeStatus ? "status.active" : "status.inactive");
            _realTimeStatusLabel.text = LocalizationService.Format("panel.realTimeStatus", statusText);
            _realTimeTimeOffsetTicksLabel.text = LocalizationService.Format("panel.timeOffsetTicks", _timeOffsetTicks);
            _realTimeDayTimeFramesLabel.text = LocalizationService.Format("panel.dayTimeFrames", _dayTimeframes);
            _realTimeDayTimeOffsetFramesLabel.text = LocalizationService.Format("panel.dayTimeOffsetFrames", _dayTimeOffsetFrames);
        }

        private void BuildStatisticsInfoTabContent(UIScrollablePanel parentPanel)
        {
            const float itemSpacing = 22f;
            const float xPosition = 1f;
            var yPosition = 10f;
            var probabilityBarStartX = xPosition + DisasterRowHelper.probabilityBarX;
            var probabilityBarCenterX = probabilityBarStartX + DisasterRowHelper.barWidth * 0.5f;
            var probabilityBarEndX = probabilityBarStartX + DisasterRowHelper.barWidth;
            var intensityBarStartX = xPosition + DisasterRowHelper.intensityBarX;
            var intensityBarCenterX = intensityBarStartX + DisasterRowHelper.barWidth * 0.5f;
            var intensityBarEndX = intensityBarStartX + DisasterRowHelper.barWidth;

            _populationLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal, "");

            DefineMinPopulationLabelContent();

            yPosition += 22f;
            AddLabel(parentPanel, xPosition + 32f, yPosition, LabelTextScaleSmall, LocalizationService.Get("panel.header.disaster"), "Disaster name");

            AddCenteredLabel(parentPanel, probabilityBarCenterX - 3f, yPosition, LabelTextScaleSmall, LocalizationService.Get("panel.header.howOften"));
            AddCenteredLabel(parentPanel, intensityBarCenterX, yPosition, LabelTextScaleSmall, LocalizationService.Get("panel.header.maxStrength"));

            yPosition += 15;
            AddCenteredLabel(parentPanel, probabilityBarStartX + 2f, yPosition, LabelTextScaleSmall, "1");
            AddCenteredLabel(parentPanel, probabilityBarCenterX, yPosition, LabelTextScaleSmall, "50");
            AddCenteredLabel(parentPanel, probabilityBarEndX - 10f, yPosition, LabelTextScaleSmall, "100");
            AddCenteredLabel(parentPanel, intensityBarStartX + 10f, yPosition, LabelTextScaleSmall, "0.0");
            AddCenteredLabel(parentPanel, intensityBarEndX - 13f, yPosition, LabelTextScaleSmall, "25.5");

            var disasterCount = _disasterHandler.Container.DisasterList.Count;
            _disasterRows = new DisasterRowHelper[disasterCount];

            //Add each statistic item to the Panel
            yPosition += 15;
            for (var i = 0; i < disasterCount; i++)
            {
                var disaster = _disasterHandler.Container.DisasterList[i];
                var disasterRow = parentPanel.AddUIComponent<DisasterRowHelper>();
                disasterRow.Initialize(disaster, xPosition, yPosition, ToggleDisasterState);
                disasterRow.Refresh();
                _disasterRows[i] = disasterRow;

                yPosition += itemSpacing;
            }

            yPosition += 10;
            BuildStopDisasterButton(parentPanel, xPosition, yPosition);
        }

        private void DefineMinPopulationLabelContent()
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";

            var maxPopulationToTriggerDisaster = (int)_disasterHandler.Container.MaxPopulationToTriggerHigherDisasters;

            var formatNumber = maxPopulationToTriggerDisaster.ToString("#,0", nfi);

            _populationLabel.text = LocalizationService.Format("panel.populationThreshold", formatNumber);
            _populationLabel.tooltip = LocalizationService.Get("panel.populationThreshold.tooltip");
        }

        private static void OnTakeItEasySelected()
        {
            // Lógica para "Take it easy"
            Debug.Log("Take it easy selected. Implement your logic here.");
        }

        private static void OnExtraChaosSelected()
        {
            // Lógica para "I need some extra caos"
            Debug.Log("Extra chaos selected. Implement your logic here.");
        }

        private void BuildSettingsTabContent(UIComponent parentPanel)
        {
            GetRealTimeModValues();

            const int xPosition = 20;
            var yPosition = 10;

            _realTimeStatusLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal,
                LocalizationService.Format("panel.realTimeStatus", LocalizationService.Get(_realTimeStatus ? "status.active" : "status.inactive")));
            _realTimeStatusLabel.tooltip = LocalizationService.Get("panel.timeFlowNote");

            yPosition += 20;
            _realTimeTimeOffsetTicksLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal,
                LocalizationService.Format("panel.timeOffsetTicks", _timeOffsetTicks));

            yPosition += 20;
            _realTimeDayTimeFramesLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal,
                LocalizationService.Format("panel.dayTimeFrames", _dayTimeframes));

            yPosition += 20;
            _realTimeDayTimeOffsetFramesLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal,
                LocalizationService.Format("panel.dayTimeOffsetFrames", _dayTimeOffsetFrames));

            yPosition += 30;
            AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleSmall, LocalizationService.Get("panel.timeFlowNote"));

            yPosition += 30;

            var radioGroup = parentPanel.AddUIComponent<UIPanel>();
            radioGroup.relativePosition = new Vector3(20, yPosition);
            radioGroup.size = new Vector2(350, 60);
            radioGroup.backgroundSprite = "SubcategoriesPanel";
            radioGroup.name = "radioGroup";
            radioGroup.isVisible = true;

            var radioEasyButton = CreateRadioButton(radioGroup, "Take it easy",
                10f, 10f, true, "Not everything is destruction and chaos. ");
            var radioChaosButton = CreateRadioButton(radioGroup, "I need some extra chaos",
                10f, 35f, tooltipText: "Lets make this city a bit more challenging");
            var spriteEasy = radioEasyButton.components.OfType<UISprite>().FirstOrDefault();
            var spriteChaos = radioChaosButton.components.OfType<UISprite>().FirstOrDefault();

            _selectedRadioPanel = radioEasyButton;

            radioEasyButton.eventClick += (c, p) =>
            {
                if (_selectedRadioPanel == radioEasyButton) return;

                if (spriteEasy != null) spriteEasy.spriteName = "check-checked";
                if (spriteChaos != null) spriteChaos.spriteName = "check-unchecked";
                _selectedRadioPanel = radioEasyButton;
                OnTakeItEasySelected();
            };

            radioChaosButton.eventClick += (c, p) =>
            {
                if (_selectedRadioPanel == radioChaosButton) return;

                if (spriteEasy != null) spriteEasy.spriteName = "check-unchecked";
                if (spriteChaos != null) spriteChaos.spriteName = "check-checked";
                _selectedRadioPanel = radioChaosButton;
                OnExtraChaosSelected();
            };
        }

        private void GetRealTimeModValues()
        {
            _realTimeStatus = _disasterHandler.CheckRealTimeModActive();

            var sm = SimulationManager.instance;
            _timeOffsetTicks = sm.m_timeOffsetTicks;
            _dayTimeframes = sm.m_dayTimeFrame;
            _dayTimeOffsetFrames = sm.m_dayTimeOffsetFrames;
        }

        private static UIPanel CreateRadioButton(UIPanel radioGroup, string text, float xPosition,
            float yPosition, bool isChecked = false, string tooltipText = "")
        {
            var radioPanel = radioGroup.AddUIComponent<UIPanel>();
            radioPanel.size = new Vector2(200, 20);
            radioPanel.relativePosition = new Vector3(xPosition, yPosition);
            radioPanel.tooltip = string.IsNullOrEmpty(tooltipText) ? text : tooltipText;

            var uiSprite = radioPanel.AddUIComponent<UISprite>();
            uiSprite.spriteName = isChecked ? "check-checked" : "check-unchecked";
            uiSprite.size = new Vector2(16, 16);
            uiSprite.relativePosition = new Vector3(0, 0);

            var label = radioPanel.AddUIComponent<UILabel>();
            label.text = text;
            label.relativePosition = new Vector3(xPosition + 12, 0);

            return radioPanel;
        }

        private void BuildInformationBar()
        {
            var titleBar = AddUIComponent<UIPanel>();
            titleBar.name = "titleBar";
            titleBar.relativePosition = Vector3.zero;
            titleBar.size = new Vector2(PanelWidth - 80f, 40f);
            titleBar.tooltip = LocalizationService.Get("panel.drag.tooltip");
            titleBar.isInteractive = true;

            BuildPanelTitle(titleBar);
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
                LocalizationService.Get("panel.help.button.tooltip"),
                clickHandler:HelpBtn_eventClick,
                textPadding:new RectOffset(1, 0, 5, 0));

            ActionButtonHelper.CreateTextButton(
                parentPanel,
                "closeBtn",
                "X",
                new Vector3(PanelWidth - 35f, 8f),
                new Vector2(25f, 25f),
                clickHandler:ClosePanelBtn_eventClick,
                textPadding:new RectOffset(2, 0, 5, 0));
        }

        private void HelpBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ShowHelpPanel();
        }

        private static void ShowHelpPanel()
        {
            var advisorPanel = ToolsModifierControl.advisorPanel;
            if (advisorPanel == null)
            {
                return;
            }

            RegisterHelpTutorialLocalization();
            advisorPanel.Show(HelpTutorialKey, "ToolbarIconZoomOutGlobe", string.Empty, 0f);
        }

        private static void RegisterHelpTutorialLocalization()
        {
            var locale = GetLocale();
            if (locale == null)
            {
                return;
            }

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
            catch (System.ArgumentException)
            {
                TryUpdateLocalizedString(locale, key, value);
            }
        }

        private static void TryUpdateLocalizedString(Locale locale, Locale.Key key, string value)
        {
            var localeType = locale.GetType();
            var fields = localeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (!typeof(IDictionary).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                if (field.GetValue(locale) is not IDictionary dictionary)
                {
                    continue;
                }

                dictionary[key] = value;
                return;
            }
        }

        private static Locale GetLocale()
        {
            return typeof(LocaleManager)
                .GetField("m_Locale", BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(LocaleManager.instance) as Locale;
        }

        private static void BuildPanelTitle(UIComponent parentPanel)
        {
            var lTitle = parentPanel.AddUIComponent<UILabel>();
            lTitle.relativePosition = new Vector3(10, 15);
            lTitle.text = LocalizationService.Get("panel.title");
            lTitle.tooltip = LocalizationService.Get("panel.drag.tooltip");
        }

        private void BuildTabContainer()
        {
            // Create tab container 
            var tabContainer = AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, 70);
            tabContainer.size = new Vector2(width, height - 50);

            // Create tab strip
            var tabStrip = AddUIComponent<UITabstrip>();
            tabStrip.relativePosition = new Vector3(0, 40);
            tabStrip.size = new Vector2(width, 30);
            tabStrip.backgroundSprite = "SubcategoriesPanel";
            tabStrip.tabPages = tabContainer;

            var statisticsTab = TabHelper.CreateStyledTab(tabStrip, LocalizationService.Get("panel.tab.statistics"), 0f);
            TabHelper.CreateStyledTab(tabStrip, LocalizationService.Get("panel.tab.settings"), statisticsTab.width);

            if (tabContainer.components.Count < 2) return;
            const float yPosition = 10f;

            // Tab 1: Statistics
            var statisticsTabPanel = tabContainer.components[0] as UIPanel;
            var statisticsScrollablePanel = ScrollablePanelHelper.Create(statisticsTabPanel, yPosition);

            BuildStatisticsInfoTabContent(statisticsScrollablePanel);
            if (statisticsTabPanel != null) statisticsTabPanel.isVisible = true;

            // Tab 2: Settings
            var settingsTabPanel = tabContainer.components[1] as UIPanel;
            var settingsScrollablePanel = ScrollablePanelHelper.Create(settingsTabPanel, yPosition);

            BuildSettingsTabContent(settingsScrollablePanel);
            if (settingsTabPanel != null) settingsTabPanel.isVisible = false;
        }

        private void BuildStopDisasterButton(UIComponent parentPanel, float xPosition, float yPosition)
        {
            ActionButtonHelper.CreateTextButton(
                parentPanel,
                "stopDisasterBtn",
                "■",
                new Vector3(xPosition, yPosition - 5),
                new Vector2(18f, 18f),
                textColor:Color.red,
                clickHandler:StopAllDisastersBtn_eventClick,
                normalBgSprite:"ButtonMenu",
                hoveredBgSprite:"ButtonMenuHovered",
                pressedBgSprite:"ButtonMenuHovered",
                tooltip:LocalizationService.Get("panel.stopAll"));

            ActionButtonHelper.CreateTextButton(
                parentPanel,
                "resetDisasterBtn",
                "↺",
                new Vector3(xPosition + 22, yPosition - 5),
                new Vector2(18f, 18f),
                textColor:Color.yellow,
                clickHandler:ResetAllDisastersBtn_eventClick,
                normalBgSprite:"ButtonMenu",
                hoveredBgSprite:"ButtonMenuHovered",
                pressedBgSprite:"ButtonMenuHovered",
                tooltip:LocalizationService.Get("panel.resetAll"));
        }

        private static void ResetAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            // Stop active disasters first
            StopAllDisasters();

            // Reset each disaster
            var diasterManager = CommonServices.Disasters;

            // Stop active evacuations
            if (diasterManager != null)
                diasterManager.EvacuateAll(true);

            // Reset all disasters
            ResetAllDisasters();

            Debug.Log("All disasters have been reset");
        }

        private static void ResetAllDisasters()
        {
            // Get the instance of the natural disaster handler
            var diasterHandler = CommonServices.DisasterHandler;

            // If there is no instance, exit
            if (diasterHandler == null) return;

            // Stop all active evacuations
            var dm = CommonServices.Disasters;
            if (dm != null) dm.EvacuateAll(true);

            // Iterate through all disasters and reset them
            foreach (var disaster in diasterHandler.Container.DisasterList)
            {
                disaster.ResetDisasterProbabilities();
            }

            Debug.Log("All disasters have been reset to default values");
        }

        private static void StopAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            StopAllDisasters();
        }

        private static void StopAllDisasters()
        {
            // Max vehicle buffer size in Cities: Skylines (2^14 = 16384)
            const int maxVehicleBufferSize = 16384;
            
            var cancellingDisasterFlags = new StringBuilder();
            var cancelledDisasters = 0;
            var releasedVehicles = 0;
            var vehicleManager = CommonServices.Vehicles;

            for (var i = 1; i < maxVehicleBufferSize; i++)
            {
                if ((vehicleManager.m_vehicles.m_buffer[i].m_flags & Vehicle.Flags.Created) == 0) continue;

                if (vehicleManager.m_vehicles.m_buffer[i].Info.m_vehicleAI is MeteorAI)
                {
                    vehicleManager.ReleaseVehicle((ushort)i);
                    releasedVehicles++;
                }

                if (vehicleManager.m_vehicles.m_buffer[i].Info.m_vehicleAI is not VortexAI) continue;
                
                vehicleManager.ReleaseVehicle((ushort)i);
                releasedVehicles++;
            }

            var waterSimulation = CommonServices.Water;
            if (waterSimulation != null)
            {
                for (var i = waterSimulation.m_waterWaves.m_size; i >= 1; i--)
                    CommonServices.Terrain.WaterSimulation.ReleaseWaterWave((ushort)i);
            }

            var disasterManager = CommonServices.Disasters;
            for (ushort i = 0; i < disasterManager.m_disasterCount; i++)
            {
                cancellingDisasterFlags.AppendLine(disasterManager.m_disasters.m_buffer[i].Info.name + " flags: " +
                                                   disasterManager.m_disasters.m_buffer[i].m_flags);
                if ((disasterManager.m_disasters.m_buffer[i].m_flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active |
                                                                        DisasterData.Flags.Clearing)) ==
                    DisasterData.Flags.None) continue;
                if (!IsStoppableDisaster(disasterManager.m_disasters.m_buffer[i].Info.m_disasterAI)) continue;
                cancellingDisasterFlags.AppendLine("Trying to cancel " + disasterManager.m_disasters.m_buffer[i].Info.name);
                disasterManager.m_disasters.m_buffer[i].m_flags =
                    disasterManager.m_disasters.m_buffer[i].m_flags & ~(DisasterData.Flags.Emerging |
                                                                        DisasterData.Flags.Active |
                                                                        DisasterData.Flags.Clearing) |
                    DisasterData.Flags.Finished;
                cancelledDisasters++;
            }

            var disasterHandler = CommonServices.DisasterHandler;
            if (disasterHandler != null)
            {
                disasterHandler.Container.ActiveDisasters.Clear();
            }

            disasterManager.EvacuateAll(true);
            cancellingDisasterFlags.AppendLine($"Cancelled disasters: {cancelledDisasters}");
            cancellingDisasterFlags.AppendLine($"Released disaster vehicles: {releasedVehicles}");
            Debug.Log(cancellingDisasterFlags.ToString());
        }

        private void ClosePanelBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            _disasterHandler.SetDisasterPanelVisibility(false);
        }

        private void ToggleDisasterState(DisasterBaseModel disaster)
        {
            disaster.IsDisasterEnabled = !disaster.IsDisasterEnabled;
            switch (disaster.GetDisasterType())
            {
                case DisasterType.Earthquake:
                    _disasterHandler.Container.Earthquake.IsDisasterEnabled = disaster.IsDisasterEnabled;
                    break;

                case DisasterType.ForestFire:
                    _disasterHandler.Container.ForestFire.IsDisasterEnabled = disaster.IsDisasterEnabled;
                    break;

                case DisasterType.MeteorStrike:
                    _disasterHandler.Container.MeteorStrike.IsDisasterEnabled = disaster.IsDisasterEnabled;
                    break;

                case DisasterType.Sinkhole:
                    _disasterHandler.Container.Sinkhole.IsDisasterEnabled = disaster.IsDisasterEnabled;
                    break;

                case DisasterType.ThunderStorm:
                    _disasterHandler.Container.Thunderstorm.IsDisasterEnabled = disaster.IsDisasterEnabled;
                    break;

                case DisasterType.Tornado:
                    _disasterHandler.Container.Tornado.IsDisasterEnabled = disaster.IsDisasterEnabled;
                    break;

                case DisasterType.Tsunami:
                    _disasterHandler.Container.Tsunami.IsDisasterEnabled = disaster.IsDisasterEnabled;
                    break;
            }

            if (_disasterRows != null)
            {
                foreach (var disasterRow in _disasterRows)
                {
                    disasterRow.Refresh();
                }
            }

            SettingsScreen.UpdateUISettingsOptions();
        }

        private static bool IsStoppableDisaster(DisasterAI ai)
        {
            return ai as ThunderStormAI != null || ai as SinkholeAI != null || ai as TornadoAI != null ||
                   ai as EarthquakeAI != null || ai as MeteorStrikeAI != null || ai as ForestFireAI != null ||
                   ai as TsunamiAI != null;
        }

        private static UILabel AddLabel(UIComponent parentPanel, float x, float y, float textScale,
            string text, string tooltipText = "")
        {
            var label = parentPanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(x, y);
            label.textScale = textScale;
            label.text = text;
            if (tooltipText != "")
                label.tooltip = tooltipText;

            return label;
        }

        private static UILabel AddCenteredLabel(UIComponent parentPanel, float centerX, float y, float textScale,
            string text, string tooltipText = "")
        {
            var label = AddLabel(parentPanel, 0f, y, textScale, text, tooltipText);
            // label.PerformLayout();
            label.relativePosition = new Vector3(centerX - label.width * 0.5f, y);
            return label;
        }
    }
}

//TODO:
// * Make all possible elementes of UI reusable components
// * Adjust recurrence earthquakes and tsunamis, calculation is getting extremely slow
