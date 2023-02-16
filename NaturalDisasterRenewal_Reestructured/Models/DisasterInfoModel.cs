using ICities;
using NaturalDisasterRenewal_Reestructured.Common.Enums;
using System.Collections.Generic;

namespace NaturalDisasterRenewal_Reestructured.Models
{
    public class DisasterInfoModel
    {
        public DisasterSettings DisasterInfo;
        public ushort DisasterId;
        public EvacuationOptions EvacuationMode = EvacuationOptions.ManualEvacuation;
        public bool IgnoreDestructionZone = true;
        public List<ushort> ShelterList = new List<ushort>();
    }
}