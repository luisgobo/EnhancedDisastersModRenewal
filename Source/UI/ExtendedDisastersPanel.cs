using System.Globalization;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.Common;
using UnityEngine;

namespace NaturalDisastersRenewal.UI
{
    public class ExtendedDisastersPanel : UIPanel
    {
        public int Counter;
        private UILabel[] labels;
        private UILabel OccurrenceAndMaxProb;
        private UIProgressBar[] progressBars_maxIntensity;
        private UIProgressBar[] progressBars_probability;
        private UILabel pupulationLabel;
        private UIButton[] statusButtons;

        public override void Awake()
        {
            base.Awake();

            backgroundSprite = "MenuPanel";
            canFocus = true;

            height = 280;
            width = 410;

            isVisible = false;
        }

        public override void Start()
        {
            base.Start();

            RebuildLocalizedContent();
        }

        public override void Update()
        {
            base.Update();

            if (!isVisible) return;

            if (--Counter > 0) return;
            Counter = 10;

            var disasterHandler = Services.DisasterHandler;
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";
            var formatNumber =
                ((int)disasterHandler.container.MaxPopulationToTrigguerHigherDisasters).ToString("#,0", nfi);
            pupulationLabel.text = $"MPTHD: {formatNumber}";
            pupulationLabel.textScale = 0.7f;

            var disasterCount = disasterHandler.container.AllDisasters.Count;

            for (var i = 0; i < disasterCount; i++)
            {
                var disaster = disasterHandler.container.AllDisasters[i];
                var currentOcurrencePerYear = disaster.GetCurrentOccurrencePerYear();

                var maxIntensityCalculated = disaster.GetMaximumIntensity();

                statusButtons[i].isVisible = true;

                if (disaster.Enabled)
                {
                    statusButtons[i].normalFgSprite = "ButtonPause";
                    labels[i].text = SetDisasterInfoLabel(disaster.GetName(), currentOcurrencePerYear,
                        maxIntensityCalculated);

                    //Calculate probability

                    var propbabilityValue = GetProbabilityProgressValueLog(currentOcurrencePerYear);

                    progressBars_probability[i].value = propbabilityValue;
                    SetProgressBarColor(progressBars_probability[i]);
                    //progressBars_probability[i].tooltip = disaster.GetProbabilityTooltip(progressBars_probability[i].value);
                    progressBars_probability[i].tooltip = disaster.GetProbabilityTooltip(propbabilityValue);

                    //Calculate intensity
                    var maxIntensity = 255f;
                    var progressbarCalculatedValue = maxIntensityCalculated * (1 / maxIntensity);

                    progressBars_maxIntensity[i].value = progressbarCalculatedValue;
                    SetProgressBarColor(progressBars_maxIntensity[i]);
                    progressBars_maxIntensity[i].tooltip =
                        disaster.GetIntensityTooltip(progressBars_maxIntensity[i].value);
                }
                else
                {
                    statusButtons[i].normalFgSprite = "ButtonPlayFocused";
                    labels[i].text = disaster.GetName() + " - " + LocalizationService.Get("panel.disabled");

                    progressBars_probability[i].value = 0;
                    progressBars_probability[i].progressColor = Color.black;
                    progressBars_maxIntensity[i].value = 0;
                    progressBars_maxIntensity[i].progressColor = Color.black;
                }
            }
        }

        private void BuildPanelTitle()
        {
            //Add Panel Title
            var lTitle = AddUIComponent<UILabel>();
            lTitle.position = new Vector3(10, -15);
            lTitle.text = LocalizationService.Get("panel.title");
        }

        public void RebuildLocalizedContent()
        {
            var currentVisibility = isVisible;

            foreach (var child in components.OfType<UIComponent>().ToArray()) Destroy(child.gameObject);

            BuildPanelTitle();
            BuildStatisticsInfo();
            BuildStopDisasterButton();

            var closeBtn = AddUIComponent<UIButton>();
            closeBtn.position = new Vector3(375, -5);
            closeBtn.size = new Vector2(30, 30);
            closeBtn.normalFgSprite = "buttonclose";
            closeBtn.eventClick += ClosePanelBtn_eventClick;

            isVisible = currentVisibility;
            Counter = 0;
        }

