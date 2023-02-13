using ColossalFramework.UI;
using ICities;
using NaturalDisasterRenewal_Reestructured.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturalDisasterRenewal_Reestructured.UI.ComponentHelper
{
    public class ComponentHelpers
    {
        public static void AddDropDown(bool freezeUI, ref UIDropDown dropDown, ref UIHelperBase group, string description, string[] itemList, ref EvacuationOptions value, OnDropdownSelectionChanged eventCallback)
        {
            dropDown = (UIDropDown)group.AddDropdown(
                description,
                itemList,
                (int)value,
                eventCallback
            );

            dropDown.width = (int)Math.Round(dropDown.width * 2.1f);
        }
    }
}
