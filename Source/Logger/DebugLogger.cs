using System.IO;
using NaturalDisastersRenewal.Common;
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
                File.AppendAllText(GeFilePath(), msg + CommonProperties.NewLine);
                Debug.Log(msg);
            }
        }

        static string GeFilePath()
        {
            return CommonProperties.GetOptionsFilePath(CommonProperties.logFilename);
        }
    }
}