using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public static class ScrollablePanelHelper
    {
        private const float ScrollbarWidth = 12f;
        private const float ContentLeftInset = 10f;

        public static UIScrollablePanel Create(UIPanel tabPanel, float yPosition)
        {
            UIScrollablePanel scrollablePanel = tabPanel.AddUIComponent<UIScrollablePanel>();
            UIScrollbar scrollbar = tabPanel.AddUIComponent<UIScrollbar>();
            UISlicedSprite track = scrollbar.AddUIComponent<UISlicedSprite>();
            UISlicedSprite thumb = track.AddUIComponent<UISlicedSprite>();

            scrollablePanel.size = new Vector2(tabPanel.width - ContentLeftInset - ScrollbarWidth, tabPanel.height - 20f);
            scrollablePanel.relativePosition = new Vector2(ContentLeftInset, yPosition);
            scrollablePanel.height = tabPanel.height - 40f;
            scrollablePanel.autoLayout = false;
            scrollablePanel.clipChildren = true;
            scrollablePanel.scrollWheelAmount = 20;

            scrollbar.orientation = UIOrientation.Vertical;
            scrollbar.width = ScrollbarWidth;
            scrollbar.relativePosition = new Vector2(tabPanel.width - ScrollbarWidth, yPosition);
            scrollbar.height = tabPanel.height - 20f;

            track.spriteName = "ScrollbarTrack";
            track.size = new Vector2(ScrollbarWidth, tabPanel.height - 20f);
            track.relativePosition = new Vector2(0f, -10f);
            scrollbar.trackObject = track;

            thumb.spriteName = "ScrollbarThumb";
            thumb.height = 10f;
            scrollbar.thumbObject = thumb;

            scrollablePanel.verticalScrollbar = scrollbar;
            scrollablePanel.eventMouseWheel += delegate(UIComponent component, UIMouseEventParameter eventParam)
            {
                scrollablePanel.scrollPosition +=
                    new Vector2(0f, -eventParam.wheelDelta * scrollablePanel.scrollWheelAmount);
            };

            return scrollablePanel;
        }
    }
}
