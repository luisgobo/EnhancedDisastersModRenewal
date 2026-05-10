using System;
using System.IO;
using NaturalDisastersRenewal.Common;

namespace NaturalDisastersRenewal.Logger
{
    public static class DisasterLogger
    {
        public static bool StartedByMod;

        public static void AddDisaster(DateTime dt, string disasterName, byte intensity)
        {
            if (!Services.DisasterSetup.RecordDisasterEvents) return;

            var filePath = GetDisasterListFilePath();

            if (!File.Exists(filePath))
                File.AppendAllText(filePath, "date,disaster,intensity,started by" + CommonProperties.NewLine);

            var startedBy = "Vanilla";
            if (StartedByMod)
            {
                startedBy = "Mod";
                StartedByMod = false;
            }

            File.AppendAllText(filePath,
                dt.ToString("yyyy/MM/dd HH:mm") + "," + disasterName + "," + intensity + "," + startedBy +
                Environment.NewLine);
        }

        private static string GetDisasterListFilePath()
        {
            return CommonProperties.GetOptionsFilePath(CommonProperties.DisasterListFileName);
        }
    }
}