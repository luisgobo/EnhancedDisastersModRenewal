﻿using ColossalFramework;
using ColossalFramework.UI;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NaturalDisastersRenewal.UI
{
    public class ExtendedDisastersPanel : UIPanel
    {
        UILabel[] labels;
        UIButton[] statusButtons;
        UILabel pupulationLabel;
        UILabel OccurrenceAndMaxProb;
        UIProgressBar[] progressBars_probability;
        UIProgressBar[] progressBars_maxIntensity;
        public int Counter = 0;

        public override void Awake()
        {
            base.Awake();

            this.backgroundSprite = "MenuPanel";
            this.canFocus = true;

            height = 280;
            width = 410;

            isVisible = false;
        }

        public override void Start()
        {
            base.Start();

            BuildPanelTitle();
            BuildStatisticsInfo();
            BuildStopDisasterButton();

            #region CloseBtn

            UIButton closeBtn = this.AddUIComponent<UIButton>();
            closeBtn.position = new Vector3(375, -5);
            closeBtn.size = new Vector2(30, 30);
            closeBtn.normalFgSprite = "buttonclose";
            closeBtn.eventClick += ClosePanelBtn_eventClick;

            #endregion CloseBtn
        }

        void BuildPanelTitle()
        {
            //Add Panel Title
            UILabel lTitle = this.AddUIComponent<UILabel>();
            lTitle.position = new Vector3(10, -15);
            lTitle.text = "Disasters info";
        }

        void BuildStatisticsInfo()
        {
            int y = -50;
            int h = -20;

            NaturalDisasterHandler disasterHandler = Singleton<NaturalDisasterHandler>.instance;
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";
            string formatNumber = ((int)disasterHandler.container.MaxPopulationToTrigguerHigherDisasters).ToString("#,0", nfi);

            pupulationLabel = AddLabel(28, y);
            pupulationLabel.text = $"MPTHD: {formatNumber}";
            pupulationLabel.tooltip = "Max population to trigger higher disasters";            
            y -= 22;

            //currentOccurrencePerYear:0.00}/{maxIntensity}
            OccurrenceAndMaxProb = AddLabel(28, y);
            OccurrenceAndMaxProb.text = $"Disaster - COY/Max Int";
            OccurrenceAndMaxProb.tooltip = "Disaster name and current Occurrence per Year/ Max Intensity";
            OccurrenceAndMaxProb.textScale = 0.7f;

            //Add Axis titles
            AddAxisTitle(210, y, "Probability");
            AddAxisTitle(310, y, "Max intensity");
            y -= 15;

            //Add Axis Labels
            AddAxisLabel(210, y, "0.1");
            AddAxisLabel(250, y, "1");
            AddAxisLabel(285, y, "10");
            AddAxisLabel(310, y, "0.0");
            AddAxisLabel(370, y, "25.5");
            y -= 15;

            int disasterCount = disasterHandler.container.AllDisasters.Count;
            labels = new UILabel[disasterCount];
            statusButtons = new UIButton[disasterCount];
            progressBars_probability = new UIProgressBar[disasterCount];
            progressBars_maxIntensity = new UIProgressBar[disasterCount];

            //Add each statistic item to the Panel
            for (int i = 0; i < disasterCount; i++)
            {
                DisasterBaseModel disaster = disasterHandler.container.AllDisasters[i];
                statusButtons[i] = BuildDisasterStatusButton(5, y, disaster.GetName(), disaster.Enabled);
                labels[i] = AddLabel(28, y);
                labels[i].text = SetDisasterInfoLabel(disaster.GetName(), 0, 0);

                progressBars_probability[i] = AddProgressBar(210, y);
                progressBars_maxIntensity[i] = AddProgressBar(310, y);
                y += h;
            }
        }

        UIButton BuildDisasterStatusButton(int x, int y, string disasterName, bool isEnabled)
        {
            UIButton disasterStateBtn = this.AddUIComponent<UIButton>();
            disasterStateBtn.name = $"disasterState{disasterName}Btn";
            disasterStateBtn.position = new Vector3(x, y + 4);
            disasterStateBtn.size = new Vector2(18, 18);
            disasterStateBtn.normalBgSprite = "ButtonMenu";
            disasterStateBtn.hoveredBgSprite = "ButtonMenuHovered";
            disasterStateBtn.normalFgSprite = isEnabled ? "ButtonPause" : "ButtonPlayFocused";
            disasterStateBtn.eventClick += disasterStateChk_eventCheckChanged;

            return disasterStateBtn;
        }

        void BuildStopDisasterButton()
        {
            UIButton stopAllDisastersBtn = this.AddUIComponent<UIButton>();
            stopAllDisastersBtn.name = "stopDisasterBtn";
            stopAllDisastersBtn.position = new Vector3(10, -height + 30);
            stopAllDisastersBtn.size = new Vector2(22, 22);
            stopAllDisastersBtn.focusedColor = Color.red;
            stopAllDisastersBtn.textColor = Color.red;
            stopAllDisastersBtn.focusedTextColor = Color.red;
            stopAllDisastersBtn.text = "■";
            stopAllDisastersBtn.normalBgSprite = "ButtonMenu";
            stopAllDisastersBtn.hoveredBgSprite = "ButtonMenuHovered";
            stopAllDisastersBtn.eventClick += StopAllDisastersBtn_eventClick;

            UILabel stopAllDisastersLabel = this.AddUIComponent<UILabel>();
            stopAllDisastersLabel.name = "bigRedBtnLabel";
            stopAllDisastersLabel.position = new Vector3(40, -height + 27);
            stopAllDisastersLabel.size = new Vector2(width - 30, 20);
            stopAllDisastersLabel.textColor = Color.white;
            //bigRedBtnLabel.textScale = 0.7f;
            stopAllDisastersLabel.text = "← Emergency Button (stop all disasters)";
        }

        void StopAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            StringBuilder sb = new StringBuilder();

            VehicleManager vm = Singleton<VehicleManager>.instance;
            for (int i = 1; i < 16384; i++)
            {
                if ((vm.m_vehicles.m_buffer[i].m_flags & Vehicle.Flags.Created) != (Vehicle.Flags)0)
                {
                    if (vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is MeteorAI)
                    {
                        vm.ReleaseVehicle((ushort)i);
                    }

                    if (vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is VortexAI)
                    {
                        vm.ReleaseVehicle((ushort)i);
                    }
                }
            }

            WaterSimulation ws = Singleton<WaterSimulation>.instance;
            for (int i = ws.m_waterWaves.m_size; i >= 1; i--)
            {
                Singleton<TerrainManager>.instance.WaterSimulation.ReleaseWaterWave((ushort)i);
            }

            DisasterManager dm = Singleton<DisasterManager>.instance;
            for (ushort i = 0; i < dm.m_disasterCount; i++)
            {
                sb.AppendLine(dm.m_disasters.m_buffer[i].Info.name + " flags: " + dm.m_disasters.m_buffer[i].m_flags.ToString());
                if ((dm.m_disasters.m_buffer[i].m_flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active | DisasterData.Flags.Clearing)) != DisasterData.Flags.None)
                {
                    if (IsStopableDisaster(dm.m_disasters.m_buffer[i].Info.m_disasterAI))
                    {
                        sb.AppendLine("Trying to cancel " + dm.m_disasters.m_buffer[i].Info.name);
                        dm.m_disasters.m_buffer[i].m_flags = ((dm.m_disasters.m_buffer[i].m_flags & ~(DisasterData.Flags.Emerging | DisasterData.Flags.Active | DisasterData.Flags.Clearing)) | DisasterData.Flags.Finished);
                    }
                }
            }
            Debug.Log(sb.ToString());
        }

        void ClosePanelBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            this.Hide();
        }

        void disasterStateChk_eventCheckChanged(UIComponent component, UIMouseEventParameter eventParam)
        {
            NaturalDisasterHandler disasterHandler = Singleton<NaturalDisasterHandler>.instance;
            DisasterBaseModel disaster = disasterHandler.container.AllDisasters.Where(dis => component.name.Contains(dis.GetName())).FirstOrDefault();

            if (disaster != null)
            {
                disaster.Enabled = !disaster.Enabled;
                switch (disaster.GetName())
                {
                    case CommonProperties.EarthquakeName:
                        Singleton<NaturalDisasterHandler>.instance.container.Earthquake.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.forestFireName:
                        Singleton<NaturalDisasterHandler>.instance.container.ForestFire.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.meteorStrikeName:
                        Singleton<NaturalDisasterHandler>.instance.container.MeteorStrike.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.sinkholeName:
                        Singleton<NaturalDisasterHandler>.instance.container.Sinkhole.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.thunderstormName:
                        Singleton<NaturalDisasterHandler>.instance.container.Thunderstorm.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.tornadoName:
                        Singleton<NaturalDisasterHandler>.instance.container.Tornado.Enabled = disaster.Enabled;
                        break;

                    case CommonProperties.tsunamiName:
                        Singleton<NaturalDisasterHandler>.instance.container.Tsunami.Enabled = disaster.Enabled;
                        break;

                    default:
                        break;
                }

                if (disaster.Enabled)
                    ((UIButton)component).normalFgSprite = "ButtonPause";
                else
                    ((UIButton)component).normalFgSprite = "ButtonPlayFocused";

                SettingsScreen.UpdateUISettingsOptions();
            }
        }

        string SetDisasterInfoLabel(string disasterName, float currentOccurrencePerYear, float maxIntensity)
        {
            return $"{disasterName} - {currentOccurrencePerYear:0.00}/{(maxIntensity/10):0.0}";
        }

        bool IsStopableDisaster(DisasterAI ai)
        {
            return (ai as ThunderStormAI != null) || (ai as SinkholeAI != null) || (ai as TornadoAI != null) || (ai as EarthquakeAI != null) || (ai as MeteorStrikeAI != null);
        }

        UILabel AddLabel(int x, int y)
        {
            UILabel label = this.AddUIComponent<UILabel>();
            label.position = new Vector3(x, y);
            label.textScale = 0.8f;

            return label;
        }

        void AddAxisLabel(int x, int y, string text)
        {
            UILabel l = this.AddUIComponent<UILabel>();
            l.position = new Vector3(x, y);
            l.textScale = 0.7f;
            l.text = text;
        }

        void AddAxisTitle(int x, int y, string text)
        {
            UILabel l = this.AddUIComponent<UILabel>();
            l.position = new Vector3(x, y);
            l.textScale = 0.7f;
            l.text = text;
        }

        UIProgressBar AddProgressBar(int x, int y)
        {
            UIProgressBar progressBar = this.AddUIComponent<UIProgressBar>();
            progressBar.backgroundSprite = "LevelBarBackground";
            progressBar.progressSprite = "LevelBarForeground";
            progressBar.progressColor = Color.red;
            progressBar.position = new Vector3(x, y);
            progressBar.width = 90;
            progressBar.value = 0.5f;

            return progressBar;
        }

        float GetProbabilityProgressValueLog(float currentOcurrencePerYear)
        {
            if (currentOcurrencePerYear <= 0.1)
                return 0;
            if (currentOcurrencePerYear >= 10)
                return 1;

            return (1f + Mathf.Log10(currentOcurrencePerYear)) / 2f;
        }

        public override void Update()
        {
            base.Update();

            if (!isVisible) return;

            if (--Counter > 0) return;
            Counter = 10;

            NaturalDisasterHandler disasterHandler = Singleton<NaturalDisasterHandler>.instance;            
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";
            string formatNumber = ((int)disasterHandler.container.MaxPopulationToTrigguerHigherDisasters).ToString("#,0", nfi);
            pupulationLabel.text = $"MPTHD: {formatNumber}";
            pupulationLabel.textScale = 0.7f;

            int disasterCount = disasterHandler.container.AllDisasters.Count;

            for (int i = 0; i < disasterCount; i++)
            {
                DisasterBaseModel disaster = disasterHandler.container.AllDisasters[i];                
                float currentOcurrencePerYear = disaster.GetCurrentOccurrencePerYear();

                byte maxIntensityCalculated = disaster.GetMaximumIntensity();

                statusButtons[i].isVisible = true;

                if (disaster.Enabled)
                {
                    statusButtons[i].normalFgSprite = "ButtonPause";
                    labels[i].text = SetDisasterInfoLabel(disaster.GetName(), currentOcurrencePerYear, maxIntensityCalculated);

                    //Calculate probability                    
                    
                    var propbabilityValue = GetProbabilityProgressValueLog(currentOcurrencePerYear);

                    progressBars_probability[i].value = propbabilityValue;
                    SetProgressBarColor(progressBars_probability[i]);
                    //progressBars_probability[i].tooltip = disaster.GetProbabilityTooltip(progressBars_probability[i].value);
                    progressBars_probability[i].tooltip = disaster.GetProbabilityTooltip(propbabilityValue);

                    //Calculate intensity
                    float maxIntensity = 255f;
                    float progressbarCalculatedValue = maxIntensityCalculated * (1 / maxIntensity);

                    progressBars_maxIntensity[i].value = progressbarCalculatedValue;
                    SetProgressBarColor(progressBars_maxIntensity[i]);
                    progressBars_maxIntensity[i].tooltip = disaster.GetIntensityTooltip(progressBars_maxIntensity[i].value);
                }
                else
                {
                    statusButtons[i].normalFgSprite = "ButtonPlayFocused";
                    labels[i].text = $"{disaster.GetName()} - Disabled";

                    progressBars_probability[i].value = 0;
                    progressBars_probability[i].progressColor = Color.black;
                    progressBars_maxIntensity[i].value = 0;
                    progressBars_maxIntensity[i].progressColor = Color.black;
                }
            }
        }

        void SetProgressBarColor(UIProgressBar progressBar)
        {
            float value = progressBar.value;
            progressBar.progressColor = new Color(2.0f * value, 2.0f * (1 - value), 0);
        }
    }
}