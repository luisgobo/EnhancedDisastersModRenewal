using UnityEngine;

namespace NaturalDisastersRenewal.Common
{
    internal static class RealTimeDisasterTiming
    {
        public static void ClampScheduleToRange(
            float minMinutes,
            float maxMinutes,
            ref float currentPeriodMinutes,
            ref float minutesUntilNext)
        {
            if (currentPeriodMinutes <= 0f || minutesUntilNext < 0f)
                return;

            if (currentPeriodMinutes >= minMinutes && currentPeriodMinutes <= maxMinutes)
                return;

            var progress = Mathf.Clamp01(1f - minutesUntilNext / currentPeriodMinutes);
            currentPeriodMinutes = Mathf.Clamp(currentPeriodMinutes, minMinutes, maxMinutes);
            minutesUntilNext = currentPeriodMinutes * (1f - progress);
        }
    }
}
