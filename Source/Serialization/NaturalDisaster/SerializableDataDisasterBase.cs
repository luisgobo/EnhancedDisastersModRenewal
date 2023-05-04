using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using UnityEngine;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataDisasterBase
    {
        public void SerializeCommonParameters(DataSerializer dataSeralizer, DisasterBaseModel disaster, int disasterIndex = 1)
        {
            dataSeralizer.WriteBool(disaster.Enabled);
            dataSeralizer.WriteFloat(disaster.BaseOccurrencePerYear);
            dataSeralizer.WriteFloat(disaster.calmDaysLeft);
            dataSeralizer.WriteFloat(disaster.probabilityWarmupDaysLeft);
            dataSeralizer.WriteFloat(disaster.intensityWarmupDaysLeft);
            dataSeralizer.WriteInt32((int)disaster.EvacuationMode * disasterIndex);
        }

        public void DeserializeCommonParameters(DataSerializer dataSeralizer, DisasterBaseModel disaster, int disasterIndex = 1)
        {
            disaster.Enabled = dataSeralizer.ReadBool();
            disaster.BaseOccurrencePerYear = dataSeralizer.ReadFloat();
            if (dataSeralizer.version <= 2)
            {
                float daysPerFrame = 1f / 585f;
                disaster.calmDaysLeft = dataSeralizer.ReadInt32() * daysPerFrame;
                disaster.probabilityWarmupDaysLeft = dataSeralizer.ReadInt32() * daysPerFrame;
                disaster.intensityWarmupDaysLeft = dataSeralizer.ReadInt32() * daysPerFrame;
                disaster.EvacuationMode = (EvacuationOptions)(dataSeralizer.ReadInt32() * disasterIndex);
            }
            else
            {
                disaster.calmDaysLeft = dataSeralizer.ReadFloat();
                disaster.probabilityWarmupDaysLeft = dataSeralizer.ReadFloat();
                disaster.intensityWarmupDaysLeft = dataSeralizer.ReadFloat();
                disaster.EvacuationMode = (EvacuationOptions)(dataSeralizer.ReadInt32() * disasterIndex);
            }
        }

        public void AfterDeserializeLog(string className)
        {
            Debug.Log(CommonProperties.logMsgPrefix + className + " data loaded.");
        }
    }
}