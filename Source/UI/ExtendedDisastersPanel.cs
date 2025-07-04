using System.Globalization;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.UI;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using UnityEngine;
using UnityEngine.Serialization;

namespace NaturalDisastersRenewal.UI
{
    public class ExtendedDisastersPanel : UIPanel
    {
        private const string pauseSprite = "ButtonPause";
        private const string playSprite = "ButtonPlayFocused";
        private const float labelTextScaleSmall = 0.7f;
        private const float labelTextScaleNormal = 0.8f;

        [FormerlySerializedAs("Counter")] public int counter;
        private readonly NaturalDisasterHandler _disasterHandler = Singleton<NaturalDisasterHandler>.instance;
        private uint _dayTimeframes;
        private uint _dayTimeOffsetFrames;

        private UILabel[] _disasterLabelCalculations;

        private UILabel[] _disasterLabelNames;


        private UILabel _populationLabel;
        private UIProgressBar[] _progressBarsMaxIntensity;
        private UIProgressBar[] _progressBarsProbability;
        private UILabel _realTimeDayTimeFramesLabel;
        private UILabel _realTimeDayTimeOffsetFramesLabel;
        private bool _realTimeStatus;
        private UILabel _realTimeStatusLabel;
        private UILabel _realTimeTimeOffsetTicksLabel;
        private UIPanel _selectedRadioPanel;
        private UIButton[] _statusButtons;

        private long _timeOffsetTicks;

        public override void Awake()
        {
            base.Awake();

            backgroundSprite = "MenuPanel";
            height = 320;
            width = 434;
            canFocus = true;
            isVisible = false;
        }

        public override void Start()
        {
            base.Start();

            BuildInformationBar();
            BuildTabContainer();
        }

        public override void Update()
        {
            base.Update();

            if (!isVisible) return;

            if (--counter > 0) return;
            counter = 10;

            DefineMinPopulationLabelContent();

            var disasterCount = _disasterHandler.container.AllDisasters.Count;

            for (var i = 0; i < disasterCount; i++)
            {
                var disaster = _disasterHandler.container.AllDisasters[i];
                var currentOccurrencePerYear = disaster.GetCurrentOccurrencePerYear();
                var maxIntensityCalculated = disaster.GetMaximumIntensity();

                _statusButtons[i].isVisible = true;

                if (disaster.Enabled)
                {
                    var button = _statusButtons[i];
                    var icon = button.components.OfType<UISprite>().FirstOrDefault();

                    if (icon)
                        icon.spriteName = pauseSprite;

                    _disasterLabelNames[i].text = disaster.GetName();
                    _disasterLabelCalculations[i].text = SetDisasterInfoLabel(currentOccurrencePerYear,
                        maxIntensityCalculated);

                    //Calculate probability                    
                    var probabilityValue = GetProbabilityProgressValue(currentOccurrencePerYear);

                    _progressBarsProbability[i].value = probabilityValue;
                    SetProgressBarColor(_progressBarsProbability[i]);
                    _progressBarsProbability[i].tooltip = disaster.GetProbabilityTooltip(probabilityValue);

                    //Calculate intensity
                    const float maxIntensity = 255f;
                    var progressBarCalculatedValue = maxIntensityCalculated * (1 / maxIntensity);

                    _progressBarsMaxIntensity[i].value = progressBarCalculatedValue;
                    SetProgressBarColor(_progressBarsMaxIntensity[i]);
                    _progressBarsMaxIntensity[i].tooltip =
                        disaster.GetIntensityTooltip(_progressBarsMaxIntensity[i].value);
                }
                else
                {
                    var button = _statusButtons[i];
                    var icon = button.components.OfType<UISprite>().FirstOrDefault();

                    if (icon)
                        icon.spriteName = playSprite;

                    _disasterLabelNames[i].text = $"{disaster.GetName()} - Disabled";
                    _disasterLabelCalculations[i].text = string.Empty;

                    _progressBarsProbability[i].value = 0;
                    _progressBarsProbability[i].progressColor = Color.black;
                    _progressBarsMaxIntensity[i].value = 0;
                    _progressBarsMaxIntensity[i].progressColor = Color.black;
                }
            }

            GetRealTimeModValues();

            _realTimeStatusLabel.text = $"Real Time Status: {(_realTimeStatus ? "Active" : "Inactive")}";
            _realTimeTimeOffsetTicksLabel.text = $"Time Offset Ticks: {_timeOffsetTicks}";
            _realTimeDayTimeFramesLabel.text = $"Day-Time Frames: {_dayTimeframes}";
            _realTimeDayTimeOffsetFramesLabel.text = $"Day-Time Offset Frames: {_dayTimeOffsetFrames}";
        }

