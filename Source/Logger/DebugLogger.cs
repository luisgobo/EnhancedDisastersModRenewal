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
            if (IsDebug && System.Diagnostics.Debugger.IsAttached)
            {
                File.AppendAllText(GeFilePath(), msg + CommonProperties.newLine);
                Debug.Log(msg);
            }
        }

        static string GeFilePath()
        {
            return CommonProperties.GetOptionsFilePath(CommonProperties.logFilename);
        }
    }
}