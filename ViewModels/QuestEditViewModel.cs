using System;
using System.Windows.Input;
using System.Windows;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Services;
using Schedule1ModdingTool.Utils;

namespace Schedule1ModdingTool.ViewModels
{
    public class QuestEditViewModel : ObservableObject
    {
        private readonly MainViewModel _parentViewModel;
        private QuestBlueprint _originalQuest;
        private QuestBlueprint _quest;
        private string _generatedCode;
        private string _windowTitle;

        public event EventHandler<bool> CloseRequested;

        public QuestEditViewModel(QuestBlueprint quest, MainViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
            _originalQuest = quest ?? throw new ArgumentNullException(nameof(quest));
            
            // Create a deep copy of the quest for editing (to allow cancellation)
            _quest = quest.DeepCopy();
            
            WindowTitle = $"Edit Quest: {_quest.DisplayName}";
            
            // Initialize commands
            AddObjectiveCommand = new RelayCommand(AddObjective);
            RemoveObjectiveCommand = new RelayCommand<QuestObjective>(RemoveObjective);
            RegenerateCodeCommand = new RelayCommand(RegenerateCode);
            CopyCodeCommand = new RelayCommand(CopyCode);
            ApplyChangesCommand = new RelayCommand(ApplyChanges);
            CancelCommand = new RelayCommand(Cancel);
            
            // Generate initial code
            RegenerateCode();
            
            // Listen for property changes to auto-regenerate code
            _quest.PropertyChanged += (s, e) => RegenerateCode();
        }

        public QuestBlueprint Quest
        {
            get => _quest;
            set => SetProperty(ref _quest, value);
        }

        public string GeneratedCode
        {
            get => _generatedCode;
            set => SetProperty(ref _generatedCode, value);
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        public ICommand AddObjectiveCommand { get; }
        public ICommand RemoveObjectiveCommand { get; }
        public ICommand RegenerateCodeCommand { get; }
        public ICommand CopyCodeCommand { get; }
        public ICommand ApplyChangesCommand { get; }
        public ICommand CancelCommand { get; }

        private void AddObjective()
        {
            try
            {
                _quest.AddObjective();
                RegenerateCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add objective: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveObjective(QuestObjective objective)
        {
            if (objective == null) return;

            try
            {
                _quest.RemoveObjective(objective);
                RegenerateCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove objective: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegenerateCode()
        {
            try
            {
                var codeService = new CodeGenerationService();
                GeneratedCode = codeService.GenerateQuestCode(_quest);
            }
            catch (Exception ex)
            {
                GeneratedCode = $"// Error generating code:\n// {ex.Message}";
            }
        }

        private void CopyCode()
        {
            try
            {
                if (!string.IsNullOrEmpty(GeneratedCode))
                {
                    Clipboard.SetText(GeneratedCode);
                    MessageBox.Show("Code copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy code: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyChanges()
        {
            try
            {
                // Validate the quest data
                if (string.IsNullOrWhiteSpace(_quest.ClassName))
                {
                    MessageBox.Show("Class Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_quest.QuestId))
                {
                    MessageBox.Show("Quest ID is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_quest.QuestTitle))
                {
                    MessageBox.Show("Quest Title is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Apply changes back to the original quest
                _originalQuest.CopyFrom(_quest);
                
                // Mark the project as modified
                _parentViewModel.CurrentProject.MarkAsModified();
                
                // Regenerate the main project code
                _parentViewModel.RegenerateCodeCommand.Execute(null);
                
                // Close the window with success result
                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            // Close the window without applying changes
            CloseRequested?.Invoke(this, false);
        }
    }
}