        private static UIButton CreateTab(UITabstrip tabStrip, string title, float xPosition)
        {
            var tab = tabStrip.AddTab(title);
            tab.width = 100;
            tab.height = 30;
            tab.normalBgSprite = "SubBarButtonBase";
            tab.hoveredBgSprite = "SubBarButtonBaseHovered";
            tab.pressedBgSprite = "SubBarButtonBasePressed";
            tab.focusedBgSprite = "SubBarButtonBaseFocused";
            tab.textColor = Color.white;
            tab.focusedTextColor = Color.yellow;
            tab.hoveredTextColor = Color.cyan;
            tab.pressedTextColor = Color.gray;
            tab.relativePosition = new Vector2(xPosition, 40);
            return tab;
        }

        private void BuildStatisticsInfoTabContent(UIScrollablePanel parentPanel)
        {
            const float itemSpacing = 22f;
            const float xPosition = 10f;
            var yPosition = 10f;

            _populationLabel = AddLabel(parentPanel, xPosition, yPosition, labelTextScaleNormal, "");

            DefineMinPopulationLabelContent();

            yPosition += 22;
            AddLabel(parentPanel, xPosition, yPosition, labelTextScaleSmall, "Disaster", "Disaster name");
            AddLabel(parentPanel, xPosition + 140f, yPosition, labelTextScaleSmall,
                "COxY/M.I", "Current Occurrence per Year / Max Intensity");

            //Add Axis titles
            AddLabel(parentPanel, xPosition + 212f, yPosition, labelTextScaleSmall, "Probability");
            AddLabel(parentPanel, xPosition + 312f, yPosition, labelTextScaleSmall, "Max intensity");

            //Add Axis Labels
            yPosition += 15;
            AddLabel(parentPanel, xPosition + 212, yPosition, labelTextScaleSmall, "0.1");
            AddLabel(parentPanel, xPosition + 252, yPosition, labelTextScaleSmall, "1");
            AddLabel(parentPanel, xPosition + 287, yPosition, labelTextScaleSmall, "10");
            AddLabel(parentPanel, xPosition + 312, yPosition, labelTextScaleSmall, "0.0");
            AddLabel(parentPanel, xPosition + 375, yPosition, labelTextScaleSmall, "25.5");

            var disasterCount = _disasterHandler.container.AllDisasters.Count;
            _disasterLabelNames = new UILabel[disasterCount];
            _disasterLabelCalculations = new UILabel[disasterCount];
            _statusButtons = new UIButton[disasterCount];
            _progressBarsProbability = new UIProgressBar[disasterCount];
            _progressBarsMaxIntensity = new UIProgressBar[disasterCount];

            //Add each statistic item to the Panel
            yPosition += 15;
            for (var i = 0; i < disasterCount; i++)
            {
                var disaster = _disasterHandler.container.AllDisasters[i];
                _statusButtons[i] =
                    BuildDisasterStatusButton(parentPanel, xPosition, yPosition, disaster.GetName(),
                        disaster.Enabled);

                _disasterLabelNames[i] = AddLabel(parentPanel, xPosition + 26, yPosition, labelTextScaleNormal,
                    disaster.GetName());
                _disasterLabelCalculations[i] = AddLabel(parentPanel, xPosition + 136, yPosition, labelTextScaleNormal,
                    SetDisasterInfoLabel(0, 0));

                _progressBarsProbability[i] = AddProgressBar(parentPanel, xPosition + 212, yPosition);
                _progressBarsMaxIntensity[i] = AddProgressBar(parentPanel, xPosition + 312, yPosition);
                yPosition += itemSpacing;
            }

            yPosition += 10;
            BuildStopDisasterButton(parentPanel, xPosition, yPosition);
        }

        private void DefineMinPopulationLabelContent()
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";

            var maxPopulationToTriggerDisaster = (int)_disasterHandler.container.MaxPopulationToTriggerHigherDisasters;

