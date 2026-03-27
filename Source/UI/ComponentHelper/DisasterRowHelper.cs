using System;
using ColossalFramework.UI;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper;

public sealed class DisasterRowHelper : UIPanel
{
    private const string PauseSprite = "ButtonPause";
    private const string PlaySprite = "ButtonPlayFocused";
    private const float LabelTextScaleNormal = 0.8f;
    private const float LabelTextScaleTiny = 0.6f;
    private const float MaxIntensity = 255f;
    private const float RowWidth = 410f;
    private const float NameLabelX = 26f;
    public const float probabilityBarX = 195f;
    public const float intensityBarX = 295f;
    public const float barWidth = 90f;
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
        _nameLabel.relativePosition = new Vector3(NameLabelX, 0f);
        _nameLabel.textScale = LabelTextScaleNormal;

        _probabilityBar = AddUIComponent<ProgressBarHelper>();
        _probabilityBar.Initialize(probabilityBarX, 0f, barWidth, ValueLabelY, LabelTextScaleTiny);

        _intensityBar = AddUIComponent<ProgressBarHelper>();
        _intensityBar.Initialize(intensityBarX, 0f, barWidth, ValueLabelY, LabelTextScaleTiny);
    }

    public void Refresh()
    {
        var disasterIsDisasterEnabled = Disaster.IsDisasterEnabled;
        var maxIntensityCalculated = Disaster.GetMaximumIntensity();

        _statusIcon.spriteName = disasterIsDisasterEnabled ? PauseSprite : PlaySprite;
        _nameLabel.text = disasterIsDisasterEnabled ? Disaster.GetName() : $"{Disaster.GetName()} - Disabled";

        _probabilityBar.SetState(
            disasterIsDisasterEnabled,
            Disaster.GetDisasterProbability(),
            disasterIsDisasterEnabled ? Disaster.GetDisasterProbabilityPercentageValue() : string.Empty,
            disasterIsDisasterEnabled ? Disaster.GetTooltipInformation() : string.Empty);

        var normalizedIntensity = maxIntensityCalculated / MaxIntensity;
        _intensityBar.SetState(
            disasterIsDisasterEnabled,
            normalizedIntensity,
            disasterIsDisasterEnabled ? $"{maxIntensityCalculated / 10f:0.0}" : string.Empty,
            disasterIsDisasterEnabled ? Disaster.GetIntensityTooltip(normalizedIntensity) : string.Empty);
    }

    private void BuildStatusButton()
    {
        var statusButton = AddUIComponent<UIButton>();
        statusButton.name = $"disasterState{Disaster.GetName()}Btn";
        statusButton.relativePosition = new Vector3(0f, -4f);
        statusButton.size = new Vector2(18f, 18f);
        statusButton.normalBgSprite = "ButtonMenu";
        statusButton.hoveredBgSprite = "ButtonMenuHovered";
        statusButton.eventClick += (component, eventParam) =>
        {
            if (_toggleHandler != null)
                _toggleHandler(Disaster);
        };

        _statusIcon = statusButton.AddUIComponent<UISprite>();
        _statusIcon.size = new Vector2(12f, 12f);
        _statusIcon.relativePosition = new Vector2(3f, 3f);
    }
}