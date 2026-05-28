using System;
using System.IO;

namespace DisasterCommandCenter.Common
{
    public static class CommandCenterProperties
    {
        public const string ModName = "Disaster Command Center";
        public const string InternalName = "DisasterCommandCenter";
        public const string HarmonyId = "lugobo.DisasterCommandCenter";
        public const string ModVersion = "0.1.0-dev";

        public const string ContentFolder = "DisasterCommandCenter";
        public const string XmlFilename = "DisasterCommandCenterOptions.xml";
        public const string LogFilename = "DisasterCommandCenter.log";
        public const string DataId = "DisasterCommandCenter";

        private const string ContentMainPath = "Colossal Order";
        private const string ContentSubPath = "Cities_Skylines";

        public static string ModLastUpdate
        {
            get { return BuildInfo.BuildMonth + ", " + BuildInfo.BuildYear; }
        }

        public static string GetModDescription()
        {
            return "Control, tune, respond to, and recover from disasters. " +
                   "Successor foundation for Natural Disasters Renewal. " +
                   "Version: " + ModVersion + ". Last Update: " + ModLastUpdate;
        }

        public static string GetOptionsFilePath(string filename)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, ContentMainPath);
            path = Path.Combine(path, ContentSubPath);
            path = Path.Combine(path, ContentFolder);

            EnsureFolder(path);

            return Path.Combine(path, filename);
        }

        private static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
