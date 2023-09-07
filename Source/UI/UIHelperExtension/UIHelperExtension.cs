using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;

namespace NaturalDisastersRenewal.UI.UIHelperExtension
{
    public class UIHelperExtension : UIHelper
    {

        private static readonly string kLabelTemplate = "OptionsLabelTemplate";
        private UIComponent m_Root;

        public UIHelperExtension(UIComponent panel) : base(panel)
        {
            m_Root = panel;
        }

        public UIHelperBase AddLabel(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                UIPanel uIPanel = m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(kLabelTemplate)) as UIPanel;
                uIPanel.Find<UILabel>("Label").text = text;
                return new UIHelper(uIPanel.Find("Content"));
            }

            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create group with no name");
            return null;
        }
    }
}