            var formatNumber = maxPopulationToTriggerDisaster.ToString("#,0", nfi);

            _populationLabel.text = $"Min. Population: {formatNumber} Cims to trigger disasters.";
            _populationLabel.tooltip = "Minimum population to trigger higher disasters";
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

            _realTimeStatusLabel = AddLabel(parentPanel, xPosition, yPosition, labelTextScaleNormal,
                $"Real Time Status: {(_realTimeStatus ? "Active" : "Inactive")}");
            _realTimeStatusLabel.tooltip = "Check if \"Real Time\" Mod status is active";

            yPosition += 20;
            _realTimeTimeOffsetTicksLabel = AddLabel(parentPanel, xPosition, yPosition, labelTextScaleNormal,
                $"Time Offset Ticks: {_timeOffsetTicks}");

            yPosition += 20;
            _realTimeDayTimeFramesLabel = AddLabel(parentPanel, xPosition, yPosition, labelTextScaleNormal,
                $"Day-Time Frames: {_dayTimeframes}");

            yPosition += 20;
            _realTimeDayTimeOffsetFramesLabel = AddLabel(parentPanel, xPosition, yPosition, labelTextScaleNormal,
                $"Day-Time Offset Frames: {_dayTimeOffsetFrames}");

            yPosition += 30;

            var radioGroup = parentPanel.AddUIComponent<UIPanel>();
            radioGroup.relativePosition = new Vector3(20, yPosition);
            // radioGroup.relativePosition = new Vector3(20, 240); // Use for scroll functionality checking prurpose 
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
            BuildPanelTitle();
            BuildPanelCloseButton(this);
        }

        private void BuildPanelCloseButton(UIPanel parentPanel)
        {
            var closeBtn = parentPanel.AddUIComponent<UIButton>();
            closeBtn.relativePosition = new Vector3(407, 0);
            closeBtn.size = new Vector2(30, 30);
            closeBtn.normalFgSprite = "buttonclose";
            closeBtn.eventClick += ClosePanelBtn_eventClick;
        }

        private void BuildPanelTitle()
        {
            var lTitle = AddUIComponent<UILabel>();
            lTitle.relativePosition = new Vector3(10, 15);
            lTitle.text = "Disasters info";
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

            var tab1 = CreateTab(tabStrip, "Statistics", 0);
            CreateTab(tabStrip, "Settings", tab1.width);

            if (tabContainer.components.Count < 2) return;
            const float yPosition = 10f;

            // Tab 1: Statistics
            var statisticsTabPanel = tabContainer.components[0] as UIPanel;
            var statisticsScrollablePanel = ConfigureScrollablePanelWithScrollbar(
                statisticsTabPanel,
                yPosition
            );

            BuildStatisticsInfoTabContent(statisticsScrollablePanel);
            if (statisticsTabPanel != null) statisticsTabPanel.isVisible = true;

            // Tab 2: Settings
            var settingsTabPanel = tabContainer.components[1] as UIPanel;
            var settingsScrollablePanel = ConfigureScrollablePanelWithScrollbar(
                settingsTabPanel,
                yPosition
            );

            BuildSettingsTabContent(settingsScrollablePanel);
            if (settingsTabPanel != null) settingsTabPanel.isVisible = false;
        }

        private static UIScrollablePanel ConfigureScrollablePanelWithScrollbar(
            UIPanel tabPanel,
            float yPosition
        )
        {
            var scrollablePanel = tabPanel.AddUIComponent<UIScrollablePanel>();
            var scrollbar = tabPanel.AddUIComponent<UIScrollbar>();
            var track = scrollbar.AddUIComponent<UISlicedSprite>();
            var thumb = track.AddUIComponent<UISlicedSprite>();

            scrollablePanel.size = new Vector2(tabPanel.width - 12f, tabPanel.height - 20f);
            scrollablePanel.relativePosition = new Vector2(10f, yPosition);
            scrollablePanel.height = tabPanel.height - 40f;
            scrollablePanel.autoLayout = false;
            scrollablePanel.clipChildren = true;
            scrollablePanel.scrollWheelAmount = 20;


            scrollbar.orientation = UIOrientation.Vertical;
            scrollbar.width = 12f;
            scrollbar.relativePosition = new Vector2(tabPanel.width - 12f, yPosition);
            scrollbar.height = tabPanel.height - 20f;

            track.spriteName = "ScrollbarTrack";
            track.size = new Vector2(12f, tabPanel.height - 20f);
            track.relativePosition = new Vector2(0, -10);
            scrollbar.trackObject = track;

            thumb.spriteName = "ScrollbarThumb";
            thumb.height = 10f;
            scrollbar.thumbObject = thumb;

            scrollablePanel.verticalScrollbar = scrollbar;

            scrollablePanel.eventMouseWheel += (component, eventParam) =>
            {
                scrollablePanel.scrollPosition +=
                    new Vector2(0, -eventParam.wheelDelta * scrollablePanel.scrollWheelAmount);
            };

            return scrollablePanel;
        }

