using System.Windows;
using Schedule1ModdingTool.ViewModels;
using Schedule1ModdingTool.Models;

namespace Schedule1ModdingTool.Views
{
    /// <summary>
    /// Interaction logic for QuestEditWindow.xaml
    /// </summary>
    public partial class QuestEditWindow : Window
    {
        public QuestEditWindow(QuestBlueprint quest, MainViewModel parentViewModel)
        {
            InitializeComponent();
            
            // Create the ViewModel for this edit window
            var questEditViewModel = new QuestEditViewModel(quest, parentViewModel);
            DataContext = questEditViewModel;
            
            // Handle dialog result when commands execute
            questEditViewModel.CloseRequested += (sender, result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}