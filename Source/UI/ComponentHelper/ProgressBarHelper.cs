using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public sealed class ProgressBarHelper : UIPanel
    {
        private const float NarrowCharacterAdvanceFactor = 0.45f;
        private static readonly Color32 DisabledColor = UIStyleHelper.MutedColor;
        private static readonly Color32 DarkContrastTextColor = new Color32(28, 34, 40, 255);
        private static readonly Color32 LightContrastTextColor = UIStyleHelper.PrimaryTextColor;
        private readonly List<UILabel> _valueLabels = new List<UILabel>();
        private float _labelOffsetY;
        private float _labelTextScale;

        private UIProgressBar _progressBar;

        public void Initialize(float x, float y, float componentWidth, float labelOffsetY, float labelTextScale)
        {
            relativePosition = new Vector3(x, y);
            size = new Vector2(componentWidth, 18f);
            _labelOffsetY = labelOffsetY;
            _labelTextScale = labelTextScale;

            _progressBar = AddUIComponent<UIProgressBar>();
            _progressBar.backgroundSprite = "LevelBarBackground";
            _progressBar.progressSprite = "LevelBarForeground";
            _progressBar.progressColor = GetProgressColor(0f);
            _progressBar.relativePosition = Vector3.zero;
            _progressBar.width = componentWidth;
            _progressBar.value = 0f;

        }

        public void SetState(bool isEnabledState, float value, string text, string tooltipText)
        {
            _progressBar.tooltip = tooltipText;
            SetValueText(text, tooltipText);

            if (!isEnabledState)
            {
                _progressBar.value = 0f;
                _progressBar.progressColor = DisabledColor;
                SetValueLabelColors(LightContrastTextColor);
                return;
            }

            _progressBar.value = Mathf.Clamp01(value);
            _progressBar.progressColor = GetProgressColor(_progressBar.value);
            UpdateValueLabelColors();
        }

        private static Color GetProgressColor(float value)
        {
            var progress = Mathf.Clamp01(value);
            return new Color(2f * progress, 2f * (1f - progress), 0f);
        }

        private void SetValueText(string text, string tooltipText)
        {
            var safeText = text ?? string.Empty;

            while (_valueLabels.Count < safeText.Length)
            {
                var label = AddUIComponent<UILabel>();
                label.autoSize = true;
                label.textScale = _labelTextScale;
                _valueLabels.Add(label);
            }

            for (var i = 0; i < _valueLabels.Count; i++)
            {
                var label = _valueLabels[i];
                var isVisible = i < safeText.Length;
                label.isVisible = isVisible;
                label.tooltip = tooltipText;
                if (!isVisible)
                    continue;

                label.text = safeText[i].ToString();
                label.PerformLayout();
            }

            CenterValueLabels(safeText.Length);
        }

        private void CenterValueLabels(int visibleLabelCount)
        {
            var totalWidth = 0f;
            for (var i = 0; i < visibleLabelCount; i++)
                totalWidth += GetCharacterAdvance(_valueLabels[i]);

            var xPosition = Mathf.Max(0f, (_progressBar.width - totalWidth) * 0.5f);
            for (var i = 0; i < visibleLabelCount; i++)
            {
                var label = _valueLabels[i];
                label.relativePosition = new Vector3(xPosition, _labelOffsetY + 4f);
                xPosition += GetCharacterAdvance(label);
            }
        }

        private void UpdateValueLabelColors()
        {
            var filledWidth = _progressBar.width * _progressBar.value;

            for (var i = 0; i < _valueLabels.Count; i++)
            {
                var label = _valueLabels[i];
                if (!label.isVisible)
                    continue;

                var labelCenterX = label.relativePosition.x + GetCharacterAdvance(label) * 0.5f;
                label.textColor = labelCenterX <= filledWidth
                    ? DarkContrastTextColor
                    : LightContrastTextColor;
            }
        }

        private static float GetCharacterAdvance(UILabel label)
        {
            if (label.text == "." || label.text == ",")
                return label.width * NarrowCharacterAdvanceFactor;

            return label.width;
        }

        private void SetValueLabelColors(Color32 color)
        {
            for (var i = 0; i < _valueLabels.Count; i++)
                _valueLabels[i].textColor = color;
        }
    }
}
