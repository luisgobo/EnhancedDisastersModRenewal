using ColossalFramework;
using ColossalFramework.UI;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using System;
using System.Text;
using UnityEngine;

namespace NaturalDisastersRenewal.UI
{
    public class ExtendedDisastersPanel : UIPanel
    {
        UILabel[] labels;
        UIProgressBar[] progressBars_probability;
        UIProgressBar[] progressBars_maxIntensity;
        readonly string labelFormat = "{0} ({1:0.00}/{2})";
        public int Counter = 0;

        public override void Awake()
        {
            base.Awake();

            //this.backgroundSprite = "GenericPanel";
            //this.color = new Color32(255, 0, 0, 100);
            this.backgroundSprite = "MenuPanel";
            this.canFocus = true;
            //this.isInteractive = true;

            height = 280;
            width = 410;

            isVisible = false;
        }

        public override void Start()
        {
            base.Start();

            UILabel lTitle = this.AddUIComponent<UILabel>();
            lTitle.position = new Vector3(10, -15);
            lTitle.text = "Disasters info";

            int y = -50;
            int h = -20;

            AddAxisTitle(200, y, "Probability");
            AddAxisTitle(300, y, "Max intensity");
            y -= 15;

            AddAxisLabel(200, y, "0.1");
            AddAxisLabel(240, y, "1");
            AddAxisLabel(275, y, "10");
            AddAxisLabel(300, y, "0.0");
            AddAxisLabel(375, y, "25.5");
            y -= 15;

            int disasterCount = Singleton<NaturalDisasterHandler>.instance.container.AllDisasters.Count;
            labels = new UILabel[disasterCount];
            progressBars_probability = new UIProgressBar[disasterCount];
            progressBars_maxIntensity = new UIProgressBar[disasterCount];

            NaturalDisasterHandler disasterHandler = Singleton<NaturalDisasterHandler>.instance;
            for (int i = 0; i < disasterCount; i++)
            {
                DisasterBaseModel disaster = disasterHandler.container.AllDisasters[i];
                labels[i] = AddLabel(10, y);
                labels[i].text = string.Format(labelFormat, disaster.GetName(), 0, 0);
                progressBars_probability[i] = AddProgressBar(200, y);
                progressBars_maxIntensity[i] = AddProgressBar(300, y);
                y += h;
            }

            #region bigRedBtn

            UIButton bigRedBtn = this.AddUIComponent<UIButton>();
            bigRedBtn.name = "bigRedBtn";
            bigRedBtn.position = new Vector3(10, -height + 30);
            bigRedBtn.size = new Vector2(22, 22);
            //bigRedBtn.color = Color.red;
            bigRedBtn.focusedColor = Color.red;
            bigRedBtn.textColor = Color.red;
            bigRedBtn.focusedTextColor = Color.red;
            bigRedBtn.text = "■";
            bigRedBtn.normalBgSprite = "ButtonMenu";
            bigRedBtn.hoveredBgSprite = "ButtonMenuHovered";
            bigRedBtn.eventClick += BigRedBtn_eventClick;

            UILabel bigRedBtnLabel = this.AddUIComponent<UILabel>();
            bigRedBtnLabel.name = "bigRedBtnLabel";
            bigRedBtnLabel.position = new Vector3(40, -height + 27);
            bigRedBtnLabel.size = new Vector2(width - 30, 20);
            bigRedBtnLabel.textColor = Color.white;
            //bigRedBtnLabel.textScale = 0.7f;
            bigRedBtnLabel.text = "← Emergency Button (stop all disasters)";

            #endregion

            #region CloseBtn
            UIButton closeBtn = this.AddUIComponent<UIButton>();
            closeBtn.position = new Vector3(375, -5);
            closeBtn.size = new Vector2(30, 30);
            closeBtn.normalFgSprite = "buttonclose";
            closeBtn.eventClick += CloseBtn_eventClick;

            #endregion

        }
        void BigRedBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
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
        void CloseBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            this.Hide();
        }               

        bool IsStopableDisaster(DisasterAI ai)
        {
            return (ai as ThunderStormAI != null) || (ai as SinkholeAI != null) || (ai as TornadoAI != null) || (ai as EarthquakeAI != null) || (ai as MeteorStrikeAI != null);
        }


        UILabel AddLabel(int x, int y)
        {
            UILabel l = this.AddUIComponent<UILabel>();
            l.position = new Vector3(x, y);
            l.textScale = 0.8f;

            return l;
        }

        void AddAxisLabel(int x, int y, string text)
        {
            //switch (labelTextAlignment)
            //{
            //    case UIHorizontalAlignment.Center:
            //        x -= 15;
            //        break;
            //    case UIHorizontalAlignment.Right:
            //        x -= 30;
            //        break;
            //}

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

        float GetProgressValueLog(float value)
        {
            if (value <= 0.1) return 0;
            if (value >= 10) return 1;
            return (1f + Mathf.Log10(value)) / 2f;
        }

        public override void Update()
        {
            base.Update();

            if (!isVisible) return;

            if (--Counter > 0) return;
            Counter = 10;

            NaturalDisasterHandler edm = Singleton<NaturalDisasterHandler>.instance;
            int disasterCount = edm.container.AllDisasters.Count;

            for (int i = 0; i < disasterCount; i++)
            {
                DisasterBaseModel disaster = edm.container.AllDisasters[i];
                float currentOcurrencePerYear = disaster.GetCurrentOccurrencePerYear();
                byte maxIntensity = disaster.GetMaximumIntensity();
                if (disaster.Enabled)
                {
                    labels[i].text = string.Format(labelFormat, disaster.GetName(), currentOcurrencePerYear, maxIntensity);

                    progressBars_probability[i].value = GetProgressValueLog(currentOcurrencePerYear);
                    SetProgressBarColor(progressBars_probability[i]);
                    progressBars_probability[i].tooltip = disaster.GetProbabilityTooltip();
                    
                    //string nl = Environment.NewLine;
                    //string arr = $">>>>>>>>>{nl}";
                    
                    //DebugLogger.Log($"{arr+arr+arr+arr+arr+nl} maxIntensity: {maxIntensity}" +
                    //    $"maxIntensity * 0.01f =  {maxIntensity * 0.01f}{nl}" +
                    //    $"disaster.GetIntensityTooltip =  {disaster.GetIntensityTooltip()}" +
                    //    $"{arr + arr + arr + arr + arr}");

                    progressBars_maxIntensity[i].value = maxIntensity * 0.01f;
                    SetProgressBarColor(progressBars_maxIntensity[i]);
                    progressBars_maxIntensity[i].tooltip = disaster.GetIntensityTooltip();
                }
                else
                {
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