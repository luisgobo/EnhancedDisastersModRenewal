using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using NaturalDisastersRenewal.Common;
using UnityEngine;

namespace NaturalDisastersRenewal.UI
{
    public class ShelterHoverDebugOverlay : UIPanel
    {
        private const float VerticalOffset = 25f;
        private const float PaddingX = 8f;
        private const float PaddingY = 5f;
        private const float FloodedStreetDepth = 0.25f;
        private static readonly FieldInfo HoverInstanceField =
            typeof(DefaultTool).GetField("m_hoverInstance", BindingFlags.Instance | BindingFlags.NonPublic);

        private UILabel _label;
        private UIView _view;

        public override void Awake()
        {
            base.Awake();

            _view = UIView.GetAView();
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
            UpdateLabelText(shelterId, ref building);

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

            if (!TryGetShelterStreetFloodState(ref building, out segmentId, out waterDepth, out isFlooded))
            {
                _label.text = string.Format("Shelter {0}\nCalle: desconocida", shelterId);
            }
            else
            {
                _label.text = string.Format(
                    "Shelter {0}\nCalle: {1}\nSegmento: {2}\nAgua: {3:0.00}",
                    shelterId,
                    isFlooded ? "inundada" : "seca",
                    segmentId,
                    waterDepth);
            }

            width = _label.width + PaddingX * 2f;
            height = _label.height + PaddingY * 2f;
        }

        private static bool TryGetShelterStreetFloodState(ref Building building, out ushort segmentId,
            out float waterDepth, out bool isFlooded)
        {
            segmentId = building.m_accessSegment;
            waterDepth = 0f;
            isFlooded = false;

            if (segmentId == 0)
                return false;

            var netManager = Singleton<NetManager>.instance;
            if (netManager == null)
                return false;

            var segment = netManager.m_segments.m_buffer[segmentId];
            if ((segment.m_flags & NetSegment.Flags.Created) == 0)
                return false;

            var streetPosition = GetClosestPointOnSegment(netManager, ref segment, building.m_position);
            waterDepth = GetWaterDepthAt(streetPosition);
            isFlooded = (segment.m_flags & NetSegment.Flags.Flooded) != 0 || waterDepth >= FloodedStreetDepth;
            return true;
        }

        private static Vector3 GetClosestPointOnSegment(NetManager netManager, ref NetSegment segment, Vector3 position)
        {
            var startPosition = netManager.m_nodes.m_buffer[segment.m_startNode].m_position;
            var endPosition = netManager.m_nodes.m_buffer[segment.m_endNode].m_position;
            var segmentVector = endPosition - startPosition;
            var segmentLengthSqr = segmentVector.sqrMagnitude;

            if (segmentLengthSqr <= 0.01f)
                return segment.m_middlePosition;

            var t = Mathf.Clamp01(Vector3.Dot(position - startPosition, segmentVector) / segmentLengthSqr);
            return startPosition + segmentVector * t;
        }

        private static float GetWaterDepthAt(Vector3 position)
        {
            var terrain = Services.Terrain;
            if (terrain == null)
                return 0f;

            var terrainHeight = terrain.SampleRawHeightSmooth(position);
            var waterSurfaceHeight = terrain.SampleRawHeightSmoothWithWater(position, false, 0f);
            return Mathf.Max(0f, waterSurfaceHeight - terrainHeight);
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
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
