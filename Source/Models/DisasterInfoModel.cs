using ICities;
using NaturalDisastersRenewal.Common.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NaturalDisastersRenewal.Models
{
    public class DisasterInfoModel
    {
        public DisasterSettings DisasterInfo;
        public ushort DisasterId;
        public EvacuationOptions EvacuationMode = EvacuationOptions.ManualEvacuation;
        public bool IgnoreDestructionZone = true;
    }
}
