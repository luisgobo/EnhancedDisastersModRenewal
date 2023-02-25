using ColossalFramework;
using ColossalFramework.IO;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using UnityEngine;

namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    public class SerializableDataEarthquake : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer dataSerializer)
        {
            EarthquakeModel earthquake = Singleton<NaturalDisasterHandler>.instance.container.Earthquake;
            SerializeCommonParameters(dataSerializer, earthquake);

            dataSerializer.WriteFloat(earthquake.WarmupYears);
            dataSerializer.WriteInt8((int)earthquake.EarthquakeCrackMode);

            dataSerializer.WriteInt8(earthquake.aftershocksCount);
            dataSerializer.WriteInt8(earthquake.aftershockMaxIntensity);
            dataSerializer.WriteInt8(earthquake.mainStrikeIntensity);

            dataSerializer.WriteFloat(earthquake.lastTargetPosition.x);
            dataSerializer.WriteFloat(earthquake.lastTargetPosition.y);
            dataSerializer.WriteFloat(earthquake.lastTargetPosition.z);
            dataSerializer.WriteFloat(earthquake.lastAngle);
        }

        public void Deserialize(DataSerializer dataSerializer)
        {
            EarthquakeModel earthquake = Singleton<NaturalDisasterHandler>.instance.container.Earthquake;
            DeserializeCommonParameters(dataSerializer, earthquake);

            earthquake.WarmupYears = dataSerializer.ReadFloat();
            if (dataSerializer.version >= 3)
            {
                earthquake.EarthquakeCrackMode = (EarthquakeCrackOptions)dataSerializer.ReadInt8();
            }

            earthquake.aftershocksCount = (byte)dataSerializer.ReadInt8();
            earthquake.aftershockMaxIntensity = (byte)dataSerializer.ReadInt8();
            if (dataSerializer.version >= 2)
            {
                earthquake.mainStrikeIntensity = (byte)dataSerializer.ReadInt8();
            }

            earthquake.lastTargetPosition = new Vector3(dataSerializer.ReadFloat(), dataSerializer.ReadFloat(), dataSerializer.ReadFloat());
            earthquake.lastAngle = dataSerializer.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer s)
        {
            AfterDeserializeLog("EarthquakeModel");
        }
    }
}