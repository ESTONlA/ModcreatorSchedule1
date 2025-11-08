using System.Diagnostics;
using System.Windows.Controls;
using Schedule1ModdingTool.ViewModels;

namespace Schedule1ModdingTool.Views
{
    /// <summary>
    /// Interaction logic for ResourcesView.xaml
    /// </summary>
    public partial class ResourcesView : UserControl
    {
        public ResourcesView()
        {
            InitializeComponent();
            Loaded += ResourcesView_Loaded;
        }

        private void ResourcesView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Debug.WriteLine($"[ResourcesView] Loaded - DataContext type: {DataContext?.GetType().Name ?? "null"}");
            if (DataContext is MainViewModel vm)
            {
                Debug.WriteLine($"[ResourcesView] MainViewModel found - AddResourceCommand is null: {vm.AddResourceCommand == null}");
            }
            else
            {
                Debug.WriteLine("[ResourcesView] DataContext is NOT MainViewModel!");
            }
        }
    }
}

