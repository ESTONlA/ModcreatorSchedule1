using System.Windows;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.ViewModels;

namespace Schedule1ModdingTool.Views
{
    /// <summary>
    /// Interaction logic for ModExceptionWindow.xaml
    /// </summary>
    public partial class ModExceptionWindow : Window
    {
        private readonly ModExceptionData _exceptionData;

        public ModExceptionWindow(ModExceptionData exceptionData)
        {
            InitializeComponent();
            _exceptionData = exceptionData ?? throw new System.ArgumentNullException(nameof(exceptionData));
            DataContext = new ModExceptionViewModel(exceptionData);
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_exceptionData.GetFormattedString());
                MessageBox.Show("Exception details copied to clipboard.", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to copy to clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    /// <summary>
    /// ViewModel for displaying exception data in the UI.
    /// </summary>
    public class ModExceptionViewModel : ObservableObject
    {
        private readonly ModExceptionData _exceptionData;

        public ModExceptionViewModel(ModExceptionData exceptionData)
        {
            _exceptionData = exceptionData ?? throw new System.ArgumentNullException(nameof(exceptionData));
        }

        public string ExceptionType => _exceptionData.ExceptionType;
        public string Message => _exceptionData.Message;
        public string? StackTrace => _exceptionData.StackTrace;
        public string? SourceAssembly => _exceptionData.SourceAssembly;
        public string? ModName => _exceptionData.ModName;
        public System.DateTime Timestamp => _exceptionData.Timestamp;
        public bool IsUnhandled => _exceptionData.IsUnhandled;
        public string FormattedString => _exceptionData.GetFormattedString();
    }
}

