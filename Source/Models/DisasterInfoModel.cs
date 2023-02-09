using ICities;
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
        public int EvacuationMode = 0;
        public bool IgnoreDestructionZone = true;
    }
}
