using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Services;
using Schedule1ModdingTool.Utils;
using Schedule1ModdingTool.Views;

namespace Schedule1ModdingTool.ViewModels
{
    /// <summary>
    /// Main view model for the application
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        private QuestProject _currentProject;
        private QuestBlueprint? _selectedQuest;
        private string _generatedCode = "";
        private bool _isCodeVisible = true;

        public QuestProject CurrentProject
        {
            get => _currentProject;
            set => SetProperty(ref _currentProject, value);
        }

        public QuestBlueprint? SelectedQuest
        {
            get => _selectedQuest;
            set
            {
                SetProperty(ref _selectedQuest, value);
                if (value != null)
                {
                    RegenerateCode();
                }
            }
        }

        public string GeneratedCode
        {
            get => _generatedCode;
            set => SetProperty(ref _generatedCode, value);
        }

        public bool IsCodeVisible
        {
            get => _isCodeVisible;
            set => SetProperty(ref _isCodeVisible, value);
        }

        public ObservableCollection<BlueprintTemplate> AvailableBlueprints { get; } = new ObservableCollection<BlueprintTemplate>();

        // Commands
        public ICommand NewProjectCommand { get; private set; }
        public ICommand OpenProjectCommand { get; private set; }
        public ICommand SaveProjectCommand { get; private set; }
        public ICommand SaveProjectAsCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand AddQuestCommand { get; private set; }
        public ICommand RemoveQuestCommand { get; private set; }
        public ICommand EditQuestCommand { get; private set; }
        public ICommand DuplicateQuestCommand { get; private set; }
        public ICommand RegenerateCodeCommand { get; private set; }
        public ICommand CompileCommand { get; private set; }
        public ICommand ToggleCodeViewCommand { get; private set; }

        private readonly CodeGenerationService _codeGenService;
        private readonly ProjectService _projectService;

        public MainViewModel()
        {
            _currentProject = new QuestProject();
            _codeGenService = new CodeGenerationService();
            _projectService = new ProjectService();

            InitializeCommands();
            InitializeBlueprints();
        }

        private void InitializeCommands()
        {
            NewProjectCommand = new RelayCommand(NewProject);
            OpenProjectCommand = new RelayCommand(OpenProject);
            SaveProjectCommand = new RelayCommand(SaveProject, () => CurrentProject.IsModified);
            SaveProjectAsCommand = new RelayCommand(SaveProjectAs);
            ExitCommand = new RelayCommand(Exit);
            AddQuestCommand = new RelayCommand<BlueprintTemplate>(AddQuest);
            RemoveQuestCommand = new RelayCommand(RemoveQuest, () => SelectedQuest != null);
            EditQuestCommand = new RelayCommand(EditQuest, () => SelectedQuest != null);
            DuplicateQuestCommand = new RelayCommand(DuplicateQuest, () => SelectedQuest != null);
            RegenerateCodeCommand = new RelayCommand(RegenerateCode, () => SelectedQuest != null);
            CompileCommand = new RelayCommand(Compile, () => SelectedQuest != null);
            ToggleCodeViewCommand = new RelayCommand(() => IsCodeVisible = !IsCodeVisible);
        }

        private void InitializeBlueprints()
        {
            AvailableBlueprints.Add(new BlueprintTemplate("Quest Blueprint", QuestBlueprintType.Standard, "📝"));
            AvailableBlueprints.Add(new BlueprintTemplate("Advanced Quest Blueprint", QuestBlueprintType.Advanced, "⚡"));
        }

        private void NewProject()
        {
            if (ConfirmUnsavedChanges())
            {
                CurrentProject = new QuestProject();
                SelectedQuest = null;
                GeneratedCode = "";
            }
        }

        private void OpenProject()
        {
            if (!ConfirmUnsavedChanges()) return;

            var project = _projectService.OpenProject();
            if (project != null)
            {
                CurrentProject = project;
                SelectedQuest = CurrentProject.Quests.FirstOrDefault();
            }
        }

        private void SaveProject()
        {
            _projectService.SaveProject(CurrentProject);
        }

