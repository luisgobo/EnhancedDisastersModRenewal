using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Models.Disaster;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using NaturalDisastersRenewal.Models.Setup;
using NaturalDisastersRenewal.UI;
using System;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace NaturalDisastersRenewal.Handlers
{
    public class NaturalDisasterHandler : Singleton<NaturalDisasterHandler>
    {
        public DisasterSetupModel container;
        ExtendedDisastersPanel dPanel;
        UIButton toggleButton;
        readonly Harmony harmony = new Harmony(CommonProperties.ModNameForHarmony);
        DisasterWrapper disasterWrapper;

        NaturalDisasterHandler()
        {
            ReadValuesFromFile();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void ReadValuesFromFile()
        {
            DisasterSetupModel newContainer = DisasterSetupModel.CreateFromFile() ?? new DisasterSetupModel();
            newContainer.CheckObjects();

            CopySettings(newContainer);
        }

        public void ResetToDefaultValues()
        {
            DisasterSetupModel newContainer = new DisasterSetupModel();
            newContainer.CheckObjects();
            CopySettings(newContainer);
        }

        public void ResetToDefaultValues(bool resetButtonPos, bool resetPanelPos)
        {
            DisasterSetupModel newContainer = new DisasterSetupModel();
            newContainer.CheckObjects();

            if (resetButtonPos || resetPanelPos)
                ResetInterfaceElementPosition(newContainer, resetButtonPos, resetPanelPos);
            else
                CopySettings(newContainer);
        }

        public void RedefineDisasterMaxIntensity()
        {
            var optionPanel = UnityObject.FindObjectOfType<DisastersOptionPanel>();
            var slider = optionPanel.GetComponentInChildren<UISlider>();
            slider.maxValue = byte.MaxValue;
            slider.minValue = byte.MinValue;
        }

        void CopySettings(DisasterSetupModel fromContainer)
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

                container.DisableDisasterFocus = fromContainer.DisableDisasterFocus;
                container.PauseOnDisasterStarts = fromContainer.PauseOnDisasterStarts;
                container.PartialEvacuationRadius = fromContainer.PartialEvacuationRadius;

                container.ScaleMaxIntensityWithPopulation = fromContainer.ScaleMaxIntensityWithPopulation;
                container.RecordDisasterEvents = fromContainer.RecordDisasterEvents;
                container.ShowDisasterPanelButton = fromContainer.ShowDisasterPanelButton;
            }
        }

        void ResetInterfaceElementPosition(DisasterSetupModel fromContainer, bool resetButtonPos = false, bool resetPanelPos = false)
        {
            if (container == null)
            {
                container = fromContainer;
            }
            else
            {
                if (resetButtonPos)
                {
                    toggleButton.absolutePosition = new Vector3(90, 62);
                    container.ToggleButtonPos = new Vector3(90, 62);
                }

                if (resetPanelPos)
                {
                    dPanel.absolutePosition = new Vector3(90, 100);
                    container.ToggleButtonPos = new Vector3(90, 100);
                }
            }
        }

        public void OnSimulationFrame()
        {
            CheckUnlocks();

            foreach (DisasterBaseModel ed in container.AllDisasters)
            {
                ed.OnSimulationFrame();
            }
        }

        public void OnCreated(IDisaster disasters)
        {
            disasterWrapper = (DisasterWrapper)disasters;
        }

        public void OnDisasterStarted(DisasterAI disasterAI, byte intensity)
        {
            foreach (DisasterBaseModel ed in container.AllDisasters)
            {
                if (ed.CheckDisasterAIType(disasterAI))
                {
                    ed.OnDisasterStarted(intensity);
                    return;
                }
            }
        }

        public void OnDisasterActivated(DisasterAI disasterAI, ushort disasterId)
        {
            var disasterInfo = disasterWrapper.GetDisasterSettings(disasterId);
            var msg = $"EvacuationService.OnDisasterActivated. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
            DebugLogger.Log(msg);

            foreach (DisasterBaseModel ed in container.AllDisasters)
            {
                if (ed.CheckDisasterAIType(disasterAI))
                {
                    ed.OnDisasterActivated(disasterInfo, disasterId, ref container.activeDisasters);
                    return;
                }
            }
        }

        public void OnDisasterDeactivated(DisasterAI disasterAI, ushort disasterId)
        {
            try
            {
                var disasterInfo = disasterWrapper.GetDisasterSettings(disasterId);

                var msg = $"EvacuationService.OnDisasterDeactivated. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
                DebugLogger.Log(msg);

                foreach (DisasterBaseModel ed in container.AllDisasters)
                {
                    if (ed.CheckDisasterAIType(disasterAI))
                    {
                        ed.OnDisasterDeactivated(new DisasterInfoModel()
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        }, ref container.activeDisasters);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log(ex.ToString());

                throw;
            }
        }

        public void OnDisasterDetected(DisasterAI disasterAI, ushort disasterId)
        {
            try
            {

                foreach (DisasterBaseModel disasterService in container.AllDisasters)
                {
                    if (disasterService.CheckDisasterAIType(disasterAI))
                    {

                        var disasterInfo = disasterWrapper.GetDisasterSettings(disasterId);
                        var msg = $"EvacuationService.OnDisasterDetected. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
                        DebugLogger.Log(msg);
                        DisasterInfoModel disasterInfoUnified = new DisasterInfoModel()
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        };

                        disasterService.OnDisasterDetected(disasterInfoUnified, ref container.activeDisasters);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log(ex.ToString());
                throw;
            }
        }

        public void OnDisasterFinished(DisasterAI disasterAI, ushort disasterId)
        {
            try
            {
                foreach (DisasterBaseModel disasterService in container.AllDisasters)
                {
                    if (disasterService.CheckDisasterAIType(disasterAI))
                    {
                        var disasterInfo = disasterWrapper.GetDisasterSettings(disasterId);
                        var msg = $"EvacuationService.OnDisasterFinished. Id: {disasterId}, " +
                            $"Name: {disasterInfo.name}, " +
                            $"Type: {disasterInfo.type}, " +
                            $"Intensity: {disasterInfo.intensity}";
                        DebugLogger.Log(msg);

                        DisasterInfoModel disasterInfoUnified = new DisasterInfoModel()
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        };

                        disasterService.OnDisasterFinished(disasterInfoUnified, ref container.activeDisasters);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log(ex.ToString());
                throw;
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
            if (dPanel != null) 
                return;

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
            toggleButton.normalBgSprite = "ToolbarIconZoomOutGlobeHovered";
            toggleButton.normalFgSprite = "IconPolicyPowerSavingDisabled";
            toggleButton.hoveredFgSprite = "IconPolicyPowerSavingPressed"; 
            toggleButton.width = 38f;
            toggleButton.height = 38f;
            toggleButton.absolutePosition = new Vector3(90, 62);
            toggleButton.tooltip = "Extended Disasters (drag by right-click)";
            toggleButton.isVisible = container.ShowDisasterPanelButton;
            toggleButton.eventClick += ToggleButton_eventClick;
            toggleButton.eventMouseMove += ToggleButton_eventMouseMove;

            dPanel.tooltip = "Drag by right-click to set the panel position.";
            dPanel.eventMouseMove += DPanel_eventMouseMove;

            UpdateDisastersPanelToggleBtn();
            UpdateDisastersDPanel();

            UIInput.eventProcessKeyEvent += UIInput_eventProcessKeyEvent;
        }

        void ToggleDisasterPanel()
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

        public void UpdateDisastersDPanel()
        {
            if (dPanel != null && container != null)
            {
                if (container.DPanelPos.x > 10 && container.DPanelPos.y > 10)
                {
                    dPanel.absolutePosition = container.DPanelPos;
                }
            }
        }

        public DisasterWrapper GetDisasterWrapper()
        {
            return disasterWrapper;
        }
        void ToggleButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ToggleDisasterPanel();
        }

        void ToggleButton_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (eventParam.buttons.IsFlagSet(UIMouseButton.Right))
            {
                var ratio = UIView.GetAView().ratio;
                toggleButton.position = SetUIItemPosition(toggleButton.position, eventParam.moveDelta.x, eventParam.moveDelta.y, ratio);
                container.ToggleButtonPos = toggleButton.absolutePosition;
            }
        }
        void DPanel_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (eventParam.buttons.IsFlagSet(UIMouseButton.Right))
            {
                var ratio = UIView.GetAView().ratio;
                dPanel.position = SetUIItemPosition(dPanel.position, eventParam.moveDelta.x, eventParam.moveDelta.y, ratio);
                container.DPanelPos = dPanel.absolutePosition;
            }
        }

        private Vector3 SetUIItemPosition(Vector3 currentPosition, float x, float y, float ratio)
        {
            return new Vector3(
                    currentPosition.x + (x * ratio),
                    currentPosition.y + (y * ratio),
                    currentPosition.z);
        }

        void UIInput_eventProcessKeyEvent(EventType eventType, KeyCode keyCode, EventModifiers modifiers)
        {
            //Hide Panel when main menu is triggered
            if (eventType == EventType.KeyDown && keyCode == KeyCode.Escape)
            {
                dPanel.isVisible = false;
                return;
            }

            //Show / Hide Panel hotkey
            if (eventType == EventType.KeyDown && modifiers == EventModifiers.Shift && keyCode == KeyCode.D)
            {
                ToggleDisasterPanel();
            }
        }
    }
}