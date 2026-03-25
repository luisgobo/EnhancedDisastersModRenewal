using System;
using NaturalDisastersRenewal.Common.enums;
using UnityEngine;

namespace NaturalDisastersRenewal.Common
{
    public static class Helper
    {
        private const float RealTimeCompatibilityFactor = 365f;

        private static float GameDaysPerFrame => (float)Services.Simulation.m_timePerFrame.TotalDays;

        public static float GetDaysPerFrame(TimeBehaviorMode mode)
        {
            switch (mode)
            {
                case TimeBehaviorMode.RealTimeCompatible:
                    return GameDaysPerFrame * RealTimeCompatibilityFactor;
                default:
                    return GameDaysPerFrame;
            }
        }
        
        public static string[] GetMonths()
        {
            return ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        }

        public static string[] GetAllEvacuationOptions(bool allowsFocusedEvacuation = false)
        {
            const string focusedEvacuation = "Focused auto evacuation/release";
            
            string[] evacuationOptions =
            [
                "Manual evacuation",
                "Auto evacuation"
            ];

            if (!allowsFocusedEvacuation) return evacuationOptions;
            
            Array.Resize(ref evacuationOptions, evacuationOptions.Length + 1);
            evacuationOptions[evacuationOptions.Length - 1] = focusedEvacuation;

            return evacuationOptions;
        }
        
        public static string[] GetManualAndFocusedEvacuationOptions()
        {
            string[] evacuationOptions =
            [
                "Manual evacuation",
                "Focused auto evacuation/release"
            ];

            return evacuationOptions;
        }

        public static string[] GetCrackModes()
        {
            string[] crackModes =
            [
                "No Cracks",
                "Always Cracks",
                "Allow Cracks Based on intensity"
            ];            
            return crackModes;
        }

        public static float FramesPerDay => 1.0f / DaysPerFrame;

        public static float DaysPerFrame => (float)Services.Simulation.m_timePerFrame.TotalDays;

        public static string FormatTimeSpan(float daysFloat)
        {
            switch (daysFloat)
            {
                case <= 0:
                    return "0 days";
                case < 1:
                    return "Less than one day";
                case < 60:
                    return FormatValue(daysFloat, "day");
            }

            var months = Mathf.FloorToInt(daysFloat / 30);
            if (months < 13)
            {
                if (months > 3) return FormatValue(months, "month");
                
                var days = Mathf.FloorToInt(daysFloat - months * 30);
                if (days == 0) return FormatValue(months, "month");
                
                return FormatValue(months, "month") + " and " + FormatValue(days, "day");
            }

            var years = Mathf.FloorToInt(daysFloat / 365);
            months = Mathf.FloorToInt((daysFloat - years * 365) / 30);

            if (years > 5 || months == 0) return FormatValue(years, "year");

            return FormatValue(years, "year") + " and " + FormatValue(months, "month");
        }

        private static string FormatValue(float value, string countableWord)
        {
            return FormatValue(Mathf.FloorToInt(value), countableWord);
        }

        private static string FormatValue(int value, string countableWord)
        {
            return value + " " + countableWord + (value == 1 ? "" : "s");
        }

        public static int GetPopulation()
        {
            var districts = Services.Districts;
            if (districts)
            {
                return (int)districts.m_districts.m_buffer[0].m_populationData.m_finalCount;
            }
            return 0;
        }
    }
}
