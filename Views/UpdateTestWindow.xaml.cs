using System;
using System.IO;
using System.Windows;
using Schedule1ModdingTool.Services;
using Schedule1ModdingTool.Utils;

namespace Schedule1ModdingTool.Views
{
    /// <summary>
    /// Test window for debugging auto-update functionality
    /// Only available in Debug builds
    /// </summary>
    public partial class UpdateTestWindow : Window
    {
#if DEBUG
        private string? _testXmlPath;

        public UpdateTestWindow()
        {
            InitializeComponent();
            CurrentVersionText.Text = $"{VersionInfo.Version} (Base: {VersionInfo.BaseVersion})";
        }

        private async void TestNormalButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Configure UpdateService based on UI settings
                UpdateService.DebugMode = DebugModeCheckBox.IsChecked ?? false;
                UpdateService.TestXmlUrl = string.IsNullOrWhiteSpace(TestXmlUrlTextBox.Text)
                    ? null
                    : TestXmlUrlTextBox.Text;

                // Run the update check
                await UpdateService.CheckForUpdatesAsync(silent: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error during test: {ex.Message}\n\n{ex.StackTrace}",
                    "Test Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // Reset after test
                UpdateService.DebugMode = false;
                UpdateService.TestXmlUrl = null;
            }
        }

        private void CreateTestXmlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create test XML in temp directory
                var tempPath = Path.Combine(Path.GetTempPath(), "Schedule1ModdingTool_TestUpdate");
                Directory.CreateDirectory(tempPath);

                _testXmlPath = Path.Combine(tempPath, "AutoUpdater.xml");

                // Create test XML with a very high version number to simulate update available
                // NOTE: AutoUpdater.NET cannot handle semantic versioning suffixes like "-beta.99"
                // It only accepts major.minor.patch format (e.g., "99.99.99")
                var testVersion = "99.99.99";
                var testXml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<item>
    <version>{testVersion}</version>
    <url>https://github.com/ESTONlA/ModcreatorSchedule1/releases/download/v{testVersion}/Schedule1ModdingTool-{testVersion}.zip</url>
    <changelog>https://github.com/ESTONlA/ModcreatorSchedule1/releases</changelog>
    <mandatory>false</mandatory>
</item>";

                File.WriteAllText(_testXmlPath, testXml);

                // Auto-fill the text box with file:// URL
                TestXmlUrlTextBox.Text = new Uri(_testXmlPath).AbsoluteUri;
                TestXmlPathText.Text = $"Test XML created at: {_testXmlPath}";

                MessageBox.Show(
                    $"Test XML file created successfully!\n\nLocation: {_testXmlPath}\n\nSimulated version: {testVersion}\n\nThe URL has been auto-filled above. Click 'Test Normal Update Check' to test.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to create test XML: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Clean up test settings
            UpdateService.DebugMode = false;
            UpdateService.TestXmlUrl = null;
            Close();
        }
#else
        public UpdateTestWindow()
        {
            InitializeComponent();
            // This window is only available in Debug builds
            throw new NotSupportedException("UpdateTestWindow is only available in Debug builds.");
        }

        private void TestNormalButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotSupportedException("UpdateTestWindow is only available in Debug builds.");
        }

        private void CreateTestXmlButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotSupportedException("UpdateTestWindow is only available in Debug builds.");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
#endif
    }
}
