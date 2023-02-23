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
        public void Serialize(DataSerializer s)
        {
            EarthquakeModel d = Singleton<NaturalDisasterHandler>.instance.container.Earthquake;
            SerializeCommonParameters(s, d);

            s.WriteFloat(d.WarmupYears);
            s.WriteInt8((int)d.EarthquakeCrackMode);

            s.WriteInt8(d.aftershocksCount);
            s.WriteInt8(d.aftershockMaxIntensity);
            s.WriteInt8(d.mainStrikeIntensity);

            s.WriteFloat(d.lastTargetPosition.x);
            s.WriteFloat(d.lastTargetPosition.y);
            s.WriteFloat(d.lastTargetPosition.z);
            s.WriteFloat(d.lastAngle);
        }

        public void Deserialize(DataSerializer s)
        {
            EarthquakeModel d = Singleton<NaturalDisasterHandler>.instance.container.Earthquake;
            DeserializeCommonParameters(s, d);

            d.WarmupYears = s.ReadFloat();
            if (s.version >= 3)
            {
                d.EarthquakeCrackMode = (EarthquakeCrackOptions)s.ReadInt8();
            }

            d.aftershocksCount = (byte)s.ReadInt8();
            d.aftershockMaxIntensity = (byte)s.ReadInt8();
            if (s.version >= 2)
            {
                d.mainStrikeIntensity = (byte)s.ReadInt8();
            }

            d.lastTargetPosition = new Vector3(s.ReadFloat(), s.ReadFloat(), s.ReadFloat());
            d.lastAngle = s.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer s)
        {
            AfterDeserializeLog("EnhancedEarthquake");
        }
    }
}