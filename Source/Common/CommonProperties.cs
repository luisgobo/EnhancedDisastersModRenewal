using NaturalDisastersRenewal.Models.Disaster;
using System;
using System.IO;

namespace NaturalDisastersRenewal.Common
{
    public static class CommonProperties
    {
        public const string modName = "Natural Disasters Renewal";
        public const string modNameForHarmony = "NaturalDisastersRenewal";
        public const string modVersion = "1.3.0";
        public const string modLastVersionYear = "2023";
        public const string modLastVersionMonth = "June";
        public const string modSteamId = "2957578256";
        public const string contentMainPath = "Colossal Order";
        public const string contentSubPath = "Cities_Skylines";
        public const string contentFolder = "NaturalDisastersRenewalMod";
        public const string xmlFilename = "NaturalDisastersRenewalModOptions.xml";
        public const string logFilename = "NaturalDisastersRenewalMod.log";
        public const string enabledModslogFilename = "EnabledModslogFilename.log";
        public const string conflictModLog = "ConflictModLog.log";
        public const string dataId = "NaturalDisastersRenewalMod";
        public const string disasterListFileName = "DisasterList.csv";
        public const string logMsgPrefix = ">>> " + modName + ": ";

        public const string EarthquakeName = "Earthquake";
        public const string forestFireName = "ForestFire";
        public const string meteorStrikeName = "Meteor Strike";
        public const string sinkholeName = "Sinkhole";
        public const string thunderstormName = "Thunderstorm";
        public const string tornadoName = "Tornado";
        public const string tsunamiName = "Tsunami";

        public const string modDependencyType = "modinfo";
        public const string modDependencyToLowerAcme = "acme";
        public const string modDependencyToLowerRealTime = "realtime";
        public const string modDependencyToLowerUUI = "unifieduimod";
        public const string modDependencyToLowerUUILib = "unifieduilib";

        public static string newLine = $"{Environment.NewLine}";//Used in debug mode

        public static string GetModDescription()
            => string.Format($"It takes \"Natural Disaster Overhault\" and \"Ragnarok\" mods content to be unified in only one, including some other awesome stuffs." +
                $"Version: {modVersion}. Last Update: {modLastVersionMonth},{modLastVersionYear}");

        public static string GetOptionsFilePath(string filename)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, contentMainPath);
            path = Path.Combine(path, contentSubPath);
            path = Path.Combine(path, contentFolder);

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