using ColossalFramework;
using NaturalDisastersRenewal.Handlers;
using UnityEngine;
using UnifUI = UnifiedUI;

namespace NaturalDisastersRenewal.UI.Keybinding
{
    public class UnsavedInputKey : UnifUI.Helpers.UnsavedInputKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsavedInputKey"/> class.
        /// </summary>
        /// <param name="name">Reference name.</param>
        /// <param name="keyCode">Keycode.</param>
        /// <param name="control">Control modifier key status.</param>
        /// <param name="shift">Shift modifier key status.</param>
        /// <param name="alt">Alt modifier key status.</param>
        public UnsavedInputKey(string name, KeyCode keyCode, bool control, bool shift, bool alt)
            : base(keyName: name, modName: "Natural Disasters Renewal", Encode(keyCode, control: control, shift: shift, alt: alt))
        {
        }

        /// <summary>
        /// Gets or sets the current key as a Keybinding.
        /// </summary>
        public Keybinding Keybinding
        {
            get => new Keybinding(Key, Control, Shift, Alt);
            set => this.value = value.Encode();
        }

        /// <summary>
        /// Called by UUI when a key conflict is resolved.
        /// Used here to save the new key setting.
        /// </summary>
        public override void OnConflictResolved() => Singleton<NaturalDisasterHandler>.instance.container.Save();
    }
}