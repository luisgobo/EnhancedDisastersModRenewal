﻿using System;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Models.Disaster;
using NaturalDisastersRenewal.Models.Setup;
using NaturalDisastersRenewal.UI;
using NaturalDisastersRenewal.UI.ComponentHelper;
using UnityEngine;

namespace NaturalDisastersRenewal.Handlers
{
    public class NaturalDisasterHandler : Singleton<NaturalDisasterHandler>
    {
        private readonly Harmony harmony = new Harmony(CommonProperties.modNameForHarmony);
        public DisasterSetupModel container;
        private DisasterWrapper disasterWrapper;
        private ExtendedDisastersPanel dPanel;
        private bool keyHandlerRegistered;
        private UIButton toggleButton;

        private NaturalDisasterHandler()
        {
            ReadValuesFromFile();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public DisasterSetupModel Container => container;

        public void ReadValuesFromFile()
        {
            var newContainer = DisasterSetupModel.CreateFromFile() ?? new DisasterSetupModel();
            newContainer.CheckObjects();

            CopySettings(newContainer);
        }

        public void ResetToDefaultValues()
        {
            var newContainer = new DisasterSetupModel();
            newContainer.CheckObjects();
            CopySettings(newContainer);
        }

        public void ResetToDefaultValues(bool resetButtonPos, bool resetPanelPos)
        {
            var newContainer = new DisasterSetupModel();
            newContainer.CheckObjects();

            if (resetButtonPos || resetPanelPos)
                ResetInterfaceElementPosition(newContainer, resetButtonPos, resetPanelPos);
            else
                CopySettings(newContainer);
        }

        public void RedefineDisasterMaxIntensity()
        {
            var optionPanel = FindObjectOfType<DisastersOptionPanel>();
            var slider = optionPanel.GetComponentInChildren<UISlider>();
            slider.maxValue = byte.MaxValue;
            slider.minValue = byte.MinValue;
        }

        private void CopySettings(DisasterSetupModel fromContainer)
        {
            if (container == null)
            {
                container = fromContainer;
            }
            else
            {
                for (var i = 0; i < container.AllDisasters.Count; i++)
                    container.AllDisasters[i].CopySettings(fromContainer.AllDisasters[i]);

                container.DisableDisasterFocus = fromContainer.DisableDisasterFocus;
                container.PauseOnDisasterStarts = fromContainer.PauseOnDisasterStarts;
                container.PartialEvacuationRadius = fromContainer.PartialEvacuationRadius;
                container.MaxPopulationToTriggerHigherDisasters = fromContainer.MaxPopulationToTriggerHigherDisasters;

                container.ScaleMaxIntensityWithPopulation = fromContainer.ScaleMaxIntensityWithPopulation;
                container.RecordDisasterEvents = fromContainer.RecordDisasterEvents;
                container.ShowDisasterPanelButton = fromContainer.ShowDisasterPanelButton;
                container.Language = fromContainer.Language;
                container.TogglePanelHotkey = fromContainer.TogglePanelHotkey;
                container.TogglePanelHotkeyModifiers = fromContainer.TogglePanelHotkeyModifiers;
                container.ToggleButtonPos = fromContainer.ToggleButtonPos;
                container.DPanelPos = fromContainer.DPanelPos;
            }
        }

        private void ResetInterfaceElementPosition(DisasterSetupModel fromContainer, bool resetButtonPos = false,
            bool resetPanelPos = false)
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
                    container.DPanelPos = new Vector3(90, 100);
                }
            }
        }

        public void OnSimulationFrame()
        {
            CheckUnlocks();

            foreach (var ed in container.AllDisasters) ed.OnSimulationFrame();
        }

        public void OnCreated(IDisaster disasters)
        {
            disasterWrapper = (DisasterWrapper)disasters;
        }

        public void OnDisasterStarted(DisasterAI disasterAI, byte intensity)
        {
            foreach (var ed in container.AllDisasters)
                if (ed.CheckDisasterAIType(disasterAI))
                {
                    ed.OnDisasterStarted(intensity);
                    return;
                }
        }

        public void OnDisasterActivated(DisasterAI disasterAI, ushort disasterId)
        {
            var disasterInfo = disasterWrapper.GetDisasterSettings(disasterId);
            var msg =
                $"EvacuationService.OnDisasterActivated. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
            DebugLogger.Log(msg);

            foreach (var ed in container.AllDisasters)
                if (ed.CheckDisasterAIType(disasterAI))
                {
                    ed.OnDisasterActivated(disasterInfo, disasterId, ref container.activeDisasters);
                    return;
                }
        }

        public void OnDisasterDeactivated(DisasterAI disasterAI, ushort disasterId)
        {
            try
            {
                var disasterInfo = disasterWrapper.GetDisasterSettings(disasterId);

                var msg =
                    $"EvacuationService.OnDisasterDeactivated. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
                DebugLogger.Log(msg);

                foreach (var ed in container.AllDisasters
                             .Where(ed => ed.CheckDisasterAIType(disasterAI)))
                {
                    ed.OnDisasterDeactivated(new DisasterInfoModel
                    {
                        DisasterInfo = disasterInfo,
                        DisasterId = disasterId
                    }, ref container.activeDisasters);
                    return;
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
                foreach (var disasterService in container.AllDisasters)
                    if (disasterService.CheckDisasterAIType(disasterAI))
                    {
                        var disasterInfo = disasterWrapper.GetDisasterSettings(disasterId);
                        var msg =
                            $"EvacuationService.OnDisasterDetected. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
                        DebugLogger.Log(msg);
                        var disasterInfoUnified = new DisasterInfoModel
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        };

                        disasterService.OnDisasterDetected(disasterInfoUnified, ref container.activeDisasters);
                        return;
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
                foreach (var disasterService in container.AllDisasters)
                    if (disasterService.CheckDisasterAIType(disasterAI))
                    {
                        var disasterInfo = disasterWrapper.GetDisasterSettings(disasterId);
                        var msg = $"EvacuationService.OnDisasterFinished. Id: {disasterId}, " +
                                  $"Name: {disasterInfo.name}, " +
                                  $"Type: {disasterInfo.type}, " +
                                  $"Intensity: {disasterInfo.intensity}";
                        DebugLogger.Log(msg);

                        var disasterInfoUnified = new DisasterInfoModel
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        };

                        disasterService.OnDisasterFinished(disasterInfoUnified, ref container.activeDisasters);
                        return;
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
            var prefabCount = PrefabCollection<DisasterInfo>.PrefabCount();

            for (var i = 0; i < prefabCount; i++)
            {
                var disasterInfo = PrefabCollection<DisasterInfo>.GetPrefab((uint)i);
                if (disasterInfo != null && MatchesDisasterType(disasterType, disasterInfo.m_disasterAI))
                    return disasterInfo;
            }

            return null;
        }

        private static bool MatchesDisasterType(DisasterType disasterType, DisasterAI disasterAI)
        {
            switch (disasterType)
            {
                case DisasterType.Earthquake:
                    return disasterAI is EarthquakeAI;
                case DisasterType.ForestFire:
                    return disasterAI is ForestFireAI;
                case DisasterType.MeteorStrike:
                    return disasterAI is MeteorStrikeAI;
                case DisasterType.ThunderStorm:
                    return disasterAI is ThunderStormAI;
                case DisasterType.Tornado:
                    return disasterAI is TornadoAI;
                case DisasterType.Tsunami:
                    return disasterAI is TsunamiAI;
                case DisasterType.StructureCollapse:
                    return disasterAI is StructureCollapseAI;
                case DisasterType.StructureFire:
                    return disasterAI is StructureFireAI;
                case DisasterType.Sinkhole:
                    return disasterAI is SinkholeAI;
                default:
                    return false;
            }
        }

        public void CheckUnlocks()
        {
            var milestoneNum = 99; // Unlock all disasters in case of error

            var mi = Services.Unlocks.GetCurrentMilestone();
            if (mi != null) int.TryParse(mi.name.Substring(9), out milestoneNum);

            UnlockDisasterWhenReached(milestoneNum, 3, container.ForestFire.Unlock);
            UnlockDisasterWhenReached(milestoneNum, 3, container.Thunderstorm.Unlock);
            UnlockDisasterWhenReached(milestoneNum, 4, container.Sinkhole.Unlock);
            UnlockDisasterWhenReached(milestoneNum, 5, container.Tsunami.Unlock);
            UnlockDisasterWhenReached(milestoneNum, 5, container.Tornado.Unlock);
            UnlockDisasterWhenReached(milestoneNum, 6, container.Earthquake.Unlock);
            UnlockDisasterWhenReached(milestoneNum, 6, container.MeteorStrike.Unlock);
        }

        private static void UnlockDisasterWhenReached(int milestoneNum, int requiredMilestone, Action unlockAction)
        {
            if (milestoneNum >= requiredMilestone)
                unlockAction();
        }

        public void CreateExtendedDisasterPanel()
        {
            if (dPanel != null)
                return;

            var v = UIView.GetAView();

            var obj = new GameObject("ExtendedDisastersPanel");
            obj.transform.parent = v.cachedTransform;
            dPanel = obj.AddComponent<ExtendedDisastersPanel>();
            dPanel.absolutePosition = new Vector3(90, 100);

            var toggleButtonObject = new GameObject("ExtendedDisastersPanelButton");
            toggleButtonObject.transform.parent = v.transform;
            toggleButtonObject.transform.localPosition = Vector3.zero;
            toggleButton = toggleButtonObject.AddComponent<UIButton>();
            toggleButton.name = "ExtendedDisastersPanelToggleButton";
            ApplyDefaultToggleButtonSprites();
            toggleButton.width = 48f;
            toggleButton.height = 48f;
            toggleButton.absolutePosition = new Vector3(90, 62);
            toggleButton.tooltip = LocalizationService.Get("panel.toggle_button.tooltip");
            toggleButton.isVisible = container.ShowDisasterPanelButton;
            toggleButton.eventClick += ToggleButton_eventClick;
            toggleButton.eventMouseMove += ToggleButton_eventMouseMove;

            dPanel.tooltip = LocalizationService.Get("panel.drag_panel.tooltip");
            dPanel.eventMouseMove += DPanel_eventMouseMove;

            UpdateDisastersPanelToggleBtn();
            UpdateDisastersDPanel();

            if (keyHandlerRegistered) return;
            
            UIInput.eventProcessKeyEvent += UIInput_eventProcessKeyEvent;
            keyHandlerRegistered = true;
        }

        private void ToggleDisasterPanel()
        {
            dPanel.isVisible = !dPanel.isVisible;
            UpdateToggleButtonIcon();

            if (dPanel.isVisible) dPanel.counter = 0;
        }

        public void UpdateDisastersPanelToggleBtn()
        {
            if (toggleButton == null || container == null) return;
            
            toggleButton.isVisible = container.ShowDisasterPanelButton;
            UpdateToggleButtonIcon();

            if (container.ToggleButtonPos.x > 10 && container.ToggleButtonPos.y > 10)
                toggleButton.absolutePosition = container.ToggleButtonPos;
        }

        public void UpdateDisastersDPanel()
        {
            if (dPanel == null || container == null) return;
            
            if (container.DPanelPos.x > 10 && container.DPanelPos.y > 10)
                dPanel.absolutePosition = container.DPanelPos;
        }

        public void RefreshLocalizedUI()
        {
            if (toggleButton != null)
            {
                toggleButton.tooltip = LocalizationService.Get("panel.toggle_button.tooltip");
                UpdateToggleButtonIcon();
            }

            if (dPanel == null) return;
            
            dPanel.tooltip = LocalizationService.Get("panel.drag_panel.tooltip");
            dPanel.RebuildLocalizedContent();
        }

        public DisasterWrapper GetDisasterWrapper()
        {
            return disasterWrapper;
        }

        private void UpdateToggleButtonIcon()
        {
            if (toggleButton == null)
                return;

            ApplyDefaultToggleButtonSprites();

            if (!ToggleButtonIconHelper.Apply(toggleButton, dPanel != null && dPanel.isVisible))
                ToggleButtonIconHelper.Hide(toggleButton);
        }

        private void ApplyDefaultToggleButtonSprites()
        {
            if (toggleButton == null)
                return;

            toggleButton.normalBgSprite = string.Empty;
            toggleButton.hoveredBgSprite = string.Empty;
            toggleButton.focusedBgSprite = string.Empty;
            toggleButton.pressedBgSprite = string.Empty;
            toggleButton.disabledBgSprite = string.Empty;
            toggleButton.normalFgSprite = string.Empty;
            toggleButton.hoveredFgSprite = string.Empty;
            toggleButton.focusedFgSprite = string.Empty;
            toggleButton.pressedFgSprite = string.Empty;
            toggleButton.disabledFgSprite = string.Empty;
        }

        private void ToggleButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ToggleDisasterPanel();
        }

        private void ToggleButton_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (!eventParam.buttons.IsFlagSet(UIMouseButton.Right)) return;
            
            var ratio = UIView.GetAView().ratio;
            
            toggleButton.position = SetUIItemPosition(toggleButton.position, eventParam.moveDelta.x,
                eventParam.moveDelta.y, ratio);
            container.ToggleButtonPos = toggleButton.absolutePosition;
        }

        private void DPanel_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (eventParam.buttons.IsFlagSet(UIMouseButton.Right))
            {
                var ratio = UIView.GetAView().ratio;
                dPanel.position =
                    SetUIItemPosition(dPanel.position, eventParam.moveDelta.x, eventParam.moveDelta.y, ratio);
                container.DPanelPos = dPanel.absolutePosition;
            }
        }

        private Vector3 SetUIItemPosition(Vector3 currentPosition, float x, float y, float ratio)
        {
            return new Vector3(
                currentPosition.x + x * ratio,
                currentPosition.y + y * ratio,
                currentPosition.z);
        }

        private void UIInput_eventProcessKeyEvent(EventType eventType, KeyCode keyCode, EventModifiers modifiers)
        {
            if (SettingsScreen.IsCapturingHotkey)
                return;

            //Hide Panel when main menu is triggered
            if (eventType == EventType.KeyDown && keyCode == KeyCode.Escape)
            {
                dPanel.isVisible = false;
                return;
            }

            //Show / Hide Panel hotkey
            if (eventType == EventType.KeyDown &&
                HotkeyHelper.MatchesHotkey(container.TogglePanelHotkey, container.TogglePanelHotkeyModifiers, keyCode,
                    modifiers))
                ToggleDisasterPanel();
        }
    }
}
