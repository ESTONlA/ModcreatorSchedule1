using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.ViewModels;

namespace Schedule1ModdingTool.Views
{
    /// <summary>
    /// Interaction logic for QuestItemsView.xaml
    /// </summary>
    public partial class QuestItemsView : UserControl
    {
        private readonly System.Windows.Threading.DispatcherTimer _doubleClickTimer;
        private QuestBlueprint? _lastClickedQuest;
        private const int DoubleClickDelay = 300;

        public QuestItemsView()
        {
            InitializeComponent();
            _doubleClickTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = System.TimeSpan.FromMilliseconds(DoubleClickDelay)
            };
            _doubleClickTimer.Tick += (_, _) =>
            {
                _doubleClickTimer.Stop();
                _lastClickedQuest = null;
            };
        }

        private void BackToCategories_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this);
            if (mainWindow?.DataContext is MainViewModel vm)
            {
                vm.WorkspaceViewModel.SelectedCategory = null;
            }
        }

        private void QuestTile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is QuestBlueprint quest)
            {
                var mainWindow = Window.GetWindow(this);
                if (mainWindow?.DataContext is MainViewModel vm)
                {
                    vm.SelectedQuest = quest;

                    if (_lastClickedQuest == quest && _doubleClickTimer.IsEnabled)
                    {
                        _doubleClickTimer.Stop();
                        _lastClickedQuest = null;
                        vm.OpenQuestInTab(quest);
                    }
                    else
                    {
                        _lastClickedQuest = quest;
                        _doubleClickTimer.Start();
                    }
                }
            }
        }

        private void QuestTile_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is QuestBlueprint quest)
            {
                var mainWindow = Window.GetWindow(this);
                if (mainWindow?.DataContext is MainViewModel vm)
                {
                    vm.SelectedQuest = quest;
                    // Show context menu
                    var contextMenu = this.Resources["QuestContextMenu"] as ContextMenu;
                    if (contextMenu != null)
                    {
                        contextMenu.PlacementTarget = element;
                        contextMenu.IsOpen = true;
                    }
                }
            }
        }

        private void AddQuest_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this);
            if (mainWindow?.DataContext is MainViewModel vm && vm.AvailableBlueprints.Count > 0)
            {
                vm.AddQuestCommand.Execute(vm.AvailableBlueprints[0]);
            }
        }

        private void RemoveQuest_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this);
            if (mainWindow?.DataContext is MainViewModel vm)
            {
                // Remove the currently selected quest
                if (vm.SelectedQuest != null)
                {
                    vm.RemoveQuestCommand.Execute(null);
                }
            }
        }
    }
}

