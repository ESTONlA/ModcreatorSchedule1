using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Schedule1ModdingTool.ViewModels;

namespace Schedule1ModdingTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            
            // Set up key bindings
            SetupKeyBindings();
            
            // Code editor is now a simple TextBox - no syntax highlighting setup needed
        }

        private void SetupKeyBindings()
        {
            // Add keyboard shortcuts
            var vm = DataContext as MainViewModel;
            if (vm == null) return;

            // Ctrl+N - New Project
            InputBindings.Add(new KeyBinding(vm.NewProjectCommand, Key.N, ModifierKeys.Control));
            
            // Ctrl+O - Open Project
            InputBindings.Add(new KeyBinding(vm.OpenProjectCommand, Key.O, ModifierKeys.Control));
            
            // Ctrl+S - Save Project
            InputBindings.Add(new KeyBinding(vm.SaveProjectCommand, Key.S, ModifierKeys.Control));
            
            // Ctrl+Shift+S - Save Project As
            InputBindings.Add(new KeyBinding(vm.SaveProjectAsCommand, Key.S, ModifierKeys.Control | ModifierKeys.Shift));
            
            // F5 - Regenerate Code
            InputBindings.Add(new KeyBinding(vm.RegenerateCodeCommand, Key.F5, ModifierKeys.None));
            
            // F6 - Compile
            InputBindings.Add(new KeyBinding(vm.CompileCommand, Key.F6, ModifierKeys.None));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm?.CurrentProject.IsModified == true)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save them before closing?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        vm.SaveProjectCommand.Execute(null);
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                }
            }

            base.OnClosing(e);
        }

        private void Blueprint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double-click to add quest
                if (sender is FrameworkElement element && element.DataContext is BlueprintTemplate template)
                {
                    var vm = DataContext as MainViewModel;
                    vm?.AddQuestCommand.Execute(template);
                }
            }
            else
            {
                // Start drag operation
                if (sender is FrameworkElement element && element.DataContext is BlueprintTemplate template)
                {
                    DragDrop.DoDragDrop(element, template, DragDropEffects.Copy);
                }
            }
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