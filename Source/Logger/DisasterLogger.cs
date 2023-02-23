using ColossalFramework;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Services.Handlers;
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

            string filePath = getDisasterListFilePath();

            if (!File.Exists(filePath))
            {
                File.AppendAllText(filePath, "date,disaster,intensity,started by" + Environment.NewLine);
            }

            string startedBy = "Vanilla";
            if (StartedByMod)
            {
                startedBy = "Mod";
                StartedByMod = false;
            }

            File.AppendAllText(filePath, dt.ToString("yyyy/MM/dd HH:mm") + "," + disasterName + "," + intensity.ToString() + "," + startedBy + Environment.NewLine);
        }

        static string getDisasterListFilePath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, CommonProperties.xmlMainPath);
            path = Path.Combine(path, CommonProperties.xmlSubPath);
            path = Path.Combine(path, CommonProperties.disasterListFileName);
            return path;
        }
    }
}