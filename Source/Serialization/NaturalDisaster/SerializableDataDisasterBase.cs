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
            dataSeralizer.WriteBool(disaster.IsDisasterEnabled);
            dataSeralizer.WriteFloat(disaster.BaseOccurrencePerYear);
            dataSeralizer.WriteFloat(disaster.CalmDaysLeft);
            dataSeralizer.WriteFloat(disaster.ProbabilityWarmupDaysLeft);
            dataSeralizer.WriteFloat(disaster.IntensityWarmupDaysLeft);
            dataSeralizer.WriteInt32((int)disaster.EvacuationMode * disasterIndex);
        }

        public void DeserializeCommonParameters(DataSerializer dataSeralizer, DisasterBaseModel disaster, int disasterIndex = 1)
        {
            disaster.IsDisasterEnabled = dataSeralizer.ReadBool();
            disaster.BaseOccurrencePerYear = dataSeralizer.ReadFloat();
            if (dataSeralizer.version <= 2)
            {
                float daysPerFrame = 1f / 585f;
                disaster.CalmDaysLeft = dataSeralizer.ReadInt32() * daysPerFrame;
                disaster.ProbabilityWarmupDaysLeft = dataSeralizer.ReadInt32() * daysPerFrame;
                disaster.IntensityWarmupDaysLeft = dataSeralizer.ReadInt32() * daysPerFrame;
                disaster.EvacuationMode = (EvacuationOptions)(dataSeralizer.ReadInt32() * disasterIndex);
            }
            else
            {
                disaster.CalmDaysLeft = dataSeralizer.ReadFloat();
                disaster.ProbabilityWarmupDaysLeft = dataSeralizer.ReadFloat();
                disaster.IntensityWarmupDaysLeft = dataSeralizer.ReadFloat();
                disaster.EvacuationMode = (EvacuationOptions)(dataSeralizer.ReadInt32() * disasterIndex);
            }
        }

        public void AfterDeserializeLog(string className)
        {
            Debug.Log(CommonProperties.logMsgPrefix + className + " data loaded.");
        }
    }
}