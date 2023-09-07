using NaturalDisastersRenewal.Common;
using System.IO;
using UnityEngine;

namespace NaturalDisastersRenewal.Models.Disaster
{
    public static class DebugLogger
    {
        public static bool IsDebug = true;
        public static bool IsLogInFile = false;

        public static void Log(string msg)
        {
            if (IsDebug)
            //if (IsDebug && System.Diagnostics.Debugger.IsAttached)
            {
                File.AppendAllText(GeFilePath(CommonProperties.logFilename), msg + CommonProperties.newLine);
                Debug.Log(msg);
            }
        }

        public static void EnabledModsLog(string msg)
        {
            if (IsDebug)
            //if (IsDebug && System.Diagnostics.Debugger.IsAttached)
            {
                File.AppendAllText(GeFilePath(CommonProperties.enabledModslogFilename), msg + CommonProperties.newLine);
                Debug.Log(msg);
            }
        }

        static string GeFilePath(string logFileName)
        {
            return CommonProperties.GetOptionsFilePath(logFileName);
        }
    }
}