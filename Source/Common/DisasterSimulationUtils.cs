using System;
using UnityEngine;

namespace NaturalDisastersRenewal.Common
{
    public static class DisasterSimulationUtils
    {
        public const float VanillaSimulationDaysPerFrame = 1f / 585f;

        public static float FramesPerDay => 1.0f / DaysPerFrame;

        public static float DaysPerFrame => (float)Services.Simulation.m_timePerFrame.TotalDays;

        public static float FramesPerYear => FramesPerDay * 365f;

        public static string[] GetMonths()
        {
            return LocalizationService.GetMonths();
        }

        public static string[] GetAllEvacuationOptions(bool allowsFocusedEvacuation = false)
        {
            string[] evacuationOptions =
            {
                LocalizationService.Get("evacuation.manual"),
                LocalizationService.Get("evacuation.auto")
            };

            if (allowsFocusedEvacuation)
            {
                var focusedEvacuation = LocalizationService.Get("evacuation.focused");
                Array.Resize(ref evacuationOptions, evacuationOptions.Length + 1);
                evacuationOptions[evacuationOptions.Length - 1] = focusedEvacuation;
            }

            return evacuationOptions;
        }

        public static string[] GetManualAndFocusedEvacuationOptions()
        {
            string[] evacuationOptions =
            {
                LocalizationService.Get("evacuation.manual"),
                LocalizationService.Get("evacuation.focused")
            };

            return evacuationOptions;
        }

        public static string[] GetCrackModes()
        {
            string[] crackModes =
            {
                LocalizationService.Get("cracks.none"),
                LocalizationService.Get("cracks.always"),
                LocalizationService.Get("cracks.by_intensity")
            };
            return crackModes;
        }

        public static bool IsRealTimeModActive()
        {
            return ModCompatibilityService.IsActive("realTime");
        }

        public static string FormatTimeSpan(float daysFloat)
        {
            if (daysFloat <= 0) return "0 " + LocalizationService.Get("time.day") + "s";

            if (daysFloat < 1) return LocalizationService.Get("time.less_than_one_day");

            if (daysFloat < 60) return FormatValue(daysFloat, LocalizationService.Get("time.day"));

            var months = Mathf.FloorToInt(daysFloat / 30);
            if (months < 13)
            {
                if (months > 3) return FormatValue(months, LocalizationService.Get("time.month"));
                var days = Mathf.FloorToInt(daysFloat - months * 30);
                if (days == 0) return FormatValue(months, LocalizationService.Get("time.month"));
                return FormatValue(months, LocalizationService.Get("time.month")) + " " +
                       LocalizationService.Get("time.and") + " " +
                       FormatValue(days, LocalizationService.Get("time.day"));
            }

            var years = Mathf.FloorToInt(daysFloat / 365);
            months = Mathf.FloorToInt((daysFloat - years * 365) / 30);

            if (years > 5 || months == 0) return FormatValue(years, LocalizationService.Get("time.year"));

            return FormatValue(years, LocalizationService.Get("time.year")) + " " +
                   LocalizationService.Get("time.and") + " " +
                   FormatValue(months, LocalizationService.Get("time.month"));
        }

        public static string FormatValue(float value, string countableWord)
        {
            return FormatValue(Mathf.FloorToInt(value), countableWord);
        }

        public static string FormatValue(int value, string countableWord)
        {
            return value + " " + countableWord + (value == 1 ? "" : "s");
        }

        public static int GetPopulation()
        {
            var districts = Services.Districts;
            if (districts) return (int)districts.m_districts.m_buffer[0].m_populationData.m_finalCount;
            return 0;
        }
    }
}
