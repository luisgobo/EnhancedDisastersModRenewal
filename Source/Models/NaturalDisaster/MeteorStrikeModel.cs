using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.Disaster;
using UnityEngine;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Models.NaturalDisaster
{
    public class MeteorStrikeModel : DisasterBaseModel
    {
        [XmlIgnore] public MeteorEvent[] MeteorEvents;
        
        private const float DefaultRealTimeFrequencyMultiplier = 4f;
        private readonly bool _isRealTimeActive = CommonServices.DisasterHandler.CheckRealTimeModActive();

        protected override TimeBehaviorMode CurrentTimeBehaviorMode => TimeBehaviorMode.VanillaSimulationCompatible;

        public struct MeteorEvent
        {
            private readonly string _name;
            public float PeriodDays;
            public byte MaxIntensity;
            public float DaysUntilNextEvent;
            public int MeteorsFallen;
            public bool Enabled;

           

            public MeteorEvent(string name, float periodDays, byte maxIntensity, float daysUntilNextEvent)
            {
                _name = name;
                PeriodDays = periodDays;
                MaxIntensity = maxIntensity;
                DaysUntilNextEvent = daysUntilNextEvent;
                MeteorsFallen = 0;
                Enabled = true;
            }

            public static MeteorEvent Init(string eventName, float periodYears, byte maxIntensity)
            {
                var simulationManager = Singleton<SimulationManager>.instance;

                var periodDays = periodYears * 365;
                var periodDaysCalc = periodDays * 0.95f + simulationManager.m_randomizer.Int32((uint)(periodDays * 0.1f));
                var daysUntilNextEventCalc = periodDays / 2 + simulationManager.m_randomizer.Int32((uint)(periodDays / 2));

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

                if (MeteorsFallen > 0)
                {
                    return 0;
                }

                const float fallPeriodHalfInDays = 30;
                var daysDiffFromPeak = Mathf.Abs(DaysUntilNextEvent - fallPeriodHalfInDays);
                var multiplier = Mathf.Max(0, 1f - daysDiffFromPeak / fallPeriodHalfInDays);

                return multiplier;
            }

            public byte GetActualMaxIntensity()
            {
                if (!Enabled) return 1;

                return DaysUntilNextEvent < 60 ? MaxIntensity : (byte)1;

            }

            public void OnSimulationFrame(float daysPerFrame)
            {
                if (!Enabled) return;

                DaysUntilNextEvent -= daysPerFrame;

                if (!(DaysUntilNextEvent <= 0)) return;
                DaysUntilNextEvent = PeriodDays;
                MeteorsFallen = 0;
            }

            public void OnMeteorFallen()
            {
                if (MeteorsFallen > 0) return;

                MeteorsFallen++;
            }

            public override string ToString()
            {
                return $"Period {PeriodDays / 365} years, max intensity {MaxIntensity}, next meteor in {Helper.FormatTimeSpan(DaysUntilNextEvent)}";
            }

            public string GetStateDescription()
            {
                if (!Enabled) return "";

                if (MeteorsFallen > 0)
                {
                    return LocalizationService.Format("tooltip.meteor.alreadyFallen", _name);
                }

                return DaysUntilNextEvent <= 60 ?
                    LocalizationService.Format("tooltip.meteor.approaching", _name) :
                    LocalizationService.Format("tooltip.meteor.closeIn", _name, Helper.FormatTimeSpan(DaysUntilNextEvent));
            }
        }

        

        public MeteorStrikeModel()
        {
            DType = DisasterType.MeteorStrike;
            OccurrenceAreaAfterUnlock = OccurrenceAreas.UnlockedAreas;
            BaseOccurrencePerYear = 10.0f;
            ProbabilityDistribution = ProbabilityDistributions.Uniform;

            //Change
            MeteorEvents =
            [
                MeteorEvent.Init("Long period meteor", 9, 200),
                MeteorEvent.Init("Medium period meteor", 5, 120),
                MeteorEvent.Init("Short period meteor", 2, 30)
            ];
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
            get => field;

            set => field = Mathf.Max(1f, value);
        } = DefaultRealTimeFrequencyMultiplier;

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
            var daysPerFrame = Helper.GetDaysPerFrame(CurrentTimeBehaviorMode);
            if (_isRealTimeActive)
            {
                daysPerFrame *= Mathf.Max(1f, RealTimeFrequencyMultiplier);
            }

            for (var i = 0; i < MeteorEvents.Length; i++)
            {
                MeteorEvents[i].OnSimulationFrame(daysPerFrame);
            }
        }

        public override void OnDisasterActivated(DisasterSettings disasterInfo, ushort disasterId, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfo.type |= DisasterType.MeteorStrike;
            base.OnDisasterActivated(disasterInfo, disasterId, ref activeDisasters);
        }

        public override void OnDisasterDeactivated(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type |= DisasterType.MeteorStrike;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;
            base.OnDisasterDeactivated(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterDetected(DisasterInfoModel disasterInfoUnified, ref List<DisasterInfoModel> activeDisasters)
        {
            disasterInfoUnified.DisasterInfo.type = DisasterType.MeteorStrike;
            disasterInfoUnified.EvacuationMode = EvacuationMode;
            disasterInfoUnified.IgnoreDestructionZone = false;

            base.OnDisasterDetected(disasterInfoUnified, ref activeDisasters);
        }

        public override void OnDisasterStarted(byte intensity)
        {
            int meteorIndex = -1;
            float maxProbability = 0;

            for (var i = 0; i < MeteorEvents.Length; i++)
            {
                var prob = MeteorEvents[i].GetProbabilityMultiplier();
                if (prob > maxProbability)
                {
                    maxProbability = prob;
                    meteorIndex = i;
                }
            }

            // Should not happen
            if (meteorIndex == -1)
            {
                meteorIndex = 2;
            }

            MeteorEvents[meteorIndex].OnMeteorFallen();

            base.OnDisasterStarted(intensity);
        }

        protected override float GetBaseOccurrencePerYear()
        {
            var baseValue = base.GetBaseOccurrencePerYear();

            float result = 0;

            for (var i = 0; i < MeteorEvents.Length; i++)
            {
                result += baseValue * MeteorEvents[i].GetProbabilityMultiplier();
            }

            return result;
        }

        public override byte GetMaximumIntensity()
        {
            byte intensity = baseIntensity;

            for (var i = 0; i < MeteorEvents.Length; i++)
            {
                intensity = Math.Max(intensity, MeteorEvents[i].GetActualMaxIntensity());
            }

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

        public override string GetTooltipInformation()
        {
            if (!Unlocked)
            {
                return "Not Unlocked yet";
            }

            var probability = GetDisasterProbabilityPercentageValue() + Environment.NewLine;

            for (var i = 0; i < MeteorEvents.Length; i++)
                probability += $"{MeteorEvents[i].GetStateDescription()}{Environment.NewLine}";

            return probability;
        }

        protected override float CalculateDestructionRadio(byte intensity)
        {
            int unitSize = 8;
            var unitsBase = 24;
            float unitCalculation;
            int intensityInt = intensity / 10;
            int intensityDec = intensity % 10;

            switch (intensity)
            {
                case byte n when (n < 25):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase + 4 - (intensityDec * 0.04f) - (intensityInt * 0.4f);
                    break;

                case byte n when (n >= 25 && n < 50):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase + 3 - ((intensityInt - 2) * 0.8f) - ((intensityDec - 5) * 0.08f);
                    break;

                case byte n when (n >= 50 && n <= 55):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase + 1;
                    break;

                case byte n when (n > 55 && n <= 75):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase + 1 - ((intensityInt - 5) * 0.5f) - ((intensityDec - 5) * 0.05f);
                    break;

                case byte n when (n > 75 && n <= 100):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - ((intensityInt - 7) * 0.8f) - ((intensityDec - 5) * 0.08f);
                    break;

                case byte n when (n > 100 && n <= 125):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - 2 - ((intensityInt - 10) * 0.4f) - ((intensityDec) * 0.04f);
                    break;

                case byte n when (n > 125 && n <= 150):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - 3 + ((intensityInt - 12) * 0.4f) + ((intensityDec - 5) * 0.04f);
                    break;

                case byte n when (n > 150 && n <= 175):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - 2 - ((intensityInt - 15) * 1.2f) - (intensityDec * 0.12f);
                    break;

                case byte n when (n > 175 && n <= 250):
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - 5 - ((intensityInt - 17) * 0.4f) - ((intensityDec - 5) * 0.04f);
                    break;

                default:
                    unitCalculation = ((((intensityInt - 5) * 10) + intensityDec) * 0.4f) + unitsBase - 5 - ((intensityInt - 17) * 0.4f) - ((intensityDec - 5) * 0.04f) + (intensityDec * 0.24f);
                    break;
            }

            return (float)Math.Sqrt((unitCalculation / 2) * unitSize);
        }

        public override void CopySettings(DisasterBaseModel disaster)
        {
            base.CopySettings(disaster);

            MeteorStrikeModel d = disaster as MeteorStrikeModel;
            if (d != null)
            {
                MeteorLongPeriodEnabled = d.MeteorLongPeriodEnabled;
                MeteorMediumPeriodEnabled = d.MeteorMediumPeriodEnabled;
                MeteorShortPeriodEnabled = d.MeteorShortPeriodEnabled;
                RealTimeFrequencyMultiplier = d.RealTimeFrequencyMultiplier;
            }
        }

        protected override void ResetDisasterState()
        {
            MeteorEvents =
            [
                MeteorEvent.Init("Long period meteor", 9, 200),
                MeteorEvent.Init("Medium period meteor", 5, 120),
                MeteorEvent.Init("Short period meteor", 2, 30)
            ];
        }
    }
}
