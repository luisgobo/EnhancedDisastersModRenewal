using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.Common.enums;
using System;
using System.Security.Principal;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public class ComponentHelpers
    {
        public static void AddDropDown<T>(ref UIDropDown dropDown, ref UIHelperBase group, string description, string[] itemList, ref T value, OnDropdownSelectionChanged eventCallback, int valueIndex = 1)
        {
            dropDown = (UIDropDown)group.AddDropdown(
                description,
                itemList,
                Convert.ToInt32(value) * valueIndex,
                eventCallback
            );

            dropDown.width = (int)Math.Round(dropDown.width * 2.1f);
        }        
    }
}