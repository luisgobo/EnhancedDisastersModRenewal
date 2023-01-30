using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.DisasterServices;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.UI;
using System;
using System.IO;
using UnityEngine;

namespace NaturalDisastersRenewal.Serialization
{
    public class SerializableDataExtension : ISerializableDataExtension
    {
        public const string DataID = "EnhancedDisastersMod";
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
                byte[] data;

                using (var stream = new MemoryStream())
                {
                    DisasterServices.DisasterManager edm = Singleton<DisasterServices.DisasterManager>.instance;

                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new DisastersServiceBase.Data());

                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new ForestFireService.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new ThunderstormService.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new SinkholeService.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new TsunamiService.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new TornadoService.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new EarthquakeService.Data());
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new MeteorStrikeService.Data());
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
                byte[] data = serializableData.LoadData(DataID);

                if (data == null)
                {
                    Debug.Log(CommonProperties.LogMsgPrefix + "No saved data");
                    return;
                }

                using (var stream = new MemoryStream(data))
                {
                    DataSerializer.Deserialize<DisastersServiceBase.Data>(stream, DataSerializer.Mode.Memory);

                    DataSerializer.Deserialize<ForestFireService.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<ThunderstormService.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<SinkholeService.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<TsunamiService.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<TornadoService.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<EarthquakeService.Data>(stream, DataSerializer.Mode.Memory);
                    DataSerializer.Deserialize<MeteorStrikeService.Data>(stream, DataSerializer.Mode.Memory);
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