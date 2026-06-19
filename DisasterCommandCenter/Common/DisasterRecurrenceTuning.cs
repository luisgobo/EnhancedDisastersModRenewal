using UnityEngine;

namespace NaturalDisastersRenewal.Common
{
    internal static class DisasterRecurrenceTuning
    {
        public static float ClampOccurrencePerYear(float value, float minValue, float maxValue)
        {
            return Mathf.Clamp(value, minValue, maxValue);
        }

        public static void ClampPeriodDays(float minPeriodDays, ref float periodDays, ref float daysUntilNext)
        {
            if (periodDays <= 0f)
            {
                periodDays = minPeriodDays;
                daysUntilNext = minPeriodDays;
                return;
            }

            if (periodDays >= minPeriodDays)
            {
                if (daysUntilNext > periodDays)
                    daysUntilNext = periodDays;
                return;
            }

            var progress = Mathf.Clamp01(1f - daysUntilNext / periodDays);
            periodDays = minPeriodDays;
            daysUntilNext = periodDays * (1f - progress);
        }
    }
}
