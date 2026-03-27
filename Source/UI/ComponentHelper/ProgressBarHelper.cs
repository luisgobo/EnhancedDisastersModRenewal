using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public sealed class ProgressBarHelper : UIPanel
    {
        private static readonly Color32 DisabledColor = new (0, 0, 0, 255);
        private static readonly Color32 LowerTextColor = new (85, 85, 100, 255);
        private static readonly Color32 HigherTextColor = new (255, 255, 255, 255);

        private UIProgressBar _progressBar;
        private UILabel _valueLabel;
        private float _labelOffsetY;

        public float Value => _progressBar.value;

        public void Initialize(float x, float y, float width, float labelOffsetY, float labelTextScale)
        {
            relativePosition = new Vector3(x, y);
            size = new Vector2(width, 18f);
            _labelOffsetY = labelOffsetY;

            _progressBar = AddUIComponent<UIProgressBar>();
            _progressBar.backgroundSprite = "LevelBarBackground";
            _progressBar.progressSprite = "LevelBarForeground";
            _progressBar.progressColor = Color.red;
            _progressBar.relativePosition = Vector3.zero;
            _progressBar.width = width;
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
                _valueLabel.textColor = HigherTextColor;
                return;
            }

            _progressBar.value = Mathf.Clamp01(value);
            _progressBar.progressColor = new Color(2f * _progressBar.value, 2f * (1f - _progressBar.value), 0f);
            _valueLabel.textColor = _progressBar.value > 0.33f ? LowerTextColor : HigherTextColor;
        }

        private void CenterValueLabel()
        {
            // _valueLabel.PerformLayout();

            var centeredX = Mathf.Max(0f, (_progressBar.width - _valueLabel.width) * 0.5f);
            _valueLabel.relativePosition = new Vector3(centeredX, _labelOffsetY);
        }
    }
}
