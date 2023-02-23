using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using UnityEngine;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataDisasterBase
    {
        public void SerializeCommonParameters(DataSerializer s, DisasterBaseModel disaster)
        {
            s.WriteBool(disaster.Enabled);
            s.WriteFloat(disaster.BaseOccurrencePerYear);
            s.WriteFloat(disaster.calmDaysLeft);
            s.WriteFloat(disaster.probabilityWarmupDaysLeft);
            s.WriteFloat(disaster.intensityWarmupDaysLeft);
            s.WriteInt32((int)disaster.EvacuationMode);
        }

        public void DeserializeCommonParameters(DataSerializer s, DisasterBaseModel disaster)
        {
            disaster.Enabled = s.ReadBool();
            disaster.BaseOccurrencePerYear = s.ReadFloat();
            if (s.version <= 2)
            {
                float daysPerFrame = 1f / 585f;
                disaster.calmDaysLeft = s.ReadInt32() * daysPerFrame;
                disaster.probabilityWarmupDaysLeft = s.ReadInt32() * daysPerFrame;
                disaster.intensityWarmupDaysLeft = s.ReadInt32() * daysPerFrame;
                disaster.EvacuationMode = (EvacuationOptions)s.ReadInt32();
            }
            else
            {
                disaster.calmDaysLeft = s.ReadFloat();
                disaster.probabilityWarmupDaysLeft = s.ReadFloat();
                disaster.intensityWarmupDaysLeft = s.ReadFloat();
                disaster.EvacuationMode = (EvacuationOptions)s.ReadInt32();
            }
        }

        public void AfterDeserializeLog(string className)
        {
            Debug.Log(CommonProperties.LogMsgPrefix + className + " data loaded.");
        }
    }
}