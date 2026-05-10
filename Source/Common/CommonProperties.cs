using System;
using System.IO;
using System.Globalization;
using System.Reflection;

namespace NaturalDisastersRenewal.Common
{
    public static class CommonProperties
    {
        public const string ModName = "Natural Disasters Renewal";
        public const string ModNameForHarmony = "NaturalDisastersRenewal";
        private const string ModVersion = "1.3.0";
        public const string ModSteamId = "2957578256";
        private const string ContentMainPath = "Colossal Order";
        private const string ContentSubPath = "Cities_Skylines";
        private const string ContentFolder = "NaturalDisastersRenewalMod";
        public const string XmlFilename = "NaturalDisastersRenewalModOptions.xml";
        public const string LogFilename = "NaturalDisastersRenewalMod.log";
        public const string DataId = "NaturalDisastersRenewalMod";
        public const string DisasterListFileName = "DisasterList.csv";
        public const string LogMessagePrefix = ">>> " + ModName + ": ";

        public static readonly string NewLine = $"{Environment.NewLine}";

        private static DateTime ModBuildDate => File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);

        private static string ModLastVersionYear => ModBuildDate.ToString("yyyy", CultureInfo.InvariantCulture);

        private static string ModLastVersionMonth => ModBuildDate.ToString("MMMM", CultureInfo.InvariantCulture);

        public static string GetModDescription()
        {
            return string.Format(
                "Make your city feel alive, vulnerable, and thrilling to protect. " +
                $"Version: {ModVersion}. Last Update: {ModLastVersionMonth}, {ModLastVersionYear}");
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

        private static void CheckFolderExistence(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
