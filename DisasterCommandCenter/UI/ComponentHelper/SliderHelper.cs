using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public static class SliderHelper
    {
        private const float TitleBottomPadding = 2f;
        private const float SliderPaddingX = 6f;
        private const float SliderPaddingY = 6f;
        private const float ValueLabelSpacing = 15f;

        private const float BottomPadding = 10f;

        private static readonly Color32 SliderBackgroundColor = UIStyleHelper.SurfaceColor;
        private static readonly Color32 SliderBorderColor = UIStyleHelper.SurfaceBorderColor;
        private static readonly Color32 SliderTrackColor = UIStyleHelper.AccentColor;
        private static readonly Color32 SliderThumbColor = UIStyleHelper.PrimaryTextColor;

        /// <summary>
        ///     Adds a slider to the provided UI helper group and configures a live value label.
        /// </summary>
        /// <param name="group">Target UI helper group where the slider control will be added.</param>
        /// <param name="description">Text shown as the slider label.</param>
        /// <param name="min">Minimum allowed slider value.</param>
        /// <param name="max">Maximum allowed slider value.</param>
        /// <param name="step">Increment used when the slider value changes.</param>
        /// <param name="value">Initial slider value.</param>
        /// <param name="eventCallback">Callback invoked when the slider value changes.</param>
        /// <param name="postfix">Optional text appended to the value label (for example, "%" or "x").</param>
        /// <param name="tooltip">Optional tooltip text shown when hovering the slider.</param>
        /// <returns>The created <see cref="UISlider" /> instance.</returns>
        public static UISlider AddSlider(
            ref UIHelperBase group,
            string description,
            float min,
            float max,
            float step,
            float value,
            OnValueChanged eventCallback,
            string postfix = "",
            string tooltip = "")
        {
            var slider = (UISlider)group.AddSlider(description, min, max, step, value, eventCallback);

            slider.tooltip = tooltip;
            ApplySliderStyle(slider);
            ConfigureValueLabel(slider, postfix);

            return slider;
        }

        private static void ApplySliderStyle(UISlider slider)
        {
            if (slider == null || slider.parent == null)
                return;

            var sliderParent = slider.parent;

            var border = sliderParent.AddUIComponent<UIPanel>();
            border.name = "SliderBorder";
            border.zOrder = 0;
            border.isInteractive = false;
            border.canFocus = false;
            border.color = SliderBorderColor;
            border.opacity = 1f;
            border.autoLayout = false;

            var background = border.AddUIComponent<UIPanel>();
            background.name = "SliderBackground";
            background.zOrder = 1;
            background.isInteractive = false;
            background.canFocus = false;
            background.color = SliderBackgroundColor;
            background.opacity = 1f;

            slider.zOrder = 2;
            slider.color = SliderTrackColor;

            if (slider.thumbObject != null)
                slider.thumbObject.color = SliderThumbColor;
        }

        private static void ConfigureValueLabel(UISlider slider, string postfix)
        {
            if (slider == null || slider.parent == null)
                return;

            var parentPanel = slider.parent as UIPanel;
            if (parentPanel == null)
                return;

            var valueLabel = parentPanel.AddUIComponent<UILabel>();
            valueLabel.text = slider.value + postfix;
            valueLabel.textScale = 0.9f;
            valueLabel.textColor = UIStyleHelper.SecondaryTextColor;

            parentPanel.autoLayout = false;

            var titleLabel = parentPanel.Find<UILabel>("Label");
            if (titleLabel != null)
            {
                titleLabel.anchor = UIAnchorStyle.None;
                titleLabel.wordWrap = false;
                titleLabel.autoHeight = false;
                titleLabel.autoSize = true;
                titleLabel.textColor = UIStyleHelper.PrimaryTextColor;
                titleLabel.textScale = 0.9f;

                titleLabel.relativePosition = Vector3.zero;

                var sliderY = titleLabel.height + TitleBottomPadding;
                slider.relativePosition = new Vector3(slider.relativePosition.x, sliderY);
                valueLabel.relativePosition =
                    new Vector3(slider.relativePosition.x + slider.width + ValueLabelSpacing, sliderY);
                ConfigureBackground(parentPanel, slider, sliderY);
                parentPanel.height = sliderY + slider.height + BottomPadding;
            }
            else
            {
                slider.relativePosition = new Vector3(slider.relativePosition.x, slider.relativePosition.y);
                valueLabel.relativePosition = new Vector3(
                    slider.relativePosition.x + slider.width + ValueLabelSpacing,
                    slider.relativePosition.y);
                ConfigureBackground(parentPanel, slider, slider.relativePosition.y);
            }

            slider.eventValueChanged += delegate { valueLabel.text = slider.value + postfix; };
        }

        private static void ConfigureBackground(UIPanel parentPanel, UISlider slider, float sliderY)
        {
            if (parentPanel == null)
                return;

            var border = parentPanel.Find<UIPanel>("SliderBorder");
            if (border == null)
                return;

            var backgroundX = Mathf.Max(0f, slider.relativePosition.x - SliderPaddingX);
            var backgroundY = Mathf.Max(0f, sliderY - SliderPaddingY);
            var backgroundWidth = slider.width + SliderPaddingX * 2f;
            var backgroundHeight = slider.height + SliderPaddingY * 2f;

            border.relativePosition = new Vector3(backgroundX, backgroundY);
            border.size = new Vector2(backgroundWidth, backgroundHeight);

            var background = border.Find<UIPanel>("SliderBackground");
            if (background != null)
            {
                background.relativePosition = new Vector3(1f, 1f);
                background.size = new Vector2(
                    Mathf.Max(0f, backgroundWidth - 2f),
                    Mathf.Max(0f, backgroundHeight - 2f));
            }

            border.SendToBack();
        }
    }
}
