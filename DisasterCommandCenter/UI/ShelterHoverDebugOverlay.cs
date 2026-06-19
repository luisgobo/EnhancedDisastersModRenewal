using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.Development;
using UnityEngine;

namespace NaturalDisastersRenewal.UI
{
    public class ShelterHoverDebugOverlay : UIPanel
    {
        private const float VerticalOffset = 25f;
        private const float PaddingX = 8f;
        private const float PaddingY = 5f;

        private static readonly FieldInfo HoverInstanceField =
            typeof(DefaultTool).GetField("m_hoverInstance", BindingFlags.Instance | BindingFlags.NonPublic);

        private ShelterFloodQueryService _floodQueryService;
        private UILabel _label;
        private UIView _view;

        public override void Awake()
        {
            base.Awake();

            _view = UIView.GetAView();
            _floodQueryService = new ShelterFloodQueryService();
            backgroundSprite = "GenericPanel";
            color = new Color32(40, 40, 40, 230);
            isInteractive = false;
            isVisible = false;
            zOrder = 1000;

            _label = AddUIComponent<UILabel>();
            _label.text = "Hello World";
            _label.textScale = 0.85f;
            _label.textColor = Color.white;
            _label.relativePosition = new Vector3(PaddingX, PaddingY);

            width = _label.width + PaddingX * 2f;
            height = _label.height + PaddingY * 2f;
        }

        public override void Update()
        {
            base.Update();

            if (!IsDevelopmentModeEnabled())
            {
                isVisible = false;
                return;
            }

            ushort shelterId;
            if (!TryGetHoveredShelter(out shelterId))
            {
                isVisible = false;
                return;
            }

            var building = Services.Buildings.m_buildings.m_buffer[shelterId];
            UpdateLabelText(shelterId, ref building); //

            var worldPosition = building.m_position + new Vector3(0f, VerticalOffset, 0f);

            var camera = Camera.main;
            if (camera == null || camera.WorldToScreenPoint(worldPosition).z <= 0f)
            {
                isVisible = false;
                return;
            }

            var guiPosition = _view.WorldPointToGUI(camera, worldPosition);
            absolutePosition = new Vector3(guiPosition.x - width * 0.5f, guiPosition.y - height, 0f);
            isVisible = true;
        }

        private void UpdateLabelText(ushort shelterId, ref Building building)
        {
            ushort segmentId;
            float waterDepth;
            bool isFlooded;

            if (!_floodQueryService.TryGetShelterStreetFloodState(ref building, out segmentId, out waterDepth, out isFlooded))
                _label.text = string.Format("Shelter {0}\nCalle: desconocida", shelterId);
            else
                _label.text = string.Format(
                    "Shelter {0}\nCalle: {1}\nSegmento: {2}\nAgua: {3:0.00}",
                    shelterId,
                    isFlooded ? "inundada" : "seca",
                    segmentId,
                    waterDepth);

            string meteorInfo;
            if (_floodQueryService.TryGetNearbyMeteorWaterImpactInfo(shelterId, building.m_position, waterDepth, out meteorInfo))
                _label.text += "\n" + meteorInfo;

            width = _label.width + PaddingX * 2f;
            height = _label.height + PaddingY * 2f;
        }

        private static bool TryGetHoveredShelter(out ushort shelterId)
        {
            shelterId = 0;

            var toolManager = Singleton<ToolManager>.instance;
            if (toolManager == null || toolManager.m_properties == null)
                return false;

            var defaultTool = toolManager.m_properties.CurrentTool as DefaultTool;
            if (defaultTool == null || HoverInstanceField == null)
                return false;

            var hoverInstance = (InstanceID)HoverInstanceField.GetValue(defaultTool);
            shelterId = hoverInstance.Building;
            if (shelterId == 0)
                return false;

            var building = Services.Buildings.m_buildings.m_buffer[shelterId];
            return building.Info != null && building.Info.m_buildingAI as ShelterAI != null;
        }

        private static bool IsDevelopmentModeEnabled()
        {
            return DevelopmentProperties.IsDevelopmentMode;
        }
    }
}