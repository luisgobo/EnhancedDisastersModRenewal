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
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace NaturalDisastersRenewal.Handlers
{
    public class NaturalDisasterHandler : Singleton<NaturalDisasterHandler>
    {
        public DisasterSetupModel container;
        List<DisasterInfoModel> activeDisasters = new List<DisasterInfoModel>();
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
            DisasterSetupModel newContainer = DisasterSetupModel.CreateFromFile();
            if (newContainer == null)
            {
                newContainer = new DisasterSetupModel();
            }

            newContainer.CheckObjects();

            CopySettings(newContainer);
        }

        public void ResetToDefaultValues()
        {
            DisasterSetupModel newContainer = new DisasterSetupModel();
            newContainer.CheckObjects();
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

        public void OnDisasterStarted(DisasterAI dai, byte intensity)
        {
            foreach (DisasterBaseModel ed in container.AllDisasters)
            {
                if (ed.CheckDisasterAIType(dai))
                {
                    ed.OnDisasterStarted(intensity);
                    return;
                }
            }
        }

        public void OnDisasterActivated(DisasterAI dai, ushort disasterId)
        {
            var disasterInfo = disasterWrapper.GetDisasterSettings(disasterId);
            var msg = $"EvacuationService.OnDisasterDeactivated. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
            DebugLogger.Log(msg);

            foreach (DisasterBaseModel ed in container.AllDisasters)
            {
                if (ed.CheckDisasterAIType(dai))
                {
                    ed.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
                    return;
                }
            }
        }

        public void OnDisasterDeactivated(DisasterAI dai, ushort disasterId)
        {
            try
            {
                var disasterInfo = disasterWrapper.GetDisasterSettings(disasterId);

                var msg = $"EvacuationService.OnDisasterDeactivated. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
                DebugLogger.Log(msg);

                foreach (DisasterBaseModel ed in container.AllDisasters)
                {
                    if (ed.CheckDisasterAIType(dai))
                    {
                        ed.OnDisasterDeactivated(new DisasterInfoModel()
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        }, ref activeDisasters);
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

                        var msg = $"disasterInfo1: type: {disasterInfo.type}, name:{disasterInfo.name}, " +
                                  $"location => x:{disasterInfo.targetX} y:{disasterInfo.targetX} z:{disasterInfo.targetZ}. " +
                                  $"Angle: {disasterInfo.angle}, intensity: {disasterInfo.intensity} ";
                        DebugLogger.Log(msg);

                        DisasterInfoModel disasterInfoUnified = new DisasterInfoModel()
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        };

                        disasterService.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
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

            dPanel.tooltip = "Drag by right-click to set the panel position.";
            dPanel.eventMouseMove += DPanel_eventMouseMove;

            UpdateDisastersPanelToggleBtn();
            UpdateDisastersDPanel();

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

        void ToggleButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ToggleDisasterPanel();
        }

        void DPanel_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (eventParam.buttons.IsFlagSet(UIMouseButton.Right))
            {
                var ratio = UIView.GetAView().ratio;
                dPanel.position = new Vector3(
                    dPanel.position.x + (eventParam.moveDelta.x * ratio),
                    dPanel.position.y + (eventParam.moveDelta.y * ratio),
                    dPanel.position.z);

                container.DPanelPos = dPanel.absolutePosition;
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
                ToggleDisasterPanel();
            }
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
    }
}