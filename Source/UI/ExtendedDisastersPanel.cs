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
        private readonly string _pauseSprite = "ButtonPause";
        private readonly string _playSprite = "ButtonPlayFocused";
        private readonly NaturalDisasterHandler disasterHandler = Singleton<NaturalDisasterHandler>.instance;
        private UILabel[] _disasterLabelCalculations;
        private UILabel[] _disasterLabelNames;
        private UILabel _occurrenceAndMaxProb;
        private UIProgressBar[] _progressBarsMaxIntensity;
        private UIProgressBar[] _progressBarsProbability;
        private UILabel _realTimeStatusLabel;
        private UIButton[] _statusButtons;

        public override void Awake()
        {
            base.Awake();

            backgroundSprite = "MenuPanel";
            canFocus = true;

            height = 320;
            width = 420;

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

            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";
            var formatNumber =
                ((int)disasterHandler.container.MaxPopulationToTriggerHigherDisasters).ToString("#,0", nfi);
            // pupulationLabel.text = $"Min. Population: {formatNumber} Cims to trigger disasters.";
            // pupulationLabel.textScale = 0.7f;

            var disasterCount = disasterHandler.container.AllDisasters.Count;

            for (var i = 0; i < disasterCount; i++)
            {
                var disaster = disasterHandler.container.AllDisasters[i];
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
                    var maxIntensity = 255f;
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

        private void BuildStatisticsInfoTabContent(UIPanel parentPanel)
        {
            const int itemSpacing = 22;
            const int xBorderPadding = 10;

            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            var yPosition = -10;
            var formatNumber =
                ((int)disasterHandler.container.MaxPopulationToTriggerHigherDisasters).ToString("#,0", nfi);

            nfi.NumberGroupSeparator = ",";

            var populationLabel = AddLabel(parentPanel, xBorderPadding + 18, yPosition);
            populationLabel.text = $"Min. Population: {formatNumber} Cims to trigger disasters.";
            populationLabel.tooltip = "Minimum population to trigger higher disasters";
            yPosition -= 22;

            _occurrenceAndMaxProb = AddLabel(parentPanel, xBorderPadding, yPosition);
            _occurrenceAndMaxProb.text = "Disaster";
            _occurrenceAndMaxProb.tooltip = "Disaster name";
            _occurrenceAndMaxProb.textScale = 0.7f;

            _occurrenceAndMaxProb = AddLabel(parentPanel, 152, yPosition);
            _occurrenceAndMaxProb.text = "COxY/M.I";
            _occurrenceAndMaxProb.tooltip = "Current Occurrence per Year / Max Intensity";
            _occurrenceAndMaxProb.textScale = 0.7f;

            //Add Axis titles
            AddAxisTitle(parentPanel, 220, yPosition, "Probability");
            AddAxisTitle(parentPanel, 320, yPosition, "Max intensity");
            yPosition -= 15;

            //Add Axis Labels
            AddAxisLabel(parentPanel, 220, yPosition, "0.1");
            AddAxisLabel(parentPanel, 260, yPosition, "1");
            AddAxisLabel(parentPanel, 295, yPosition, "10");
            AddAxisLabel(parentPanel, 320, yPosition, "0.0");
            AddAxisLabel(parentPanel, 383, yPosition, "25.5");
            yPosition -= 15;

            var disasterCount = disasterHandler.container.AllDisasters.Count;
            _disasterLabelNames = new UILabel[disasterCount];
            _disasterLabelCalculations = new UILabel[disasterCount];
            _statusButtons = new UIButton[disasterCount];
            _progressBarsProbability = new UIProgressBar[disasterCount];
            _progressBarsMaxIntensity = new UIProgressBar[disasterCount];

            //Add each statistic item to the Panel
            for (var i = 0; i < disasterCount; i++)
            {
                var disaster = disasterHandler.container.AllDisasters[i];
                _statusButtons[i] =
                    BuildDisasterStatusButton(parentPanel, xBorderPadding, yPosition, disaster.GetName(),
                        disaster.Enabled);

                _disasterLabelNames[i] = AddLabel(parentPanel, 34, yPosition);
                _disasterLabelCalculations[i] = AddLabel(parentPanel, 144, yPosition);

                _disasterLabelNames[i].text = disaster.GetName();
                _disasterLabelCalculations[i].text = SetDisasterInfoLabel(0, 0);

                _progressBarsProbability[i] = AddProgressBar(parentPanel, 220, yPosition);
                _progressBarsMaxIntensity[i] = AddProgressBar(parentPanel, 320, yPosition);
                yPosition -= itemSpacing;
            }

            yPosition -= 10;
            BuildStopDisasterButton(parentPanel, yPosition);
        }

        private static UICheckBox CreateRadioButton(UIPanel parent, string text, Vector3 position, bool isChecked)
        {
            var checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.relativePosition = position;
            checkBox.size = new Vector2(200, 20);

            // Configure unchecked state
            var uncheckedSprite = checkBox.AddUIComponent<UISprite>();
            uncheckedSprite.spriteName = "check-unchecked";
            uncheckedSprite.size = new Vector2(16, 16);
            uncheckedSprite.relativePosition = new Vector3(0, 2);
            checkBox.checkedBoxObject = uncheckedSprite;

            // Configure checked state
            var checkedSprite = checkBox.AddUIComponent<UISprite>();
            checkedSprite.spriteName = "check-checked";
            checkedSprite.size = new Vector2(16, 16);
            checkedSprite.relativePosition = new Vector3(0, 2);
            checkBox.checkedBoxObject = checkedSprite;

            // Configure disabled state
            var disabledSprite = checkBox.AddUIComponent<UISprite>();
            disabledSprite.spriteName = "check-disabled";
            disabledSprite.size = new Vector2(16, 16);
            disabledSprite.relativePosition = new Vector3(0, 2);

            // Configure hovered state
            var hoveredSprite = checkBox.AddUIComponent<UISprite>();
            hoveredSprite.spriteName = "check-unchecked-hover";
            hoveredSprite.size = new Vector2(16, 16);
            hoveredSprite.relativePosition = new Vector3(0, 2);

            // Set initial state
            checkBox.isChecked = isChecked;

            // Add label for the checkbox
            var label = checkBox.AddUIComponent<UILabel>();
            label.text = text;
            label.relativePosition = new Vector3(22, 0);

            return checkBox;
        }

        private void OnTakeItEasySelected()
        {
            // Lógica para "Take it easy"
        }

        private void OnExtraChaosSelected()
        {
            // Lógica para "I need some extra caos"
        }

        private void BuildSettingsTabContent(UIPanel parentPanel)
        {
            var realTimeStatus = disasterHandler.CheckRealTimeModActive();

            var sm = SimulationManager.instance;
            var timeOffsetTicks = sm.m_timeOffsetTicks;
            var dayTimeframes = sm.m_dayTimeFrame;
            var dayTimeOffsetFrames = sm.m_dayTimeOffsetFrames;

            const int xPosition = 18;
            var yPosition = -10;

            _realTimeStatusLabel = AddLabel(parentPanel, xPosition, yPosition);
            _realTimeStatusLabel.text = $"Real Time Status: {(realTimeStatus ? "Active" : "Inactive")}";
            _realTimeStatusLabel.tooltip = "Check if \"Real Time\" Mod status is active";

            yPosition -= 20;
            _realTimeStatusLabel = AddLabel(parentPanel, xPosition, yPosition);
            _realTimeStatusLabel.text = $"Time Offset Ticks: {timeOffsetTicks}";

            yPosition -= 20;
            _realTimeStatusLabel = AddLabel(parentPanel, xPosition, yPosition);
            _realTimeStatusLabel.text = $"Day-Time Frames: {dayTimeframes}";

            yPosition -= 20;
            _realTimeStatusLabel = AddLabel(parentPanel, xPosition, yPosition);
            _realTimeStatusLabel.text = $"Day-Time Offset Frames: {dayTimeOffsetFrames}";

            yPosition -= 30;

            var radioGroup = parentPanel.AddUIComponent<UIPanel>();
            radioGroup.position = new Vector3(xPosition, yPosition);
            radioGroup.size = new Vector2(350, 60);
            radioGroup.backgroundSprite = "SubcategoriesPanel";
            radioGroup.name = "radioGroup";
            radioGroup.isVisible = true;

            var radioEasy = CreateRadioButton(radioGroup, "Take it easy", new Vector3(10, 10), true);
            var radioChaos = CreateRadioButton(radioGroup, "I need some extra chaos", new Vector3(10, 35), false);

            radioEasy.eventCheckChanged += (c, state) =>
            {
                if (state) radioChaos.isChecked = false;
                if (state) OnTakeItEasySelected();
            };
            radioChaos.eventCheckChanged += (c, state) =>
            {
                if (state) radioEasy.isChecked = false;
                if (state) OnExtraChaosSelected();
            };
        }

        private void BuildInformationBar()
        {
            BuildPanelTitle();
            BuildPanelCloseButton(this);
        }

        private void BuildPanelCloseButton(UIPanel parentPanel)
        {
            var closeBtn = parentPanel.AddUIComponent<UIButton>();
            closeBtn.position = new Vector3(385, -5);
            closeBtn.size = new Vector2(30, 30);
            closeBtn.normalFgSprite = "buttonclose";
            closeBtn.eventClick += ClosePanelBtn_eventClick;
        }

        private void BuildPanelTitle()
        {
            var lTitle = AddUIComponent<UILabel>();
            lTitle.position = new Vector3(10, -15);
            lTitle.text = "Disasters info";
        }

        private void BuildTabContainer()
        {
            //Create Tab Container
            var tabContainer = AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, 70);
            tabContainer.size = new Vector2(width, height - 50);

            //Create tab strip
            var tabStrip = AddUIComponent<UITabstrip>();
            tabStrip.relativePosition = new Vector3(0, 40);
            tabStrip.size = new Vector2(width, 30);
            tabStrip.backgroundSprite = "SubcategoriesPanel";
            tabStrip.tabPages = tabContainer;

            var tab1 = CreateTab(tabStrip, "Statistics", 0);
            var tab2 = CreateTab(tabStrip, "Settings", tab1.width);

            if (tabContainer.components.Count >= 2)
            {
                var tab1Panel = tabContainer.components[0] as UIPanel;
                BuildStatisticsInfoTabContent(tab1Panel);
                tab1Panel.isVisible = true;

                var tab2Panel = tabContainer.components[1] as UIPanel;
                BuildSettingsTabContent(tab2Panel);
                tab2Panel.isVisible = false;
            }
        }

        private UIButton BuildDisasterStatusButton(UIPanel parentPanel, int x, int y, string disasterName,
            bool isDisasterEnabled)
        {
            var disasterStateBtn = parentPanel.AddUIComponent<UIButton>();
            disasterStateBtn.name = $"disasterState{disasterName}Btn";
            disasterStateBtn.position = new Vector3(x, y + 4);
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

        private void BuildStopDisasterButton(UIPanel parentPanel, int yPosition)
        {
            var stopAllDisastersBtn = parentPanel.AddUIComponent<UIButton>();
            stopAllDisastersBtn.name = "stopDisasterBtn";
            stopAllDisastersBtn.position = new Vector3(10, yPosition + 3);
            stopAllDisastersBtn.size = new Vector2(18, 18);
            stopAllDisastersBtn.focusedColor = Color.red;
            stopAllDisastersBtn.textColor = Color.red;
            stopAllDisastersBtn.textColor = Color.red;
            stopAllDisastersBtn.focusedTextColor = Color.red;
            stopAllDisastersBtn.text = "■";
            stopAllDisastersBtn.normalBgSprite = "ButtonMenu";
            stopAllDisastersBtn.hoveredBgSprite = "ButtonMenuHovered";
            stopAllDisastersBtn.eventClick += StopAllDisastersBtn_eventClick;

            var stopAllDisastersLabel = parentPanel.AddUIComponent<UILabel>();
            stopAllDisastersLabel.name = "bigRedBtnLabel";
            stopAllDisastersLabel.position = new Vector3(40, yPosition);
            stopAllDisastersLabel.size = new Vector2(width - 30, 20);
            stopAllDisastersLabel.textColor = Color.white;
            stopAllDisastersLabel.text = "← Stop all disasters";
        }

        private static void StopAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            var sb = new StringBuilder();

            var vm = Singleton<VehicleManager>.instance;
            for (var i = 1; i < 16384; i++)
                if ((vm.m_vehicles.m_buffer[i].m_flags & Vehicle.Flags.Created) != 0)
                {
                    if (vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is MeteorAI) vm.ReleaseVehicle((ushort)i);

                    if (vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is VortexAI) vm.ReleaseVehicle((ushort)i);
                }

            var ws = Singleton<WaterSimulation>.instance;
            for (var i = ws.m_waterWaves.m_size; i >= 1; i--)
                Singleton<TerrainManager>.instance.WaterSimulation.ReleaseWaterWave((ushort)i);

            var dm = Singleton<DisasterManager>.instance;
            for (ushort i = 0; i < dm.m_disasterCount; i++)
            {
                sb.AppendLine(dm.m_disasters.m_buffer[i].Info.name + " flags: " + dm.m_disasters.m_buffer[i].m_flags);
                if ((dm.m_disasters.m_buffer[i].m_flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active |
                                                           DisasterData.Flags.Clearing)) != DisasterData.Flags.None)
                    if (IsStoppableDisaster(dm.m_disasters.m_buffer[i].Info.m_disasterAI))
                    {
                        sb.AppendLine("Trying to cancel " + dm.m_disasters.m_buffer[i].Info.name);
                        dm.m_disasters.m_buffer[i].m_flags =
                            (dm.m_disasters.m_buffer[i].m_flags & ~(DisasterData.Flags.Emerging |
                                                                    DisasterData.Flags.Active |
                                                                    DisasterData.Flags.Clearing)) |
                            DisasterData.Flags.Finished;
                    }
            }

            Debug.Log(sb.ToString());
        }

        private void ClosePanelBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }

        private void disasterStateChk_eventCheckChanged(UIComponent component, UIMouseEventParameter eventParam)
        {
            var disaster = disasterHandler.container.AllDisasters.Where(dis => component.name.Contains(dis.GetName()))
                .FirstOrDefault();

            if (disaster != null)
            {
                disaster.Enabled = !disaster.Enabled;
                switch (disaster.GetName())
                {
                    case CommonProperties.EarthquakeName:
                        disasterHandler.container.Earthquake.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.forestFireName:
                        disasterHandler.container.ForestFire.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.meteorStrikeName:
                        disasterHandler.container.MeteorStrike.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.sinkholeName:
                        disasterHandler.container.Sinkhole.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.thunderstormName:
                        disasterHandler.container.Thunderstorm.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.tornadoName:
                        disasterHandler.container.Tornado.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.tsunamiName:
                        disasterHandler.container.Tsunami.Enabled = disaster.Enabled;
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

        private static UILabel AddLabel(UIPanel parentPanel, int x, int y)
        {
            var label = parentPanel.AddUIComponent<UILabel>();
            label.position = new Vector3(x, y);
            label.textScale = 0.8f;

            return label;
        }

        private static void AddAxisLabel(UIPanel parentPanel, int x, int y, string text)
        {
            var l = parentPanel.AddUIComponent<UILabel>();
            l.position = new Vector3(x, y);
            l.textScale = 0.7f;
            l.text = text;
        }

        private static void AddAxisTitle(UIPanel parentPanel, int x, int y, string text)
        {
            var l = parentPanel.AddUIComponent<UILabel>();
            l.position = new Vector3(x, y);
            l.textScale = 0.7f;
            l.text = text;
        }

        private static UIProgressBar AddProgressBar(UIPanel parentPanel, int x, int y)
        {
            var progressBar = parentPanel.AddUIComponent<UIProgressBar>();
            progressBar.backgroundSprite = "LevelBarBackground";
            progressBar.progressSprite = "LevelBarForeground";
            progressBar.progressColor = Color.red;
            progressBar.position = new Vector3(x, y);
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