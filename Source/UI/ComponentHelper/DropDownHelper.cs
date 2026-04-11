using ColossalFramework.UI;
using ICities;
using System;


namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public static class DropDownHelper
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