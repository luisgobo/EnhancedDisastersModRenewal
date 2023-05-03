using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Serialization.NaturalDisaster;
using NaturalDisastersRenewal.UI;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

//This class allows to save/load specific configuration for loadded game.
namespace NaturalDisastersRenewal.Serialization.Setup
{    
    public class LoadedGameSerializableDataExtension : ISerializableDataExtension
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
                
                //Remove Current Data
                serializableData.EraseData(DataID);
                if (serializableData.LoadData(DataID)!= null)
                    throw new Exception("There was an issue cleaning disaster in-game setup. Try saving again");

                //Save new etup
                serializableData.SaveData(DataID, data);
                Debug.Log($"Disaster setup saved for current game");
            }
            catch (Exception ex)
            {
                Debug.Log(CommonProperties.logMsgPrefix + "(save error) " + ex.Message);
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
                    Debug.Log(CommonProperties.logMsgPrefix + "No saved data");
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
                SettingsScreen.UpdateUISettingsOptions();
                Debug.Log($"Disaster setup data loaded for current game");
            }
            catch (Exception ex)
            {
                Debug.Log(CommonProperties.logMsgPrefix + "(load error) " + ex.Message);
            }
        }

        public void OnReleased()
        {
            serializableData = null;
        }
    }
}