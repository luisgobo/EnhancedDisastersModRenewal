using ColossalFramework;
using System;
using UnityEngine;

namespace NaturalDisastersRenewal.Common
{
    public static class Helper
    {
        public static string[] GetMonths()
        {
            return new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        }

        public static string[] GetEvacuationOptions(bool allowsFocusedEvacuation = false)
        {
            string[] evacuationOptions = {
                //"Disabled",
                "Manual evacuation",
                "Auto evacuation"
            };

            if (allowsFocusedEvacuation)
            {
                string focusedEvacuation = "Focused auto evacuation/release";
                Array.Resize(ref evacuationOptions, evacuationOptions.Length + 1);
                evacuationOptions[evacuationOptions.Length - 1] = focusedEvacuation;
            }

            return evacuationOptions;
        }

        public static float FramesPerDay
        {
            get
            {
                return 1.0f / DaysPerFrame;
            }
        }

        public static float DaysPerFrame
        {
            get
            {
                return (float)SimulationManager.instance.m_timePerFrame.TotalDays;
            }
        }

        public static float FramesPerYear
        {
            get
            {
                return FramesPerDay * 365f;
            }
        }

        public static string FormatTimeSpan(float daysFloat)
        {
            if (daysFloat <= 0)
            {
                return "0 days";
            }

            if (daysFloat < 1)
            {
                return "Less than one day";
            }

            if (daysFloat < 60)
            {
                return FormatValue(daysFloat, "day");
            }

            int months = Mathf.FloorToInt(daysFloat / 30);
            if (months < 13)
            {
                if (months > 3) return FormatValue(months, "month");
                int days = Mathf.FloorToInt(daysFloat - months * 30);
                if (days == 0) return FormatValue(months, "month");
                return FormatValue(months, "month") + " and " + FormatValue(days, "day");
            }

            int years = Mathf.FloorToInt(daysFloat / 365);
            months = Mathf.FloorToInt((daysFloat - years * 365) / 30);

            if (years > 5 || months == 0) return FormatValue(years, "year");

            return FormatValue(years, "year") + " and " + FormatValue(months, "month");
        }

        public static string FormatValue(float value, string countableWord)
        {
            return FormatValue(Mathf.FloorToInt(value), countableWord);
        }

        public static string FormatValue(int value, string countableWord)
        {
            return value.ToString() + " " + countableWord + (value == 1 ? "" : "s");
        }

        public static int GetPopulation()
        {
            if (Singleton<DistrictManager>.exists)
            {
                return (int)Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_populationData.m_finalCount;
            }
            return 0;
        }
    }
}