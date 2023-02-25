using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.UI;
using System;
using System.IO;
using UnityEngine;

//This class allows to save /load specific configuration for loadded game. This config is not necesarelly reflected in setup menu (It should)
//PROBABLY A NEW ENHANCEMENT WITH CURRENT LOADED GAME INFO SHOULD BE NEEDED TO AVOID CONFUSIONS
namespace NaturalDisastersRenewal.Serialization.NaturalDisaster
{
    //public class SerializableDataExtension : ISerializableDataExtension
    public class LoadedGameSerializableDataExtension
    {
        public const string DataID = CommonProperties.dataId;
        public const uint DataVersion = 3;
        ISerializableData serializableData;

        public void OnCreated(ISerializableData serializedData)
        {
            serializableData = serializedData;
        }

        public void OnSaveData()
        {
            try
            {
                Debug.Log($"Saving disaster setup for current game");
                byte[] data;

                using (var stream = new MemoryStream())
                {
                    NaturalDisasterHandler edm = Singleton<NaturalDisasterHandler>.instance;

                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new SerializableDataDisasterSetup());

                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new SerializableDataForestFire());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new SerializableDataThunderstorm());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new SerializableDataSinkhole());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new SerializableDataTsunami());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new SerializableDataTornado());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new SerializableDataEarthquake());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new SerializableDataMeteorStrike());
                    data = stream.ToArray();
                }

                serializableData.SaveData(DataID, data);
            }
            catch (Exception ex)
            {
                Debug.Log(CommonProperties.LogMsgPrefix + "(save error) " + ex.Message);
            }
        }

        public void OnLoadData()
        {
            try
            {
                Debug.Log($"Loading disaster setup for current game");
                byte[] data = serializableData.LoadData(DataID);

                if (data == null)
                {
                    Debug.Log(CommonProperties.LogMsgPrefix + "No saved data");
                    return;
                }

                using (var stream = new MemoryStream(data))
                {
                    DataSerializer.Deserialize<SerializableDataDisasterSetup>(stream, DataSerializer.Mode.Memory);

                    DataSerializer.Deserialize<SerializableDataForestFire>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<SerializableDataThunderstorm>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<SerializableDataSinkhole>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<SerializableDataTsunami>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<SerializableDataTornado>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<SerializableDataEarthquake>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<SerializableDataMeteorStrike>(stream, DataSerializer.Mode.Memory);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(CommonProperties.LogMsgPrefix + "(load error) " + ex.Message);
            }

            SettingsScreen.UpdateUISettingsOptions();
        }

        public void OnReleased()
        {
            serializableData = null;
        }
    }
}