﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NaturalDisastersRenewal.Common
{
    public static class CommonProperties
    {
        public const string ModName = "Natural Disasters Renewal";
        
        public const string ModVersion = "1.0";
        public const string ModLastVersionYear = "2023";
        public const string xmlMainPath = "Colossal Order";
        public const string xmlSubPath = "Cities_Skylines";
        public const string xmlFilename = "NaturalDisastersRenewalModOptions.xml";
        public const string LogMsgPrefix = ">>> " + ModName + ": ";

        public static string getModDescription() 
            => string.Format("Natural Disaster Overhaul Base including Ragnarokg's mod enhancements and more. (Version \" {0} \")", ModLastVersionYear);
        

        public static string GetOptionsFilePath()
        {
            //return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Colossal Order", "Cities_Skylines", optionsFileName);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, xmlMainPath);
            path = Path.Combine(path, xmlSubPath);
            path = Path.Combine(path, xmlFilename);
            return path;
        }


    }
}