        private void BuildStatisticsInfo()
        {
            var y = -50;
            var h = -20;

            var disasterHandler = Services.DisasterHandler;
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";
            var formatNumber =
                ((int)disasterHandler.container.MaxPopulationToTrigguerHigherDisasters).ToString("#,0", nfi);

            pupulationLabel = AddLabel(28, y);
            pupulationLabel.text = $"MPTHD: {formatNumber}";
            pupulationLabel.tooltip = LocalizationService.Get("panel.population_threshold");
            y -= 22;

            //currentOccurrencePerYear:0.00}/{maxIntensity}
            OccurrenceAndMaxProb = AddLabel(28, y);
            OccurrenceAndMaxProb.text = LocalizationService.Get("panel.disaster_header");
            OccurrenceAndMaxProb.tooltip = LocalizationService.Get("panel.disaster_header");
            OccurrenceAndMaxProb.textScale = 0.7f;

            //Add Axis titles
            AddAxisTitle(210, y, LocalizationService.Get("panel.axis.probability"));
            AddAxisTitle(310, y, LocalizationService.Get("panel.axis.max_intensity"));
            y -= 15;

            //Add Axis Labels
            AddAxisLabel(210, y, "0.1");
            AddAxisLabel(250, y, "1");
            AddAxisLabel(285, y, "10");
            AddAxisLabel(310, y, "0.0");
            AddAxisLabel(370, y, "25.5");
            y -= 15; 

            var disasterCount = disasterHandler.container.AllDisasters.Count;
            labels = new UILabel[disasterCount];
            statusButtons = new UIButton[disasterCount];
            progressBars_probability = new UIProgressBar[disasterCount];
            progressBars_maxIntensity = new UIProgressBar[disasterCount];

            //Add each statistic item to the Panel
            for (var i = 0; i < disasterCount; i++)
            {
                var disaster = disasterHandler.container.AllDisasters[i];
                statusButtons[i] =
                    BuildDisasterStatusButton(5, y, disaster.GetDisasterType().ToString(), disaster.Enabled);
                labels[i] = AddLabel(28, y);
                labels[i].text = SetDisasterInfoLabel(disaster.GetName(), 0, 0);

                progressBars_probability[i] = AddProgressBar(210, y);
                progressBars_maxIntensity[i] = AddProgressBar(310, y);
                y += h;
            }
        }

        private UIButton BuildDisasterStatusButton(int x, int y, string disasterKey, bool isEnabled)
        {
            var disasterStateBtn = AddUIComponent<UIButton>();
            disasterStateBtn.name = $"disasterState{disasterKey}Btn";
            disasterStateBtn.position = new Vector3(x, y + 4);
            disasterStateBtn.size = new Vector2(18, 18);
            disasterStateBtn.normalBgSprite = "ButtonMenu";
            disasterStateBtn.hoveredBgSprite = "ButtonMenuHovered";
            disasterStateBtn.normalFgSprite = isEnabled ? "ButtonPause" : "ButtonPlayFocused";
            disasterStateBtn.eventClick += DisasterStateChk_eventCheckChanged;

            return disasterStateBtn;
        }

        private void BuildStopDisasterButton()
        {
            var stopAllDisastersBtn = AddUIComponent<UIButton>();
            stopAllDisastersBtn.name = "stopDisasterBtn";
            stopAllDisastersBtn.position = new Vector3(10, -height + 30);
            stopAllDisastersBtn.size = new Vector2(22, 22);
            stopAllDisastersBtn.focusedColor = Color.red;
            stopAllDisastersBtn.textColor = Color.red;
            stopAllDisastersBtn.focusedTextColor = Color.red;
            stopAllDisastersBtn.text = "\u25A0";
            stopAllDisastersBtn.normalBgSprite = "ButtonMenu";
            stopAllDisastersBtn.hoveredBgSprite = "ButtonMenuHovered";
            stopAllDisastersBtn.eventClick += StopAllDisastersBtn_eventClick;

            var stopAllDisastersLabel = AddUIComponent<UILabel>();
            stopAllDisastersLabel.name = "bigRedBtnLabel";
            stopAllDisastersLabel.position = new Vector3(40, -height + 27);
            stopAllDisastersLabel.size = new Vector2(width - 30, 20);
            stopAllDisastersLabel.textColor = Color.white;
            //bigRedBtnLabel.textScale = 0.7f;
            stopAllDisastersLabel.text = LocalizationService.Get("panel.stop_all");
        }

        private void StopAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            var sb = new StringBuilder();

            var vm = Services.Vehicles;
            for (var i = 1; i < 16384; i++)
                if ((vm.m_vehicles.m_buffer[i].m_flags & Vehicle.Flags.Created) != 0)
                {
                    if (vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is MeteorAI) vm.ReleaseVehicle((ushort)i);

                    if (vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is VortexAI) vm.ReleaseVehicle((ushort)i);
                }

