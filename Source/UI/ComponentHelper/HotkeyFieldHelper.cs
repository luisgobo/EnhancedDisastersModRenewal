using System;
using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public static class HotkeyFieldHelper
    {
        public static UITextField AddHotkeyField(
            UIPanel parentPanel,
            string labelText,
            string value,
            string tooltip,
            Action focusHandler,
            Action blurHandler)
        {
            var container = parentPanel.AddUIComponent<UIPanel>();
            container.autoLayout = true;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayoutPadding = new RectOffset(0, 10, 2, 0);
            container.width = parentPanel.width > 0f ? parentPanel.width - 20f : 700f;
            container.height = 32f;
            container.autoFitChildrenHorizontally = false;
            container.autoFitChildrenVertically = false;

            var label = container.AddUIComponent<UILabel>();
            label.text = labelText;
            label.textScale = 0.9f;
            label.tooltip = tooltip;
            label.width = 110f;
            label.height = 28f;
            label.textAlignment = UIHorizontalAlignment.Left;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.padding = new RectOffset(0, 0, 6, 0);
            label.textColor = UIStyleHelper.SecondaryTextColor;

            var textField = container.AddUIComponent<UITextField>();
            textField.width = 170f;
            textField.height = 28f;
            textField.padding = new RectOffset(8, 8, 6, 4);
            textField.text = value;
            textField.tooltip = tooltip;
            textField.builtinKeyNavigation = false;
            textField.readOnly = true;
            textField.canFocus = true;
            textField.cursorWidth = 0;
            UIStyleHelper.ApplyTextFieldStyle(textField);

            if (focusHandler != null)
            {
                textField.eventGotFocus += delegate { focusHandler(); };
                textField.eventMouseDown += delegate(UIComponent component, UIMouseEventParameter eventParam)
                {
                    focusHandler();
                };
            }

            if (blurHandler != null)
                textField.eventLostFocus += delegate { blurHandler(); };

            return textField;
        }
    }
}
