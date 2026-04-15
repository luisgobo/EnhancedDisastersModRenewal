using System;
using ColossalFramework.UI;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public sealed class DisasterRowHelper : UIPanel
    {
        private const string PauseSprite = "ButtonPause";
        private const string PlaySprite = "ButtonPlayFocused";
        private const float LabelTextScaleNormal = 0.8f;
        private const float LabelTextScaleTiny = 0.6f;
        private const float MaxIntensity = 255f;
        private const float RowWidth = 410f;
        private const float NameLabelX = 26f;
        public const float ProbabilityBarX = 195f;
        public const float IntensityBarX = 295f;
        public const float BarWidth = 90f;
        private const float ValueLabelY = 0f;

        private Action<DisasterBaseModel> _toggleHandler;
        private UISprite _statusIcon;
        private UILabel _nameLabel;
        private ProgressBarHelper _probabilityBar;
        private ProgressBarHelper _intensityBar;

        public DisasterBaseModel Disaster { get; private set; }

        public void Initialize(DisasterBaseModel disaster, float xPosition, float yPosition, Action<DisasterBaseModel> toggleHandler)
        {
            Disaster = disaster;
            _toggleHandler = toggleHandler;

            relativePosition = new Vector3(xPosition, yPosition);
            size = new Vector2(RowWidth, 20f);

            BuildStatusButton();

            _nameLabel = AddUIComponent<UILabel>();
            _nameLabel.relativePosition = new Vector3(NameLabelX, 2f);
            _nameLabel.textScale = LabelTextScaleNormal;
            _nameLabel.textColor = UIStyleHelper.PrimaryTextColor;

            _probabilityBar = AddUIComponent<ProgressBarHelper>();
            _probabilityBar.Initialize(ProbabilityBarX, 0f, BarWidth, ValueLabelY, LabelTextScaleTiny);

            _intensityBar = AddUIComponent<ProgressBarHelper>();
            _intensityBar.Initialize(IntensityBarX, 0f, BarWidth, ValueLabelY, LabelTextScaleTiny);
        }

        public void Refresh()
        {
            bool isEnabled = Disaster.Enabled;
            float occurrencePerYear = Disaster.GetCurrentOccurrencePerYear();
            byte maxIntensityCalculated = Disaster.GetMaximumIntensity();

            _statusIcon.spriteName = isEnabled ? PauseSprite : PlaySprite;
            _nameLabel.text = isEnabled
                ? Disaster.GetName()
                : Disaster.GetName() + " - " + LocalizationService.Get("panel.disabled");

            _probabilityBar.SetState(
                isEnabled,
                GetProbabilityProgressValueLog(occurrencePerYear),
                isEnabled ? string.Format("{0:0.00}", occurrencePerYear) : string.Empty,
                isEnabled ? Disaster.GetProbabilityTooltip(GetProbabilityProgressValueLog(occurrencePerYear)) : string.Empty);

            float normalizedIntensity = maxIntensityCalculated / MaxIntensity;
            _intensityBar.SetState(
                isEnabled,
                normalizedIntensity,
                isEnabled ? string.Format("{0:0.0}", maxIntensityCalculated / 10f) : string.Empty,
                isEnabled ? Disaster.GetIntensityTooltip(normalizedIntensity) : string.Empty);
        }

        private void BuildStatusButton()
        {
            UIButton statusButton = AddUIComponent<UIButton>();
            statusButton.name = "disasterState" + Disaster.GetDisasterType() + "Btn";
            statusButton.relativePosition = new Vector3(0f, -4f);
            statusButton.size = new Vector2(18f, 18f);
            UIStyleHelper.ApplyActionButtonStyle(statusButton);
            statusButton.eventClick += delegate
            {
                if (_toggleHandler != null)
                    _toggleHandler(Disaster);
            };

            _statusIcon = statusButton.AddUIComponent<UISprite>();
            _statusIcon.size = new Vector2(12f, 12f);
            _statusIcon.relativePosition = new Vector2(3f, 3f);
        }

        private static float GetProbabilityProgressValueLog(float currentOccurrencePerYear)
        {
            if (currentOccurrencePerYear <= 0.1f)
                return 0f;
            if (currentOccurrencePerYear >= 10f)
                return 1f;

            return (1f + Mathf.Log10(currentOccurrencePerYear)) / 2f;
        }
    }
}
