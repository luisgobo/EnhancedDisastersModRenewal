﻿using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using NaturalDisasterRenewal_Reestructured.Common;
using NaturalDisasterRenewal_Reestructured.Common.Helpers;
using NaturalDisasterRenewal_Reestructured.Logger;
using NaturalDisasterRenewal_Reestructured.Models;
using NaturalDisasterRenewal_Reestructured.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace NaturalDisasterRenewal_Reestructured.Handlers
{
    internal class DisasterGeneralSetupHandler: Singleton<DisasterGeneralSetupHandler>
    {
        public DisasterGeneralSetupModel disasterGeneralSetup; //container
        ExtendedDisastersPanel dPanel;
        UIButton toggleButton;
        readonly Harmony harmony = new Harmony(CommonProperties.ModNameForHarmony);
        DisasterWrapper _disasterWrapper;

        public DisasterGeneralSetupHandler() 
        {
            ReadValuesFromFile();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
       
        public void Save()
        {
            SerializationHelper.Serialize(disasterGeneralSetup);
        }

        public void ReadValuesFromFile()
        {
            DisasterGeneralSetupModel newContainer = SerializationHelper.Deserialize<DisasterGeneralSetupModel>();
            if (newContainer == null)
            {
                DebugLogger.Log("newContainer is null");
                newContainer = new DisasterGeneralSetupModel();
                disasterGeneralSetup = newContainer;
            }
            else
                DebugLogger.Log("newContainer has something, then check it: ");            
        }

        public void ResetToDefaultValues()
        {
            DisasterGeneralSetupModel newContainer = new DisasterGeneralSetupModel();
            //newContainer.CheckObjects();            
        }



        public void OnSimulationFrame()
        {
            //CheckUnlocks();

            foreach (DisasterBaseModel ed in disasterGeneralSetup.AllDisasters)
            {
                ed.OnSimulationFrame();
            }
        }

        public void OnCreated(IDisaster disasters)
        {
            DebugLogger.Log("EvacuationService: OnCreated");
            _disasterWrapper = (DisasterWrapper)disasters;
        }

        public void OnDisasterStarted(DisasterAI dai, byte intensity)
        {
            foreach (DisasterBaseModel ed in disasterGeneralSetup.AllDisasters)
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
            var disasterInfo = _disasterWrapper.GetDisasterSettings(disasterId);
            var msg = $"EvacuationService.OnDisasterDeactivated. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
            DebugLogger.Log(msg);
            DebugLogger.Log("Disaster detected");

            foreach (DisasterBaseModel ed in disasterGeneralSetup.AllDisasters)
            {
                if (ed.CheckDisasterAIType(dai))
                {
                    ed.OnDisasterActivated(disasterInfo, disasterId);
                    return;
                }
            }
        }

        public void OnDisasterDeactivated(DisasterAI dai, ushort disasterId)
        {
            try
            {
                var disasterInfo = _disasterWrapper.GetDisasterSettings(disasterId);

                var msg = $"EvacuationService.OnDisasterDeactivated. Id: {disasterId}, Name: {disasterInfo.name}, Type: {disasterInfo.type}, Intensity: {disasterInfo.intensity}";
                DebugLogger.Log(msg);
                DebugLogger.Log("Disaster detected");

                foreach (DisasterBaseModel ed in disasterGeneralSetup.AllDisasters)
                {
                    if (ed.CheckDisasterAIType(dai))
                    {

                        ed.OnDisasterDeactivated(new DisasterInfoModel()
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        });
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
                DebugLogger.Log("Disaster detected");
                foreach (DisasterBaseModel disasterService in disasterGeneralSetup.AllDisasters)
                {
                    if (disasterService.CheckDisasterAIType(disasterAI))
                    {
                        var disasterInfo = _disasterWrapper.GetDisasterSettings(disasterId);

                        var msg = $"disasterInfo1: type: {disasterInfo.type}, name:{disasterInfo.name}, " +
                                  $"location => x:{disasterInfo.targetX} y:{disasterInfo.targetX} z:{disasterInfo.targetZ}. " +
                                  $"Angle: {disasterInfo.angle}, intensity: {disasterInfo.intensity} ";
                        DebugLogger.Log(msg);

                        DisasterInfoModel disasterInfoUnified = new DisasterInfoModel()
                        {
                            DisasterInfo = disasterInfo,
                            DisasterId = disasterId
                        };

                        disasterService.OnDisasterDetected(disasterInfoUnified);
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

        //public void CheckUnlocks()
        //{
        //    int milestoneNum = 99; // Unlock all disasters in case of error

        //    MilestoneInfo mi = Singleton<UnlockManager>.instance.GetCurrentMilestone();
        //    if (mi != null)
        //    {
        //        int.TryParse(mi.name.Substring(9), out milestoneNum);
        //    }

        //    if (milestoneNum >= 3) disasterGeneralSetup.ForestFire.Unlock();
        //    if (milestoneNum >= 3) disasterGeneralSetup.Thunderstorm.Unlock();
        //    if (milestoneNum >= 4) disasterGeneralSetup.Sinkhole.Unlock();
        //    if (milestoneNum >= 5) disasterGeneralSetup.Tsunami.Unlock();
        //    if (milestoneNum >= 5) disasterGeneralSetup.Tornado.Unlock();
        //    if (milestoneNum >= 6) disasterGeneralSetup.Earthquake.Unlock();
        //    if (milestoneNum >= 6) disasterGeneralSetup.MeteorStrike.Unlock();
        //}

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
            toggleButton.isVisible = disasterGeneralSetup.ShowDisasterPanelButton;
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

                disasterGeneralSetup.ToggleButtonPos = toggleButton.absolutePosition;
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

        void ToggleButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ToggleDisasterPanel();
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
            if (toggleButton != null && disasterGeneralSetup != null)
            {
                toggleButton.isVisible = disasterGeneralSetup.ShowDisasterPanelButton;

                if (disasterGeneralSetup.ToggleButtonPos.x > 10 && disasterGeneralSetup.ToggleButtonPos.y > 10)
                {
                    toggleButton.absolutePosition = disasterGeneralSetup.ToggleButtonPos;
                }
            }
        }
    }
}