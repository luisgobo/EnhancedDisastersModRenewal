using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public sealed class ProgressBarHelper : UIPanel
    {
        private static readonly Color32 DisabledColor = new (0, 0, 0, 255);
        private static readonly Color32 UnfilledBarColor = new (56, 60, 62, 255);
        private static readonly Color32 DarkContrastTextColor = new (30, 36, 32, 255);
        private static readonly Color32 LightContrastTextColor = new (242, 245, 240, 255);

        private UIProgressBar _progressBar;
        private UILabel _valueLabel;
        private float _labelOffsetY;

        public float Value => _progressBar.value;

        public void Initialize(float x, float y, float componentWidth, float labelOffsetY, float labelTextScale)
        {
            relativePosition = new Vector3(x, y);
            size = new Vector2(componentWidth, 18f);
            _labelOffsetY = labelOffsetY;

            _progressBar = AddUIComponent<UIProgressBar>();
            _progressBar.backgroundSprite = "LevelBarBackground";
            _progressBar.progressSprite = "LevelBarForeground";
            _progressBar.progressColor = Color.red;
            _progressBar.relativePosition = Vector3.zero;
            _progressBar.width = componentWidth;
            _progressBar.value = 0f;

            _valueLabel = AddUIComponent<UILabel>();
            _valueLabel.autoSize = true;
            _valueLabel.textScale = labelTextScale;
            _valueLabel.text = string.Empty;
        }

        public void SetState(bool isProgressBarEnabled, float value, string text, string tooltipText)
        {
            _valueLabel.text = text;
            _valueLabel.tooltip = tooltipText;
            _progressBar.tooltip = tooltipText;
            CenterValueLabel();

            if (!isProgressBarEnabled)
            {
                _progressBar.value = 0f;
                _progressBar.progressColor = DisabledColor;
                _valueLabel.textColor = LightContrastTextColor;
                return;
            }

            _progressBar.value = Mathf.Clamp01(value);
            _progressBar.progressColor = new Color(2f * _progressBar.value, 2f * (1f - _progressBar.value), 0f);
            _valueLabel.textColor = GetContrastTextColor();
        }

        private void CenterValueLabel()
        {
            var centeredX = Mathf.Max(0f, (_progressBar.width - _valueLabel.width) * 0.5f);
            _valueLabel.relativePosition = new Vector3(centeredX, _labelOffsetY + 4f);
        }

        private Color32 GetContrastTextColor()
        {
            var fillWidth = _progressBar.width * _progressBar.value;
            var labelStartX = _valueLabel.relativePosition.x;
            var labelEndX = labelStartX + _valueLabel.width;
            var overlapWithFill = Mathf.Clamp(Mathf.Min(labelEndX, fillWidth) - labelStartX, 0f, _valueLabel.width);
            var overlapWithUnfilled = Mathf.Max(0f, _valueLabel.width - overlapWithFill);

            var estimatedBackground = BlendBackground(overlapWithFill, overlapWithUnfilled);
            return GetPerceivedLuminance(estimatedBackground) >= 0.45f
                ? DarkContrastTextColor
                : LightContrastTextColor;
        }

        private Color BlendBackground(float fillWeight, float unfilledWeight)
        {
            var totalWeight = Mathf.Max(0.0001f, fillWeight + unfilledWeight);
            var fillColor = _progressBar.progressColor;

            return new Color(
                ((fillColor.r * fillWeight) + (UnfilledBarColor.r / 255f * unfilledWeight)) / totalWeight,
                ((fillColor.g * fillWeight) + (UnfilledBarColor.g / 255f * unfilledWeight)) / totalWeight,
                ((fillColor.b * fillWeight) + (UnfilledBarColor.b / 255f * unfilledWeight)) / totalWeight);
        }

        private static float GetPerceivedLuminance(Color color)
        {
            return color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
        }
    }
}
