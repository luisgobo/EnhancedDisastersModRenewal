using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public static class ActionButtonHelper
    {
        public static UIButton CreateTextButton(UIComponent parent, string name, string text, Vector3 position,
            Vector2 size, string tooltip, MouseEventHandler clickHandler, Color? textColor,
            string normalBgSprite, string hoveredBgSprite, string pressedBgSprite, RectOffset textPadding)
        {
            UIButton button = parent.AddUIComponent<UIButton>();
            button.name = name;
            button.relativePosition = position;
            button.size = size;
            button.normalBgSprite = normalBgSprite;
            button.hoveredBgSprite = hoveredBgSprite;
            button.pressedBgSprite = pressedBgSprite;
            button.focusedColor = Color.white;
            button.textColor = textColor ?? Color.white;
            button.focusedTextColor = Color.black;
            button.text = text;
            button.tooltip = tooltip;
            if (textPadding != null) button.textPadding = textPadding;
            if (clickHandler != null) button.eventClick += clickHandler;
            return button;
        }
    }
}
