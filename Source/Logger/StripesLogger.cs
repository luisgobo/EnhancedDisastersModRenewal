using System;
using System.IO;
using NaturalDisastersRenewal.Common;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.Logger
{
    public static class StripesLogger
    {
        public static void AddStripe(string name, string dimensions, string region)
        {
            if (!CommonServices.DisasterSetup.RecordDisasterEvents) return;

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