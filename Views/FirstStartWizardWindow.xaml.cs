using System.Windows;
using Schedule1ModdingTool.ViewModels;

namespace Schedule1ModdingTool.Views
{
    /// <summary>
    /// Interaction logic for FirstStartWizardWindow.xaml
    /// </summary>
    public partial class FirstStartWizardWindow : Window
    {
        public FirstStartWizardWindow()
        {
            InitializeComponent();
            DataContext = new FirstStartWizardViewModel();
            
            var vm = DataContext as FirstStartWizardViewModel;
            if (vm != null)
            {
                vm.WizardCompleted += () =>
                {
                    DialogResult = true;
                    Close();
                };
            }
        }
    }
}

