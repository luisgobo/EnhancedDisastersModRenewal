using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.DisasterServices.HarmonyPatches;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class TornadoModel : DisasterBaseModel
    {
        private const float GuaranteedOccurrencePerFrame = 1f;
        private const float SecondsPerMinute = 60f;
        private const float MaxRealTimeDeltaSeconds = 5f;
        private const float LightFogProgressFactor = 0.35f;
        private const float DenseFogProgressFactor = 0.1f;
        private const float MaxRainProgressBonus = 0.5f;
        private const string ExtendedInfoPanel2ModKey = "extendedInfoPanel2";

        private static readonly RealTimeDisasterFrequencyPreset[] RealTimeTornadoFrequencyOptionValues =
        {
            RealTimeDisasterFrequencyPreset.Apocalypse,
            RealTimeDisasterFrequencyPreset.Frequent,
            RealTimeDisasterFrequencyPreset.Occasional,
            RealTimeDisasterFrequencyPreset.Uncommon,
            RealTimeDisasterFrequencyPreset.Rare
        };

        [XmlIgnore] private float _lastRealTimeScheduleUpdateSeconds = -1f;
        public bool EnableTornadoDestruction = true;

        public int MaxProbabilityMonth = 5;
        public byte MinimalIntensityForDestruction = 10;
        public bool NoTornadoDuringFog = true;
        [XmlIgnore] public float RealTimeCurrentTornadoPeriodMinutes = -1f;
        [XmlIgnore] public float RealTimeMinutesUntilNextTornado = -1f;

        public RealTimeDisasterFrequencyPreset RealTimeTornadoFrequency =
            RealTimeDisasterFrequencyPreset.Occasional;

        public TornadoModel()
        {
            DType = DisasterType.Tornado;
            BaseOccurrencePerYear = 1.5f;
            ProbabilityDistribution = ProbabilityDistributions.PowerLow;

            calmDays = 360 * 2;
            probabilityWarmupDays = 180;
            intensityWarmupDays = 180;
            intensityWarmupDays = 180;
        }

        protected override void OnSimulationFrameLocal()
        {
            if (IsRealTimePatternActive())
            {
                ClearRealTimeCooldownState();
                UpdateRealTimeTornadoSchedule();
            }
        }

        protected override float GetCurrentOccurrencePerYearLocal()
        {
            if (!IsRealTimePatternActive() && NoTornadoDuringFog && GetCurrentFog() > 0f) return 0;

            if (IsRealTimePatternActive())
                return IsRealTimeTornadoDue()
                    ? 365f / GetSimulationDaysPerFrame() * GuaranteedOccurrencePerFrame
                    : 0f;

            return base.GetCurrentOccurrencePerYearLocal() * GetSeasonFactor();
        }

        protected override float GetSimulationDaysPerFrame()
        {
            return IsRealTimePatternActive()
                ? DisasterSimulationUtils.VanillaSimulationDaysPerFrame
                : base.GetSimulationDaysPerFrame();
        }

        public override string GetProbabilityTooltip(float value)
        {
            if (IsRealTimePatternActive())
                return GetRealTimeProbabilityTooltip(value);

            if (calmDaysLeft <= 0)
                if (!IsRealTimePatternActive() && NoTornadoDuringFog && GetCurrentFog() > 0f)
                    return LocalizationService.Format("tooltip.tornado.no_during_fog", GetName());

            return base.GetProbabilityTooltip(value);
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as TornadoAI != null;
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.Tornado;
            DisasterHelpersModified.DisasterIntensity = disasterInfo.intensity;
            DisasterHelpersModified.IntensityStartDestruction = MinimalIntensityForDestruction;
            DisasterHelpersModified.EnableDestruction = EnableTornadoDestruction;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Tornado;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;
            DisasterHelpersModified.DisasterIntensity = 0;
            DisasterHelpersModified.DisasterType = DisasterType.Empty;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified,
            ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.Tornado;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = true;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override string GetName()
        {
            return LocalizationService.GetDisasterName(DType);
        }

        protected override bool FindTarget(DisasterInfo disasterInfo, out Vector3 targetPosition, out float angle)
        {
            if (base.FindTarget(disasterInfo, out targetPosition, out angle))
                return true;

            if (TryFindRandomUnlockedAreaTarget(out targetPosition, out angle))
            {
                DebugLogger.Log(GetDebugStr() + string.Format(
                    "Tornado fallback target selected. Target: x:{0:#.##} y:{1:#.##} z:{2:#.##}",
                    targetPosition.x,
                    targetPosition.y,
                    targetPosition.z));
                return true;
            }

            return false;
        }

        public override void OnDisasterStarted(byte intensity)
        {
            if (IsRealTimePatternActive())
            {
                ResetRealTimeSchedule();
                ClearRealTimeCooldownState();
                intensityWarmupDaysLeft = intensityWarmupDays;
                return;
            }

            base.OnDisasterStarted(intensity);
        }

        public bool IsRealTimePatternActive()
        {
            return DisasterSimulationUtils.IsRealTimeModActive();
        }

        public override void OnEnabledChanged(bool enabled)
        {
            _lastRealTimeScheduleUpdateSeconds = Time.realtimeSinceStartup;
        }

        public float GetRealTimePatternProbabilityProgress()
        {
            EnsureRealTimeSchedule();
            return Mathf.Clamp01(1f - RealTimeMinutesUntilNextTornado / RealTimeCurrentTornadoPeriodMinutes);
        }

        private string GetRealTimeProbabilityTooltip(float probabilityValue)
        {
            EnsureRealTimeSchedule();
            var fogLine = GetFogProgressTooltipLine();
            var rainLine = GetRainProgressTooltipLine();
            return string.Format(
                "{0}: {1}{2}{3}{2}{4}{2}{5}{2}{6}{7}{8}",
                LocalizationService.Get("tooltip.progress"),
                string.Format("{0:00.00}%", probabilityValue * 100),
                CommonProperties.NewLine,
                LocalizationService.Get("tooltip.tornado.realtime_active"),
                LocalizationService.Format("tooltip.tornado.realtime_reference", GetRealTimeTornadoFrequencyName()),
                LocalizationService.Format(
                    "tooltip.tornado.current_tornado_interval",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeCurrentTornadoPeriodMinutes)),
                LocalizationService.Format(
                    "tooltip.tornado.tornado_time_remaining",
                    DisasterSimulationUtils.FormatRealTimeMinutes(RealTimeMinutesUntilNextTornado)),
                fogLine,
                rainLine);
        }

        private void UpdateRealTimeTornadoSchedule()
        {
            EnsureRealTimeSchedule();

            var elapsedMinutes = GetRealTimeElapsedMinutes();
            RealTimeMinutesUntilNextTornado = Mathf.Max(
                0f,
                RealTimeMinutesUntilNextTornado - elapsedMinutes * GetWeatherProgressFactor());
        }

        private bool IsRealTimeTornadoDue()
        {
            EnsureRealTimeSchedule();
            return RealTimeMinutesUntilNextTornado <= 0f;
        }

        private static float GetCurrentFog()
        {
            var wm = Services.Weather;
            return wm == null ? 0f : Mathf.Clamp01(wm.m_currentFog);
        }

        private static float GetCurrentRain()
        {
            var wm = Services.Weather;
            return wm == null ? 0f : Mathf.Clamp01(wm.m_currentRain);
        }

        private void ClearRealTimeCooldownState()
        {
            calmDaysLeft = 0f;
            probabilityWarmupDaysLeft = 0f;
        }

        private float GetFogProgressFactor()
        {
            if (!NoTornadoDuringFog)
                return 1f;

            var fog = GetCurrentFog();
            if (fog <= 0f)
                return 1f;

            return Mathf.Lerp(LightFogProgressFactor, DenseFogProgressFactor, fog);
        }

        private float GetRainProgressFactor()
        {
            var rain = GetCurrentRain();
            if (rain <= 0f)
                return 1f;

            return 1f + rain * MaxRainProgressBonus;
        }

        private float GetWeatherProgressFactor()
        {
            return GetFogProgressFactor() * GetRainProgressFactor();
        }

        private string GetFogProgressTooltipLine()
        {
            if (!NoTornadoDuringFog || GetCurrentFog() <= 0f)
                return string.Empty;

            return CommonProperties.NewLine + LocalizationService.Format(
                "tooltip.tornado.fog_slowed",
                string.Format("{0:0}", GetFogProgressFactor() * 100));
        }

        private string GetRainProgressTooltipLine()
        {
            if (GetCurrentRain() <= 0f)
                return string.Empty;

            return CommonProperties.NewLine + LocalizationService.Format(
                "tooltip.tornado.rain_increased",
                string.Format("{0:0}", GetRainProgressFactor() * 100));
        }

        private static bool TryFindRandomUnlockedAreaTarget(out Vector3 targetPosition, out float angle)
        {
            targetPosition = Vector3.zero;
            angle = 0f;

            var gameArea = Services.GameArea;
            var simulation = Services.Simulation;
            var terrain = Services.Terrain;
            if (gameArea == null || simulation == null || terrain == null)
                return false;

            var unlockedAreaCount = 0;
            var selectedX = -1;
            var selectedZ = -1;

            for (var z = 0; z < 5; z++)
            for (var x = 0; x < 5; x++)
            {
                if (gameArea.m_areaGrid[z * 5 + x] == 0)
                    continue;

                unlockedAreaCount++;
                if (simulation.m_randomizer.Int32((uint)unlockedAreaCount) == 0)
                {
                    selectedX = x;
                    selectedZ = z;
                }
            }

            if (selectedX < 0 || selectedZ < 0)
                return false;

            float minX;
            float minZ;
            float maxX;
            float maxZ;
            gameArea.GetAreaBounds(selectedX, selectedZ, out minX, out minZ, out maxX, out maxZ);

            var randX = simulation.m_randomizer.Int32(0, 10000) * 0.0001f;
            var randZ = simulation.m_randomizer.Int32(0, 10000) * 0.0001f;
            targetPosition.x = minX + (maxX - minX) * randX;
            targetPosition.z = minZ + (maxZ - minZ) * randZ;
            targetPosition.y = terrain.SampleRawHeightSmoothWithWater(targetPosition, false, 0f);
            angle = simulation.m_randomizer.Int32(0, 10000) * 0.0006283185f;
            return true;
        }

        private void EnsureRealTimeSchedule()
        {
            if (RealTimeCurrentTornadoPeriodMinutes <= 0f || RealTimeMinutesUntilNextTornado < 0f)
                ScheduleNextRealTimeTornado();

            if (RealTimeMinutesUntilNextTornado > RealTimeCurrentTornadoPeriodMinutes)
                RealTimeMinutesUntilNextTornado = RealTimeCurrentTornadoPeriodMinutes;
        }

        private void ResetRealTimeSchedule()
        {
            ScheduleNextRealTimeTornado();
        }

        private void ScheduleNextRealTimeTornado()
        {
            ScheduleNextRealTimeTornado(0f);
        }

        private void ScheduleNextRealTimeTornado(float progressToKeep)
        {
            var progress = Mathf.Clamp01(progressToKeep);
            RealTimeCurrentTornadoPeriodMinutes = GetRandomRealTimeIntervalMinutes();
            RealTimeMinutesUntilNextTornado = RealTimeCurrentTornadoPeriodMinutes * (1f - progress);
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
            switch (RealTimeTornadoFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    minMinutes = 15f;
                    maxMinutes = 30f;
                    break;
                case RealTimeDisasterFrequencyPreset.Frequent:
                    minMinutes = 30f;
                    maxMinutes = 90f;
                    break;
                case RealTimeDisasterFrequencyPreset.Occasional:
                    minMinutes = 120f;
                    maxMinutes = 240f;
                    break;
                case RealTimeDisasterFrequencyPreset.Uncommon:
                default:
                    minMinutes = 240f;
                    maxMinutes = 480f;
                    break;
                case RealTimeDisasterFrequencyPreset.Rare:
                    minMinutes = 480f;
                    maxMinutes = 960f;
                    break;
            }
        }

        private float GetSeasonFactor()
        {
            var dt = Services.Simulation.m_currentGameTime;
            var deltaMonth = Math.Abs(dt.Month - MaxProbabilityMonth);
            if (deltaMonth > 6) deltaMonth = 12 - deltaMonth;

            return Mathf.Clamp01(1f - deltaMonth / 6f);
        }

        public void SetRealTimeTornadoFrequency(RealTimeDisasterFrequencyPreset frequency)
        {
            if (RealTimeTornadoFrequency == frequency)
                return;

            var currentProgress = GetRealTimePatternProbabilityProgress();
            RealTimeTornadoFrequency = frequency;
            ScheduleNextRealTimeTornado(currentProgress);
        }

        public static string[] GetRealTimeTornadoFrequencyOptions()
        {
            return new[]
            {
                LocalizationService.Get("settings.tornado.frequency.apocalypse"),
                LocalizationService.Get("settings.tornado.frequency.frequent"),
                LocalizationService.Get("settings.tornado.frequency.occasional"),
                LocalizationService.Get("settings.tornado.frequency.uncommon"),
                LocalizationService.Get("settings.tornado.frequency.rare")
            };
        }

        public int GetRealTimeTornadoFrequencySelectionIndex()
        {
            for (var i = 0; i < RealTimeTornadoFrequencyOptionValues.Length; i++)
                if (RealTimeTornadoFrequencyOptionValues[i] == RealTimeTornadoFrequency)
                    return i;

            return 2;
        }

        public static RealTimeDisasterFrequencyPreset GetRealTimeTornadoFrequencyFromSelection(int selection)
        {
            if (selection < 0 || selection >= RealTimeTornadoFrequencyOptionValues.Length)
                return RealTimeDisasterFrequencyPreset.Occasional;

            return RealTimeTornadoFrequencyOptionValues[selection];
        }

        public string GetRealTimeTornadoFrequencyTooltip()
        {
            return LocalizationService.Format(
                "settings.tornado.realtime_frequency.tooltip.selected",
                GetRealTimeTornadoFrequencyName());
        }

        private string GetRealTimeTornadoFrequencyName()
        {
            switch (RealTimeTornadoFrequency)
            {
                case RealTimeDisasterFrequencyPreset.Apocalypse:
                    return LocalizationService.Get("settings.tornado.frequency_name.apocalypse");
                case RealTimeDisasterFrequencyPreset.Frequent:
                    return LocalizationService.Get("settings.tornado.frequency_name.frequent");
                case RealTimeDisasterFrequencyPreset.Occasional:
                    return LocalizationService.Get("settings.tornado.frequency_name.occasional");
                case RealTimeDisasterFrequencyPreset.Uncommon:
                    return LocalizationService.Get("settings.tornado.frequency_name.uncommon");
                case RealTimeDisasterFrequencyPreset.Rare:
                    return LocalizationService.Get("settings.tornado.frequency_name.rare");
                default:
                    return RealTimeTornadoFrequency.ToString();
            }
        }

        public override float CalculateDestructionRadio(byte intensity)
        {
            var unitSize = 8;
            var unitsBase = 72;
            float unitCalculation;
            var intensityInt = intensity / 10;
            var intensityDec = intensity % 10;

            switch (intensity)
            {
                case byte n when n <= 25:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase +
                                      (intensityDec * 2.48f + intensityInt * 32.8f);
                    break;

                case byte n when n > 25 && n <= 50:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase - 4f +
                                      (intensityDec * 2.64f + intensityInt * 34.4f);
                    break;

                case byte n when n > 50 && n <= 75:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 170f +
                                      intensityDec * 0.16f + (intensityDec - 1) * 2 + (intensityInt - 5) * 29.6f;
                    break;

                case byte n when n > 75 && n <= 100:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 240f +
                                      (intensityDec - 5) * 0.48f + (intensityDec - 6) * 2 + (intensityInt - 7) * 32.8f;
                    break;

                case byte n when n > 100 && n <= 125:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 326f +
                                      intensityDec * 0.08f + (intensityDec - 1) * 2 + (intensityInt - 10) * 28.8f;
                    break;

                case byte n when n > 125 && n <= 150:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 393f +
                                      (intensityDec - 5) * 0.68f + (intensityDec - 6) + (intensityInt - 12) * 24.8f;
                    break;

                case byte n when n > 150 && n <= 175:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 461f +
                                      intensityDec * 0.28f + (intensityDec - 1) * 3 + (intensityInt - 15) * 40.8f;
                    break;

                case byte n when n > 175 && n <= 200:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 534f +
                                      (intensityDec - 5) * 0.24f + (intensityDec + 6) * 2 + (intensityInt - 17) * 30.4f;
                    break;

                case byte n when n > 200 && n <= 250:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 638f +
                                      (intensityDec - 1) * 2 + (intensityInt - 20) * 28;
                    break;

                case byte n when n > 225 && n <= 250:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 702 +
                                      (intensityDec - 5) * 2.16f + (intensityInt - 22) * 29.6f;
                    break;

                default:
                    unitCalculation = ((intensityInt - 5f) * -10f + intensityDec) * 0.4f + unitsBase + 702 +
                                      (intensityDec - 5) * 2.16f + (intensityInt - 22) * 29.6f + intensityDec * 0.24f;
                    break;
            }

            return (float)Math.Sqrt(unitCalculation / 2 * unitSize);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            if (disaster is TornadoModel tornado)
            {
                MaxProbabilityMonth = tornado.MaxProbabilityMonth;
                NoTornadoDuringFog = tornado.NoTornadoDuringFog;
                EnableTornadoDestruction = tornado.EnableTornadoDestruction;
                MinimalIntensityForDestruction = tornado.MinimalIntensityForDestruction;
                SetRealTimeTornadoFrequency(tornado.RealTimeTornadoFrequency);
            }
        }
    }
}
