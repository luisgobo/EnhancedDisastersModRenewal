using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public sealed class TabHelper : UITabstrip
    {
        public const float TAB_STRIP_HEIGHT = 40f;
        public const float TAB_PADDING = 10f;

        public UIHelper AddTabPage(string name, bool setNewLine = false)
        {
            UIButton tabButton = base.AddTab(name);
            tabButton.normalBgSprite = "SubBarButtonBase";
            tabButton.disabledBgSprite = "SubBarButtonBaseDisabled";
            tabButton.focusedBgSprite = "SubBarButtonBaseFocused";
            tabButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            tabButton.pressedBgSprite = "SubBarButtonBasePressed";
            tabButton.textPadding = new RectOffset(10, 10, 10, 6);
            tabButton.textScale = 0.65f;
            tabButton.autoSize = true;

            if (setNewLine)
            {
                float currentX = 0f;
                float currentY = 0f;

                currentX = 0f;
                currentY += TAB_STRIP_HEIGHT + TAB_PADDING;
                tabButton.position = new Vector3(currentX, currentY);

            }
            else
            {
                tabButton.position = new Vector3(0, 0);
            }

            selectedIndex = tabCount - 1;
            UIPanel currentPanel = tabContainer.components[selectedIndex] as UIPanel;
            if (currentPanel == null)
            {
                currentPanel = tabContainer.AddUIComponent<UIPanel>();
                tabContainer.components[selectedIndex] = currentPanel;
            }
            currentPanel.autoLayout = true;

            //UpdateTabPositions();

            return new UIHelper(currentPanel);
        }

        private void UpdateTabPositions()
        {
            float currentX = 0f;
            float currentY = 0f;
            int totalTabs = components.Count;

            for (int i = 0; i < totalTabs; i++)
            {
                UIButton tabButton = components[i] as UIButton;

                // Move the last three tabs to the next line
                if (i >= totalTabs - 3)
                {
                    if (i == totalTabs - 3)
                    {
                        currentX = 0f;
                        currentY += TAB_STRIP_HEIGHT + TAB_PADDING;
                    }
                }
                else if (currentX + tabButton.width > width)
                {
                    currentX = 0f;
                    currentY += TAB_STRIP_HEIGHT + TAB_PADDING;
                }

                tabButton.relativePosition = new Vector3(currentX, currentY);
                currentX += tabButton.width + TAB_PADDING;
            }

            height = currentY + TAB_STRIP_HEIGHT;
        }

        public static TabHelper Create(UIHelper helper)
        {
            UIComponent optionsContainer = helper.self as UIComponent;
            float orgOptsContainerWidth = optionsContainer.width;
            float orgOptsContainerHeight = optionsContainer.height;

            //int paddingRight = 10; //Options container is Scrollable panel itself(reserves space for scroll - which we don't use)
            //optionsContainer.size = new Vector2(orgOptsContainerWidth + paddingRight, orgOptsContainerHeight);

            TabHelper tabStrip = optionsContainer.AddUIComponent<TabHelper>();
            tabStrip.relativePosition = new Vector3(0, 0);
            tabStrip.size = new Vector2(orgOptsContainerWidth, TAB_STRIP_HEIGHT);

            UITabContainer tabContainer = optionsContainer.AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, tabStrip.height);
            tabContainer.width = orgOptsContainerWidth;
            tabContainer.height = orgOptsContainerHeight - tabStrip.height;
            tabStrip.tabPages = tabContainer;

            return tabStrip;
        }
    }
}