        private UIButton BuildDisasterStatusButton(UIComponent parentPanel, float xPosition, float yPosition,
            string disasterName,
            bool isDisasterEnabled)
        {
            var disasterStateBtn = parentPanel.AddUIComponent<UIButton>();
            disasterStateBtn.name = $"disasterState{disasterName}Btn";
            disasterStateBtn.relativePosition = new Vector3(xPosition, yPosition - 4f);
            disasterStateBtn.size = new Vector2(18, 18);
            disasterStateBtn.normalBgSprite = "ButtonMenu";
            disasterStateBtn.hoveredBgSprite = "ButtonMenuHovered";

            var icon = disasterStateBtn.AddUIComponent<UISprite>();
            icon.spriteName = isDisasterEnabled ? pauseSprite : playSprite;
            icon.size = new Vector2(12, 12);
            icon.relativePosition = new Vector2(3, 3);

            disasterStateBtn.eventClick += disasterStateChk_eventCheckChanged;

            return disasterStateBtn;
        }

        private void BuildStopDisasterButton(UIComponent parentPanel, float xPosition, float yPosition)
        {
            var stopAllDisastersBtn = parentPanel.AddUIComponent<UIButton>();
            stopAllDisastersBtn.name = "stopDisasterBtn";
            stopAllDisastersBtn.relativePosition = new Vector3(xPosition, yPosition - 5);
            stopAllDisastersBtn.size = new Vector2(18, 18);
            stopAllDisastersBtn.focusedColor = Color.red;
            stopAllDisastersBtn.textColor = Color.red;
            stopAllDisastersBtn.focusedTextColor = Color.red;
            stopAllDisastersBtn.text = "■";
            stopAllDisastersBtn.normalBgSprite = "ButtonMenu";
            stopAllDisastersBtn.hoveredBgSprite = "ButtonMenuHovered";
            stopAllDisastersBtn.eventClick += StopAllDisastersBtn_eventClick;

            var stopAllDisastersLabel = parentPanel.AddUIComponent<UILabel>();
            stopAllDisastersLabel.name = "bigRedBtnLabel";
            stopAllDisastersLabel.relativePosition = new Vector3(xPosition + 25, yPosition - 5);
            stopAllDisastersLabel.size = new Vector2(width - 30, 20);
            stopAllDisastersLabel.textColor = Color.white;
            stopAllDisastersLabel.text = "← Stop all disasters";
        }

        private static void StopAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            var cancellingDisasterFlags = new StringBuilder();

            var vehicleManager = Singleton<VehicleManager>.instance;
            for (var i = 1; i < 16384; i++)
                if ((vehicleManager.m_vehicles.m_buffer[i].m_flags & Vehicle.Flags.Created) != 0)
                {
                    if (vehicleManager.m_vehicles.m_buffer[i].Info.m_vehicleAI is MeteorAI)
                        vehicleManager.ReleaseVehicle((ushort)i);

                    if (vehicleManager.m_vehicles.m_buffer[i].Info.m_vehicleAI is VortexAI)
                        vehicleManager.ReleaseVehicle((ushort)i);
                }

            var ws = Singleton<WaterSimulation>.instance;
            for (var i = ws.m_waterWaves.m_size; i >= 1; i--)
                Singleton<TerrainManager>.instance.WaterSimulation.ReleaseWaterWave((ushort)i);

