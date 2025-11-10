using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Services;
using Schedule1ModdingTool.ViewModels;
using ComboBox = System.Windows.Controls.ComboBox;

namespace Schedule1ModdingTool.Views
{
    /// <summary>
    /// Interaction logic for PropertiesControl.xaml
    /// </summary>
    public partial class PropertiesControl : UserControl
    {
        private ObservableCollection<TriggerMetadata> _availableTriggers;
        private ObservableCollection<NpcInfo> _availableNpcs;

        public PropertiesControl()
        {
            InitializeComponent();
            Loaded += PropertiesControl_Loaded;
            DataContextChanged += PropertiesControl_DataContextChanged;
        }

        private void PropertiesControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTriggerData();
        }

        private void PropertiesControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is MainViewModel vm && vm.SelectedQuest != null)
            {
                SyncTriggerMetadata(vm.SelectedQuest);
            }
        }

        private void InitializeTriggerData()
        {
            // Load available triggers from TriggerRegistryService
            var triggers = TriggerRegistryService.GetAvailableTriggers();
            _availableTriggers = new ObservableCollection<TriggerMetadata>(triggers);

            // Build NPC list from project NPCs and base game NPCs
            var npcList = new List<NpcInfo>();

            if (DataContext is MainViewModel vm)
            {
                // Add project NPCs
                if (vm.CurrentProject?.Npcs != null)
                {
                    foreach (var npc in vm.CurrentProject.Npcs)
                    {
                        npcList.Add(new NpcInfo
                        {
                            Id = npc.NpcId,
                            DisplayName = npc.DisplayName,
                            IsModNpc = true
                        });
                    }
                }

                // Add base game NPCs
                AddBaseGameNpcs(npcList);

                _availableNpcs = new ObservableCollection<NpcInfo>(npcList.OrderBy(n => n.DisplayName));

                // Sync triggers for current quest
                if (vm.SelectedQuest != null)
                {
                    SyncTriggerMetadata(vm.SelectedQuest);
                }
            }
        }

        private void SyncTriggerMetadata(QuestBlueprint quest)
        {
            if (_availableTriggers == null) return;

            foreach (var trigger in quest.QuestTriggers)
            {
                if (!string.IsNullOrWhiteSpace(trigger.TargetAction))
                {
                    // Match by both TargetAction AND TriggerType to preserve the saved trigger type
                    var metadata = _availableTriggers.FirstOrDefault(t => 
                        t.TargetAction == trigger.TargetAction && 
                        t.TriggerType == trigger.TriggerType);
                    
                    // If no exact match, try to find by TargetAction only but preserve TriggerType
                    if (metadata == null)
                    {
                        metadata = _availableTriggers.FirstOrDefault(t => t.TargetAction == trigger.TargetAction);
                        // Only set if found and TriggerType matches (to avoid overwriting)
                        if (metadata != null && metadata.TriggerType == trigger.TriggerType)
                        {
                            trigger.SelectedTriggerMetadata = metadata;
                        }
                    }
                    else
                    {
                        trigger.SelectedTriggerMetadata = metadata;
                    }
                }
            }

            foreach (var trigger in quest.QuestFinishTriggers)
            {
                if (!string.IsNullOrWhiteSpace(trigger.TargetAction))
                {
                    // Match by both TargetAction AND TriggerType to preserve the saved trigger type
                    var metadata = _availableTriggers.FirstOrDefault(t => 
                        t.TargetAction == trigger.TargetAction && 
                        t.TriggerType == trigger.TriggerType);
                    
                    // If no exact match, try to find by TargetAction only but preserve TriggerType
                    if (metadata == null)
                    {
                        metadata = _availableTriggers.FirstOrDefault(t => t.TargetAction == trigger.TargetAction);
                        // Only set if found and TriggerType matches (to avoid overwriting)
                        if (metadata != null && metadata.TriggerType == trigger.TriggerType)
                        {
                            trigger.SelectedTriggerMetadata = metadata;
                        }
                    }
                    else
                    {
                        trigger.SelectedTriggerMetadata = metadata;
                    }
                }
            }
        }

        private static string ConvertDisplayNameToGameId(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return "";

            // Split by space and convert to lowercase
            var parts = displayName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0)
                return "";
            
            if (parts.Length == 1)
            {
                // Single name like "Ming" -> "ming"
                return parts[0].ToLowerInvariant();
            }
            
            // Multiple parts: join with underscore and lowercase
            // e.g., "Kyle Cooley" -> "kyle_cooley", "Officer Bailey" -> "officer_bailey"
            return string.Join("_", parts.Select(p => p.ToLowerInvariant()));
        }

        private void AddBaseGameNpcs(List<NpcInfo> npcList)
        {
            // Same list as in QuestEditViewModel - all base game NPCs
            // NPC IDs are in game format: firstname_lastname (lowercase with underscore)
            var baseNpcs = new[]
            {
                new NpcInfo { Id = ConvertDisplayNameToGameId("Anna Chesterfield"), DisplayName = "Anna Chesterfield", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Billy Kramer"), DisplayName = "Billy Kramer", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Cranky Frank"), DisplayName = "Cranky Frank", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Genghis Barn"), DisplayName = "Genghis Barn", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Jane Lucero"), DisplayName = "Jane Lucero", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Javier Perez"), DisplayName = "Javier Perez", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Lisa Gardener"), DisplayName = "Lisa Gardener", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Mac Cooper"), DisplayName = "Mac Cooper", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Marco Baron"), DisplayName = "Marco Baron", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Melissa Wood"), DisplayName = "Melissa Wood", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Salvador Moreno"), DisplayName = "Salvador Moreno", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Brad Crosby"), DisplayName = "Brad Crosby", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Elizabeth Homley"), DisplayName = "Elizabeth Homley", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Eugene Buckley"), DisplayName = "Eugene Buckley", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Greg Fliggle"), DisplayName = "Greg Fliggle", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Jeff Gilmore"), DisplayName = "Jeff Gilmore", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Jennifer Rivera"), DisplayName = "Jennifer Rivera", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Kevin Oakley"), DisplayName = "Kevin Oakley", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Louis Fourier"), DisplayName = "Louis Fourier", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Lucy Pennington"), DisplayName = "Lucy Pennington", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Philip Wentworth"), DisplayName = "Philip Wentworth", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Randy Caulfield"), DisplayName = "Randy Caulfield", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Albert Hoover"), DisplayName = "Albert Hoover", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Austin Steiner"), DisplayName = "Austin Steiner", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Benji Coleman"), DisplayName = "Benji Coleman", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Beth Penn"), DisplayName = "Beth Penn", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Chloe Bowers"), DisplayName = "Chloe Bowers", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Donna Martin"), DisplayName = "Donna Martin", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Geraldine Poon"), DisplayName = "Geraldine Poon", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Jessi Waters"), DisplayName = "Jessi Waters", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Kathy Henderson"), DisplayName = "Kathy Henderson", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Kyle Cooley"), DisplayName = "Kyle Cooley", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Ludwig Meyer"), DisplayName = "Ludwig Meyer", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Mick Lubbin"), DisplayName = "Mick Lubbin", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Ming"), DisplayName = "Ming", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Peggy Myers"), DisplayName = "Peggy Myers", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Peter File"), DisplayName = "Peter File", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Sam Thompson"), DisplayName = "Sam Thompson", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Alison Knight"), DisplayName = "Alison Knight", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Carl Bundy"), DisplayName = "Carl Bundy", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Chris Sullivan"), DisplayName = "Chris Sullivan", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Dennis Kennedy"), DisplayName = "Dennis Kennedy", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Hank Stevenson"), DisplayName = "Hank Stevenson", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Harold Colt"), DisplayName = "Harold Colt", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Jackie Stevenson"), DisplayName = "Jackie Stevenson", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Jack Knight"), DisplayName = "Jack Knight", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Jeremy Wilkinson"), DisplayName = "Jeremy Wilkinson", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Karen Kennedy"), DisplayName = "Karen Kennedy", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Wei Long"), DisplayName = "Wei Long", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Fiona Hancock"), DisplayName = "Fiona Hancock", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Herbert Bleuball"), DisplayName = "Herbert Bleuball", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Jen Heard"), DisplayName = "Jen Heard", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Leo Rivers"), DisplayName = "Leo Rivers", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Lily Turner"), DisplayName = "Lily Turner", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Michael Boog"), DisplayName = "Michael Boog", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Pearl Moore"), DisplayName = "Pearl Moore", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Ray Hoffman"), DisplayName = "Ray Hoffman", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Tobias Wentworth"), DisplayName = "Tobias Wentworth", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Walter Cussler"), DisplayName = "Walter Cussler", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Charles Rowland"), DisplayName = "Charles Rowland", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Dean Webster"), DisplayName = "Dean Webster", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Doris Lubbin"), DisplayName = "Doris Lubbin", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("George Greene"), DisplayName = "George Greene", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Jerry Montero"), DisplayName = "Jerry Montero", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Joyce Ball"), DisplayName = "Joyce Ball", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Keith Wagner"), DisplayName = "Keith Wagner", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Kim Delaney"), DisplayName = "Kim Delaney", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Meg Cooley"), DisplayName = "Meg Cooley", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Molly Presley"), DisplayName = "Molly Presley", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Shirley Watts"), DisplayName = "Shirley Watts", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Trent Sherman"), DisplayName = "Trent Sherman", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Officer Bailey"), DisplayName = "Officer Bailey", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Officer Cooper"), DisplayName = "Officer Cooper", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Officer Green"), DisplayName = "Officer Green", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Officer Howard"), DisplayName = "Officer Howard", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Officer Jackson"), DisplayName = "Officer Jackson", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Officer Lee"), DisplayName = "Officer Lee", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Officer Lopez"), DisplayName = "Officer Lopez", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Officer Murphy"), DisplayName = "Officer Murphy", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Officer Oakley"), DisplayName = "Officer Oakley", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Dan Samwell"), DisplayName = "Dan Samwell", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Igor Romanovich"), DisplayName = "Igor Romanovich", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Manny Oakfield"), DisplayName = "Manny Oakfield", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Oscar Holland"), DisplayName = "Oscar Holland", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Stan Carney"), DisplayName = "Stan Carney", IsModNpc = false },
                new NpcInfo { Id = ConvertDisplayNameToGameId("Uncle Nelson"), DisplayName = "Uncle Nelson", IsModNpc = false }
            };
            npcList.AddRange(baseNpcs);
        }

        private void AddObjective_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SelectedQuest != null)
            {
                vm.SelectedQuest.AddObjective();
                vm.CurrentProject.MarkAsModified();
            }
        }

        private void RemoveObjective_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is QuestObjective objective)
            {
                if (DataContext is MainViewModel vm && vm.SelectedQuest != null)
                {
                    vm.SelectedQuest.RemoveObjective(objective);
                    vm.CurrentProject.MarkAsModified();
                }
            }
        }

        private void AddQuestStartTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SelectedQuest != null)
            {
                var defaultTrigger = _availableTriggers?.FirstOrDefault(t => t.TargetAction == "TimeManager.OnDayPass");
                var trigger = new QuestTrigger
                {
                    TriggerType = QuestTriggerType.ActionTrigger,
                    TriggerTarget = QuestTriggerTarget.QuestStart,
                    TargetAction = defaultTrigger?.TargetAction ?? "TimeManager.OnDayPass",
                    SelectedTriggerMetadata = defaultTrigger
                };
                vm.SelectedQuest.QuestTriggers.Add(trigger);
                vm.CurrentProject.MarkAsModified();
            }
        }

        private void RemoveQuestStartTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is QuestTrigger trigger)
            {
                if (DataContext is MainViewModel vm && vm.SelectedQuest != null)
                {
                    vm.SelectedQuest.QuestTriggers.Remove(trigger);
                    vm.CurrentProject.MarkAsModified();
                }
            }
        }

        private void AddQuestFinishTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SelectedQuest != null)
            {
                var defaultTrigger = _availableTriggers?.FirstOrDefault(t => t.TargetAction == "TimeManager.OnDayPass");
                var trigger = new QuestFinishTrigger
                {
                    TriggerType = QuestTriggerType.ActionTrigger,
                    TriggerTarget = QuestTriggerTarget.QuestFinish,
                    TargetAction = defaultTrigger?.TargetAction ?? "TimeManager.OnDayPass",
                    FinishType = QuestFinishType.Complete,
                    SelectedTriggerMetadata = defaultTrigger
                };
                vm.SelectedQuest.QuestFinishTriggers.Add(trigger);
                vm.CurrentProject.MarkAsModified();
            }
        }

        private void RemoveQuestFinishTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is QuestFinishTrigger trigger)
            {
                if (DataContext is MainViewModel vm && vm.SelectedQuest != null)
                {
                    vm.SelectedQuest.QuestFinishTriggers.Remove(trigger);
                    vm.CurrentProject.MarkAsModified();
                }
            }
        }

        private void TriggerComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is QuestTrigger trigger)
            {
                UpdateTriggerComboBoxItemsSource(comboBox, trigger);
                
                // Also listen for TriggerType changes to update the dropdown
                trigger.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(QuestTrigger.TriggerType))
                    {
                        UpdateTriggerComboBoxItemsSource(comboBox, trigger);
                    }
                };
            }
        }

        private void TriggerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is QuestTrigger trigger)
            {
                if (comboBox.SelectedItem is TriggerMetadata metadata)
                {
                    trigger.SelectedTriggerMetadata = metadata;
                }
            }
        }

        private void UpdateTriggerComboBoxItemsSource(ComboBox comboBox, QuestTrigger trigger)
        {
            if (_availableTriggers == null) return;

            List<TriggerMetadata> filteredTriggers;
            
            if (trigger.TriggerType == QuestTriggerType.NPCEventTrigger)
            {
                // For NPCEventTrigger, show only NPC-related actions (NPC.*, NPCCustomer.*, NPCDealer.*, etc.)
                filteredTriggers = _availableTriggers
                    .Where(t => t.TriggerType == QuestTriggerType.NPCEventTrigger ||
                               t.TargetAction.StartsWith("NPC.", StringComparison.OrdinalIgnoreCase) ||
                               t.TargetAction.StartsWith("NPCCustomer.", StringComparison.OrdinalIgnoreCase) ||
                               t.TargetAction.StartsWith("NPCDealer.", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else
            {
                // For ActionTrigger, show only non-NPC triggers
                filteredTriggers = _availableTriggers
                    .Where(t => t.TriggerType == QuestTriggerType.ActionTrigger &&
                               !t.TargetAction.StartsWith("NPC.", StringComparison.OrdinalIgnoreCase) &&
                               !t.TargetAction.StartsWith("NPCCustomer.", StringComparison.OrdinalIgnoreCase) &&
                               !t.TargetAction.StartsWith("NPCDealer.", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            comboBox.ItemsSource = filteredTriggers;
        }

        private void NpcComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                // Ensure NPCs are initialized
                if (_availableNpcs == null)
                {
                    InitializeTriggerData();
                }
                
                if (_availableNpcs != null)
                {
                    comboBox.ItemsSource = _availableNpcs;
                    
                    // Migrate existing NPC ID if needed
                    MigrateNpcIdInTrigger(comboBox);
                }
            }
        }

        private void NpcComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                // When user finishes editing, migrate the NPC ID to correct format
                MigrateNpcIdInTrigger(comboBox);
            }
        }

        private void MigrateNpcIdInTrigger(ComboBox comboBox)
        {
            if (_availableNpcs == null || comboBox.DataContext is not QuestTrigger trigger || string.IsNullOrWhiteSpace(trigger.TargetNpcId))
                return;

            // Check if the ID needs migration (PascalCase format or wrong format)
            if (!trigger.TargetNpcId.Contains("_") || trigger.TargetNpcId != trigger.TargetNpcId.ToLowerInvariant())
            {
                // Try to find matching NPC by exact ID match (case-insensitive)
                var match = _availableNpcs.FirstOrDefault(n => 
                    n.Id.Equals(trigger.TargetNpcId, StringComparison.OrdinalIgnoreCase));
                
                if (match != null)
                {
                    trigger.TargetNpcId = match.Id;
                    return;
                }

                // Try to find by display name without spaces (e.g., "KyleCooley" matches "Kyle Cooley")
                match = _availableNpcs.FirstOrDefault(n => 
                    n.DisplayName.Replace(" ", "").Equals(trigger.TargetNpcId, StringComparison.OrdinalIgnoreCase));
                
                if (match != null)
                {
                    trigger.TargetNpcId = match.Id;
                    return;
                }

                // Convert PascalCase to game format
                // "KyleCooley" -> "Kyle Cooley" -> "kyle_cooley"
                var words = new System.Text.StringBuilder();
                for (int i = 0; i < trigger.TargetNpcId.Length; i++)
                {
                    if (i > 0 && char.IsUpper(trigger.TargetNpcId[i]))
                    {
                        words.Append(' ');
                    }
                    words.Append(trigger.TargetNpcId[i]);
                }
                var displayName = words.ToString().Trim();
                var convertedId = string.Join("_", displayName.Split(' ').Select(p => p.ToLowerInvariant()));
                
                // Try to find by converted ID
                var convertedMatch = _availableNpcs.FirstOrDefault(n => 
                    n.Id.Equals(convertedId, StringComparison.OrdinalIgnoreCase) ||
                    n.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
                
                if (convertedMatch != null)
                {
                    trigger.TargetNpcId = convertedMatch.Id;
                }
                else
                {
                    // No match found, but convert to game format anyway
                    trigger.TargetNpcId = convertedId;
                }
            }
        }

        private void TriggerTypeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                // Filter out CustomTrigger - only show ActionTrigger and NPCEventTrigger
                var triggerTypes = Enum.GetValues(typeof(QuestTriggerType))
                    .Cast<QuestTriggerType>()
                    .Where(t => t != QuestTriggerType.CustomTrigger)
                    .ToList();
                comboBox.ItemsSource = triggerTypes;
            }
        }

        private void TriggerTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is QuestTrigger trigger)
            {
                // When trigger type changes, update the available actions dropdown
                // Find the Target Action ComboBox in the same StackPanel
                var parent = VisualTreeHelper.GetParent(comboBox);
                while (parent != null)
                {
                    if (parent is StackPanel stackPanel)
                    {
                        // Find the ComboBox that has Loaded="TriggerComboBox_Loaded" (the Target Action ComboBox)
                        // It should be the next ComboBox after the TriggerType ComboBox
                        bool foundTriggerType = false;
                        foreach (var child in stackPanel.Children)
                        {
                            if (child == comboBox)
                            {
                                foundTriggerType = true;
                                continue;
                            }
                            
                            if (foundTriggerType && child is ComboBox actionComboBox)
                            {
                                // This should be the Target Action ComboBox
                                UpdateTriggerComboBoxItemsSource(actionComboBox, trigger);
                                // Clear selection if current selection is no longer valid
                                if (actionComboBox.SelectedItem is TriggerMetadata selected)
                                {
                                    bool isValid = false;
                                    if (trigger.TriggerType == QuestTriggerType.NPCEventTrigger)
                                    {
                                        isValid = selected.TriggerType == QuestTriggerType.NPCEventTrigger ||
                                                 selected.TargetAction.StartsWith("NPC.", StringComparison.OrdinalIgnoreCase) ||
                                                 selected.TargetAction.StartsWith("NPCCustomer.", StringComparison.OrdinalIgnoreCase) ||
                                                 selected.TargetAction.StartsWith("NPCDealer.", StringComparison.OrdinalIgnoreCase);
                                    }
                                    else
                                    {
                                        isValid = selected.TriggerType == QuestTriggerType.ActionTrigger &&
                                                 !selected.TargetAction.StartsWith("NPC.", StringComparison.OrdinalIgnoreCase) &&
                                                 !selected.TargetAction.StartsWith("NPCCustomer.", StringComparison.OrdinalIgnoreCase) &&
                                                 !selected.TargetAction.StartsWith("NPCDealer.", StringComparison.OrdinalIgnoreCase);
                                    }
                                    
                                    if (!isValid)
                                    {
                                        actionComboBox.SelectedItem = null;
                                        trigger.SelectedTriggerMetadata = null;
                                        trigger.TargetAction = "";
                                    }
                                }
                                return;
                            }
                        }
                    }
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }
        }

        private void FinishTypeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.ItemsSource = Enum.GetValues(typeof(QuestFinishType));
            }
        }

        private void AddObjectiveStartTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is QuestObjective objective)
            {
                var defaultTrigger = _availableTriggers?.FirstOrDefault(t => t.TargetAction == "TimeManager.OnDayPass");
                var trigger = new QuestObjectiveTrigger
                {
                    TriggerType = QuestTriggerType.ActionTrigger,
                    TriggerTarget = QuestTriggerTarget.ObjectiveStart,
                    TargetAction = defaultTrigger?.TargetAction ?? "TimeManager.OnDayPass",
                    ObjectiveName = objective.Name,
                    SelectedTriggerMetadata = defaultTrigger
                };
                objective.StartTriggers.Add(trigger);
                if (DataContext is MainViewModel vm)
                {
                    vm.CurrentProject.MarkAsModified();
                }
            }
        }

        private void RemoveObjectiveStartTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is QuestObjectiveTrigger trigger)
            {
                if (DataContext is MainViewModel vm && vm.SelectedQuest != null)
                {
                    var objective = vm.SelectedQuest.Objectives.FirstOrDefault(obj => obj.StartTriggers.Contains(trigger));
                    if (objective != null)
                    {
                        objective.StartTriggers.Remove(trigger);
                        vm.CurrentProject.MarkAsModified();
                    }
                }
            }
        }

        private void AddObjectiveFinishTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is QuestObjective objective)
            {
                var defaultTrigger = _availableTriggers?.FirstOrDefault(t => t.TargetAction == "TimeManager.OnDayPass");
                var trigger = new QuestObjectiveTrigger
                {
                    TriggerType = QuestTriggerType.ActionTrigger,
                    TriggerTarget = QuestTriggerTarget.ObjectiveFinish,
                    TargetAction = defaultTrigger?.TargetAction ?? "TimeManager.OnDayPass",
                    ObjectiveName = objective.Name,
                    SelectedTriggerMetadata = defaultTrigger
                };
                objective.FinishTriggers.Add(trigger);
                if (DataContext is MainViewModel vm)
                {
                    vm.CurrentProject.MarkAsModified();
                }
            }
        }

        private void RemoveObjectiveFinishTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is QuestObjectiveTrigger trigger)
            {
                if (DataContext is MainViewModel vm && vm.SelectedQuest != null)
                {
                    var objective = vm.SelectedQuest.Objectives.FirstOrDefault(obj => obj.FinishTriggers.Contains(trigger));
                    if (objective != null)
                    {
                        objective.FinishTriggers.Remove(trigger);
                        vm.CurrentProject.MarkAsModified();
                    }
                }
            }
        }
    }
}