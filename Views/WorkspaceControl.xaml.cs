using System.Windows;
using System.Windows.Controls;
using Schedule1ModdingTool.ViewModels;

namespace Schedule1ModdingTool.Views
{
    /// <summary>
    /// Interaction logic for WorkspaceControl.xaml
    /// </summary>
    public partial class WorkspaceControl : UserControl
    {
        public WorkspaceControl()
        {
            InitializeComponent();
        }

        private void QuestListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(BlueprintTemplate)))
            {
                var template = (BlueprintTemplate)e.Data.GetData(typeof(BlueprintTemplate));
                if (DataContext is MainViewModel vm)
                {
                    vm.AddQuestCommand.Execute(template);
                }
            }
        }

        private void QuestListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(BlueprintTemplate)))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
    }
}