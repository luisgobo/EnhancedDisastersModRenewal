using NaturalDisastersRenewal.Common;
using System;
using System.IO;
using UnityEngine;

namespace NaturalDisastersRenewal.Logger
{
    public static class DebugLogger
    {        
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

        static string geFilePath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, CommonProperties.xmlMainPath);
            path = Path.Combine(path, CommonProperties.xmlSubPath);
            path = Path.Combine(path, CommonProperties.logFilename);
            return path;
        }
    }
}