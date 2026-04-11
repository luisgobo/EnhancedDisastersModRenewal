using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public static class ScrollablePanelHelper
    {
        public static UIScrollablePanel Create(UIPanel tabPanel, float yPosition)
        {
            UIScrollablePanel scrollablePanel = tabPanel.AddUIComponent<UIScrollablePanel>();
            UIScrollbar scrollbar = tabPanel.AddUIComponent<UIScrollbar>();
            UISlicedSprite track = scrollbar.AddUIComponent<UISlicedSprite>();
            UISlicedSprite thumb = track.AddUIComponent<UISlicedSprite>();

            scrollablePanel.size = new Vector2(tabPanel.width - 12f, tabPanel.height - 20f);
            scrollablePanel.relativePosition = new Vector2(10f, yPosition);
            scrollablePanel.height = tabPanel.height - 40f;
            scrollablePanel.autoLayout = false;
            scrollablePanel.clipChildren = true;
            scrollablePanel.scrollWheelAmount = 20;

            scrollbar.orientation = UIOrientation.Vertical;
            scrollbar.width = 12f;
            scrollbar.relativePosition = new Vector2(tabPanel.width - 12f, yPosition);
            scrollbar.height = tabPanel.height - 20f;

            track.spriteName = "ScrollbarTrack";
            track.size = new Vector2(12f, tabPanel.height - 20f);
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
