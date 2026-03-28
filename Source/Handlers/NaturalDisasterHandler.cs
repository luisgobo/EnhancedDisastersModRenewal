using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.Models.Disaster;
using NaturalDisastersRenewal.Models.Setup;
using NaturalDisastersRenewal.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.Handlers
{
    public class NaturalDisasterHandler : Singleton<NaturalDisasterHandler>
    {
        private readonly Harmony _harmony = new (CommonProperties.modNameForHarmony);
        public DisasterSetupModel Container;
        private DisasterWrapper _disasterWrapper;
        private ExtendedDisastersPanel _dPanel;
        private UIButton _toggleButton;
        private bool _keyHandlerRegistered;

        private NaturalDisasterHandler()
        {
            ReadValuesFromFile();
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

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
            if (Container == null)
            {
                Container = fromContainer;
            }
            else
            {
                for (var i = 0; i < Container.DisasterList.Count; i++)
                    Container.DisasterList[i].CopySettings(fromContainer.DisasterList[i]);

                Container.DisableDisasterFocus = fromContainer.DisableDisasterFocus;
                Container.PauseOnDisasterStarts = fromContainer.PauseOnDisasterStarts;
                Container.PartialEvacuationRadius = fromContainer.PartialEvacuationRadius;
                Container.MaxPopulationToTriggerHigherDisasters = fromContainer.MaxPopulationToTriggerHigherDisasters;

                Container.ScaleMaxIntensityWithPopulation = fromContainer.ScaleMaxIntensityWithPopulation;
                Container.RecordDisasterEvents = fromContainer.RecordDisasterEvents;
                Container.ShowDisasterPanelButton = fromContainer.ShowDisasterPanelButton;
                Container.Language = fromContainer.Language;
                Container.TogglePanelHotkey = fromContainer.TogglePanelHotkey;
                Container.TogglePanelHotkeyModifiers = fromContainer.TogglePanelHotkeyModifiers;
                Container.ToggleButtonPos = fromContainer.ToggleButtonPos;
                Container.DPanelPos = fromContainer.DPanelPos;
            }
        }

        private void ResetInterfaceElementPosition(DisasterSetupModel fromContainer, bool resetButtonPos = false,
            bool resetPanelPos = false)
        {
            if (Container == null)
            {
                Container = fromContainer;
            }
            else
            {
                if (resetButtonPos)
                {
                    _toggleButton.absolutePosition = new Vector3(90, 62);
                    Container.ToggleButtonPos = new Vector3(90, 62);
                }

                if (resetPanelPos)
                {
                    _dPanel.absolutePosition = new Vector3(90, 100);
                    Container.DPanelPos = new Vector3(90, 100);
                }
            }
        }

        public void OnSimulationFrame()
        {
            CheckUnlocks();

            foreach (var ed in Container.DisasterList) ed.OnSimulationFrame();
        }

        public void OnCreated(IDisaster disasters)
        {
            _disasterWrapper = (DisasterWrapper)disasters;
        }

        public void OnDisasterStarted(DisasterAI disasterAI, byte intensity)
        {
            foreach (var ed in Container.DisasterList)
                if (ed.CheckDisasterAIType(disasterAI))
                {
                    ed.OnDisasterStarted(intensity);
                    return;
                }
        }

        public void OnDisasterActivated(DisasterAI disasterAI, ushort disasterId)
        {
            var disasterInfo = _disasterWrapper.GetDisasterSettings(disasterId);
            var msg =
                $"EvacuationService.OnDisasterActivated. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
            DebugLogger.Log(msg);

            foreach (var ed in Container.DisasterList)
                if (ed.CheckDisasterAIType(disasterAI))
                {
                    ed.OnDisasterActivated(disasterInfo, disasterId, ref Container.ActiveDisasters);
                    return;
                }
        }

        public void OnDisasterDeactivated(DisasterAI disasterAI, ushort disasterId)
        {
            try
            {
                var disasterInfo = _disasterWrapper.GetDisasterSettings(disasterId);

                var msg =
                    $"EvacuationService.OnDisasterDeactivated. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
                DebugLogger.Log(msg);

                foreach (var ed in Container.DisasterList)
                    if (ed.CheckDisasterAIType(disasterAI))
                    {
                        ed.OnDisasterDeactivated(new DisasterInfoModel
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        }, ref Container.ActiveDisasters);
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
                foreach (var disasterService in Container.DisasterList)
                    if (disasterService.CheckDisasterAIType(disasterAI))
                    {
                        var disasterInfo = _disasterWrapper.GetDisasterSettings(disasterId);
                        var msg =
                            $"EvacuationService.OnDisasterDetected. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
                        DebugLogger.Log(msg);
                        var disasterInfoUnified = new DisasterInfoModel
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        };

                        disasterService.OnDisasterDetected(disasterInfoUnified, ref Container.ActiveDisasters);
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
                foreach (var disasterService in Container.DisasterList)
                    if (disasterService.CheckDisasterAIType(disasterAI))
                    {
                        var disasterInfo = _disasterWrapper.GetDisasterSettings(disasterId);
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

                        disasterService.OnDisasterFinished(disasterInfoUnified, ref Container.ActiveDisasters);
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
                if (disasterInfo != null)
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

            return null;
        }

        public void CheckUnlocks()
        {
            var milestoneNum = 99; // Unlock all disasters in case of error

            var mi = Common.Services.Unlocks.GetCurrentMilestone();
            if (mi != null) int.TryParse(mi.name.Substring(9), out milestoneNum);

            if (milestoneNum >= 3) Container.ForestFire.Unlock();
            if (milestoneNum >= 3) Container.Thunderstorm.Unlock();
            if (milestoneNum >= 4) Container.Sinkhole.Unlock();
            if (milestoneNum >= 5) Container.Tsunami.Unlock();
            if (milestoneNum >= 5) Container.Tornado.Unlock();
            if (milestoneNum >= 6) Container.Earthquake.Unlock();
            if (milestoneNum >= 6) Container.MeteorStrike.Unlock();
        }

        public void CreateExtendedDisasterPanel()
        {
            if (_dPanel != null)
                return;

            var v = UIView.GetAView();

            var obj = new GameObject("ExtendedDisastersPanel");
            obj.transform.parent = v.cachedTransform;
            _dPanel = obj.AddComponent<ExtendedDisastersPanel>();
            _dPanel.absolutePosition = new Vector3(90, 100);

            var toggleButtonObject = new GameObject("ExtendedDisastersPanelButton");
            toggleButtonObject.transform.parent = v.transform;
            toggleButtonObject.transform.localPosition = Vector3.zero;
            _toggleButton = toggleButtonObject.AddComponent<UIButton>();
            _toggleButton.name = "ExtendedDisastersPanelToggleButton";
            _toggleButton.normalBgSprite = "ToolbarIconZoomOutGlobeHovered";
            _toggleButton.normalFgSprite = "IconPolicyPowerSavingDisabled";
            _toggleButton.hoveredFgSprite = "IconPolicyPowerSavingPressed";
            _toggleButton.width = 38f;
            _toggleButton.height = 38f;
            _toggleButton.absolutePosition = new Vector3(90, 62);
            _toggleButton.tooltip = LocalizationService.Get("panel.toggleButton.tooltip");
            _toggleButton.isVisible = Container.ShowDisasterPanelButton;
            _toggleButton.eventClick += ToggleButton_eventClick;
            _toggleButton.eventMouseMove += ToggleButton_eventMouseMove;

            _dPanel.tooltip = LocalizationService.Get("panel.drag.tooltip");
            _dPanel.eventMouseMove += DPanel_eventMouseMove;

            UpdateDisastersPanelToggleBtn();
            UpdateDisastersDPanel();

            if (!_keyHandlerRegistered)
            {
                UIInput.eventProcessKeyEvent += UIInput_eventProcessKeyEvent;
                _keyHandlerRegistered = true;
            }
        }

        private void ToggleDisasterPanel()
        {
            _dPanel.isVisible = !_dPanel.isVisible;

            if (_dPanel.isVisible) _dPanel.counter = 0;
        }

        public void UpdateDisastersPanelToggleBtn()
        {
            if (_toggleButton != null && Container != null)
            {
                _toggleButton.isVisible = Container.ShowDisasterPanelButton;

                if (Container.ToggleButtonPos.x > 10 && Container.ToggleButtonPos.y > 10)
                    _toggleButton.absolutePosition = Container.ToggleButtonPos;
            }
        }

        public void UpdateDisastersDPanel()
        {
            if (_dPanel != null && Container != null)
            {
                if (Container.DPanelPos.x > 10 && Container.DPanelPos.y > 10)
                    _dPanel.absolutePosition = Container.DPanelPos;
            }
        }

        public void RefreshLocalizedUI()
        {
            var panelVisible = _dPanel != null && _dPanel.isVisible;
            var panelPosition = _dPanel != null ? _dPanel.absolutePosition : Container?.DPanelPos ?? new Vector3(90, 100);
            var buttonPosition = _toggleButton != null ? _toggleButton.absolutePosition : Container?.ToggleButtonPos ?? new Vector3(90, 62);

            if (_dPanel != null)
            {
                UnityEngine.Object.Destroy(_dPanel.gameObject);
                _dPanel = null;
            }

            if (_toggleButton != null)
            {
                UnityEngine.Object.Destroy(_toggleButton.gameObject);
                _toggleButton = null;
            }

            CreateExtendedDisasterPanel();

            if (_dPanel != null)
            {
                _dPanel.absolutePosition = panelPosition;
                _dPanel.isVisible = panelVisible;
            }

            if (_toggleButton != null)
            {
                _toggleButton.absolutePosition = buttonPosition;
            }
        }

        public DisasterWrapper GetDisasterWrapper()
        {
            return _disasterWrapper;
        }

        private void ToggleButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ToggleDisasterPanel();
        }

        private void ToggleButton_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (eventParam.buttons.IsFlagSet(UIMouseButton.Right))
            {
                var ratio = UIView.GetAView().ratio;
                _toggleButton.position = SetUIItemPosition(_toggleButton.position, eventParam.moveDelta.x,
                    eventParam.moveDelta.y, ratio);
                Container.ToggleButtonPos = _toggleButton.absolutePosition;
            }
        }

        private void DPanel_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (eventParam.buttons.IsFlagSet(UIMouseButton.Right))
            {
                var ratio = UIView.GetAView().ratio;
                _dPanel.position =
                    SetUIItemPosition(_dPanel.position, eventParam.moveDelta.x, eventParam.moveDelta.y, ratio);
                Container.DPanelPos = _dPanel.absolutePosition;
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
            //Hide Panel when main menu is triggered
            if (eventType == EventType.KeyDown && keyCode == KeyCode.Escape)
            {
                _dPanel.isVisible = false;
                return;
            }

            //Show / Hide Panel hotkey
            if (eventType == EventType.KeyDown &&
                Helper.MatchesHotkey(Container.TogglePanelHotkey, Container.TogglePanelHotkeyModifiers, keyCode, modifiers))
                ToggleDisasterPanel();
        }

        public bool CheckRealTimeModActive()
        {
            var plugins = PluginManager.instance.GetPluginsInfo();
            const string realTimeModName = "Real Time";
            const ulong realTimeWorkshopId = 1420955187;
            const ulong realTimeWorkshopId26 = 3059406297;

            foreach (var plugin in plugins)
            {
                if (plugin?.userModInstance == null)
                    continue;

                var userMod = plugin.userModInstance as IUserMod;
                var modName = userMod?.Name?.ToLowerInvariant() ?? "";
                var pluginName = plugin.name?.ToLowerInvariant() ?? "";
                var publishedFileID = plugin.publishedFileID.AsUInt64;

                if ((modName.ToLower().Contains(realTimeModName.ToLower()) ||
                     pluginName.ToLower().Contains(realTimeModName.ToLower())) &&
                    plugin.isEnabled)
                    return true;

                if ((publishedFileID == realTimeWorkshopId || publishedFileID == realTimeWorkshopId26) &&
                    plugin.isEnabled)
                    return true;
            }

            return false;
        }

        public void GetSpriteNames()
        {
            var names = new List<string>();
            var atlas = UIView.GetAView().defaultAtlas;

            // Obtener fecha y hora actual para el nombre del archivo
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filePath = CommonProperties.GetOptionsFilePath(CommonProperties.spriteFileName) +
                           $"SpritesLog_{timestamp}.txt";

            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    // Escribir encabezado
                    // writer.WriteLine($"Lista de Sprites - {DateTime.Now}");
                    // writer.WriteLine("----------------------------------------");

                    // Recorrer todos los sprites del atlas
                    foreach (var sprite in atlas.sprites)
                    {
                        names.Add(sprite.name);
                        StripesLogger.AddStripe(sprite.name, $"{sprite.width}x{sprite.height}",
                            sprite.region.ToString());
                        // // Escribir información detallada del sprite
                        // writer.WriteLine($"Nombre: {sprite.name}");
                        // writer.WriteLine($"Tamaño: {sprite.width}x{sprite.height}");
                        // writer.WriteLine($"Region: {sprite.region}");
                        // writer.WriteLine("----------------------------------------");
                    }

                    // Escribir resumen
                    writer.WriteLine($"\nTotal de sprites encontrados: {names.Count}");
                }

                Debug.Log($"Lista de sprites guardada en: {Path.GetFullPath(filePath)}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error al guardar la lista de sprites: {ex.Message}");
            }
        }
        
        
    }
}
