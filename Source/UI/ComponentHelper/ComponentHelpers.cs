using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.Logger;
using System;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public class ComponentHelpers
    {
        public static void AddDropDown(bool freezeUI, ref UIDropDown dropDown, ref UIHelperBase group, string description, string[] itemList, ref int value, OnDropdownSelectionChanged eventCallback)
        {            
            dropDown = (UIDropDown)group.AddDropdown(
                description,
                itemList,
                value,
                eventCallback            
            );

            dropDown.width = (int)Math.Round(dropDown.width * 2.1f);
        }        
    }
}