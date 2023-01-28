using System;
using System.IO;
using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using UnityEngine;

namespace NaturalDisastersOverhaulRenewal
{
    public class SerializableDataExtension : ISerializableDataExtension
    {
        public const string DataID = "EnhancedDisastersMod";
        public const uint DataVersion = 3;
        private ISerializableData serializableData;

        public void OnCreated(ISerializableData serializedData)
        {
            serializableData = serializedData;
        }

        public void OnSaveData()
        {
            try
            {
                byte[] data;

                using (var stream = new MemoryStream())
                {
                    EnhancedDisastersManager edm = Singleton<EnhancedDisastersManager>.instance;

                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new DisastersContainer.Data());

                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new EnhancedForestFire.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new EnhancedThunderstorm.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new EnhancedSinkhole.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new EnhancedTsunami.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new EnhancedTornado.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new EnhancedEarthquake.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new EnhancedMeteorStrike.Data());
                    data = stream.ToArray();
                }

                serializableData.SaveData(DataID, data);
            }
            catch (Exception ex)
            {
                Debug.Log(Mod.LogMsgPrefix + "(save error) " + ex.Message);
            }
        }

        public void OnLoadData()
        {
            try
            {
                byte[] data = serializableData.LoadData(DataID);

                if (data == null)
                {
                    Debug.Log(Mod.LogMsgPrefix + "No saved data");
                    return;
                }

                using (var stream = new MemoryStream(data))
                {
                    DataSerializer.Deserialize<DisastersContainer.Data>(stream, DataSerializer.Mode.Memory);

                    DataSerializer.Deserialize<EnhancedForestFire.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<EnhancedThunderstorm.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<EnhancedSinkhole.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<EnhancedTsunami.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<EnhancedTornado.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<EnhancedEarthquake.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<EnhancedMeteorStrike.Data>(stream, DataSerializer.Mode.Memory);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(Mod.LogMsgPrefix + "(load error) " + ex.Message);
            }

            Mod.UpdateUI();
        }

        public void OnReleased()
        {
            serializableData = null;
        }
    }
}