            var dm = Singleton<DisasterManager>.instance;
            for (ushort i = 0; i < dm.m_disasterCount; i++)
            {
                cancellingDisasterFlags.AppendLine(dm.m_disasters.m_buffer[i].Info.name + " flags: " +
                                                   dm.m_disasters.m_buffer[i].m_flags);
                if ((dm.m_disasters.m_buffer[i].m_flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active |
                                                           DisasterData.Flags.Clearing)) ==
                    DisasterData.Flags.None) continue;
                if (!IsStoppableDisaster(dm.m_disasters.m_buffer[i].Info.m_disasterAI)) continue;
                cancellingDisasterFlags.AppendLine("Trying to cancel " + dm.m_disasters.m_buffer[i].Info.name);
                dm.m_disasters.m_buffer[i].m_flags =
                    (dm.m_disasters.m_buffer[i].m_flags & ~(DisasterData.Flags.Emerging |
                                                            DisasterData.Flags.Active |
                                                            DisasterData.Flags.Clearing)) |
                    DisasterData.Flags.Finished;
            }

            Debug.Log(cancellingDisasterFlags.ToString());
        }

        private void ClosePanelBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }

        private void disasterStateChk_eventCheckChanged(UIComponent component, UIMouseEventParameter eventParam)
        {
            var disaster = _disasterHandler.container.AllDisasters
                .FirstOrDefault(dis => component.name.Contains(dis.GetName()));

            if (disaster == null) return;

            disaster.Enabled = !disaster.Enabled;
            switch (disaster.GetName())
            {
                case CommonProperties.EarthquakeName:
                    _disasterHandler.container.Earthquake.Enabled = disaster.Enabled;
                    break;

                case CommonProperties.forestFireName:
                    _disasterHandler.container.ForestFire.Enabled = disaster.Enabled;
                    break;

                case CommonProperties.meteorStrikeName:
                    _disasterHandler.container.MeteorStrike.Enabled = disaster.Enabled;
                    break;

                case CommonProperties.sinkholeName:
                    _disasterHandler.container.Sinkhole.Enabled = disaster.Enabled;
                    break;

                case CommonProperties.thunderstormName:
                    _disasterHandler.container.Thunderstorm.Enabled = disaster.Enabled;
                    break;

                case CommonProperties.tornadoName:
                    _disasterHandler.container.Tornado.Enabled = disaster.Enabled;
                    break;

                case CommonProperties.tsunamiName:
                    _disasterHandler.container.Tsunami.Enabled = disaster.Enabled;
                    break;
            }

            var button = (UIButton)component;
            var icon = button.components.OfType<UISprite>().FirstOrDefault();
            if (icon != null) icon.spriteName = disaster.Enabled ? pauseSprite : playSprite;

            SettingsScreen.UpdateUISettingsOptions();
        }

        private static string SetDisasterInfoLabel(float currentOccurrencePerYear, float maxIntensity)
        {
            return $"| {currentOccurrencePerYear:00.00}/{maxIntensity / 10:0.0}";
        }

        private static bool IsStoppableDisaster(DisasterAI ai)
        {
            return ai as ThunderStormAI != null || ai as SinkholeAI != null || ai as TornadoAI != null ||
                   ai as EarthquakeAI != null || ai as MeteorStrikeAI != null;
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

        private static UIProgressBar AddProgressBar(UIComponent parentPanel, float x, float y)
        {
            var progressBar = parentPanel.AddUIComponent<UIProgressBar>();
            progressBar.backgroundSprite = "LevelBarBackground";
            progressBar.progressSprite = "LevelBarForeground";
            progressBar.progressColor = Color.red;
            progressBar.relativePosition = new Vector3(x, y);
            progressBar.width = 90;
            progressBar.value = 0.5f;
            return progressBar;
        }

        private static float GetProbabilityProgressValue(float currentOccurrencePerYear)
        {
            if (currentOccurrencePerYear <= 0.1)
                return 0;
            if (currentOccurrencePerYear >= 10)
                return 1;

            //based on calculation, accelerate if realtime mod is active, almost 6 times faster
            return (1f + Mathf.Log10(currentOccurrencePerYear)) / 2f;
        }

        private static void SetProgressBarColor(UIProgressBar progressBar)
        {
            var value = progressBar.value;
            progressBar.progressColor = new Color(2.0f * value, 2.0f * (1 - value), 0);
        }
    }
}