            var ws = Services.Water;
            for (var i = ws.m_waterWaves.m_size; i >= 1; i--)
                Services.Terrain.WaterSimulation.ReleaseWaterWave((ushort)i);

            var dm = Services.Disasters;
            for (ushort i = 0; i < dm.m_disasterCount; i++)
            {
                sb.AppendLine(dm.m_disasters.m_buffer[i].Info.name + " flags: " + dm.m_disasters.m_buffer[i].m_flags);
                if ((dm.m_disasters.m_buffer[i].m_flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active |
                                                           DisasterData.Flags.Clearing)) != DisasterData.Flags.None)
                    if (IsStopableDisaster(dm.m_disasters.m_buffer[i].Info.m_disasterAI))
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

        private void DisasterStateChk_eventCheckChanged(UIComponent component, UIMouseEventParameter eventParam)
        {
            var disasterHandler = Services.DisasterHandler;
            var disaster = disasterHandler.container.AllDisasters
                .Where(dis => component.name.Contains(dis.GetDisasterType().ToString())).FirstOrDefault();

            if (disaster != null)
            {
                disaster.Enabled = !disaster.Enabled;
                switch (disaster.GetDisasterType())
                {
                    case DisasterType.Earthquake:
                        Services.DisasterSetup.Earthquake.Enabled = disaster.Enabled;
                        break;

                    case DisasterType.ForestFire:
                        Services.DisasterSetup.ForestFire.Enabled = disaster.Enabled;
                        break;

                    case DisasterType.MeteorStrike:
                        Services.DisasterSetup.MeteorStrike.Enabled = disaster.Enabled;
                        break;

                    case DisasterType.Sinkhole:
                        Services.DisasterSetup.Sinkhole.Enabled = disaster.Enabled;
                        break;

                    case DisasterType.ThunderStorm:
                        Services.DisasterSetup.Thunderstorm.Enabled = disaster.Enabled;
                        break;

                    case DisasterType.Tornado:
                        Services.DisasterSetup.Tornado.Enabled = disaster.Enabled;
                        break;

                    case DisasterType.Tsunami:
                        Services.DisasterSetup.Tsunami.Enabled = disaster.Enabled;
                        break;
                }

                if (disaster.Enabled)
                    ((UIButton)component).normalFgSprite = "ButtonPause";
                else
                    ((UIButton)component).normalFgSprite = "ButtonPlayFocused";

                SettingsScreen.UpdateUISettingsOptions();
            }
        }

        private string SetDisasterInfoLabel(string disasterName, float currentOccurrencePerYear, float maxIntensity)
        {
            return $"{disasterName} - {currentOccurrencePerYear:0.00}/{maxIntensity / 10:0.0}";
        }

        private bool IsStopableDisaster(DisasterAI ai)
        {
            return ai as ThunderStormAI != null || ai as SinkholeAI != null || ai as TornadoAI != null ||
                   ai as EarthquakeAI != null || ai as MeteorStrikeAI != null;
        }

        private UILabel AddLabel(int x, int y)
        {
            var label = AddUIComponent<UILabel>();
            label.position = new Vector3(x, y);
            label.textScale = 0.8f;

            return label;
        }

        private void AddAxisLabel(int x, int y, string text)
        {
            var l = AddUIComponent<UILabel>();
            l.position = new Vector3(x, y);
            l.textScale = 0.7f;
            l.text = text;
        }

        private void AddAxisTitle(int x, int y, string text)
        {
            var l = AddUIComponent<UILabel>();
            l.position = new Vector3(x, y);
            l.textScale = 0.7f;
            l.text = text;
        }

        private UIProgressBar AddProgressBar(int x, int y)
        {
            var progressBar = AddUIComponent<UIProgressBar>();
            progressBar.backgroundSprite = "LevelBarBackground";
            progressBar.progressSprite = "LevelBarForeground";
            progressBar.progressColor = Color.red;
            progressBar.position = new Vector3(x, y);
            progressBar.width = 90;
            progressBar.value = 0.5f;

            return progressBar;
        }

        private float GetProbabilityProgressValueLog(float currentOcurrencePerYear)
        {
            if (currentOcurrencePerYear <= 0.1)
                return 0;
            if (currentOcurrencePerYear >= 10)
                return 1;

            return (1f + Mathf.Log10(currentOcurrencePerYear)) / 2f;
        }

        private void SetProgressBarColor(UIProgressBar progressBar)
        {
            var value = progressBar.value;
            progressBar.progressColor = new Color(2.0f * value, 2.0f * (1 - value), 0);
        }
    }
}