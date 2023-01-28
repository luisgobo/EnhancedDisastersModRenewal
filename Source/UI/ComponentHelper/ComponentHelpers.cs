using ColossalFramework.UI;
using ICities;
using System;
using System.IO;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    internal class ComponentHelpers
    {
        public static void AddDropDown(bool freezeUI, ref UIDropDown dropDown, ref UIHelperBase group, string description, string[] itemList, int value)
        {
            DebugLogger.Log("Creating dropbox: " + description);
            DebugLogger.Log("freezeUI Value: " + freezeUI.ToString());

            dropDown = (UIDropDown)group.AddDropdown(
                description,
                itemList,
                value,
                delegate (int selection)
                {
                    DebugLogger.Log("dropbox delegate. FreezeUI: " + freezeUI.ToString());
                    if (!freezeUI)
                    {
                        value = selection;
                        DebugLogger.Log("Value = " + value.ToString());
                    }
                }
            );

            dropDown.width = (int)Math.Round(dropDown.width * 1.4f);
            
            DebugLogger.Log("Dropbox created");
        }
    }
}
