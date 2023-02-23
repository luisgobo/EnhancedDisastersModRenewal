using ColossalFramework.IO;
using ColossalFramework;
using NaturalDisastersRenewal.Common.enums;
using NaturalDisastersRenewal.Services.Handlers;
using NaturalDisastersRenewal.Services.NaturalDisaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static NaturalDisastersRenewal.Services.NaturalDisaster.DisasterBaseModel;

namespace NaturalDisastersRenewal.Serialization
{
    public class SerializableDataSinkhole : SerializableDataDisasterBase, IDataContainer
    {
        public void Serialize(DataSerializer s)
        {
            SinkholeModel d = Singleton<NaturalDisasterHandler>.instance.container.Sinkhole;
            SerializeCommonParameters(s, d);
            s.WriteFloat(d.GroundwaterCapacity);
            s.WriteFloat(d.groundwaterAmount);
        }

        public void Deserialize(DataSerializer s)
        {
            SinkholeModel d = Singleton<NaturalDisasterHandler>.instance.container.Sinkhole;
            DeserializeCommonParameters(s, d);
            d.GroundwaterCapacity = s.ReadFloat();
            d.groundwaterAmount = s.ReadFloat();
        }

        public void AfterDeserialize(DataSerializer s)
        {
            AfterDeserializeLog("Sinkhole");
        }
    }
}
