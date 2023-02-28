using System;
using System.IO;

namespace NaturalDisastersRenewal.Common
{
    public static class CommonProperties
    {
        public const string ModName = "Natural Disasters Renewal";
        public const string ModNameForHarmony = "NaturalDisastersRenewal";
        public const string ModVersion = "1.0";
        public const string ModLastVersionYear = "2023";
        public const string contentMainPath = "Colossal Order";
        public const string contentSubPath = "Cities_Skylines";
        public const string contentFolder = "NaturalDisastersRenewalMod";
        public const string xmlFilename = "NaturalDisastersRenewalModOptions.xml";
        public const string logFilename = "NaturalDisastersRenewalMod.log";
        public const string dataId = "NaturalDisastersRenewalMod";
        public const string disasterListFileName = "DisasterList.csv";
        public const string LogMsgPrefix = ">>> " + ModName + ": ";   

        public static string GetModDescription()
            => string.Format($"Natural Disaster Renewal takes Zenya's 'Natural Disaster Overhault' and [SUSU]yang yang's Ragnarok mods to be unified in only one, including some other awesome stuffs. " +
                $"(Version \"{ModVersion}\". {ModLastVersionYear})");

        public static string GetOptionsFilePath(string filename)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, contentMainPath);
            path = Path.Combine(path, contentSubPath);
            path = Path.Combine(path, contentFolder);
            path = Path.Combine(path, filename);
            return path;
        }
    }
}