using ColossalFramework;
using ColossalFramework.UI;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.Disaster;
using System;
using UnifiedUI.Helpers;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.UnifiedUI
{
    internal class SelectionTool : DefaultTool
    {
        readonly NaturalDisasterHandler naturalDisasterSetup = Singleton<NaturalDisasterHandler>.instance;
        internal static Keybinding.UnsavedInputKey UUIKey { get; set; } = new Keybinding.UnsavedInputKey("Natural DisasterRenewal hotkey", keyCode: KeyCode.N, control: false, shift: false, alt: true);
        public static SelectionTool Instance = null;
        UIComponent m_button = null;

        public static void AddSelectionTool()
        {
            //if (TransferManagerLoader.IsLoaded())
            //{
            if (Instance is null)
            {
                try
                {
                    //s_bLoadingTool = true;
                    Instance = ToolsModifierControl.toolController.gameObject.AddComponent<SelectionTool>();
                }
                catch (Exception e)
                {
                    DebugLogger.Log("UnifiedUI failed to load: "+ e.Message);
                }
                //finally
                //{
                //s_bLoadingTool = false;
                    
                //}
            }
        //}
            else
            {
                DebugLogger.Log("Game not loaded");                
            }   
        }   

        public static bool HasUnifiedUIButtonBeenAdded()
        {
            return (Instance != null && Instance.m_button != null);
        }

        public static void RemoveUnifiedUITool()
        {
            if (Instance != null)
            {
                if (Instance.m_button != null)
                {
                    UUIHelpers.Destroy(Instance.m_button);
                    Instance.m_button = null;
                }
                Instance.Destroy();
            }
        }

        protected override void Awake()        
        {
            base.Awake();
            if (naturalDisasterSetup.container.isUnifiedUIActive)
            {
                m_button = UUIHelpers.RegisterToolButton(
                   name: CommonProperties.modNameForHarmony,
                   groupName: null,
                   tooltip: CommonProperties.modName,
                   tool: this,
                   icon: UUIHelpers.LoadTexture(UUIHelpers.GetFullPath<Mod>("Resources", "NaturalDisasterRenewal-UUI.png")),
                   hotkeys: new UUIHotKeys { ActivationKey = UUIKey }

                );
            }
        }

        public static void Release()
        {
            Destroy(FindObjectOfType<SelectionTool>());
        }

        public void Enable()
        {
            if (Instance != null && !Instance.enabled)
            {
                OnEnable();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            naturalDisasterSetup.ToggleDisasterPanel();
        }

        public void Disable()
        {
            // Ensure we are in normal mode.

            ToolBase oCurrentTool = ToolsModifierControl.toolController.CurrentTool;
            if (oCurrentTool != null && oCurrentTool == Instance && oCurrentTool.enabled)
            {
                OnDisable();
            }
        }

        protected override void OnDisable()
        {
            
            base.OnDisable();
            ToolsModifierControl.SetTool<DefaultTool>();

            /**********************/
            naturalDisasterSetup.ToggleDisasterPanel();
            /**********************/            
        }

        public void ToogleSelectionTool()
        {
            if (isActiveAndEnabled)
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }       

        protected override bool CheckDisaster(ushort disaster, ref ToolErrors errors) => true;
    }
}