        private void SaveProjectAs()
        {
            _projectService.SaveProjectAs(CurrentProject);
        }

        private void Exit()
        {
            if (ConfirmUnsavedChanges())
            {
                Application.Current.Shutdown();
            }
        }

        private void AddQuest(BlueprintTemplate? template)
        {
            if (template == null) return;

            var quest = new QuestBlueprint(template.Type)
            {
                ClassName = $"Quest{CurrentProject.Quests.Count + 1}Mod",
                QuestId = $"Quest{CurrentProject.Quests.Count + 1}",
                QuestTitle = $"New Quest {CurrentProject.Quests.Count + 1}",
                QuestDescription = "A new quest for Schedule 1",
                BlueprintType = template.Type,
                // MelonLoader defaults
                ModName = $"My Quest Mod {CurrentProject.Quests.Count + 1}",
                ModVersion = "1.0.0",
                ModAuthor = "YourName",
                GameDeveloper = "TVGS",
                GameName = "Schedule I",
                Namespace = "MyProject"
            };

            CurrentProject.AddQuest(quest);
            SelectedQuest = quest;
        }

        private void RemoveQuest()
        {
            if (SelectedQuest == null) return;

            var result = MessageBox.Show($"Are you sure you want to remove '{SelectedQuest.DisplayName}'?", 
                "Remove Quest", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                CurrentProject.RemoveQuest(SelectedQuest);
                SelectedQuest = CurrentProject.Quests.FirstOrDefault();
            }
        }

        private void EditQuest()
        {
            if (SelectedQuest == null) return;

            try
            {
                // Open the quest edit window
                var editWindow = new QuestEditWindow(SelectedQuest, this)
                {
                    Owner = Application.Current.MainWindow
                };

                // Show as dialog and handle the result
                var result = editWindow.ShowDialog();
                
                if (result == true)
                {
                    // Changes were applied - refresh the code view
                    RegenerateCode();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open quest editor: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DuplicateQuest()
        {
            if (SelectedQuest == null) return;

            try
            {
                // Create a deep copy of the selected quest
                var duplicatedQuest = SelectedQuest.DeepCopy();
                
                // Modify the name to indicate it's a copy
                duplicatedQuest.ClassName = $"{duplicatedQuest.ClassName}_Copy";
                duplicatedQuest.QuestTitle = $"{duplicatedQuest.QuestTitle} (Copy)";
                duplicatedQuest.QuestId = $"{duplicatedQuest.QuestId}_copy";

                // Add to project and select
                CurrentProject.AddQuest(duplicatedQuest);
                SelectedQuest = duplicatedQuest;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to duplicate quest: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegenerateCode()
        {
            if (SelectedQuest == null) return;
            GeneratedCode = _codeGenService.GenerateQuestCode(SelectedQuest);
        }

        private void Compile()
        {
            if (SelectedQuest == null) return;
            
            try
            {
                var success = _codeGenService.CompileToDll(SelectedQuest, GeneratedCode);
                if (success)
                {
                    var outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Schedule1_Mods");
                    MessageBox.Show($"Successfully generated MelonLoader mod source code!\n\nFiles saved to:\n{outputDir}\n\n- {SelectedQuest.ClassName}.cs (source code)\n- COMPILATION_INSTRUCTIONS.txt (how to compile)\n\nSee instructions file for compilation steps.", 
                        "Mod Generated Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Compilation failed: {ex.Message}", "Compilation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ConfirmUnsavedChanges()
        {
            if (!CurrentProject.IsModified) return true;

            var result = MessageBox.Show(
                "You have unsaved changes. Do you want to save them before continuing?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveProject();
                    return true;
                case MessageBoxResult.No:
                    return true;
                case MessageBoxResult.Cancel:
                default:
                    return false;
            }
        }
    }

    public class BlueprintTemplate
    {
        public string Name { get; }
        public QuestBlueprintType Type { get; }
        public string Icon { get; }

        public BlueprintTemplate(string name, QuestBlueprintType type, string icon)
        {
            Name = name;
            Type = type;
            Icon = icon;
        }
    }
}