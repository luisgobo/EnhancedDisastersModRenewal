using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public sealed class ProgressBarHelper : UIPanel
    {
        private static readonly Color32 DisabledColor = UIStyleHelper.MutedColor;
        private static readonly Color32 UnfilledBarColor = UIStyleHelper.SurfaceColor;
        private static readonly Color32 DarkContrastTextColor = new Color32(28, 34, 40, 255);
        private static readonly Color32 LightContrastTextColor = UIStyleHelper.PrimaryTextColor;

        private UIProgressBar _progressBar;
        private UILabel _valueLabel;
        private float _labelOffsetY;

        public void Initialize(float x, float y, float componentWidth, float labelOffsetY, float labelTextScale)
        {
            relativePosition = new Vector3(x, y);
            size = new Vector2(componentWidth, 18f);
            _labelOffsetY = labelOffsetY;

            _progressBar = AddUIComponent<UIProgressBar>();
            _progressBar.backgroundSprite = "LevelBarBackground";
            _progressBar.progressSprite = "LevelBarForeground";
            _progressBar.progressColor = UIStyleHelper.AccentColor;
            _progressBar.relativePosition = Vector3.zero;
            _progressBar.width = componentWidth;
            _progressBar.value = 0f;

            _valueLabel = AddUIComponent<UILabel>();
            _valueLabel.autoSize = true;
            _valueLabel.textScale = labelTextScale;
            _valueLabel.text = string.Empty;
        }

        public void SetState(bool isEnabled, float value, string text, string tooltipText)
        {
            _valueLabel.text = text;
            _valueLabel.tooltip = tooltipText;
            _progressBar.tooltip = tooltipText;
            CenterValueLabel();

            if (!isEnabled)
            {
                _progressBar.value = 0f;
                _progressBar.progressColor = DisabledColor;
                _valueLabel.textColor = LightContrastTextColor;
                return;
            }

            _progressBar.value = Mathf.Clamp01(value);
            _progressBar.progressColor = Color.Lerp(
                UIStyleHelper.AccentColor,
                UIStyleHelper.WarmAccentColor,
                _progressBar.value);
            _valueLabel.textColor = GetContrastTextColor();
        }

        private void CenterValueLabel()
        {
            float centeredX = Mathf.Max(0f, (_progressBar.width - _valueLabel.width) * 0.5f);
            _valueLabel.relativePosition = new Vector3(centeredX, _labelOffsetY + 4f);
        }

        private Color32 GetContrastTextColor()
        {
            float fillWidth = _progressBar.width * _progressBar.value;
            float labelStartX = _valueLabel.relativePosition.x;
            float labelEndX = labelStartX + _valueLabel.width;
            float overlapWithFill = Mathf.Clamp(Mathf.Min(labelEndX, fillWidth) - labelStartX, 0f, _valueLabel.width);
            float overlapWithUnfilled = Mathf.Max(0f, _valueLabel.width - overlapWithFill);

            Color estimatedBackground = BlendBackground(overlapWithFill, overlapWithUnfilled);
            return GetPerceivedLuminance(estimatedBackground) >= 0.45f
                ? DarkContrastTextColor
                : LightContrastTextColor;
        }

        private Color BlendBackground(float fillWeight, float unfilledWeight)
        {
            float totalWeight = Mathf.Max(0.0001f, fillWeight + unfilledWeight);
            Color fillColor = _progressBar.progressColor;

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
