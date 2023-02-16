using ICities;
using NaturalDisastersRenewal.Common.enums;
using System.Collections.Generic;

namespace NaturalDisastersRenewal.Models
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