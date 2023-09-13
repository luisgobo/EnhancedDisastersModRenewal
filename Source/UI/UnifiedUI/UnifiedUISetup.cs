using ColossalFramework;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using UnifiedUI.Helpers;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.UnifiedUI
{
    /// <summary>
    /// Static class to handle UUI interface.
    /// </summary>
    internal static class UnifiedUISetup
    {
        // UUI Button.
        private static UUICustomButton s_uuiButton;

        /// <summary>
        /// Gets or sets the UnsavedInputKey reference for communicating with UUI.
        /// </summary>
        internal static Keybinding.UnsavedInputKey UUIKey { get; set; } = new Keybinding.UnsavedInputKey("Natural DisasterRenewal hotkey", keyCode: KeyCode.N, control: false, shift: false, alt: true);

        /// <summary>
        /// Gets the UUI button instance.
        /// </summary>
        internal static UUICustomButton UUIButton => s_uuiButton;

        /// <summary>
        /// Performs initial setup and creates the UUI button.
        /// </summary>
        internal static void Setup()
        {
            // Add UUI button.
            if (s_uuiButton == null)
            {
                var naturalDisasterSetup = Singleton<NaturalDisasterHandler>.instance;
                s_uuiButton = UUIHelpers.RegisterCustomButton(
                    name: CommonProperties.modName,
                    groupName: null, // default group
                    tooltip: CommonProperties.modName,
                    icon: UUIHelpers.LoadTexture(UUIHelpers.GetFullPath<Mod>("Resources", "NaturalDisasterRenewal-UUI.png")),
                    onToggle: (value) => naturalDisasterSetup.ToggleDisasterPanel(),
                    hotkeys: new UUIHotKeys { ActivationKey = UUIKey }
                    );
                ////UUICustomButton RegisterCustomButton (string name, string groupName, string tooltip, string spritefile, Action<bool> onToggle, Action<ToolBase> onToolChanged = null, SavedInputKey activationKey = null, Dictionary<SavedInputKey, Func<bool>> activeKeys = null)
                ////UIComponent RegisterToolButton       (string name, string groupName, string tooltip, ToolBase tool, UnifiedUI.Helpers.UUISprites sprites, UUIHotKeys hotkeys = null)
                //s_uuiButton = UUIHelpers.RegisterToolButton(
                //    name: "TransferManagerCE",
                //    groupName: null,
                //    tooltip: CommonProperties.modName,
                //    tool: this,
                //    icon: UUIHelpers.LoadTexture(UUIHelpers.GetFullPath<Mod>("Resources", "NaturalDisasterRenewal-UUI.png")),
                //    hotkeys: new UUIHotKeys { ActivationKey = UUIKey }
                //);

                // Set initial state.
                s_uuiButton.IsPressed = false;
            }
        }

        //internal static void RemoveUnifiedUITool()
        //{
        //    if (s_uuiButton != null)
        //    {
        //        UUIHelpers.Destroy(s_uuiButton);

        //        s_uuiButton = null;
        //    }
        //}
    }
}