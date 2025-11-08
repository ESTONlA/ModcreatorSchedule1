using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Schedule1ModdingTool.Data;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Utils;
using Schedule1ModdingTool.ViewModels;

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
    }
}
