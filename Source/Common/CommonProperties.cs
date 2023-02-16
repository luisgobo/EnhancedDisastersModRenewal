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
        public const string xmlMainPath = "Colossal Order";
        public const string xmlSubPath = "Cities_Skylines";
        public const string xmlFilename = "NaturalDisastersRenewalModOptions.xml";
        public const string logFilename = "NaturalDisastersRenewalMod.log";
        public const string dataId = "NaturalDisastersRenewalMod";
        public const string disasterListFileName = "Disasters.csv";
        public const string LogMsgPrefix = ">>> " + ModName + ": ";

        public static string getModDescription()
            => string.Format("Natural Disaster Overhaul Base including Ragnarokg's mod enhancements and more. (Version \" {0} \")", ModLastVersionYear);

        public static string GetOptionsFilePath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, xmlMainPath);
            path = Path.Combine(path, xmlSubPath);
            path = Path.Combine(path, xmlFilename);
            return path;
        }
    }
}