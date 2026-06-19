using ColossalFramework.IO;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Common.enums;
using UnityEngine;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataEarthquake : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            var earthquake = Services.DisasterSetup.Earthquake;
            SerializeCommonParameters(dataSerializer, earthquake);

            dataSerializer.WriteFloat(earthquake.WarmupYears);
            dataSerializer.WriteInt8((int)earthquake.EarthquakeCrackMode);
            dataSerializer.WriteInt8((int)earthquake.MinimalIntensityForCracks);

            dataSerializer.WriteInt8(earthquake.AftershocksCount);
            dataSerializer.WriteInt8(earthquake.AftershockMaxIntensity);
            dataSerializer.WriteInt8(earthquake.MainStrikeIntensity);

            dataSerializer.WriteFloat(earthquake.LastTargetPosition.x);
            dataSerializer.WriteFloat(earthquake.LastTargetPosition.y);
            dataSerializer.WriteFloat(earthquake.LastTargetPosition.z);
            dataSerializer.WriteFloat(earthquake.LastAngle);

            dataSerializer.WriteInt32((int)earthquake.RealTimeEarthquakeFrequency);
            dataSerializer.WriteFloat(earthquake.RealTimeCurrentSeismicPeriodMinutes);
            dataSerializer.WriteFloat(earthquake.RealTimeMinutesUntilNextEarthquake);
            dataSerializer.WriteFloat(earthquake.RealTimeCurrentAftershockPeriodMinutes);
            dataSerializer.WriteFloat(earthquake.RealTimeMinutesUntilNextAftershock);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            var earthquake = Services.DisasterSetup.Earthquake;
            DeserializeCommonParameters(dataSerializer, earthquake);

            earthquake.WarmupYears = dataSerializer.ReadFloat();
            if (dataSerializer.version >= 3)
            {
                earthquake.EarthquakeCrackMode = (EarthquakeCrackOptions)dataSerializer.ReadInt8();
                earthquake.MinimalIntensityForCracks = dataSerializer.ReadInt8();
            }

            earthquake.AftershocksCount = (byte)dataSerializer.ReadInt8();
            earthquake.AftershockMaxIntensity = (byte)dataSerializer.ReadInt8();
            if (dataSerializer.version >= 2) earthquake.MainStrikeIntensity = (byte)dataSerializer.ReadInt8();

            earthquake.LastTargetPosition = new Vector3(dataSerializer.ReadFloat(), dataSerializer.ReadFloat(),
                dataSerializer.ReadFloat());
            earthquake.LastAngle = dataSerializer.ReadFloat();

            if (dataSerializer.version >= 16)
            {
                earthquake.RealTimeEarthquakeFrequency =
                    (RealTimeDisasterFrequencyPreset)dataSerializer.ReadInt32();
                earthquake.RealTimeCurrentSeismicPeriodMinutes = dataSerializer.ReadFloat();
                earthquake.RealTimeMinutesUntilNextEarthquake = dataSerializer.ReadFloat();
                earthquake.RealTimeCurrentAftershockPeriodMinutes = dataSerializer.ReadFloat();
                earthquake.RealTimeMinutesUntilNextAftershock = dataSerializer.ReadFloat();
            }

            earthquake.NormalizeRealisticRecurrenceSettings();
        }

        public void AfterDeserialize(DataSerializer s)
        {
            AfterDeserializeLog("EarthquakeModel");
        }
    }
}