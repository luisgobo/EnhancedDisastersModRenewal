using System.Reflection;
using UnityEngine;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;

namespace NaturalDisastersOverhaulRenewal
{
    public class EnhancedDisastersManager : Singleton<EnhancedDisastersManager>
    {
        public DisastersContainer container;
        ExtendedDisastersPanel dPanel;
        UIButton toggleButton;
        readonly Harmony harmony = new Harmony("EnhancedDisastersManagerRenewal");

        EnhancedDisastersManager()
        {
            ReadValuesFromFile();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void ReadValuesFromFile()
        {
            DisastersContainer newContainer = DisastersContainer.CreateFromFile();
            if (newContainer == null)
            {
                newContainer = new DisastersContainer();
            }

            newContainer.CheckObjects();

            copySettings(newContainer);
        }

        public void ResetToDefaultValues()
        {
            DisastersContainer newContainer = new DisastersContainer();
            newContainer.CheckObjects();

            copySettings(newContainer);
        }

        void copySettings(DisastersContainer fromContainer)
        {
            if (container == null)
            {
                container = fromContainer;
            }
            else
            {
                for (int i = 0; i < container.AllDisasters.Count; i++)
                {
                    container.AllDisasters[i].CopySettings(fromContainer.AllDisasters[i]);
                }
            }
        }

        public void OnSimulationFrame()
        {
            CheckUnlocks();

            foreach (EnhancedDisaster ed in container.AllDisasters)
            {
                ed.OnSimulationFrame();
            }
        }

        public void OnDisasterStarted(DisasterAI dai, byte intensity)
        {
            foreach (EnhancedDisaster ed in container.AllDisasters)
            {
                if (ed.CheckDisasterAIType(dai))
                {
                    ed.OnDisasterStarted(intensity);
                    return;
                }
            }
        }

        public static DisasterInfo GetDisasterInfo(DisasterType disasterType)
        {
            int prefabCount = PrefabCollection<DisasterInfo>.PrefabCount();

            for (int i = 0; i < prefabCount; i++)
            {
                DisasterInfo disasterInfo = PrefabCollection<DisasterInfo>.GetPrefab((uint)i);
                if (disasterInfo != null)
                {
                    switch (disasterType)
                    {
                        case DisasterType.Earthquake:
                            if (disasterInfo.m_disasterAI as EarthquakeAI != null) return disasterInfo;
                            break;
                        case DisasterType.ForestFire:
                            if (disasterInfo.m_disasterAI as ForestFireAI != null) return disasterInfo;
                            break;
                        case DisasterType.MeteorStrike:
                            if (disasterInfo.m_disasterAI as MeteorStrikeAI != null) return disasterInfo;
                            break;
                        case DisasterType.ThunderStorm:
                            if (disasterInfo.m_disasterAI as ThunderStormAI != null) return disasterInfo;
                            break;
                        case DisasterType.Tornado:
                            if (disasterInfo.m_disasterAI as TornadoAI != null) return disasterInfo;
                            break;
                        case DisasterType.Tsunami:
                            if (disasterInfo.m_disasterAI as TsunamiAI != null) return disasterInfo;
                            break;
                        case DisasterType.StructureCollapse:
                            if (disasterInfo.m_disasterAI as StructureCollapseAI != null) return disasterInfo;
                            break;
                        case DisasterType.StructureFire:
                            if (disasterInfo.m_disasterAI as StructureFireAI != null) return disasterInfo;
                            break;
                        case DisasterType.Sinkhole:
                            if (disasterInfo.m_disasterAI as SinkholeAI != null) return disasterInfo;
                            break;
                    }
                }
            }

            return null;
        }

        public void CheckUnlocks()
        {
            int milestoneNum = 99; // Unlock all disasters in case of error

            MilestoneInfo mi = Singleton<UnlockManager>.instance.GetCurrentMilestone();
            if (mi != null)
            {
                int.TryParse(mi.name.Substring(9), out milestoneNum);
            }

            if (milestoneNum >= 3) container.ForestFire.Unlock();
            if (milestoneNum >= 3) container.Thunderstorm.Unlock();
            if (milestoneNum >= 4) container.Sinkhole.Unlock();
            if (milestoneNum >= 5) container.Tsunami.Unlock();
            if (milestoneNum >= 5) container.Tornado.Unlock();
            if (milestoneNum >= 6) container.Earthquake.Unlock();
            if (milestoneNum >= 6) container.MeteorStrike.Unlock();
        }

        public void CreateExtendedDisasterPanel()
        {
            if (dPanel != null) return;

            UIView v = UIView.GetAView();

            GameObject obj = new GameObject("ExtendedDisastersPanel");
            obj.transform.parent = v.cachedTransform;
            dPanel = obj.AddComponent<ExtendedDisastersPanel>();
            dPanel.absolutePosition = new Vector3(90, 100);

            GameObject toggleButtonObject = new GameObject("ExtendedDisastersPanelButton");
            toggleButtonObject.transform.parent = v.transform;
            toggleButtonObject.transform.localPosition = Vector3.zero;
            toggleButton = toggleButtonObject.AddComponent<UIButton>();
            toggleButton.name = "ExtendedDisastersPanelToggleButton";
            toggleButton.normalBgSprite = "InfoIconBasePressed";
            toggleButton.normalFgSprite = "InfoIconElectricity";
            toggleButton.width = 30f;
            toggleButton.height = 30f;
            toggleButton.absolutePosition = new Vector3(90, 62);
            toggleButton.tooltip = "Extended Disasters (drag by right-click)";
            toggleButton.eventClick += ToggleButton_eventClick;
            toggleButton.isVisible = container.ShowDisasterPanelButton;
            toggleButton.eventMouseMove += ToggleButton_eventMouseMove;

            UpdateDisastersPanelToggleBtn();

            UIInput.eventProcessKeyEvent += UIInput_eventProcessKeyEvent;
        }

        void ToggleButton_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (eventParam.buttons.IsFlagSet(UIMouseButton.Right))
            {
                var ratio = UIView.GetAView().ratio;
                toggleButton.position = new Vector3(
                    toggleButton.position.x + (eventParam.moveDelta.x * ratio),
                    toggleButton.position.y + (eventParam.moveDelta.y * ratio),
                    toggleButton.position.z);

                container.ToggleButtonPos = toggleButton.absolutePosition;
            }
        }

        void UIInput_eventProcessKeyEvent(EventType eventType, KeyCode keyCode, EventModifiers modifiers)
        {
            if (eventType == EventType.KeyDown && keyCode == KeyCode.Escape)
            {
                dPanel.isVisible = false;
                return;
            }

            if (eventType == EventType.KeyDown && modifiers == EventModifiers.Shift && keyCode == KeyCode.D)
            {
                toggleDisasterPanel();
            }
        }

        void ToggleButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            toggleDisasterPanel();
        }

        void toggleDisasterPanel()
        {
            dPanel.isVisible = !dPanel.isVisible;

            if (dPanel.isVisible)
            {
                dPanel.Counter = 0;
            }
        }

        public void UpdateDisastersPanelToggleBtn()
        {
            if (toggleButton != null && container != null)
            {
                toggleButton.isVisible = container.ShowDisasterPanelButton;

                if (container.ToggleButtonPos.x > 10 && container.ToggleButtonPos.y > 10)
                {
                    toggleButton.absolutePosition = container.ToggleButtonPos;
                }
            }
        }
    }
}
