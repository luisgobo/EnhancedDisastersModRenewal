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
        [FormerlySerializedAs("Counter")] public int counter;
        private readonly NaturalDisasterHandler _disasterHandler = Singleton<NaturalDisasterHandler>.instance;
        private readonly string _pauseSprite = "ButtonPause";

        private readonly string _playSprite = "ButtonPlayFocused";

        private UILabel[] _disasterLabelCalculations;

        private UILabel[] _disasterLabelNames;
        private UILabel _occurrenceAndMaxProb;
        private UIProgressBar[] _progressBarsMaxIntensity;

        private UIProgressBar[] _progressBarsProbability;

        // private UICheckBox _radioEasy, _radioChaos;
        // private UIPanel _radioEasyPanel, _radioChaosPanel;
        // private UICheckBox _selectedRadioButton;
        // private UISprite _spriteEasy, _spriteChaos;
        private UILabel _realTimeStatusLabel;
        private UIPanel _selectedRadioPanel;
        private UIButton[] _statusButtons;

        public override void Awake()
        {
            base.Awake();

            backgroundSprite = "MenuPanel";
            height = 320;
            width = 442;
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
                        icon.spriteName = _pauseSprite;

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
                        icon.spriteName = _playSprite;

                    _disasterLabelNames[i].text = $"{disaster.GetName()} - Disabled";
                    _disasterLabelCalculations[i].text = string.Empty;

                    _progressBarsProbability[i].value = 0;
                    _progressBarsProbability[i].progressColor = Color.black;
                    _progressBarsMaxIntensity[i].value = 0;
                    _progressBarsMaxIntensity[i].progressColor = Color.black;
                }
            }
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

            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";

            var formatNumber =
                ((int)_disasterHandler.container.MaxPopulationToTriggerHigherDisasters).ToString("#,0", nfi);

            var populationLabel = AddLabel(parentPanel, xPosition, yPosition);
            populationLabel.text = $"Min. Population: {formatNumber} Cims to trigger disasters.";
            populationLabel.tooltip = "Minimum population to trigger higher disasters";

            yPosition += 22;
            _occurrenceAndMaxProb = AddLabel(parentPanel, xPosition, yPosition);
            _occurrenceAndMaxProb.text = "Disaster";
            _occurrenceAndMaxProb.tooltip = "Disaster name";
            _occurrenceAndMaxProb.textScale = 0.7f;

            _occurrenceAndMaxProb = AddLabel(parentPanel, xPosition + 152f, yPosition);
            _occurrenceAndMaxProb.text = "COxY/M.I";
            _occurrenceAndMaxProb.tooltip = "Current Occurrence per Year / Max Intensity";
            _occurrenceAndMaxProb.textScale = 0.7f;

            //Add Axis titles
            AddAxisTitle(parentPanel, xPosition + 220f, yPosition, "Probability");
            AddAxisTitle(parentPanel, xPosition + 320f, yPosition, "Max intensity");

            //Add Axis Labels
            yPosition += 15;
            AddAxisLabel(parentPanel, xPosition + 220, yPosition, "0.1");
            AddAxisLabel(parentPanel, xPosition + 260, yPosition, "1");
            AddAxisLabel(parentPanel, xPosition + 295, yPosition, "10");
            AddAxisLabel(parentPanel, xPosition + 320, yPosition, "0.0");
            AddAxisLabel(parentPanel, xPosition + 383, yPosition, "25.5");

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

                _disasterLabelNames[i] = AddLabel(parentPanel, xPosition + 34, yPosition + 5);
                _disasterLabelCalculations[i] = AddLabel(parentPanel, xPosition + 144, yPosition);

                _disasterLabelNames[i].text = disaster.GetName();
                _disasterLabelCalculations[i].text = SetDisasterInfoLabel(0, 0);

                _progressBarsProbability[i] = AddProgressBar(parentPanel, xPosition + 220, yPosition);
                _progressBarsMaxIntensity[i] = AddProgressBar(parentPanel, xPosition + 320, yPosition);
                yPosition += itemSpacing;
            }

            yPosition += 10;
            BuildStopDisasterButton(parentPanel, xPosition, yPosition);
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
            var realTimeStatus = _disasterHandler.CheckRealTimeModActive();

            var sm = SimulationManager.instance;
            var timeOffsetTicks = sm.m_timeOffsetTicks;
            var dayTimeframes = sm.m_dayTimeFrame;
            var dayTimeOffsetFrames = sm.m_dayTimeOffsetFrames;

            const int xPosition = 20;
            var yPosition = 10;

            _realTimeStatusLabel = AddLabel(parentPanel, xPosition, yPosition);
            _realTimeStatusLabel.text = $"Real Time Status: {(realTimeStatus ? "Active" : "Inactive")}";
            _realTimeStatusLabel.tooltip = "Check if \"Real Time\" Mod status is active";

            yPosition += 20;
            _realTimeStatusLabel = AddLabel(parentPanel, xPosition, yPosition);
            _realTimeStatusLabel.text = $"Time Offset Ticks: {timeOffsetTicks}";

            yPosition += 20;
            _realTimeStatusLabel = AddLabel(parentPanel, xPosition, yPosition);
            _realTimeStatusLabel.text = $"Day-Time Frames: {dayTimeframes}";

            yPosition += 20;
            _realTimeStatusLabel = AddLabel(parentPanel, xPosition, yPosition);
            _realTimeStatusLabel.text = $"Day-Time Offset Frames: {dayTimeOffsetFrames}";

            yPosition += 30;

            var radioGroup = parentPanel.AddUIComponent<UIPanel>();
            radioGroup.relativePosition = new Vector3(20, yPosition);
            // radioGroup.relativePosition = new Vector3(20, 240); // Use for scroll functionality checking prurpose 
            radioGroup.size = new Vector2(350, 60);
            radioGroup.backgroundSprite = "SubcategoriesPanel";
            radioGroup.name = "radioGroup";
            radioGroup.isVisible = true;

            var radioEasyPanel = CreateRadioButton(radioGroup, "I need some extra chaos",
                10f, 10f, true);
            var radioChaosPanel = CreateRadioButton(radioGroup, "I need some extra chaos",
                10f, 35f);
            var spriteEasy = radioEasyPanel.components.OfType<UISprite>().FirstOrDefault();
            var spriteChaos = radioChaosPanel.components.OfType<UISprite>().FirstOrDefault();

            _selectedRadioPanel = radioEasyPanel;

            radioEasyPanel.eventClick += (c, p) =>
            {
                if (_selectedRadioPanel == radioEasyPanel) return;

                if (spriteEasy != null) spriteEasy.spriteName = "check-checked";
                if (spriteChaos != null) spriteChaos.spriteName = "check-unchecked";
                _selectedRadioPanel = radioEasyPanel;
                OnTakeItEasySelected();
            };
            radioChaosPanel.eventClick += (c, p) =>
            {
                if (_selectedRadioPanel == radioChaosPanel) return;

                if (spriteEasy != null) spriteEasy.spriteName = "check-unchecked";
                if (spriteChaos != null) spriteChaos.spriteName = "check-checked";
                _selectedRadioPanel = radioChaosPanel;
                OnExtraChaosSelected();
            };
        }

        private static UIPanel CreateRadioButton(UIPanel radioGroup, string text, float xPosition,
            float yPosition, bool isChecked = false)
        {
            var radioPanel = radioGroup.AddUIComponent<UIPanel>();
            radioPanel.size = new Vector2(200, 20);
            radioPanel.relativePosition = new Vector3(xPosition, yPosition);

            var uiSprite = radioPanel.AddUIComponent<UISprite>();
            uiSprite.spriteName = isChecked ? "check-checked" : "check-unchecked";
            uiSprite.size = new Vector2(16, 16);
            uiSprite.relativePosition = new Vector3(0, 2);

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
            closeBtn.relativePosition = new Vector3(407, 5);
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

        private UIButton BuildDisasterStatusButton(UIComponent parentPanel, float x, float y, string disasterName,
            bool isDisasterEnabled)
        {
            var disasterStateBtn = parentPanel.AddUIComponent<UIButton>();
            disasterStateBtn.name = $"disasterState{disasterName}Btn";
            disasterStateBtn.relativePosition = new Vector3(x, y + 4);
            disasterStateBtn.size = new Vector2(18, 18);
            disasterStateBtn.normalBgSprite = "ButtonMenu";
            disasterStateBtn.hoveredBgSprite = "ButtonMenuHovered";

            var icon = disasterStateBtn.AddUIComponent<UISprite>();
            icon.spriteName = isDisasterEnabled ? _pauseSprite : _playSprite;
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
            stopAllDisastersLabel.relativePosition = new Vector3(xPosition + 35, yPosition - 5);
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

            if (disaster != null)
            {
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
                if (icon != null) icon.spriteName = disaster.Enabled ? _pauseSprite : _playSprite;

                SettingsScreen.UpdateUISettingsOptions();
            }
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

        private static UILabel AddLabel(UIComponent parentPanel, float x, float y)
        {
            var label = parentPanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(x, y);
            label.textScale = 0.8f;
            return label;
        }

        private static void AddAxisLabel(UIComponent parentPanel, float x, float y, string text)
        {
            var l = parentPanel.AddUIComponent<UILabel>();
            l.relativePosition = new Vector3(x, y);
            l.textScale = 0.7f;
            l.text = text;
        }

        private static void AddAxisTitle(UIComponent parentPanel, float x, float y, string text)
        {
            var l = parentPanel.AddUIComponent<UILabel>();
            l.relativePosition = new Vector3(x, y);
            l.textScale = 0.7f;
            l.text = text;
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

        private float GetProbabilityProgressValue(float currentOccurrencePerYear)
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