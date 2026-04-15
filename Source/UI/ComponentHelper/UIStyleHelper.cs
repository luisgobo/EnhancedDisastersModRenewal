using System.Linq;
using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public static class UIStyleHelper
    {
        public static readonly Color32 PrimaryTextColor = new Color32(236, 241, 245, 255);
        public static readonly Color32 SecondaryTextColor = new Color32(182, 194, 204, 255);
        public static readonly Color32 AccentColor = new Color32(95, 146, 173, 255);
        public static readonly Color32 AccentHoverColor = new Color32(117, 167, 195, 255);
        public static readonly Color32 AccentPressedColor = new Color32(73, 116, 141, 255);
        public static readonly Color32 SurfaceColor = new Color32(46, 56, 68, 255);
        public static readonly Color32 SurfaceAltColor = new Color32(59, 72, 86, 255);
        public static readonly Color32 SurfaceBorderColor = new Color32(90, 108, 124, 255);
        public static readonly Color32 MutedColor = new Color32(92, 102, 112, 255);
        public static readonly Color32 WarmAccentColor = new Color32(198, 136, 84, 255);

        public static void ApplySectionButtonStyle(UIButton button)
        {
            if (button == null)
                return;

            button.normalBgSprite = "SubBarButtonBase";
            button.disabledBgSprite = "SubBarButtonBaseFocused";
            button.focusedBgSprite = "SubBarButtonBaseFocused";
            button.hoveredBgSprite = "SubBarButtonBaseHovered";
            button.pressedBgSprite = "SubBarButtonBasePressed";
            button.textColor = PrimaryTextColor;
            button.disabledTextColor = PrimaryTextColor;
            button.focusedTextColor = Color.white;
            button.hoveredTextColor = Color.white;
            button.pressedTextColor = Color.white;
            button.textScale = 0.9f;
        }

        public static void ApplyActionButtonStyle(UIButton button, bool leftAligned = false)
        {
            if (button == null)
                return;

            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenu";
            button.focusedBgSprite = "ButtonMenuFocused";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textColor = PrimaryTextColor;
            button.disabledTextColor = SecondaryTextColor;
            button.focusedTextColor = Color.white;
            button.hoveredTextColor = Color.white;
            button.pressedTextColor = Color.white;
            button.textScale = 0.85f;
            button.textHorizontalAlignment = leftAligned ? UIHorizontalAlignment.Left : UIHorizontalAlignment.Center;
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textPadding = leftAligned
                ? new RectOffset(12, 8, 8, 0)
                : new RectOffset(8, 8, 8, 0);
        }

        public static void ApplyTextFieldStyle(UITextField textField)
        {
            if (textField == null)
                return;

            textField.normalBgSprite = "TextFieldPanel";
            textField.disabledBgSprite = "TextFieldPanelDisabled";
            textField.focusedBgSprite = "TextFieldPanelHovered";
            textField.hoveredBgSprite = "TextFieldPanelHovered";
            textField.color = SurfaceColor;
            textField.disabledColor = MutedColor;
            textField.textColor = PrimaryTextColor;
            textField.disabledTextColor = SecondaryTextColor;
            textField.selectionSprite = "EmptySprite";
            textField.textScale = 0.9f;
        }

        public static void ApplyDropDownStyle(UIDropDown dropDown)
        {
            if (dropDown == null)
                return;

            dropDown.normalBgSprite = "ButtonMenu";
            dropDown.disabledBgSprite = "ButtonMenu";
            dropDown.focusedBgSprite = "ButtonMenuFocused";
            dropDown.hoveredBgSprite = "ButtonMenuHovered";
            dropDown.height = 30f;
            dropDown.color = SurfaceColor;
            dropDown.disabledColor = MutedColor;
            dropDown.textColor = PrimaryTextColor;
            dropDown.disabledTextColor = SecondaryTextColor;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.listBackground = "OptionsDropboxListbox";
            dropDown.popupColor = new Color32(30, 38, 48, 255);
            dropDown.popupTextColor = PrimaryTextColor;
            dropDown.itemHeight = 28;
            dropDown.itemPadding = new RectOffset(10, 8, 6, 0);
            dropDown.listPadding = new RectOffset(2, 2, 2, 2);
            dropDown.textScale = 0.85f;
            dropDown.textFieldPadding = new RectOffset(10, 28, 10, 0);
            dropDown.listWidth = Mathf.Max(dropDown.listWidth, (int)dropDown.width);
        }

        public static void ApplyCheckboxStyle(UICheckBox checkBox)
        {
            if (checkBox == null)
                return;

            checkBox.height = Mathf.Max(checkBox.height, 24f);

            var label = checkBox.components.OfType<UILabel>().FirstOrDefault();
            if (label != null)
            {
                label.textScale = 0.9f;
                label.textColor = PrimaryTextColor;
                label.disabledColor = SecondaryTextColor;
                label.padding = new RectOffset(0, 0, 3, 0);
            }

            var sprites = checkBox.components.OfType<UISprite>().ToArray();
            for (var i = 0; i < sprites.Length; i++)
            {
                sprites[i].color = i == sprites.Length - 1 ? AccentColor : SurfaceBorderColor;
            }
        }

        public static void ApplyScrollbarStyle(UISlicedSprite track, UISlicedSprite thumb)
        {
            if (track != null)
                track.color = SurfaceColor;

            if (thumb != null)
                thumb.color = SurfaceBorderColor;
        }
    }
}
