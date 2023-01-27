using System;
using System.IO;
using UnityEngine;

namespace NaturalDisastersOverhaulRenewal
{
    public static class DebugLogger
    {
        private static string fileName = "EnhancedDisastersMod.log";
        public static bool IsDebug = true;
        public static bool IsLogInFile = false;

        public static void Log(string msg)
        {
            if (IsDebug)
            {
                if (IsLogInFile)
                {
                    File.AppendAllText(geFilePath(), msg + Environment.NewLine);
                }
                else
                {
                    Debug.Log(msg);
                }
            }
        }

        private static string geFilePath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, "Colossal Order");
            path = Path.Combine(path, "Cities_Skylines");
            path = Path.Combine(path, fileName);
            return path;
        }
    }
}
