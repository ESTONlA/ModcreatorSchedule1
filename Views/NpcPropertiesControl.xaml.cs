using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Schedule1ModdingTool.Data;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Utils;
using Schedule1ModdingTool.ViewModels;
using Schedule1ModdingTool.Views.Controls;

namespace Schedule1ModdingTool.Views
{
    public partial class NpcPropertiesControl : UserControl
    {
        public NpcPropertiesControl()
        {
            InitializeComponent();
        }

        private MainViewModel? ViewModel => DataContext as MainViewModel;
        private NpcBlueprint? CurrentNpc => ViewModel?.SelectedNpc;

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            // Validate mutual exclusivity on load
            ValidateCustomerDealerExclusivity();
        }

        private void ValidateCustomerDealerExclusivity()
        {
            if (CurrentNpc == null)
                return;

            // If both are checked, prefer Customer over Dealer
            if (CurrentNpc.EnableCustomer && CurrentNpc.IsDealer)
            {
                CurrentNpc.IsDealer = false;
            }
        }

        private void PickAppearanceColor_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentNpc == null || sender is not Button btn || btn.Tag is not string property)
                return;

            var appearance = CurrentNpc.Appearance;
            var currentHex = property switch
            {
                "SkinColor" => appearance.SkinColor,
                "LeftEyeLidColor" => appearance.LeftEyeLidColor,
                "RightEyeLidColor" => appearance.RightEyeLidColor,
                "EyeBallTint" => appearance.EyeBallTint,
                "HairColor" => appearance.HairColor,
                _ => "#FFFFFFFF"
            };

            var picked = PickColor(currentHex);
            if (picked == null)
                return;

            switch (property)
            {
                case "SkinColor":
                    appearance.SkinColor = picked;
                    break;
                case "LeftEyeLidColor":
                    appearance.LeftEyeLidColor = picked;
                    break;
                case "RightEyeLidColor":
                    appearance.RightEyeLidColor = picked;
                    break;
                case "EyeBallTint":
                    appearance.EyeBallTint = picked;
                    break;
                case "HairColor":
                    appearance.HairColor = picked;
                    break;
            }
        }

        private void PickLayerColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not NpcAppearanceLayer layer)
                return;

            var picked = PickColor(layer.ColorHex);
            if (picked != null)
            {
                layer.ColorHex = picked;
            }
        }

        private void AddFaceLayer_Click(object sender, RoutedEventArgs e)
        {
            var defaultPath = AppearancePresets.FaceLayers.Count > 0
                ? AppearancePresets.FaceLayers[0].Path
                : "Avatar/Layers/Face/Face_Neutral";

            CurrentNpc?.Appearance.FaceLayers.Add(new NpcAppearanceLayer
            {
                LayerPath = defaultPath,
                ColorHex = "#FFFFFFFF"
            });
        }

        private void AddBodyLayer_Click(object sender, RoutedEventArgs e)
        {
            var defaultPath = AppearancePresets.BodyLayers.Count > 0
                ? AppearancePresets.BodyLayers[0].Path
                : "Avatar/Layers/Top/T-Shirt";

            CurrentNpc?.Appearance.BodyLayers.Add(new NpcAppearanceLayer
            {
                LayerPath = defaultPath,
                ColorHex = "#FFFFFFFF"
            });
        }

        private void AddAccessoryLayer_Click(object sender, RoutedEventArgs e)
        {
            var defaultPath = AppearancePresets.AccessoryLayers.Count > 0
                ? AppearancePresets.AccessoryLayers[0].Path
                : "Avatar/Accessories/Feet/Sneakers/Sneakers";

            CurrentNpc?.Appearance.AccessoryLayers.Add(new NpcAppearanceLayer
            {
                LayerPath = defaultPath,
                ColorHex = "#FFFFFFFF"
            });
        }

        private void RemoveFaceLayer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is NpcAppearanceLayer layer)
            {
                CurrentNpc?.Appearance.FaceLayers.Remove(layer);
            }
        }

        private void RemoveBodyLayer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is NpcAppearanceLayer layer)
            {
                CurrentNpc?.Appearance.BodyLayers.Remove(layer);
            }
        }

        private void RemoveAccessoryLayer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is NpcAppearanceLayer layer)
            {
                CurrentNpc?.Appearance.AccessoryLayers.Remove(layer);
            }
        }

        private static string? PickColor(string currentHex)
        {
            var (a, r, g, b) = ColorUtils.ParseHex(currentHex);
            using var dialog = new System.Windows.Forms.ColorDialog
            {
                AllowFullOpen = true,
                FullOpen = true,
                Color = Color.FromArgb(a, r, g, b)
            };

            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return null;

            return $"#{dialog.Color.A:X2}{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
        }

        private void EnableCustomer_Checked(object sender, RoutedEventArgs e)
        {
            if (CurrentNpc != null && CurrentNpc.IsDealer)
            {
                CurrentNpc.IsDealer = false;
            }
        }

        private void EnableCustomer_Unchecked(object sender, RoutedEventArgs e)
        {
            // Allow unchecking without restrictions
        }

        private void IsDealer_Checked(object sender, RoutedEventArgs e)
        {
            if (CurrentNpc != null && CurrentNpc.EnableCustomer)
            {
                CurrentNpc.EnableCustomer = false;
            }
        }

        private void IsDealer_Unchecked(object sender, RoutedEventArgs e)
        {
            // Allow unchecking without restrictions
        }

        // Relationship Defaults Handlers
        private void AddConnection_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentNpc == null || ConnectionIdTextBox == null || string.IsNullOrWhiteSpace(ConnectionIdTextBox.Text))
                return;

            var npcId = ConnectionIdTextBox.Text.Trim();
            
            // Validate the NPC ID format
            if (!ValidationHelpers.IsValidNpcId(npcId))
            {
                // Auto-correct if possible
                var corrected = ValidationHelpers.NormalizeNpcId(npcId);
                if (ValidationHelpers.IsValidNpcId(corrected))
                {
                    npcId = corrected;
                    ConnectionIdTextBox.Text = corrected;
                }
                else
                {
                    AppUtils.ShowWarning($"Invalid NPC ID format: {npcId}\n\nNPC IDs must be lowercase with underscores (e.g., 'bobby_cooley')");
                    return;
                }
            }

            if (!CurrentNpc.RelationshipDefaults.Connections.Contains(npcId))
            {
                CurrentNpc.RelationshipDefaults.Connections.Add(npcId);
                ConnectionIdTextBox.Text = string.Empty;
            }
        }

        private void RemoveConnection_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentNpc == null)
                return;

            var listBox = FindName("ConnectionIdTextBox") as System.Windows.Controls.ListBox;
            if (listBox?.SelectedItem is string selectedConnection)
            {
                CurrentNpc.RelationshipDefaults.Connections.Remove(selectedConnection);
            }
        }

        // Schedule Action Handlers
        private void AddScheduleAction_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentNpc == null)
                return;

            var newAction = new NpcScheduleAction
            {
                ActionType = ScheduleActionType.WalkTo,
                StartTime = 10 // Use time 10 (0:10 AM) instead of 0 to avoid sort comparison issues
            };

            CurrentNpc.ScheduleActions.Add(newAction);
            
            // Sort actions by time after adding
            var sorted = CurrentNpc.ScheduleActions.OrderBy(a => a.StartTime).ToList();
            CurrentNpc.ScheduleActions.Clear();
            foreach (var action in sorted)
            {
                CurrentNpc.ScheduleActions.Add(action);
            }
            
            // Update ViewModel's SelectedScheduleAction to trigger binding
            // The newAction reference is preserved since we're re-adding the same objects
            if (ViewModel != null)
            {
                ViewModel.SelectedScheduleAction = newAction;
            }
            
            // Warn if time is 0
            if (newAction.StartTime == 0)
            {
                AppUtils.ShowWarning("Warning: Scheduling actions at time 0 (midnight) can cause sort comparison issues.\n\nConsider using time 1 or later (e.g., 10 minutes = 0:10 AM).");
            }
        }

        private void RemoveScheduleAction_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentNpc == null || ScheduleActionsListBox.SelectedItem == null)
                return;

            if (ScheduleActionsListBox.SelectedItem is NpcScheduleAction action)
            {
                CurrentNpc.ScheduleActions.Remove(action);
            }
        }

        // Customer Settings Handlers
        private void AddPreferredProperty_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentNpc == null || PreferredPropertyComboBox.SelectedItem == null)
                return;

            if (PreferredPropertyComboBox.SelectedItem is ComboBoxItem item)
            {
                var property = item.Content.ToString();
                if (property != null && !CurrentNpc.CustomerDefaults.PreferredProperties.Contains(property))
                {
                    CurrentNpc.CustomerDefaults.PreferredProperties.Add(property);
                }
            }
        }

        private void RemovePreferredProperty_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentNpc == null)
                return;

            // Find the ListBox by walking the visual tree or by name
            var listBox = FindListBoxInVisualTree("PreferredProperties");
            if (listBox?.SelectedItem is string selectedProperty)
            {
                CurrentNpc.CustomerDefaults.PreferredProperties.Remove(selectedProperty);
            }
        }

        // Inventory Defaults Handlers
        private void AddStartupItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentNpc == null || string.IsNullOrWhiteSpace(StartupItemTextBox.Text))
                return;

            var itemId = StartupItemTextBox.Text.Trim();
            if (!CurrentNpc.InventoryDefaults.StartupItems.Contains(itemId))
            {
                CurrentNpc.InventoryDefaults.StartupItems.Add(itemId);
                StartupItemTextBox.Clear();
            }
        }

        private void RemoveStartupItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentNpc == null)
                return;

            var listBox = FindListBoxInVisualTree("StartupItems");
            if (listBox?.SelectedItem is string selectedItem)
            {
                CurrentNpc.InventoryDefaults.StartupItems.Remove(selectedItem);
            }
        }

        // Helper method to find ListBox in visual tree
        private System.Windows.Controls.ListBox? FindListBoxInVisualTree(string partialName)
        {
            // This is a simplified approach - in a real app you might want to use VisualTreeHelper
            // For now, we'll rely on the ListBox selection being available through data context
            return null; // Placeholder - WPF binding will handle selection automatically
        }
    }
}
