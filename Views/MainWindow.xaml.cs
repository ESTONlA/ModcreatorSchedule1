using System.ComponentModel;
using System.Linq;
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
        private GridLength? _storedCodeRowHeight; // Store user's preferred height
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            
            // Set up key bindings
            SetupKeyBindings();
            
            // Set up code editor syntax highlighting
            if (CodeEditor != null)
            {
                CodeEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");
            }

            // Show wizard on startup if no project is loaded
            Loaded += MainWindow_Loaded;
            
            // Subscribe to IsCodeVisible changes to fix grid divider issue
            if (DataContext is MainViewModel vm)
            {
                vm.PropertyChanged += MainViewModel_PropertyChanged;
            }
        }
        
        private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsCodeVisible) && sender is MainViewModel vm)
            {
                if (CodeContentRow == null) return;
                
                if (!vm.IsCodeVisible)
                {
                    // When hiding: store the current height ratio, then collapse
                    StoreCurrentHeightRatio();
                    // Set height to 0 to ensure proper collapse
                    CodeContentRow.Height = new GridLength(0);
                }
                else
                {
                    // When showing: restore stored height or default to half-and-half (1*)
                    // Use Dispatcher to ensure this happens after the binding updates
                    Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        if (CodeContentRow != null && vm.IsCodeVisible)
                        {
                            if (_storedCodeRowHeight.HasValue)
                            {
                                CodeContentRow.Height = _storedCodeRowHeight.Value;
                            }
                            else
                            {
                                // Default to half-and-half: both rows get 1*
                                CodeContentRow.Height = new GridLength(1, GridUnitType.Star);
                            }
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }
        
        private void StoreCurrentHeightRatio()
        {
            if (CodeContentRow == null || WorkspaceGrid == null) return;
            
            var currentHeight = CodeContentRow.Height;
            if (currentHeight.Value <= 0) return;
            
            // Get the row definitions from the workspace grid
            if (WorkspaceGrid.RowDefinitions.Count < 3) return;
            
            var topRow = WorkspaceGrid.RowDefinitions[0];
            var codeRow = WorkspaceGrid.RowDefinitions[2];
            
            // Calculate the ratio between top and code rows
            if (codeRow.Height.GridUnitType == GridUnitType.Star)
            {
                // Already in star units, store directly
                _storedCodeRowHeight = codeRow.Height;
            }
            else if (codeRow.Height.GridUnitType == GridUnitType.Pixel && topRow.Height.GridUnitType == GridUnitType.Pixel)
            {
                // Both are pixels - calculate the ratio directly
                var topPixels = topRow.Height.Value;
                var codePixels = codeRow.Height.Value;
                if (topPixels > 0)
                {
                    // Calculate star ratio: codePixels / topPixels
                    var ratio = codePixels / topPixels;
                    _storedCodeRowHeight = new GridLength(ratio, GridUnitType.Star);
                }
                else
                {
                    _storedCodeRowHeight = new GridLength(1, GridUnitType.Star);
                }
            }
            else if (codeRow.Height.GridUnitType == GridUnitType.Pixel)
            {
                // Code row is pixels, top row is stars - need to calculate star ratio based on actual rendered heights
                // Use LayoutUpdated to measure after layout completes
                WorkspaceGrid.LayoutUpdated += OnWorkspaceGridLayoutUpdated;
            }
        }
        
        private void OnWorkspaceGridLayoutUpdated(object? sender, EventArgs e)
        {
            // Unsubscribe immediately to avoid multiple calls
            if (WorkspaceGrid != null)
            {
                WorkspaceGrid.LayoutUpdated -= OnWorkspaceGridLayoutUpdated;
            }
            
            if (WorkspaceGrid == null || WorkspaceGrid.RowDefinitions.Count < 3) return;
            
            // Measure actual rendered heights
            var topRow = WorkspaceGrid.RowDefinitions[0];
            var codeRow = WorkspaceGrid.RowDefinitions[2];
            
            // Get the actual rendered height of the grid
            var gridActualHeight = WorkspaceGrid.ActualHeight;
            if (gridActualHeight <= 0) return;
            
            // Find the Grid elements in each row to measure their heights
            var topRowElement = WorkspaceGrid.Children.Cast<UIElement>()
                .FirstOrDefault(c => Grid.GetRow(c) == 0) as FrameworkElement;
            var codeRowElement = WorkspaceGrid.Children.Cast<UIElement>()
                .FirstOrDefault(c => Grid.GetRow(c) == 2) as FrameworkElement;
            
            if (topRowElement != null && codeRowElement != null)
            {
                var topActualHeight = topRowElement.ActualHeight;
                var codeActualHeight = codeRowElement.ActualHeight;
                
                if (topActualHeight > 0)
                {
                    // Calculate star ratio based on actual rendered heights
                    var ratio = codeActualHeight / topActualHeight;
                    _storedCodeRowHeight = new GridLength(ratio, GridUnitType.Star);
                }
                else
                {
                    _storedCodeRowHeight = new GridLength(1, GridUnitType.Star);
                }
            }
            else
            {
                // Fallback: if code row is pixels, estimate based on pixel value
                if (codeRow.Height.GridUnitType == GridUnitType.Pixel && topRow.Height.GridUnitType == GridUnitType.Star)
                {
                    // Estimate: assume top row takes most of the space, code row is smaller
                    // Store as 1* for half-and-half as reasonable default
                    _storedCodeRowHeight = new GridLength(1, GridUnitType.Star);
                }
            }
        }
        
        private void CodeSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            // When user finishes dragging the splitter, store the new height ratio
            if (DataContext is MainViewModel vm && vm.IsCodeVisible)
            {
                StoreCurrentHeightRatio();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if first start wizard needs to be shown
            var settings = Models.ModSettings.Load();
            if (!settings.IsFirstStartComplete)
            {
                var firstStartWizard = new FirstStartWizardWindow
                {
                    Owner = this
                };

                var wizardResult = firstStartWizard.ShowDialog();
                if (wizardResult != true)
                {
                    // User cancelled first start wizard - exit application
                    Application.Current.Shutdown();
                    return;
                }
            }

            var vm = DataContext as MainViewModel;
            if (vm != null && string.IsNullOrWhiteSpace(vm.CurrentProject.ProjectName))
            {
                // Show startup dialog instead of directly showing wizard
                var startupDialog = new StartupDialog
                {
                    Owner = this
                };

                var result = startupDialog.ShowDialog();
                
                if (startupDialog.SelectedAction == StartupDialog.StartupAction.CreateNew)
                {
                    vm.NewProjectCommand.Execute(null);
                }
                else if (startupDialog.SelectedAction == StartupDialog.StartupAction.OpenExisting)
                {
                    vm.OpenProjectCommand.Execute(null);
                }
                else if (startupDialog.SelectedAction == StartupDialog.StartupAction.Exit)
                {
                    Application.Current.Shutdown();
                }
                else if (result == false)
                {
                    // User closed dialog without selecting an option
                    Application.Current.Shutdown();
                }
            }
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
            
            // F6 - Build Mod
            InputBindings.Add(new KeyBinding(vm.BuildModCommand, Key.F6, ModifierKeys.None));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            
            // Unsubscribe from property changes
            if (vm != null)
            {
                vm.PropertyChanged -= MainViewModel_PropertyChanged;
            }
            
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

        private void AddQuestButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.AvailableBlueprints.Count > 0)
            {
                vm.AddQuestCommand.Execute(vm.AvailableBlueprints[0]);
            }
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is OpenElementTab tab && DataContext is MainViewModel vm)
            {
                vm.CloseTab(tab);
            }
        }
    }
}