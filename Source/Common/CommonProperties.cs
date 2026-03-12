using System;
using System.IO;

namespace NaturalDisastersRenewal.Common
{
    public static class CommonProperties
    {
        private const string ModVersion = "1.4";
        private const string ModLastVersionYear = "2026";
        private const string ModLastVersionMonth = "March";

        private const string ContentMainPath = "Colossal Order";
        private const string ContentSubPath = "Cities_Skylines";
        private const string ContentFolder = "NaturalDisastersRenewalMod";

        public const string modSteamId = "2957578256";
        public const string dataId = "NaturalDisastersRenewalMod";
        public const string modName = "Natural Disasters Renewal";
        public const string modNameForHarmony = "NaturalDisastersRenewal";
        
        public const string xmlFilename = "NaturalDisastersRenewalModOptions.xml";
        public const string disasterListFileName = "DisasterList.csv";
        public const string spriteFileName = "SpriteList.csv";
        public const string logFilename = "NaturalDisastersRenewalMod.log";
        public const string logMsgPrefix = ">>> " + modName + ": ";

        public const string earthquakeName = "Earthquake";
        public const string forestFireName = "ForestFire";
        public const string meteorStrikeName = "Meteor Strike";
        public const string sinkholeName = "Sinkhole";
        public const string thunderstormName = "Thunderstorm";
        public const string tornadoName = "Tornado";
        public const string tsunamiName = "Tsunami";

        //Log commands
        public static readonly string NewLine = $"{Environment.NewLine}"; 

        public static string GetModDescription()
        {
            return string.Format(
                "It takes \"Natural Disaster Overhaul\" and \"Ragnarok\" mods content to be unified in only one, including some other awesome stuffs." +
                $"Version: {ModVersion}. Last Update: {ModLastVersionMonth},{ModLastVersionYear}");
        }

        public static string GetOptionsFilePath(string filename)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, ContentMainPath);
            path = Path.Combine(path, ContentSubPath);
            path = Path.Combine(path, ContentFolder);

            CheckFolderExistence(path);

            path = Path.Combine(path, filename);
            return path;
        }

        static void CheckFolderExistence(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

    }
}