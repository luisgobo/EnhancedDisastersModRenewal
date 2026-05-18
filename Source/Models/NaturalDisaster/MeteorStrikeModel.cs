using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class MeteorStrikeModel : DisasterBaseModel
    {
        private const float DefaultRealTimeFrequencyMultiplier = 4f;
        private const float DefaultRealTimePeriodDays = 146f;
        private const float GuaranteedOccurrencePerFrame = 1f;
        private const float SecondsPerMinute = 60f;
        private const float MaxRealTimeDeltaSeconds = 5f;
        private const string ExtendedInfoPanel2ModKey = "extendedInfoPanel2";
        private static readonly RealTimeDisasterFrequencyPreset[] RealTimeMeteorFrequencyOptionValues =
        {
            RealTimeDisasterFrequencyPreset.Apocalypse,
            RealTimeDisasterFrequencyPreset.Frequent,
            RealTimeDisasterFrequencyPreset.Occasional,
            RealTimeDisasterFrequencyPreset.Uncommon,
            RealTimeDisasterFrequencyPreset.Rare
        };

        [XmlIgnore] private float _lastRealTimeScheduleUpdateSeconds = -1f;

        [XmlIgnore] public readonly MeteorEvent[] MeteorEvents;
        [XmlIgnore] public float RealTimeCurrentPeriodMinutes = -1f;
        [XmlIgnore] public float RealTimeDaysUntilNextMeteor = -1f;
        private float _realTimeFrequencyMultiplier = DefaultRealTimeFrequencyMultiplier;
        public RealTimeDisasterFrequencyPreset RealTimeMeteorFrequency = RealTimeDisasterFrequencyPreset.Occasional;
        [XmlIgnore] public float RealTimeMinutesUntilNextMeteor = -1f;
        private float _realTimePeriodDays = DefaultRealTimePeriodDays;

        public MeteorStrikeModel()
        {
            DType = DisasterType.MeteorStrike;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
            BaseOccurrencePerYear = 10.0f;
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            //Change
            MeteorEvents = new[]
            {
                MeteorEvent.Init("Long period meteor", 9, 200),
                MeteorEvent.Init("Medium period meteor", 5, 120),
                MeteorEvent.Init("Short period meteor", 2, 30)
            };
        }

        [XmlElement]
        public bool MeteorLongPeriodEnabled
        {
            get => GetEnabled(0);

            set => SetEnabled(0, value);
        }

        [XmlElement]
        public bool MeteorMediumPeriodEnabled
        {
            get => GetEnabled(1);

            set => SetEnabled(1, value);
        }

        [XmlElement]
        public bool MeteorShortPeriodEnabled
        {
            get => GetEnabled(2);

            set => SetEnabled(2, value);
        }

        [XmlElement]
        public float RealTimeFrequencyMultiplier
        {
            get => _realTimeFrequencyMultiplier;
            set
            {
                _realTimeFrequencyMultiplier = Mathf.Max(1f, value);
                RealTimePeriodDays = GetPeriodDaysFromFrequencyMultiplier();
            }
        }

        [XmlElement]
        public float RealTimePeriodDays
        {
            get => _realTimePeriodDays;
            set
            {
                _realTimePeriodDays = Mathf.Max(1f, value);
                if (RealTimeDaysUntilNextMeteor > _realTimePeriodDays)
                    RealTimeDaysUntilNextMeteor = _realTimePeriodDays;
            }
        }

        public bool GetEnabled(int index)
        {
            return MeteorEvents[index].Enabled;
        }

        public void SetEnabled(int index, bool value)
        {
            MeteorEvents[index].Enabled = value;
        }

        protected override void OnSimulationFrameLocal()
        {
            if (IsRealTimePatternActive())
            {
                EnsureRealTimeSchedule();
                RealTimeMinutesUntilNextMeteor = Mathf.Max(
                    0f,
                    RealTimeMinutesUntilNextMeteor - GetRealTimeElapsedMinutes());
                return;
            }

            var daysPerFrame = DisasterSimulationUtils.VanillaSimulationDaysPerFrame;

            for (var i = 0; i < MeteorEvents.Length; i++) MeteorEvents[i].OnSimulationFrame(daysPerFrame);
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.MeteorStrike;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.MeteorStrike;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.MeteorStrike;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterStarted(byte intensity)
        {
            if (IsRealTimePatternActive())
            {
                base.OnDisasterStarted(intensity);
                ResetRealTimeSchedule();
                return;
            }

            var meteorIndex = -1;
            float maxProbability = 0;

            for (var i = 0; i < MeteorEvents.Length; i++)
            {
                var prob = MeteorEvents[i].GetProbabilityMultiplier();
                
                if (!(prob > maxProbability)) continue;
                
                maxProbability = prob;
                meteorIndex = i;
            }

            // Should not happen
            if (meteorIndex == -1) meteorIndex = 2;

            MeteorEvents[meteorIndex].OnMeteorFallen();

            base.OnDisasterStarted(intensity);
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            if (IsRealTimePatternActive())
                return IsRealTimeMeteorDue()
                    ? 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame
                    : 0f;

            for (var i = 0; i < MeteorEvents.Length; i++)
                if (MeteorEvents[i].IsDue())
                    return 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame;

            return 0f;
        }

        protected override float GetSimulationDaysPerFrame()
        {
            return IsRealTimePatternActive()
                ? DisasterSimulationUtils.VanillaSimulationDaysPerFrame
                : base.GetSimulationDaysPerFrame();
        }

        public override byte GetMaximumIntensity()
        {
            if (IsRealTimePatternActive())
                return ScaleIntensityByPopulation(baseIntensity);

            var intensity = baseIntensity;

            for (var i = 0; i < MeteorEvents.Length; i++)
                intensity = Math.Max(intensity, MeteorEvents[i].GetActualMaxIntensity());

            intensity = ScaleIntensityByPopulation(intensity);

            return intensity;
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as MeteorStrikeAI != null;
        }

        public override string GetName()
        {
            return LocalizationService.GetDisasterName(DType);
        }

        public override string GetProbabilityTooltip(float probabilityValue)
        {
            if (!unlocked) return "Not unlocked yet";

            if (IsRealTimePatternActive())
                return string.Format(
                    "Progress: {0}{1}Real Time active: meteor periods are disabled.{1}{2}{1}Current interval: {3}.{1}Time remaining: {4}.",
                    string.Format("{0:00.00}%", probabilityValue * 100),
                    CommonProperties.NewLine,
                    LocalizationService.Format("tooltip.meteor.realtime_reference", GetRealTimeMeteorFrequencyName()),
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeCurrentPeriodMinutes),
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeMinutesUntilNextMeteor));

            var probability =
                string.Format("Probability: {0:00.00}% {1}", probabilityValue * 100, CommonProperties.NewLine);

            for (var i = 0; i < MeteorEvents.Length; i++)
                probability += MeteorEvents[i].GetStateDescription() + Environment.NewLine;

            return probability;
        }

        public bool AreMeteorPeriodsEnabled()
        {
            return !IsRealTimePatternActive();
        }

        public override void OnEnabledChanged(bool enabled)
        {
            _lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        public float GetMeteorPeriodProbabilityProgress()
        {
            var progress = 0f;

            for (var i = 0; i < MeteorEvents.Length; i++)
                progress = Mathf.Max(progress, MeteorEvents[i].GetProbabilityMultiplier());

            return progress;
        }

        public override void SetDebugProbabilityProgress(float progress)
        {
            base.SetDebugProbabilityProgress(progress);

            var clampedProgress = Mathf.Clamp01(progress);
            calmDaysLeft = 0f;
            probabilityWarmupDaysLeft = 0f;
            intensityWarmupDaysLeft = 0f;

            if (IsRealTimePatternActive())
            {
                ScheduleNextRealTimeMeteor(clampedProgress);
                return;
            }

            for (var i = 0; i < MeteorEvents.Length; i++)
            {
                MeteorEvents[i].MeteorsFallen = 0;
                MeteorEvents[i].DaysUntilNextEvent = MeteorEvents[i].PeriodDays * (1f - clampedProgress);
            }
        }

        public float GetRealTimePatternProbabilityProgress()
        {
            EnsureRealTimeSchedule();
            return Mathf.Clamp01(1f - RealTimeMinutesUntilNextMeteor / RealTimeCurrentPeriodMinutes);
        }

        private bool IsRealTimeMeteorDue()
        {
            EnsureRealTimeSchedule();
            return RealTimeMinutesUntilNextMeteor <= 0f;
        }

        private void EnsureRealTimeSchedule()
        {
            if (RealTimeCurrentPeriodMinutes <= 0f || RealTimeMinutesUntilNextMeteor < 0f)
                ScheduleNextRealTimeMeteor();

            if (RealTimeMinutesUntilNextMeteor > RealTimeCurrentPeriodMinutes)
                RealTimeMinutesUntilNextMeteor = RealTimeCurrentPeriodMinutes;
        }

        private void ResetRealTimeSchedule()
        {
            ScheduleNextRealTimeMeteor();
        }

        private float GetPeriodDaysFromFrequencyMultiplier()
        {
            var effectiveOccurrencePerYear = BaseOccurrencePerYear / RealTimeFrequencyMultiplier;
            return 365f / Mathf.Max(0.0001f, effectiveOccurrencePerYear);
        }

        private void ScheduleNextRealTimeMeteor()
        {
            ScheduleNextRealTimeMeteor(0f);
        }

        private void ScheduleNextRealTimeMeteor(float progressToKeep)
        {
            var progress = Mathf.Clamp01(progressToKeep);
            RealTimeCurrentPeriodMinutes = GetRandomRealTimeIntervalMinutes();
            RealTimeMinutesUntilNextMeteor = RealTimeCurrentPeriodMinutes * (1f - progress);
            _lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        private float GetRealTimeElapsedMinutes()
        {
            var currentSeconds = Time.realtimeSinceStartup;

            if (_lastRealTimeScheduleUpdateSeconds < 0f)
            {
                _lastRealTimeScheduleUpdateSeconds = currentSeconds;
                return 0f;
            }

            var elapsedSeconds = Mathf.Clamp(
                currentSeconds - _lastRealTimeScheduleUpdateSeconds,
                0f,
                MaxRealTimeDeltaSeconds);
            _lastRealTimeScheduleUpdateSeconds = currentSeconds;
            return elapsedSeconds / SecondsPerMinute * GetRealTimeSpeedFactor();
        }

        private static float GetRealTimeSpeedFactor()
        {
            var simulation = Services.Simulation;
            if (simulation == null || simulation.SimulationPaused)
                return 0f;

            var speed = Mathf.Max(1, simulation.FinalSimulationSpeed);
            if (ModCompatibilityService.IsActive(ExtendedInfoPanel2ModKey))
                return GetExtendedInfoPanelSpeedFactor(speed);

            return GetVanillaSpeedFactor(speed);
        }

        private static float GetVanillaSpeedFactor(int speed)
        {
            if (speed <= 1)
                return 1f;

            return speed == 2 ? 1.5f : 2f;
        }

        private static float GetExtendedInfoPanelSpeedFactor(int speed)
        {
            if (speed <= 3)
                return GetVanillaSpeedFactor(speed);

            switch (speed)
            {
                case 4:
                    return 2.5f;
                case 5:
                    return 3f;
                default:
                    return 3.5f;
            }
        }

        private float GetRandomRealTimeIntervalMinutes()
        {
            GetRealTimeIntervalRange(out var minMinutes, out var maxMinutes);

            var randomValue = Services.Simulation.m_randomizer.Int32(0, 10000) / 10000f;
            return minMinutes + (maxMinutes - minMinutes) * randomValue;
        }

        private void GetRealTimeIntervalRange(out float minMinutes, out float maxMinutes)
        {
            switch (RealTimeMeteorFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    minMinutes = 5f;
                    maxMinutes = 12f;
                    break;
                case RealTimeDisasterFrequencyPreset.Frequent:
                    minMinutes = 15f;
                    maxMinutes = 45f;
                    break;
                case RealTimeDisasterFrequencyPreset.Occasional:
                default:
                    minMinutes = 45f;
                    maxMinutes = 90f;
                    break;
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    minMinutes = 90f;
                    maxMinutes = 180f;
                    break;
                case RealTimeDisasterFrequencyPreset.Rare:
                    minMinutes = 180f;
                    maxMinutes = 360f;
                    break;
            }
        }

        // TODO: Review meteor timing presets and period defaults.
        // Requirements:
        // - Increase meteor intervals so automatic strikes feel less frequent across Real Time presets.
        // - Revisit vanilla meteor stream periods if the non-Real-Time cadence still feels too compressed.
        // - Keep Apocalypse/Frequent settings clearly faster than normal gameplay, but less spammy than current values.
        // - Preserve existing saved games by migrating or clamping old shorter intervals safely.

        public void SetRealTimeMeteorFrequency(RealTimeDisasterFrequencyPreset frequency)
        {
            if (RealTimeMeteorFrequency == frequency)
                return;

            var currentProgress = GetRealTimePatternProbabilityProgress();
            RealTimeMeteorFrequency = frequency;
            ScheduleNextRealTimeMeteor(currentProgress);
        }

        public static string[] GetRealTimeMeteorFrequencyOptions()
        {
            return new[]
            {
                LocalizationService.Get("settings.meteor.frequency.apocalypse"),
                LocalizationService.Get("settings.meteor.frequency.frequent"),
                LocalizationService.Get("settings.meteor.frequency.occasional"),
                LocalizationService.Get("settings.meteor.frequency.uncommon"),
                LocalizationService.Get("settings.meteor.frequency.rare")
            };
        }

        public int GetRealTimeMeteorFrequencySelectionIndex()
        {
            for (var i = 0; i < RealTimeMeteorFrequencyOptionValues.Length; i++)
                if (RealTimeMeteorFrequencyOptionValues[i] == RealTimeMeteorFrequency)
                    return i;

            return 3;
        }

        public static RealTimeDisasterFrequencyPreset GetRealTimeMeteorFrequencyFromSelection(int selection)
        {
            if (selection < 0 || selection >= RealTimeMeteorFrequencyOptionValues.Length)
                return RealTimeDisasterFrequencyPreset.Uncommon;

            return RealTimeMeteorFrequencyOptionValues[selection];
        }

        public string GetRealTimeMeteorFrequencyTooltip()
        {
            return LocalizationService.Format(
                "settings.meteor.realtime_frequency.tooltip.selected",
                GetRealTimeMeteorFrequencyName());
        }

        private string GetRealTimeMeteorFrequencyName()
        {
            switch (RealTimeMeteorFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    return LocalizationService.Get("settings.meteor.frequency_name.apocalypse");
                case RealTimeDisasterFrequencyPreset.Frequent:
                    return LocalizationService.Get("settings.meteor.frequency_name.frequent");
                case RealTimeDisasterFrequencyPreset.Occasional:
                    return LocalizationService.Get("settings.meteor.frequency_name.occasional");
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    return LocalizationService.Get("settings.meteor.frequency_name.uncommon");
                case RealTimeDisasterFrequencyPreset.Rare:
                    return LocalizationService.Get("settings.meteor.frequency_name.rare");
                default:
                    return RealTimeMeteorFrequency.ToString();
            }
        }

        public MeteorPeriodStatus[] GetMeteorPeriodStatuses()
        {
            var statuses = new MeteorPeriodStatus[MeteorEvents.Length];
            var periodsEnabled = AreMeteorPeriodsEnabled();

            for (var i = 0; i < MeteorEvents.Length; i++)
                statuses[i] = new MeteorPeriodStatus
                {
                    Name = MeteorEvents[i].Name,
                    Enabled = periodsEnabled && MeteorEvents[i].Enabled,
                    ProbabilityMultiplier = periodsEnabled ? MeteorEvents[i].GetProbabilityMultiplier() : 0f,
                    MaxIntensity = MeteorEvents[i].MaxIntensity,
                    Description = periodsEnabled
                        ? MeteorEvents[i].GetStateDescription()
                        : "Disabled while Real Time is active."
                };

            return statuses;
        }

        private static bool IsRealTimePatternActive()
        {
            return DisasterSimulationUtils.IsRealTimeModActive();
        }

        // TODO: Evaluate a configurable meteor targeting mode that prefers populated areas.
        // Requirements:
        // - Keep the current vanilla/random targeting as the default behavior.
        // - Add an optional "near populated areas" mode that selects a populated building or dense area.
        // - Prefer an offset around the populated target instead of always hitting the building directly.
        // - Use a safe fallback to vanilla/random when no valid populated target is found.
        // - Apply the same target selection regardless of Real Time being active; Real Time only changes when meteors occur.
        // - Consider a future setting for direct-hit risk or offset radius before implementing.    
        // TODO: Evaluate coastal shelter evacuation for meteor strikes that target water.
        // Requirements:
        // - Detect water impacts by comparing terrain height against terrain-with-water height at the meteor target.
        // - Add an optional setting, disabled by default, to activate coastal shelters when a detected meteor will hit water.
        // - Infer coastal shelters by water proximity/elevation because the game does not expose a dedicated coastal shelter type.
        // - Limit activation by distance from the impact and/or unlocked areas so inland shelters are not evacuated unnecessarily.
        // - Keep normal focused evacuation behavior unchanged for land impacts.

        public override float CalculateDestructionRadio(byte intensity)
        {
            var unitSize = 8;
            var unitsBase = 24; //24 Original, Distance Fix for proximity
            float unitCalculation;
            var intensityInt = intensity / 10;
            var intensityDec = intensity % 10;

            switch (intensity)
            {
                case byte n when n < 25:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase + 4 -
                                      intensityDec * 0.04f - intensityInt * 0.4f;
                    break;

                case byte n when n >= 25 && n < 50:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase + 3 -
                                      (intensityInt - 2) * 0.8f - (intensityDec - 5) * 0.08f;
                    break;

                case byte n when n >= 50 && n <= 55:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase + 1;
                    break;

                case byte n when n > 55 && n <= 75:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase + 1 -
                                      (intensityInt - 5) * 0.5f - (intensityDec - 5) * 0.05f;
                    break;

                case byte n when n > 75 && n <= 100:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase -
                                      (intensityInt - 7) * 0.8f - (intensityDec - 5) * 0.08f;
                    break;

                case byte n when n > 100 && n <= 125:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase - 2 -
                                      (intensityInt - 10) * 0.4f - intensityDec * 0.04f;
                    break;

                case byte n when n > 125 && n <= 150:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase - 3 +
                                      (intensityInt - 12) * 0.4f + (intensityDec - 5) * 0.04f;
                    break;

                case byte n when n > 150 && n <= 175:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase - 2 -
                                      (intensityInt - 15) * 1.2f - intensityDec * 0.12f;
                    break;

                case byte n when n > 175 && n <= 250:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase - 5 -
                                      (intensityInt - 17) * 0.4f - (intensityDec - 5) * 0.04f;
                    break;

                default:
                    unitCalculation = ((intensityInt - 5) * 10 + intensityDec) * 0.4f + unitsBase - 5 -
                        (intensityInt - 17) * 0.4f - (intensityDec - 5) * 0.04f + intensityDec * 0.24f;
                    break;
            }

            return (float)Math.Sqrt(unitCalculation / 2 * unitSize);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            var d = disaster as MeteorStrikeModel;
            if (d != null)
            {
                MeteorLongPeriodEnabled = d.MeteorLongPeriodEnabled;
                MeteorMediumPeriodEnabled = d.MeteorMediumPeriodEnabled;
                MeteorShortPeriodEnabled = d.MeteorShortPeriodEnabled;
                RealTimeFrequencyMultiplier = d.RealTimeFrequencyMultiplier;
                RealTimePeriodDays = d.RealTimePeriodDays;
                SetRealTimeMeteorFrequency(d.RealTimeMeteorFrequency);
            }
        }

        public struct MeteorPeriodStatus
        {
            public string Name;
            public bool Enabled;
            public float ProbabilityMultiplier;
            public byte MaxIntensity;
            public string Description;
        }

        public struct MeteorEvent
        {
            public string Name;
            public float PeriodDays;
            public byte MaxIntensity;
            public float DaysUntilNextEvent;
            public int MeteorsFallen;
            public bool Enabled;

            public MeteorEvent(string name, float periodDays, byte maxIntensity, float daysUntilNextEvent)
            {
                Name = name;
                PeriodDays = periodDays;
                MaxIntensity = maxIntensity;
                DaysUntilNextEvent = daysUntilNextEvent;
                MeteorsFallen = 0;
                Enabled = true;
            }

            public static MeteorEvent Init(string eventName, float periodYears, byte maxIntensity)
            {
                var sm = Services.Simulation;

                var periodDays = periodYears * 365;

                var periodDaysCalc = periodDays * 0.95f + sm.m_randomizer.Int32((uint)(periodDays * 0.1f));
                var daysUntilNextEventCalc = periodDays / 2 + sm.m_randomizer.Int32((uint)(periodDays / 2));

                return new MeteorEvent(
                    eventName,
                    periodDaysCalc,
                    maxIntensity,
                    daysUntilNextEventCalc
                );
            }

            public float GetProbabilityMultiplier()
            {
                if (!Enabled) return 0;

                if (MeteorsFallen > 0) return 0;

                if (IsDue())
                    return 1f;

                return Mathf.Clamp01(1f - DaysUntilNextEvent / PeriodDays);
            }

            public bool IsDue()
            {
                return Enabled && MeteorsFallen == 0 && DaysUntilNextEvent <= 0f;
            }

            public byte GetActualMaxIntensity()
            {
                if (!Enabled) return 1;

                if (DaysUntilNextEvent < 60) return MaxIntensity;

                return 1;
            }

            public void OnSimulationFrame(float daysPerFrame)
            {
                if (!Enabled) return;

                DaysUntilNextEvent -= daysPerFrame;
            }

            public void OnMeteorFallen()
            {
                DaysUntilNextEvent = PeriodDays;
                MeteorsFallen = 0;
            }

            public override string ToString()
            {
                return string.Format("Period {0} years, max intensity {1}, next meteor in {2}",
                    PeriodDays / 365, MaxIntensity, DisasterSimulationUtils.FormatTimeSpan(DaysUntilNextEvent));
            }

            public string GetStateDescription()
            {
                if (!Enabled) return "";

                if (MeteorsFallen > 0) return Name + " already fallen.";

                if (GetProbabilityMultiplier() >= 0.9f)
                    return Name + " is approaching. Time remaining: " +
                           DisasterSimulationUtils.FormatTimeSpan(DaysUntilNextEvent);

                return Name + " will be close in " + DisasterSimulationUtils.FormatTimeSpan(DaysUntilNextEvent);
            }
        }
    }
}
