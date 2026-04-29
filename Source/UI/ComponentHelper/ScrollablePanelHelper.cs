using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public static class ScrollablePanelHelper
    {
        private const float ContentLeftInset = 10f;
        private const float ContentRightInset = 4f;
        private const float ContentBottomInset = 10f;

        public static UIScrollablePanel Create(UIPanel tabPanel, float yPosition)
        {
            UIScrollablePanel scrollablePanel = tabPanel.AddUIComponent<UIScrollablePanel>();

            float scrollablePanelWidth = tabPanel.width - ContentLeftInset - ContentRightInset;
            float scrollablePanelHeight = tabPanel.height - yPosition - ContentBottomInset;

            scrollablePanel.size = new Vector2(scrollablePanelWidth, scrollablePanelHeight);
            scrollablePanel.relativePosition = new Vector2(ContentLeftInset, yPosition);
            scrollablePanel.autoLayout = false;
            scrollablePanel.clipChildren = true;
            scrollablePanel.scrollWheelAmount = 20;
            scrollablePanel.eventMouseWheel += delegate(UIComponent component, UIMouseEventParameter eventParam)
            {
                scrollablePanel.scrollPosition +=
                    new Vector2(0f, -eventParam.wheelDelta * scrollablePanel.scrollWheelAmount);
            };

            return scrollablePanel;
        }
    }
}
