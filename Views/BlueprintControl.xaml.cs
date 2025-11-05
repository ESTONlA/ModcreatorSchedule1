using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Schedule1ModdingTool.ViewModels;

namespace Schedule1ModdingTool.Views
{
    /// <summary>
    /// Interaction logic for BlueprintControl.xaml
    /// </summary>
    public partial class BlueprintControl : UserControl
    {
        public static readonly DependencyProperty BlueprintProperty =
            DependencyProperty.Register("Blueprint", typeof(BlueprintTemplate), typeof(BlueprintControl), new PropertyMetadata(null));

        public BlueprintTemplate Blueprint
        {
            get { return (BlueprintTemplate)GetValue(BlueprintProperty); }
            set { SetValue(BlueprintProperty, value); }
        }

        public BlueprintControl()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double-click to add quest
                var mainWindow = Window.GetWindow(this);
                if (mainWindow?.DataContext is MainViewModel vm)
                {
                    vm.AddQuestCommand.Execute(Blueprint);
                }
            }
            else
            {
                // Start drag operation
                if (sender is FrameworkElement element)
                {
                    DragDrop.DoDragDrop(element, Blueprint, DragDropEffects.Copy);
                }
            }
        }
    }
}