using ICities;

namespace NaturalDisastersRenewal.UI.Extensions
{
    public static class UIHelperExtensions
    {
        public static void AddSpacing(this UIHelperBase group, int spacingValue = 10)
        {
            if (group == null)
                return;

            group.AddSpace(spacingValue);
        }
    }
}