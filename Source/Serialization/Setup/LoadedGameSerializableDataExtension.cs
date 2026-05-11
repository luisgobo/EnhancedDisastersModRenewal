using System;
using System.IO;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Serialization.NaturalDisaster;
using UnityEngine;

//This class allows to save/load specific configuration for loadded game.
namespace NaturalDisastersRenewal.Serialization.Setup
{
    public class LoadedGameSerializableDataExtension : ISerializableDataExtension
    {
        private const string DataID = CommonProperties.DataId;
        private const uint DataVersion = 11;
        private ISerializableData _serializableData;

        public void OnCreated(ISerializableData serializedData)
        {
            _serializableData = serializedData;
        }

        public void OnSaveData()
        {
            try
            {
                Debug.Log("Saving disaster setup for current game");
                byte[] data;

                using (var stream = new MemoryStream())
                {
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion,
                        new SerializableDataDisasterSetup());

                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion,
                        new SerializableDataForestFire());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion,
                        new SerializableDataThunderstorm());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion,
                        new SerializableDataSinkhole());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion,
                        new SerializableDataTsunami());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion,
                        new SerializableDataTornado());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion,
                        new SerializableDataEarthquake());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion,
                        new SerializableDataMeteorStrike());
                    data = stream.ToArray();
                }

                //Remove Current Data
                _serializableData.EraseData(DataID);
                if (_serializableData.LoadData(DataID) != null)
                    throw new Exception("There was an issue cleaning disaster in-game setup. Try saving again");

                //Save new etup
                _serializableData.SaveData(DataID, data);
                Debug.Log("Disaster setup saved for current game");
            }
            catch (Exception ex)
            {
                Debug.Log(CommonProperties.LogMessagePrefix + "(save error) " + ex.Message);
            }
        }

        public void OnLoadData()
        {
            try
            {
                Debug.Log("Loading disaster setup for current game");
                var data = _serializableData.LoadData(DataID);

                if (data == null)
                {
                    Debug.Log(CommonProperties.LogMessagePrefix + "No saved data");
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

                Debug.Log("Disaster setup data loaded for current game");
            }
            catch (Exception ex)
            {
                Debug.Log(CommonProperties.LogMessagePrefix + "(load error) " + ex.Message);
            }
        }

        public void OnReleased()
        {
            _serializableData = null;
        }
    }
}
