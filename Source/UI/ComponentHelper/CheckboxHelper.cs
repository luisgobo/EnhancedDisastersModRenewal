using ColossalFramework.UI;
using ICities;
using NaturalDisastersRenewal.UI.Extensions;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public static class CheckboxHelper
    {
        /// <summary>
        ///     Adds a checkbox to the provided UI helper group and applies an optional tooltip.
        /// </summary>
        /// <param name="group">Target UI helper group where the checkbox control will be added.</param>
        /// <param name="description">Text shown next to the checkbox.</param>
        /// <param name="defaultValue">Initial checkbox state.</param>
        /// <param name="eventCallback">Callback invoked when the checkbox value changes.</param>
        /// <param name="tooltip">Optional tooltip text shown when hovering the checkbox.</param>
        /// <param name="spacing">Optional spacing between items</param>
        /// <returns>The created <see cref="UICheckBox" /> instance.</returns>
        public static UICheckBox AddCheckbox(
            ref UIHelperBase group,
            string description,
            bool defaultValue,
            OnCheckChanged eventCallback,
            string tooltip = "",
            int spacing = 10)
        {
            var checkbox = (UICheckBox)group.AddCheckbox(description, defaultValue, eventCallback);
            checkbox.tooltip = tooltip;
            UIStyleHelper.ApplyCheckboxStyle(checkbox);
            group.AddSpacing(spacing);

            return checkbox;
        }
    }
}
