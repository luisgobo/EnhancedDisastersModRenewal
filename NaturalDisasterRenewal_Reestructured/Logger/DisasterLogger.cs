using ColossalFramework;
using NaturalDisasterRenewal_Reestructured.Common;
using NaturalDisasterRenewal_Reestructured.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturalDisasterRenewal_Reestructured.Logger
{
    public static class DisasterLogger
    {
        public static bool StartedByMod = false;

        public static void AddDisaster(DateTime dt, string disasterName, byte intensity)
        {
            if (!Singleton<DisasterGeneralSetupHandler>.instance.disasterGeneralSetup.RecordDisasterEvents) 
                return;

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
