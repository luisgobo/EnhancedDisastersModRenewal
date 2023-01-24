using System;
using System.IO;
using ColossalFramework;

namespace EnhancedDisastersMod
{
    public static class DisasterLogger
    {
        private static string disasterListFileName = "Disasters.csv";
        public static bool StartedByMod = false;

        public static void AddDisaster(DateTime dt, string disasterName, byte intensity)
        {
            if (!Singleton<EnhancedDisastersManager>.instance.container.RecordDisasterEvents) return;

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

        private static string getDisasterListFilePath()
        {
            //return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Colossal Order", "Cities_Skylines", optionsFileName);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, "Colossal Order");
            path = Path.Combine(path, "Cities_Skylines");
            path = Path.Combine(path, disasterListFileName);
            return path;
        }
    }
}
