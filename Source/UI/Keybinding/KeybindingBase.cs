using ColossalFramework;
using System.Xml.Serialization;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.Keybinding
{
    public abstract class KeybindingBase
    {
        /// <summary>
        /// This binding's KeyCode.
        /// </summary>
        [XmlIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Protected field")]
        protected KeyCode m_key;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeybindingBase"/> class.
        /// </summary>
        public KeybindingBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeybindingBase"/> class.
        /// </summary>
        /// <param name="keyCode">Key code to set.</param>
        public KeybindingBase(KeyCode keyCode)
        {
            m_key = keyCode;
        }

        /// <summary>
        /// Gets or sets the keycode as an integer value.
        /// </summary>
        [XmlAttribute("KeyCode")]
        public int Key
        {
            get => (int)m_key;

            set => m_key = (KeyCode)value;
        }

        /// <summary>
        /// Encode keybinding as saved input key for UUI.
        /// </summary>
        /// <returns>Saved input key.</returns>
        public abstract InputKey Encode();

        /// <summary>
        /// Sets the keybinding from the provided ColossalFramework InputKey.
        /// </summary>
        /// <param name="inputKey">InputKey to set from.</param>
        public abstract void SetKey(InputKey inputKey);

        /// <summary>
        /// Checks to see if the designated key is pressed.
        /// </summary>
        /// <returns>True if pressed, false otherwise.</returns>
        public abstract bool IsPressed();

        /// <summary>
        /// Checks to see if the designated key is pressed.
        /// </summary>
        /// <param name="e">Event parameter to check.</param>
        /// <returns>True if pressed, false otherwise.</returns>
        public abstract bool IsPressed(Event e);
    }
}