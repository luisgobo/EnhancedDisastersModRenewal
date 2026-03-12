using System;
using System.IO;
using ColossalFramework;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;

namespace NaturalDisastersRenewal.Logger
{
    public static class StripesLogger
    {
        public static void AddStripe(string name, string dimensions, string region)
        {
            if (!Singleton<NaturalDisasterHandler>.instance.container.RecordDisasterEvents) return;

            var filePath = GetDisasterListFilePath();

            if (!File.Exists(filePath))
                File.AppendAllText(filePath, "Name,Dimensions,Region" + CommonProperties.NewLine);

            File.AppendAllText(filePath, name + "," + dimensions + "," + region + Environment.NewLine);
        }

        private static string GetDisasterListFilePath()
        {
            return CommonProperties.GetOptionsFilePath(CommonProperties.disasterListFileName);
        }
    }
}