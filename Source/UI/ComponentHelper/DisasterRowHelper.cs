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
        private const float NameLabelX = 21f;
        private const float NameLabelBarPadding = 6f;
        private const float MeteorPeriodLabelX = 44f;
        private const float MeteorPeriodBarX = 195f;
        private const float MeteorPeriodRowHeight = 18f;
        public const float ProbabilityBarX = 195f;
        public const float IntensityBarX = 295f;
        public const float BarWidth = 90f;
        private const float ValueLabelY = 0f;

        private Action<DisasterBaseModel> _toggleHandler;
        private UISprite _statusIcon;
        private UILabel _nameLabel;
        private ProgressBarHelper _probabilityBar;
        private ProgressBarHelper _intensityBar;
        private UILabel[] _meteorPeriodLabels;
        private ProgressBarHelper[] _meteorPeriodBars;

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

            BuildMeteorPeriodBars();
        }

        public void Refresh()
        {
            var isDisasterEnabled = Disaster.Enabled;
            var occurrencePerYear = Disaster.GetCurrentOccurrencePerYear();
            var maxIntensityCalculated = Disaster.GetMaximumGeneratedIntensity();

            _statusIcon.spriteName = isDisasterEnabled ? PauseSprite : PlaySprite;
            _nameLabel.text = isDisasterEnabled
                ? Disaster.GetName()
                : Disaster.GetName() + " - " + LocalizationService.Get("panel.disabled");
            _nameLabel.textScale = LabelTextScaleNormal;
            LabelHelper.ResizeLabel(_nameLabel, ProbabilityBarX - NameLabelX - NameLabelBarPadding, 0.68f);

            var probabilityProgressValue = GetProbabilityProgressValue(occurrencePerYear);
            _probabilityBar.SetState(
                isDisasterEnabled,
                probabilityProgressValue,
                isDisasterEnabled ? FormatPercentage(probabilityProgressValue) : string.Empty,
                isDisasterEnabled ? Disaster.GetProbabilityTooltip(probabilityProgressValue) : string.Empty);

            var normalizedIntensity = maxIntensityCalculated / MaxIntensity;
            _intensityBar.SetState(
                isDisasterEnabled,
                normalizedIntensity,
                isDisasterEnabled ? string.Format("{0:0.0}", maxIntensityCalculated / 10f) : string.Empty,
                isDisasterEnabled ? Disaster.GetGeneratedIntensityTooltip(maxIntensityCalculated) : string.Empty);

            RefreshMeteorPeriodBars(isDisasterEnabled);
        }

        private float GetProbabilityProgressValue(float occurrencePerYear)
        {
            if (Disaster.TryGetDebugProbabilityProgress(out var debugProgress))
                return debugProgress;

            switch (Disaster)
            {
                case MeteorStrikeModel meteorStrike:
                    return meteorStrike.AreMeteorPeriodsEnabled()
                        ? meteorStrike.GetMeteorPeriodProbabilityProgress()
                        : meteorStrike.GetRealTimePatternProbabilityProgress();
                case SinkholeModel sinkhole when sinkhole.IsRealTimePatternActive():
                    return sinkhole.GetRealTimePatternProbabilityProgress();
                case ThunderstormModel thunderstorm when thunderstorm.IsRealTimePatternActive():
                    return thunderstorm.GetRealTimePatternProbabilityProgress();
                case ForestFireModel forestFire when forestFire.IsRealTimePatternActive():
                    return forestFire.GetRealTimePatternProbabilityProgress();
                case TornadoModel tornado when tornado.IsRealTimePatternActive():
                    return tornado.GetRealTimePatternProbabilityProgress();
                case TsunamiModel tsunami:
                    return tsunami.GetProbabilityProgress();
                case EarthquakeModel earthquake:
                    return earthquake.GetProbabilityProgress();
                default:
                    return GetProbabilityProgressValueLog(occurrencePerYear);
            }
        }

        private void BuildStatusButton()
        {
            var statusButton = AddUIComponent<UIButton>();
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

        private void BuildMeteorPeriodBars()
        {
            var meteorStrike = Disaster as MeteorStrikeModel;
            if (meteorStrike == null)
                return;

            var periodStatuses = meteorStrike.GetMeteorPeriodStatuses();
            _meteorPeriodLabels = new UILabel[periodStatuses.Length];
            _meteorPeriodBars = new ProgressBarHelper[periodStatuses.Length];

            for (var i = 0; i < periodStatuses.Length; i++)
            {
                var yPosition = 22f + i * MeteorPeriodRowHeight;

                var label = AddUIComponent<UILabel>();
                label.relativePosition = new Vector3(MeteorPeriodLabelX, yPosition + 2f);
                label.textScale = LabelTextScaleTiny;
                label.textColor = UIStyleHelper.SecondaryTextColor;
                _meteorPeriodLabels[i] = label;

                var bar = AddUIComponent<ProgressBarHelper>();
                bar.Initialize(MeteorPeriodBarX, yPosition, BarWidth, ValueLabelY, LabelTextScaleTiny);
                _meteorPeriodBars[i] = bar;
            }
        }

        private void RefreshMeteorPeriodBars(bool isEnabled)
        {
            var meteorStrike = Disaster as MeteorStrikeModel;
            if (meteorStrike == null || _meteorPeriodBars == null || _meteorPeriodLabels == null)
            {
                height = 20f;
                return;
            }

            var periodsEnabled = meteorStrike.AreMeteorPeriodsEnabled();
            var periodStatuses = meteorStrike.GetMeteorPeriodStatuses();
            height = periodsEnabled ? 22f + periodStatuses.Length * MeteorPeriodRowHeight : 20f;

            for (var i = 0; i < _meteorPeriodBars.Length; i++)
            {
                var isPeriodVisible = periodsEnabled && i < periodStatuses.Length;
                _meteorPeriodLabels[i].isVisible = isPeriodVisible;
                _meteorPeriodBars[i].isVisible = isPeriodVisible;

                if (!isPeriodVisible)
                    continue;

                var status = periodStatuses[i];
                _meteorPeriodLabels[i].text = status.Name;
                _meteorPeriodBars[i].SetState(
                    isEnabled && status.Enabled,
                    status.ProbabilityMultiplier,
                    FormatPercentage(status.ProbabilityMultiplier),
                    status.Description);
            }
        }

        private static string FormatPercentage(float normalizedValue)
        {
            return string.Format("{0:00.00}%", normalizedValue * 100f);
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
