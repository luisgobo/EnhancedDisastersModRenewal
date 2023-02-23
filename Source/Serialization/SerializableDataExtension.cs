using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.Services.Handlers;
using NaturalDisastersRenewal.Services.NaturalDisaster;
using NaturalDisastersRenewal.Services.Setup;
using NaturalDisastersRenewal.UI;
using System;
using System.IO;
using UnityEngine;

//THIS STRUCTURE IS GETTING ISSUES WITH SERIALIZATION AND LOADING DATA
namespace NaturalDisastersRenewal.Serialization
{
    //public class SerializableDataExtension : ISerializableDataExtension
    public class SerializableDataExtension
    {
        //public const string DataID = CommonProperties.dataId;
        //public const uint DataVersion = 3;
        //ISerializableData serializableData;

        public void OnCreated(ISerializableData serializedData)
        {
            DebugLogger.Log(">>>>>>>&&&&&&&>>>>>>>>>OnCreated");
            //serializableData = serializedData;
        }

        public void OnSaveData()
        {
            DebugLogger.Log(">>>>>>>&&&&&&&>>>>>>>>>OnSaveData");
            //try
            //{
            //    byte[] data;

            //    using (var stream = new MemoryStream())
            //    {
            //        NaturalDisasterHandler edm = Singleton<NaturalDisasterHandler>.instance;

            //        DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new DisasterSetupService.Data());

            //        DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new ForestFireService.Data());
            //        DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new ThunderstormService.Data());
            //        DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new SinkholeService.Data());
            //        DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new TsunamiService.Data());
            //        DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new TornadoService.Data());
            //        DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new EarthquakeService.Data());
            //        DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new MeteorStrikeService.Data());
            //        data = stream.ToArray();
            //    }

            //    serializableData.SaveData(DataID, data);
            //}
            //catch (Exception ex)
            //{
            //    Debug.Log(CommonProperties.LogMsgPrefix + "(save error) " + ex.Message);
            //}
        }

        public void OnLoadData()
        {
            DebugLogger.Log(">>>>>>>&&&&&&&>>>>>>>>>OnLoadData");
            //try
            //{
            //    byte[] data = serializableData.LoadData(DataID);

            //    if (data == null)
            //    {
            //        Debug.Log(CommonProperties.LogMsgPrefix + "No saved data");
            //        return;
            //    }

            //    using (var stream = new MemoryStream(data))
            //    {
            //        DataSerializer.Deserialize<DisasterSetupService.Data>(stream, DataSerializer.Mode.Memory);

            //        DataSerializer.Deserialize<ForestFireService.Data>(stream, DataSerializer.Mode.Memory);
            //        DataSerializer.Deserialize<ThunderstormService.Data>(stream, DataSerializer.Mode.Memory);
            //        DataSerializer.Deserialize<SinkholeService.Data>(stream, DataSerializer.Mode.Memory);
            //        DataSerializer.Deserialize<TsunamiService.Data>(stream, DataSerializer.Mode.Memory);
            //        DataSerializer.Deserialize<TornadoService.Data>(stream, DataSerializer.Mode.Memory);
            //        DataSerializer.Deserialize<EarthquakeService.Data>(stream, DataSerializer.Mode.Memory);
            //        DataSerializer.Deserialize<MeteorStrikeService.Data>(stream, DataSerializer.Mode.Memory);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Debug.Log(CommonProperties.LogMsgPrefix + "(load error) " + ex.Message);
            //}

            //SettingsScreen.UpdateUISettingsOptions();
        }

        public void OnReleased()
        {
            DebugLogger.Log(">>>>>>>&&&&&&&>>>>>>>>>OnReleased");
            //serializableData = null;
        }
    }
}