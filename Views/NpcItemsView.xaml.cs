using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.ViewModels;

namespace Schedule1ModdingTool.Views
{
    public partial class NpcItemsView : UserControl
    {
        private readonly System.Windows.Threading.DispatcherTimer _doubleClickTimer;
        private NpcBlueprint? _lastClickedNpc;
        private const int DoubleClickDelay = 300;

        public NpcItemsView()
        {
            InitializeComponent();
            _doubleClickTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = System.TimeSpan.FromMilliseconds(DoubleClickDelay)
            };
            _doubleClickTimer.Tick += (_, _) =>
            {
                _doubleClickTimer.Stop();
                _lastClickedNpc = null;
            };
        }

        private void BackToCategories_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this)?.DataContext is MainViewModel vm)
            {
                vm.WorkspaceViewModel.SelectedCategory = null;
            }
        }

        private void AddNpc_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this)?.DataContext is MainViewModel vm &&
                vm.AvailableNpcBlueprints.Count > 0)
            {
                vm.AddNpcCommand.Execute(vm.AvailableNpcBlueprints[0]);
            }
        }

        private void RemoveNpc_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this)?.DataContext is MainViewModel vm)
            {
                vm.RemoveNpcCommand.Execute(null);
            }
        }

        private void NpcTile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element || element.DataContext is not NpcBlueprint npc)
                return;

            if (Window.GetWindow(this)?.DataContext is MainViewModel vm)
            {
                vm.SelectedNpc = npc;

                if (_lastClickedNpc == npc && _doubleClickTimer.IsEnabled)
                {
                    _doubleClickTimer.Stop();
                    _lastClickedNpc = null;
                    vm.OpenNpcInTab(npc);
                }
                else
                {
                    _lastClickedNpc = npc;
                    _doubleClickTimer.Start();
                }
            }
        }

        private void NpcTile_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element || element.DataContext is not NpcBlueprint npc)
                return;

            if (Window.GetWindow(this)?.DataContext is MainViewModel vm)
            {
                vm.SelectedNpc = npc;
                if (Resources["NpcContextMenu"] is ContextMenu menu)
                {
                    menu.PlacementTarget = element;
                    menu.IsOpen = true;
                }
            }
        }
    }
}
