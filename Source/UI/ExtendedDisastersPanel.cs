using System.Globalization;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using UnityEngine;
using UnityEngine.Serialization;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.UI
{
    public class ExtendedDisastersPanel : UIPanel
    {
        private const string PauseSprite = "ButtonPause";
        private const string PlaySprite = "ButtonPlayFocused";
        private const float LabelTextScaleTiny = 0.6f;
        private const float LabelTextScaleSmall = 0.7f;
        private const float LabelTextScaleNormal = 0.8f;

        private const float PanelWidth = 434f;
        private const float PanelHeight = 320f;

        [FormerlySerializedAs("Counter")] public int counter;
        private readonly NaturalDisasterHandler _disasterHandler = CommonServices.DisasterHandler;
        private uint _dayTimeframes;
        private uint _dayTimeOffsetFrames;

        private UILabel[] _disasterLabelCalculations;
        private UILabel[] _disasterLabelMaxIntensity;

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

            var disasterCount = _disasterHandler.container.AllDisasters.Count;

            // Simulation params for probability calculation
            var simulation = SimulationManager.instance;
            var dayDurationSeconds = simulation.m_timePerFrame.TotalSeconds * SimulationManager.DAYTIME_FRAMES;
            const double hoursInGameDay = 24.0;
            var secondsPerHour = dayDurationSeconds / hoursInGameDay;

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
                        icon.spriteName = PauseSprite;

                    _disasterLabelNames[i].text = disaster.GetName();
                    _disasterLabelCalculations[i].text = SetDisasterProbabilityLabel(currentOccurrencePerYear);
                    _disasterLabelMaxIntensity[i].text = SetMaxIntensityInfoLabel(maxIntensityCalculated);

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
                        icon.spriteName = PlaySprite;

                    _disasterLabelNames[i].text = $"{disaster.GetName()} - Disabled";
                    _disasterLabelCalculations[i].text = string.Empty;
                    _disasterLabelMaxIntensity[i].text = string.Empty;
                    
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

            _populationLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal, "");

            DefineMinPopulationLabelContent();

            yPosition += 22;
            AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleSmall, "Disaster", "Disaster name");

            //Add Axis titles
            AddLabel(parentPanel, xPosition + 212f, yPosition, LabelTextScaleSmall, "Probability %");
            AddLabel(parentPanel, xPosition + 312f, yPosition, LabelTextScaleSmall, "Max intensity");

            //Add Axis Labels
            yPosition += 15;
            AddLabel(parentPanel, xPosition + 210, yPosition, LabelTextScaleSmall, "1");
            AddLabel(parentPanel, xPosition + 246, yPosition, LabelTextScaleSmall, "50");
            AddLabel(parentPanel, xPosition + 281, yPosition, LabelTextScaleSmall, "100");
            AddLabel(parentPanel, xPosition + 312, yPosition, LabelTextScaleSmall, "0.0");
            AddLabel(parentPanel, xPosition + 375, yPosition, LabelTextScaleSmall, "25.5");

            var disasterCount = _disasterHandler.container.AllDisasters.Count;
            _disasterLabelNames = new UILabel[disasterCount];
            _disasterLabelCalculations = new UILabel[disasterCount];
            _disasterLabelMaxIntensity = new UILabel[disasterCount];
            _statusButtons = new UIButton[disasterCount];
            _progressBarsProbability = new UIProgressBar[disasterCount];
            _progressBarsMaxIntensity = new UIProgressBar[disasterCount];

            //Add each statistic item to the Panel
            yPosition += 15;
            for (var i = 0; i < disasterCount; i++)
            {
                //List of disasters
                var disaster = _disasterHandler.container.AllDisasters[i];

                // Create a button for each disaster to be enabled or disabled
                _statusButtons[i] =
                    BuildDisasterStatusButton(parentPanel, xPosition, yPosition, disaster.GetName(),
                        disaster.Enabled);

                //Show disaster name
                _disasterLabelNames[i] = AddLabel(parentPanel, xPosition + 26, yPosition, LabelTextScaleNormal,
                    disaster.GetName());
                
                //Set progress bar for probability
                _progressBarsProbability[i] = AddProgressBar(parentPanel, xPosition + 212, yPosition);

                //Show disaster probability info -> the order defines the index position
                _disasterLabelCalculations[i] = AddLabel(parentPanel, xPosition + 240, yPosition + 3, LabelTextScaleTiny,
                    SetDisasterProbabilityLabel());

                //Set progress bar for max intensity
                _progressBarsMaxIntensity[i] = AddProgressBar(parentPanel, xPosition + 312, yPosition);

                //Show disaster probability info -> the order defines the index position
                _disasterLabelMaxIntensity[i] = AddLabel(parentPanel, xPosition + 350, yPosition + 3, LabelTextScaleTiny,
                    SetMaxIntensityInfoLabel());

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

            _populationLabel.text = $"Min. Population: {formatNumber} Cims to trigger strongest disasters.";
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

            _realTimeStatusLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal,
                $"Real Time Status: {(_realTimeStatus ? "Active" : "Inactive")}");
            _realTimeStatusLabel.tooltip = "Check if \"Real Time\" Mod status is active";

            yPosition += 20;
            _realTimeTimeOffsetTicksLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal,
                $"Time Offset Ticks: {_timeOffsetTicks}");

            yPosition += 20;
            _realTimeDayTimeFramesLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal,
                $"Day-Time Frames: {_dayTimeframes}");

            yPosition += 20;
            _realTimeDayTimeOffsetFramesLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal,
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
            BuildPanelButtons(this);
        }

        private void BuildPanelButtons(UIPanel parentPanel)
        {
            //Help button
            var helpBtn = parentPanel.AddUIComponent<UIButton>();
            helpBtn.relativePosition = new Vector3(PanelWidth - 70f, 8f);
            helpBtn.size = new Vector2(25, 25);
            helpBtn.normalBgSprite = "OptionBase";
            helpBtn.hoveredBgSprite = "OptionBaseHovered";
            helpBtn.pressedBgSprite = "OptionBasePressed";
            helpBtn.focusedColor = Color.white;
            helpBtn.textColor = Color.white;
            helpBtn.focusedTextColor = Color.black;
            helpBtn.text = "?";
            helpBtn.textPadding = new RectOffset(1, 0, 5, 0);
            helpBtn.tooltip = "Help about Natural Disaster Renewal";
            helpBtn.eventClick += HelpBtn_eventClick;

            //Close button
            var closeBtn = parentPanel.AddUIComponent<UIButton>();
            closeBtn.relativePosition = new Vector3(PanelWidth - 35f, 8f);
            closeBtn.size = new Vector2(25, 25);
            closeBtn.normalBgSprite = "OptionBase";
            closeBtn.hoveredBgSprite = "OptionBaseHovered";
            closeBtn.pressedBgSprite = "OptionBasePressed";
            closeBtn.focusedColor = Color.white;
            closeBtn.textColor = Color.white;
            closeBtn.focusedTextColor = Color.black;
            closeBtn.text = "X";
            closeBtn.textPadding = new RectOffset(2, 0, 5, 0);
            closeBtn.eventClick += ClosePanelBtn_eventClick;
        }

        private void HelpBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ShowHelpPanel();
        }

        private void ShowHelpPanel()
        {
            //TODO: Implement the help panel with information about the mod
            // * Set a scrolling barr For this panel,
            // * Split panel in two parts, one for the title and the other for the content
            // * replace content to english 

            // Crear panel principal
            var helpPanel = (UIPanel)UIView.GetAView().AddUIComponent(typeof(UIPanel));
            helpPanel.name = "NDRHelpPanel";
            //helpPanel.atlas = UIUtils.GetAtlas("Ingame"); // Replace with the correct atlas
            helpPanel.backgroundSprite = "GenericPanel";
            helpPanel.color = new Color32(50, 50, 50, 250);
            helpPanel.size = new Vector2(400, 300);
            helpPanel.relativePosition = new Vector3(Mathf.Floor((UIView.GetAView().fixedWidth - 400f) / 2f),
                Mathf.Floor((UIView.GetAView().fixedHeight - 300f) / 2f));

            // Título
            var title = helpPanel.AddUIComponent<UILabel>();
            title.text = "Ayuda - Natural Disasters Renewal";
            title.textScale = 1.2f;
            title.relativePosition = new Vector3(10, 10);
            title.textColor = Color.white;

            // Contenido
            var content = helpPanel.AddUIComponent<UILabel>();
            content.text =
                "ESTADÍSTICAS:\n\n" +
                "• Barras de Probabilidad (Rojo → Verde):\n" +
                "  - 0.1 a 10 eventos por año\n" +
                "  - Más rojo = Mayor probabilidad\n\n" +
                "• Barras de Intensidad (Rojo → Verde):\n" +
                "  - 0 a 25.5 de intensidad máxima\n" +
                "  - Más verde = Mayor intensidad\n\n" +
                "CONTROLES:\n" +
                "• ▶/⏸ : Activar/Desactivar desastre\n" +
                "• ■ : Detener todos los desastres activos\n" +
                "• ↺ : Reiniciar todos los desastres\n\n" +
                "Los desastres se activan según la probabilidad\n" +
                "configurada y la población de la ciudad.";
            content.textColor = Color.white;
            content.wordWrap = true;
            content.autoSize = false;
            content.size = new Vector2(380, 240);
            content.relativePosition = new Vector3(10, 40);

            // Botón cerrar
            var closeButton = helpPanel.AddUIComponent<UIButton>();
            closeButton.text = "X";
            //closeButton.atlas = UIUtils.("Ingame"); // Replace with the correct atlas
            closeButton.normalBgSprite = "ButtonMenu";
            closeButton.hoveredBgSprite = "ButtonMenuHovered";
            closeButton.size = new Vector2(30, 30);
            closeButton.textScale = 1.2f;
            closeButton.relativePosition = new Vector3(helpPanel.width - 35, 5);
            closeButton.eventClick += (c, p) => Destroy(helpPanel);
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

            var statisticsTab = CreateTab(tabStrip, "Statistics", 0);
            CreateTab(tabStrip, "Settings", statisticsTab.width);

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
            icon.spriteName = isDisasterEnabled ? PauseSprite : PlaySprite;
            icon.size = new Vector2(12, 12);
            icon.relativePosition = new Vector2(3, 3);

            disasterStateBtn.eventClick += disasterStateChk_eventCheckChanged;

            return disasterStateBtn;
        }

        private void BuildStopDisasterButton(UIComponent parentPanel, float xPosition, float yPosition)
        {
            // Stop Button
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

            // Reset Button
            var resetAllDisastersBtn = parentPanel.AddUIComponent<UIButton>();
            resetAllDisastersBtn.name = "resetDisasterBtn";
            resetAllDisastersBtn.relativePosition = new Vector3(xPosition + 200, yPosition - 5);
            resetAllDisastersBtn.size = new Vector2(18, 18);
            resetAllDisastersBtn.focusedColor = Color.yellow;
            resetAllDisastersBtn.textColor = Color.yellow;
            resetAllDisastersBtn.focusedTextColor = Color.yellow;
            resetAllDisastersBtn.text = "↺";
            resetAllDisastersBtn.normalBgSprite = "ButtonMenu";
            resetAllDisastersBtn.hoveredBgSprite = "ButtonMenuHovered";
            resetAllDisastersBtn.eventClick += ResetAllDisastersBtn_eventClick;

            var resetAllDisastersLabel = parentPanel.AddUIComponent<UILabel>();
            resetAllDisastersLabel.name = "resetBtnLabel";
            resetAllDisastersLabel.relativePosition = new Vector3(xPosition + 225, yPosition - 5);
            resetAllDisastersLabel.size = new Vector2(width - 30, 20);
            resetAllDisastersLabel.textColor = Color.white;
            resetAllDisastersLabel.text = "← Reset all disasters ";
        }

        private void ResetAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            // Stop active disasters first
            StopAllDisastersBtn_eventClick(component, eventParam);

            // Reset each disaster
            var dm = CommonServices.Disasters;

            // Stop active evacuations
            if (dm != null)
                dm.EvacuateAll(true);

            // Reset all disasters
            ResetAllDisasters();

            Debug.Log("All disasters have been reset");
        }

        private static void ResetAllDisasters()
        {
            // Get the instance of the natural disaster handler
            var ndh = CommonServices.DisasterHandler;

            // If there is no instance, exit
            if (ndh == null) return;

            // Stop all active evacuations
            var dm = CommonServices.Disasters;
            if (dm != null) dm.EvacuateAll(true);

            // Iterate through all disasters and reset them
            foreach (var disaster in ndh.container.AllDisasters)
            {
                // Reset cooldown and warmup times
                disaster.CalmDaysLeft = 0;
                disaster.ProbabilityWarmupDaysLeft = 0;
                disaster.IntensityWarmupDaysLeft = 0;

                // Reset state
                disaster.Enabled = false;

                // Reset occurrence values
                disaster.BaseOccurrencePerYear = DisasterBaseModel.defaultBaseOccurrencePerYear;

                // // Stop visual effects
                // disaster.DisableRain();
            }

            Debug.Log("All disasters have been reset to default values");
        }

        private static void StopAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            var cancellingDisasterFlags = new StringBuilder();

            var vehicleManager = CommonServices.Vehicles;
            for (var i = 1; i < 16384; i++)
                if ((vehicleManager.m_vehicles.m_buffer[i].m_flags & Vehicle.Flags.Created) != 0)
                {
                    if (vehicleManager.m_vehicles.m_buffer[i].Info.m_vehicleAI is MeteorAI)
                        vehicleManager.ReleaseVehicle((ushort)i);

                    if (vehicleManager.m_vehicles.m_buffer[i].Info.m_vehicleAI is VortexAI)
                        vehicleManager.ReleaseVehicle((ushort)i);
                }

            var waterSimulation = CommonServices.Water;
            for (var i = waterSimulation.m_waterWaves.m_size; i >= 1; i--)
                CommonServices.Terrain.WaterSimulation.ReleaseWaterWave((ushort)i);

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
                case CommonProperties.earthquakeName:
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
            if (icon != null) icon.spriteName = disaster.Enabled ? PauseSprite : PlaySprite;

            SettingsScreen.UpdateUISettingsOptions();
        }

        private static string SetDisasterProbabilityLabel(float occurrencePerYear = 0)
        {
            var probabilityValue = GetProbabilityProgressValue(occurrencePerYear);
            var percent = probabilityValue * 100f;
            return $"{percent:0.##}%";
        }

        private static string SetMaxIntensityInfoLabel(float maxIntensity = 0)
        {
            return $"{maxIntensity / 10:0.0}";
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

            //Returns a value between 0 and 1 in intervals of 0.1 units
            //based on the logarithmic scale of the occurrence per year
            return (1f + Mathf.Log10(currentOccurrencePerYear)) / 2f;
        }

        // private float GetProbabilityProgressValue(float currentOccurrencePerYear, bool disableRealTimeAdjustment)
        // {
        //     // If Real Time mod is active, then adjust the recurrence to hours
        //     if (_realTimeStatus && disableRealTimeAdjustment)
        //     {
        //         // Gets the actual length of a day in seconds
        //         var simulation = SimulationManager.instance;
        //         var dayDurationSeconds = simulation.m_timePerFrame.TotalSeconds * SimulationManager.DAYTIME_FRAMES;
        //         const double hoursInGameDay = 24.0;
        //         var secondsPerHour = dayDurationSeconds / hoursInGameDay;
        //
        //         // Converts annual occurrence to occurrence per hour of play
        //         var currentOccurrencePerHour = currentOccurrencePerYear / (hoursInGameDay * 365f);
        //
        //         // Adjusts the bar to show the probability per hour (logarithmic scale)
        //         if (currentOccurrencePerHour <= 0.001f)
        //             return 0;
        //         if (currentOccurrencePerHour >= 0.05f)
        //             return 1;
        //
        //         return (1f + Mathf.Log10((float)currentOccurrencePerHour * 10000f)) /
        //                2f; // Escala para rango 0.001 a 0.05
        //     }
        //
        //     // Original calculation (per year)
        //     if (currentOccurrencePerYear <= 0.1f)
        //         return 0;
        //     if (currentOccurrencePerYear >= 10f)
        //         return 1;
        //
        //     return (1f + Mathf.Log10(currentOccurrencePerYear)) / 2f;
        // }

        private static void SetProgressBarColor(UIProgressBar progressBar)
        {
            var value = progressBar.value;
            progressBar.progressColor = new Color(2.0f * value, 2.0f * (1 - value), 0);
        }
    }
}

//TODO:
// * Adjust the probability value if Real Time Mod is active (move from days to frames)
// * Adjust recurrence for thunderstorms and earthquakes, calculation is getting extremely fast