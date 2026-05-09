using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public sealed class ProgressBarHelper : UIPanel
    {
        private const float TextContrastSwitchStart = 0.28f;
        private const float TextContrastSwitchEnd = 0.72f;
        private static readonly Color32 DisabledColor = UIStyleHelper.MutedColor;
        private static readonly Color32 DarkContrastTextColor = new Color32(28, 34, 40, 255);
        private static readonly Color32 LightContrastTextColor = UIStyleHelper.PrimaryTextColor;
        private float _labelOffsetY;

        private UIProgressBar _progressBar;
        private UILabel _valueLabel;

        public void Initialize(float x, float y, float componentWidth, float labelOffsetY, float labelTextScale)
        {
            relativePosition = new Vector3(x, y);
            size = new Vector2(componentWidth, 18f);
            _labelOffsetY = labelOffsetY;

            _progressBar = AddUIComponent<UIProgressBar>();
            _progressBar.backgroundSprite = "LevelBarBackground";
            _progressBar.progressSprite = "LevelBarForeground";
            _progressBar.progressColor = GetProgressColor(0f);
            _progressBar.relativePosition = Vector3.zero;
            _progressBar.width = componentWidth;
            _progressBar.value = 0f;

            _valueLabel = AddUIComponent<UILabel>();
            _valueLabel.autoSize = true;
            _valueLabel.textScale = labelTextScale;
            _valueLabel.text = string.Empty;
        }

        public void SetState(bool isEnabledState, float value, string text, string tooltipText)
        {
            _valueLabel.text = text;
            _valueLabel.tooltip = tooltipText;
            _progressBar.tooltip = tooltipText;
            CenterValueLabel();

            if (!isEnabledState)
            {
                _progressBar.value = 0f;
                _progressBar.progressColor = DisabledColor;
                _valueLabel.textColor = LightContrastTextColor;
                return;
            }

            _progressBar.value = Mathf.Clamp01(value);
            _progressBar.progressColor = GetProgressColor(_progressBar.value);
            _valueLabel.textColor = GetContrastTextColor();
        }

        private static Color GetProgressColor(float value)
        {
            var progress = Mathf.Clamp01(value);
            return new Color(2f * progress, 2f * (1f - progress), 0f);
        }

        private void CenterValueLabel()
        {
            var centeredX = Mathf.Max(0f, (_progressBar.width - _valueLabel.width) * 0.5f);
            _valueLabel.relativePosition = new Vector3(centeredX, _labelOffsetY + 4f);
        }

        private Color32 GetContrastTextColor()
        {
            if (_progressBar.value < TextContrastSwitchStart || _progressBar.value >= TextContrastSwitchEnd)
                return LightContrastTextColor;

            return DarkContrastTextColor;
        }
    }
}