using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestMod.Source
{
    public class MyBehaviour: MonoBehaviour
    {
        void Start()
        {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Aplication Started.");
        }

        //void Update()
        //{
        //    //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Application updated.");
        //}
    }
}