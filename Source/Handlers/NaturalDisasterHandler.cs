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
        private readonly Harmony harmony = new Harmony(CommonProperties.modNameForHarmony);
        public DisasterSetupModel container;
        private DisasterWrapper disasterWrapper;
        private ExtendedDisastersPanel dPanel;
        private UIButton toggleButton;

        private NaturalDisasterHandler()
        {
            ReadValuesFromFile();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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
            if (container == null)
            {
                container = fromContainer;
            }
            else
            {
                for (var i = 0; i < container.DisasterList.Count; i++)
                    container.DisasterList[i].CopySettings(fromContainer.DisasterList[i]);

                container.DisableDisasterFocus = fromContainer.DisableDisasterFocus;
                container.PauseOnDisasterStarts = fromContainer.PauseOnDisasterStarts;
                container.PartialEvacuationRadius = fromContainer.PartialEvacuationRadius;
                container.MaxPopulationToTriggerHigherDisasters = fromContainer.MaxPopulationToTriggerHigherDisasters;

                container.ScaleMaxIntensityWithPopulation = fromContainer.ScaleMaxIntensityWithPopulation;
                container.RecordDisasterEvents = fromContainer.RecordDisasterEvents;
                container.ShowDisasterPanelButton = fromContainer.ShowDisasterPanelButton;
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
                    container.ToggleButtonPos = new Vector3(90, 100);
                }
            }
        }

        public void OnSimulationFrame()
        {
            CheckUnlocks();

            foreach (var ed in container.DisasterList) ed.OnSimulationFrame();
        }

        public void OnCreated(IDisaster disasters)
        {
            disasterWrapper = (DisasterWrapper)disasters;
        }

        public void OnDisasterStarted(DisasterAI disasterAI, byte intensity)
        {
            foreach (var ed in container.DisasterList)
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

            foreach (var ed in container.DisasterList)
                if (ed.CheckDisasterAIType(disasterAI))
                {
                    ed.OnDisasterActivated(disasterInfo, disasterId, ref container.ActiveDisasters);
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

                foreach (var ed in container.DisasterList)
                    if (ed.CheckDisasterAIType(disasterAI))
                    {
                        ed.OnDisasterDeactivated(new DisasterInfoModel
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        }, ref container.ActiveDisasters);
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
                foreach (var disasterService in container.DisasterList)
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

                        disasterService.OnDisasterDetected(disasterInfoUnified, ref container.ActiveDisasters);
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
                foreach (var disasterService in container.DisasterList)
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

                        disasterService.OnDisasterFinished(disasterInfoUnified, ref container.ActiveDisasters);
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

        private void ToggleDisasterPanel()
        {
            dPanel.isVisible = !dPanel.isVisible;

            if (dPanel.isVisible) dPanel.counter = 0;
        }

        public void UpdateDisastersPanelToggleBtn()
        {
            if (toggleButton != null && container != null)
            {
                toggleButton.isVisible = container.ShowDisasterPanelButton;

                if (container.ToggleButtonPos.x > 10 && container.ToggleButtonPos.y > 10)
                    toggleButton.absolutePosition = container.ToggleButtonPos;
            }
        }

        public void UpdateDisastersDPanel()
        {
            if (dPanel != null && container != null)
                if (container.DPanelPos.x > 10 && container.DPanelPos.y > 10)
                    dPanel.absolutePosition = container.DPanelPos;
        }

        public DisasterWrapper GetDisasterWrapper()
        {
            return disasterWrapper;
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
                toggleButton.position = SetUIItemPosition(toggleButton.position, eventParam.moveDelta.x,
                    eventParam.moveDelta.y, ratio);
                container.ToggleButtonPos = toggleButton.absolutePosition;
            }
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
            //Hide Panel when main menu is triggered
            if (eventType == EventType.KeyDown && keyCode == KeyCode.Escape)
            {
                dPanel.isVisible = false;
                return;
            }

            //Show / Hide Panel hotkey
            if (eventType == EventType.KeyDown && modifiers == EventModifiers.Shift && keyCode == KeyCode.D)
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