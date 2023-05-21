using ColossalFramework;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using System;
using System.IO;

namespace NaturalDisastersRenewal.Logger
{
    public static class DisasterLogger
    {
        public static bool StartedByMod = false;

        public static void AddDisaster(DateTime dt, string disasterName, byte intensity)
        {
            if (!Singleton<NaturalDisasterHandler>.instance.container.RecordDisasterEvents) return;

            string filePath = GetDisasterListFilePath();

            if (!File.Exists(filePath))
            {
                File.AppendAllText(filePath, "date,disaster,intensity,started by" + CommonProperties.newLine);
            }

            string startedBy = "Vanilla";
            if (StartedByMod)
            {
                startedBy = "Mod";
                StartedByMod = false;
            }

            File.AppendAllText(filePath, dt.ToString("yyyy/MM/dd HH:mm") + "," + disasterName + "," + intensity.ToString() + "," + startedBy + Environment.NewLine);
        }

        static string GetDisasterListFilePath()
        {
            return CommonProperties.GetOptionsFilePath(CommonProperties.disasterListFileName);
        }
    }
}