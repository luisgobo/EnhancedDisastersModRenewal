using System.Globalization;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using NaturalDisastersRenewal.UI.ComponentHelper;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace NaturalDisastersRenewal.UI
{
    public class ExtendedDisastersPanel : UIPanel
    {
        private const float LabelTextScaleSmall = 0.7f;
        private const float LabelTextScaleNormal = 0.8f;
        private const float PanelWidth = 414f;
        private const float PanelHeight = 320f;

        public int Counter;
        private DisasterRowHelper[] _disasterRows;
        private UILabel _populationLabel;

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
            RebuildLocalizedContent();
        }

        public override void Update()
        {
            base.Update();

            if (!isVisible) return;

            if (--Counter > 0) return;
            Counter = 10;

            UpdatePopulationLabel();

            if (_disasterRows == null) return;

            for (int i = 0; i < _disasterRows.Length; i++)
            {
                _disasterRows[i].Refresh();
            }
        }

        public void RebuildLocalizedContent()
        {
            bool currentVisibility = isVisible;
            foreach (UIComponent child in components.OfType<UIComponent>().ToArray())
            {
                UnityObject.Destroy(child.gameObject);
            }

            BuildInformationBar();
            BuildTabContainer();

            isVisible = currentVisibility;
            Counter = 0;
        }

        private void BuildInformationBar()
        {
            UIPanel titleBar = AddUIComponent<UIPanel>();
            titleBar.name = "titleBar";
            titleBar.relativePosition = Vector3.zero;
            titleBar.size = new Vector2(PanelWidth - 80f, 40f);
            titleBar.tooltip = LocalizationService.Get("panel.drag_panel.tooltip");
            titleBar.isInteractive = true;

            UILabel titleLabel = titleBar.AddUIComponent<UILabel>();
            titleLabel.relativePosition = new Vector3(10f, 15f);
            titleLabel.text = LocalizationService.Get("panel.title");
            titleLabel.tooltip = LocalizationService.Get("panel.drag_panel.tooltip");

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
                LocalizationService.Get("panel.help.tooltip"),
                HelpBtn_eventClick,
                null,
                "OptionBase",
                "OptionBaseHovered",
                "OptionBasePressed",
                new RectOffset(1, 0, 5, 0));

            ActionButtonHelper.CreateTextButton(
                parentPanel,
                "closeBtn",
                "X",
                new Vector3(PanelWidth - 35f, 8f),
                new Vector2(25f, 25f),
                LocalizationService.Get("panel.close.tooltip"),
                ClosePanelBtn_eventClick,
                null,
                "OptionBase",
                "OptionBaseHovered",
                "OptionBasePressed",
                new RectOffset(2, 0, 5, 0));
        }

        private void BuildTabContainer()
        {
            UITabContainer tabContainer = AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0f, 70f);
            tabContainer.size = new Vector2(width, height - 50f);

            UITabstrip tabStrip = AddUIComponent<UITabstrip>();
            tabStrip.relativePosition = new Vector3(0f, 40f);
            tabStrip.size = new Vector2(width, 30f);
            tabStrip.backgroundSprite = "SubcategoriesPanel";
            tabStrip.tabPages = tabContainer;

            UIButton statisticsTab = TabHelper.CreateStyledTab(tabStrip, LocalizationService.Get("panel.tab.statistics"), 0f);
            TabHelper.CreateStyledTab(tabStrip, LocalizationService.Get("panel.tab.controls"), statisticsTab.width);

            if (tabContainer.components.Count < 2) return;

            UIPanel statisticsTabPanel = tabContainer.components[0] as UIPanel;
            UIPanel controlsTabPanel = tabContainer.components[1] as UIPanel;
            if (statisticsTabPanel == null || controlsTabPanel == null) return;

            UIScrollablePanel statisticsScrollablePanel = ScrollablePanelHelper.Create(statisticsTabPanel, 10f);
            UIScrollablePanel controlsScrollablePanel = ScrollablePanelHelper.Create(controlsTabPanel, 10f);

            BuildStatisticsInfoTabContent(statisticsScrollablePanel);
            BuildControlsTabContent(controlsScrollablePanel);

            statisticsTabPanel.isVisible = true;
            controlsTabPanel.isVisible = false;
        }

        private void BuildStatisticsInfoTabContent(UIScrollablePanel parentPanel)
        {
            const float itemSpacing = 22f;
            const float xPosition = 1f;
            float yPosition = 10f;
            float probabilityBarStartX = xPosition + DisasterRowHelper.ProbabilityBarX;
            float probabilityBarCenterX = probabilityBarStartX + DisasterRowHelper.BarWidth * 0.5f;
            float probabilityBarEndX = probabilityBarStartX + DisasterRowHelper.BarWidth;
            float intensityBarStartX = xPosition + DisasterRowHelper.IntensityBarX;
            float intensityBarCenterX = intensityBarStartX + DisasterRowHelper.BarWidth * 0.5f;
            float intensityBarEndX = intensityBarStartX + DisasterRowHelper.BarWidth;

            _populationLabel = AddLabel(parentPanel, xPosition, yPosition, LabelTextScaleNormal, string.Empty);
            UpdatePopulationLabel();

            yPosition += 22f;
            AddLabel(parentPanel, xPosition + 32f, yPosition, LabelTextScaleSmall, LocalizationService.Get("panel.header.disaster"));
            AddCenteredLabel(parentPanel, probabilityBarCenterX - 3f, yPosition, LabelTextScaleSmall, LocalizationService.Get("panel.header.probability"));
            AddCenteredLabel(parentPanel, intensityBarCenterX, yPosition, LabelTextScaleSmall, LocalizationService.Get("panel.header.max_intensity"));

            yPosition += 15f;
            AddCenteredLabel(parentPanel, probabilityBarStartX + 2f, yPosition, LabelTextScaleSmall, "0.1");
            AddCenteredLabel(parentPanel, probabilityBarCenterX, yPosition, LabelTextScaleSmall, "1");
            AddCenteredLabel(parentPanel, probabilityBarEndX - 10f, yPosition, LabelTextScaleSmall, "10");
            AddCenteredLabel(parentPanel, intensityBarStartX + 10f, yPosition, LabelTextScaleSmall, "0.0");
            AddCenteredLabel(parentPanel, intensityBarEndX - 13f, yPosition, LabelTextScaleSmall, "25.5");

            yPosition += 15f;

            int disasterCount = Services.DisasterHandler.container.AllDisasters.Count;
            _disasterRows = new DisasterRowHelper[disasterCount];

            for (int i = 0; i < disasterCount; i++)
            {
                DisasterBaseModel disaster = Services.DisasterHandler.container.AllDisasters[i];
                DisasterRowHelper disasterRow = parentPanel.AddUIComponent<DisasterRowHelper>();
                disasterRow.Initialize(disaster, xPosition, yPosition, ToggleDisasterState);
                disasterRow.Refresh();
                _disasterRows[i] = disasterRow;
                yPosition += itemSpacing;
            }

            yPosition += 10f;
            BuildActionButtons(parentPanel, xPosition, yPosition);
            parentPanel.autoLayout = false;
            parentPanel.autoSize = false;
        }

        private void BuildControlsTabContent(UIScrollablePanel parentPanel)
        {
            float yPosition = 10f;
            AddLabel(parentPanel, 0f, yPosition, LabelTextScaleNormal, LocalizationService.Get("panel.controls.title"));
            yPosition += 28f;
            AddWrappedLabel(parentPanel, 0f, yPosition, 360f, LocalizationService.Get("panel.controls.toggle"));
            yPosition += 42f;
            AddWrappedLabel(parentPanel, 0f, yPosition, 360f, LocalizationService.Get("panel.controls.stop"));
            yPosition += 42f;
            AddWrappedLabel(parentPanel, 0f, yPosition, 360f, LocalizationService.Get("panel.controls.reset"));
            yPosition += 42f;
            AddWrappedLabel(parentPanel, 0f, yPosition, 360f, LocalizationService.Get("panel.controls.drag"));
            yPosition += 52f;
            AddWrappedLabel(parentPanel, 0f, yPosition, 360f, LocalizationService.Get("panel.controls.hotkey"));
            parentPanel.autoLayout = false;
            parentPanel.autoSize = false;
        }

        private void BuildActionButtons(UIComponent parentPanel, float xPosition, float yPosition)
        {
            ActionButtonHelper.CreateTextButton(
                parentPanel,
                "stopDisasterBtn",
                "■",
                new Vector3(xPosition, yPosition - 5f),
                new Vector2(18f, 18f),
                LocalizationService.Get("panel.stop_all"),
                StopAllDisastersBtn_eventClick,
                Color.red,
                "ButtonMenu",
                "ButtonMenuHovered",
                "ButtonMenuHovered",
                null);

            ActionButtonHelper.CreateTextButton(
                parentPanel,
                "resetDisasterBtn",
                "↺",
                new Vector3(xPosition + 22f, yPosition - 5f),
                new Vector2(18f, 18f),
                LocalizationService.Get("panel.reset_all"),
                ResetAllDisastersBtn_eventClick,
                Color.yellow,
                "ButtonMenu",
                "ButtonMenuHovered",
                "ButtonMenuHovered",
                null);
        }

        private void UpdatePopulationLabel()
        {
            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";
            string formatted = ((int)Services.DisasterHandler.container.MaxPopulationToTrigguerHigherDisasters).ToString("#,0", nfi);
            _populationLabel.text = LocalizationService.Format("panel.population_threshold.value", formatted);
            _populationLabel.tooltip = LocalizationService.Get("panel.population_threshold");
        }

        private void ToggleDisasterState(DisasterBaseModel disaster)
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

            if (_disasterRows != null)
            {
                for (int i = 0; i < _disasterRows.Length; i++)
                {
                    _disasterRows[i].Refresh();
                }
            }

            SettingsScreen.UpdateUISettingsOptions();
        }

        private void HelpBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ShowHelpPanel();
        }

        private static void ShowHelpPanel()
        {
            UIPanel helpPanel = (UIPanel)UIView.GetAView().AddUIComponent(typeof(UIPanel));
            helpPanel.name = "NDRHelpPanel";
            helpPanel.backgroundSprite = "GenericPanel";
            helpPanel.color = new Color32(50, 50, 50, 250);
            helpPanel.size = new Vector2(400f, 300f);
            helpPanel.relativePosition = new Vector3(
                Mathf.Floor((UIView.GetAView().fixedWidth - 400f) / 2f),
                Mathf.Floor((UIView.GetAView().fixedHeight - 300f) / 2f));

            UILabel title = helpPanel.AddUIComponent<UILabel>();
            title.text = LocalizationService.Get("panel.help.title");
            title.textScale = 1.2f;
            title.relativePosition = new Vector3(10f, 10f);
            title.textColor = Color.white;

            UILabel content = helpPanel.AddUIComponent<UILabel>();
            content.text = LocalizationService.Get("panel.help.content");
            content.textColor = Color.white;
            content.wordWrap = true;
            content.autoSize = false;
            content.size = new Vector2(380f, 240f);
            content.relativePosition = new Vector3(10f, 40f);

            UIButton closeButton = helpPanel.AddUIComponent<UIButton>();
            closeButton.text = "X";
            closeButton.normalBgSprite = "ButtonMenu";
            closeButton.hoveredBgSprite = "ButtonMenuHovered";
            closeButton.size = new Vector2(30f, 30f);
            closeButton.textScale = 1.2f;
            closeButton.relativePosition = new Vector3(helpPanel.width - 35f, 5f);
            closeButton.eventClick += delegate { Destroy(helpPanel); };
        }

        private static void StopAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            StopAllDisasters();
        }

        private static void ResetAllDisastersBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            StopAllDisasters();

            for (int i = 0; i < Services.DisasterHandler.container.AllDisasters.Count; i++)
            {
                DisasterBaseModel disaster = Services.DisasterHandler.container.AllDisasters[i];
                disaster.calmDaysLeft = 0f;
                disaster.probabilityWarmupDaysLeft = 0f;
                disaster.intensityWarmupDaysLeft = 0f;
            }
        }

        private static void StopAllDisasters()
        {
            StringBuilder sb = new StringBuilder();
            VehicleManager vm = Services.Vehicles;
            for (int i = 1; i < 16384; i++)
            {
                if ((vm.m_vehicles.m_buffer[i].m_flags & Vehicle.Flags.Created) == 0) continue;
                if (vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is MeteorAI ||
                    vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is VortexAI)
                {
                    vm.ReleaseVehicle((ushort)i);
                }
            }

            WaterSimulation ws = Services.Water;
            if (ws != null)
            {
                for (int i = ws.m_waterWaves.m_size; i >= 1; i--)
                {
                    Services.Terrain.WaterSimulation.ReleaseWaterWave((ushort)i);
                }
            }

            DisasterManager dm = Services.Disasters;
            for (ushort i = 0; i < dm.m_disasterCount; i++)
            {
                sb.AppendLine(dm.m_disasters.m_buffer[i].Info.name + " flags: " + dm.m_disasters.m_buffer[i].m_flags);
                if ((dm.m_disasters.m_buffer[i].m_flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active | DisasterData.Flags.Clearing)) == DisasterData.Flags.None)
                    continue;
                if (!IsStoppableDisaster(dm.m_disasters.m_buffer[i].Info.m_disasterAI))
                    continue;

                dm.m_disasters.m_buffer[i].m_flags =
                    (dm.m_disasters.m_buffer[i].m_flags & ~(DisasterData.Flags.Emerging | DisasterData.Flags.Active | DisasterData.Flags.Clearing))
                    | DisasterData.Flags.Finished;
            }

            if (Services.DisasterHandler.container.activeDisasters != null)
            {
                Services.DisasterHandler.container.activeDisasters.Clear();
            }

            Debug.Log(sb.ToString());
        }

        private static bool IsStoppableDisaster(DisasterAI ai)
        {
            return ai as ThunderStormAI != null || ai as SinkholeAI != null || ai as TornadoAI != null ||
                   ai as EarthquakeAI != null || ai as MeteorStrikeAI != null || ai as ForestFireAI != null ||
                   ai as TsunamiAI != null;
        }

        private void ClosePanelBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }

        private static UILabel AddLabel(UIComponent parentPanel, float x, float y, float textScale, string text)
        {
            UILabel label = parentPanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(x, y);
            label.textScale = textScale;
            label.text = text;
            return label;
        }

        private static UILabel AddCenteredLabel(UIComponent parentPanel, float centerX, float y, float textScale, string text)
        {
            UILabel label = AddLabel(parentPanel, 0f, y, textScale, text);
            label.relativePosition = new Vector3(centerX - label.width * 0.5f, y);
            return label;
        }

        private static UILabel AddWrappedLabel(UIComponent parentPanel, float x, float y, float width, string text)
        {
            UILabel label = parentPanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(x, y);
            label.autoSize = false;
            label.wordWrap = true;
            label.width = width;
            label.textScale = LabelTextScaleNormal;
            label.text = text;
            return label;
        }
    }
}
