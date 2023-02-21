using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.Common.enums;
using System;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public class ComponentHelpers
    {
        public static void AddDropDown<T>(bool freezeUI, ref UIDropDown dropDown, ref UIHelperBase group, string description, string[] itemList, ref T value, OnDropdownSelectionChanged eventCallback)
        {
            dropDown = (UIDropDown)group.AddDropdown(
                description,
                itemList,
                Convert.ToInt32(value),
                eventCallback
            );

            dropDown.width = (int)Math.Round(dropDown.width * 2.1f);
        }
    }
}