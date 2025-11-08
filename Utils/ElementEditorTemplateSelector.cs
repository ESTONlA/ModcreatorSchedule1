using System.Windows;
using System.Windows.Controls;
using Schedule1ModdingTool.ViewModels;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// Chooses the appropriate editor template based on the open tab's element type.
    /// </summary>
    public class ElementEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? QuestTemplate { get; set; }
        public DataTemplate? NpcTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is OpenElementTab tab)
            {
                if (tab.Quest != null && QuestTemplate != null)
                {
                    return QuestTemplate;
                }

                if (tab.Npc != null && NpcTemplate != null)
                {
                    return NpcTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
