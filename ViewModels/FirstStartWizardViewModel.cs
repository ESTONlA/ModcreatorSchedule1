using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Utils;

namespace Schedule1ModdingTool.ViewModels
{
    /// <summary>
    /// ViewModel for the First Start Wizard
    /// </summary>
    public class FirstStartWizardViewModel : ObservableObject
    {
        private int _currentStep = 1;
        private const int TotalSteps = 4;

        private string _workspacePath = "";
        private ExperienceLevel _selectedExperienceLevel = ExperienceLevel.SomeCoding;
        private string _authorName = "";
        private string _gameInstallPath = "";

        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (SetProperty(ref _currentStep, value))
                {
                    OnPropertyChanged(nameof(CanGoNext));
                    OnPropertyChanged(nameof(CanGoBack));
                    OnPropertyChanged(nameof(IsFirstStep));
                    OnPropertyChanged(nameof(IsLastStep));
                    OnPropertyChanged(nameof(StepTitle));
                    OnPropertyChanged(nameof(ProgressPercentage));
                    OnPropertyChanged(nameof(StepProgressText));
                }
            }
        }

        public int TotalStepsCount => TotalSteps;

        public double ProgressPercentage => (double)_currentStep / TotalSteps * 100;

        public string StepProgressText => $"Step {_currentStep} of {TotalSteps}";

        public bool IsFirstStep => _currentStep == 1;
        public bool IsLastStep => _currentStep == TotalSteps;

        public string StepTitle
        {
            get
            {
                return _currentStep switch
                {
                    1 => "Welcome",
                    2 => "Paths & Installation",
                    3 => "Experience Level",
                    4 => "Summary",
                    _ => "Step " + _currentStep
                };
            }
        }

        public string WorkspacePath
        {
            get => _workspacePath;
            set
            {
                if (SetProperty(ref _workspacePath, value))
                {
                    OnPropertyChanged(nameof(CanGoNext));
                }
            }
        }

        public string AuthorName
        {
            get => _authorName;
            set
            {
                if (SetProperty(ref _authorName, value))
                {
                    OnPropertyChanged(nameof(CanGoNext));
                }
            }
        }

        public string GameInstallPath
        {
            get => _gameInstallPath;
            set
            {
                if (SetProperty(ref _gameInstallPath, value))
                {
                    OnPropertyChanged(nameof(CanGoNext));
                }
            }
        }

        public ExperienceLevel SelectedExperienceLevel
        {
            get => _selectedExperienceLevel;
            set
            {
                if (SetProperty(ref _selectedExperienceLevel, value))
                {
                    OnPropertyChanged(nameof(CanGoNext));
                    OnPropertyChanged(nameof(ExperienceLevelDescription));
                }
            }
        }

        public string ExperienceLevelDescription
        {
            get
            {
                return _selectedExperienceLevel switch
                {
                    ExperienceLevel.NoCodingExperience => "No coding experience - Code view will be hidden by default",
                    ExperienceLevel.SomeCoding => "Some coding experience - Code view will be visible by default",
                    ExperienceLevel.ExperiencedCoder => "Experienced coder - Code view will be visible by default",
                    _ => ""
                };
            }
        }

        private ICommand? _nextCommand;
        private ICommand? _backCommand;
        private ICommand? _finishCommand;
        private ICommand? _browsePathCommand;
        private ICommand? _browseGamePathCommand;

        public ICommand NextCommand => _nextCommand!;
        public ICommand BackCommand => _backCommand!;
        public ICommand FinishCommand => _finishCommand!;
        public ICommand BrowsePathCommand => _browsePathCommand!;
        public ICommand BrowseGamePathCommand => _browseGamePathCommand!;

        public bool CanGoNext
        {
            get
            {
                return _currentStep switch
                {
                    1 => !string.IsNullOrWhiteSpace(_authorName), // Welcome step - requires author name
                    2 => !string.IsNullOrWhiteSpace(_workspacePath) && Directory.Exists(_workspacePath) &&
                         !string.IsNullOrWhiteSpace(_gameInstallPath) && Directory.Exists(_gameInstallPath),
                    3 => true, // Experience level always has a selection
                    4 => false, // Last step - use Finish instead
                    _ => false
                };
            }
        }

        public bool CanGoBack => _currentStep > 1;

        public FirstStartWizardViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            System.Diagnostics.Debug.WriteLine("[FirstStartWizardViewModel] Initializing commands...");
            _nextCommand = new RelayCommand(GoNext, () => CanGoNext && !IsLastStep);
            _backCommand = new RelayCommand(GoBack, () => CanGoBack);
            _finishCommand = new RelayCommand(Finish, () => IsLastStep);
            _browsePathCommand = new RelayCommand(BrowsePath);
            _browseGamePathCommand = new RelayCommand(BrowseGamePath);
            System.Diagnostics.Debug.WriteLine($"[FirstStartWizardViewModel] BrowsePathCommand created: {_browsePathCommand != null}");
            System.Diagnostics.Debug.WriteLine($"[FirstStartWizardViewModel] BrowsePathCommand.CanExecute(null): {_browsePathCommand?.CanExecute(null)}");
        }

        private void GoNext()
        {
            if (_currentStep < TotalSteps)
            {
                CurrentStep++;
            }
        }

        private void GoBack()
        {
            if (_currentStep > 1)
            {
                CurrentStep--;
            }
        }

        private void BrowsePath()
        {
            System.Diagnostics.Debug.WriteLine("[BrowsePath] Command executed");
            
            try
            {
                System.Diagnostics.Debug.WriteLine("[BrowsePath] Creating FolderBrowserDialog...");
                using var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Select default workspace folder for your mod projects",
                    ShowNewFolderButton = true
                };

                // Get the active window handle for proper parent window
                System.Diagnostics.Debug.WriteLine("[BrowsePath] Finding active window...");
                var activeWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) 
                                  ?? Application.Current.MainWindow;
                
                System.Diagnostics.Debug.WriteLine($"[BrowsePath] Active window found: {activeWindow != null}, Type: {activeWindow?.GetType().Name}");
                
                System.Windows.Forms.DialogResult result;
                if (activeWindow != null)
                {
                    System.Diagnostics.Debug.WriteLine("[BrowsePath] Using WindowInteropHelper...");
                    var helper = new WindowInteropHelper(activeWindow);
                    System.Diagnostics.Debug.WriteLine($"[BrowsePath] Window handle: {helper.Handle}");
                    result = dialog.ShowDialog(new Win32Window(helper.Handle));
                    System.Diagnostics.Debug.WriteLine($"[BrowsePath] Dialog result: {result}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[BrowsePath] No active window, showing dialog without owner...");
                    result = dialog.ShowDialog();
                    System.Diagnostics.Debug.WriteLine($"[BrowsePath] Dialog result: {result}");
                }

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    System.Diagnostics.Debug.WriteLine($"[BrowsePath] Selected path: {dialog.SelectedPath}");
                    WorkspacePath = dialog.SelectedPath;
                    System.Diagnostics.Debug.WriteLine($"[BrowsePath] WorkspacePath set to: {WorkspacePath}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[BrowsePath] Dialog cancelled or closed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BrowsePath] ERROR: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[BrowsePath] Stack trace: {ex.StackTrace}");
            }
        }

        private void BrowseGamePath()
        {
            try
            {
                using var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Select Schedule I game installation folder",
                    ShowNewFolderButton = false
                };

                var activeWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) 
                                  ?? Application.Current.MainWindow;
                
                System.Windows.Forms.DialogResult result;
                if (activeWindow != null)
                {
                    var helper = new WindowInteropHelper(activeWindow);
                    result = dialog.ShowDialog(new Win32Window(helper.Handle));
                }
                else
                {
                    result = dialog.ShowDialog();
                }

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    GameInstallPath = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BrowseGamePath] ERROR: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // Helper class to wrap IntPtr as IWin32Window
        private class Win32Window : System.Windows.Forms.IWin32Window
        {
            public IntPtr Handle { get; }

            public Win32Window(IntPtr handle)
            {
                Handle = handle;
            }
        }

        private void Finish()
        {
            // Save settings
            var settings = ModSettings.Load();
            settings.WorkspacePath = _workspacePath;
            settings.ExperienceLevel = _selectedExperienceLevel;
            settings.DefaultModAuthor = _authorName;
            settings.GameInstallPath = _gameInstallPath;
            settings.IsFirstStartComplete = true;
            settings.Save();

            WizardCompleted?.Invoke();
        }

        public event System.Action? WizardCompleted;
    }
}

