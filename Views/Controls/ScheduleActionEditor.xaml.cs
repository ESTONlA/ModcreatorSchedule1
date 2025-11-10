using System.Windows;
using System.Windows.Controls;
using Schedule1ModdingTool.Models;

namespace Schedule1ModdingTool.Views.Controls
{
    /// <summary>
    /// Editor control for schedule actions with dynamic fields based on action type
    /// </summary>
    public partial class ScheduleActionEditor : UserControl
    {
        public ScheduleActionEditor()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is NpcScheduleAction action)
            {
                UpdateVisibleFields(action.ActionType);
            }
        }

        private void ActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is NpcScheduleAction action)
            {
                UpdateVisibleFields(action.ActionType);
            }
        }

        private void UpdateVisibleFields(ScheduleActionType actionType)
        {
            // Hide all dynamic field panels
            WalkToFields.Visibility = Visibility.Collapsed;
            StayInBuildingFields.Visibility = Visibility.Collapsed;
            LocationDialogueFields.Visibility = Visibility.Collapsed;
            DriveToCarParkFields.Visibility = Visibility.Collapsed;
            NoExtraFields.Visibility = Visibility.Collapsed;

            // Show relevant panel based on action type
            switch (actionType)
            {
                case ScheduleActionType.WalkTo:
                    WalkToFields.Visibility = Visibility.Visible;
                    break;

                case ScheduleActionType.StayInBuilding:
                    StayInBuildingFields.Visibility = Visibility.Visible;
                    break;

                case ScheduleActionType.LocationDialogue:
                    LocationDialogueFields.Visibility = Visibility.Visible;
                    break;

                case ScheduleActionType.DriveToCarPark:
                    DriveToCarParkFields.Visibility = Visibility.Visible;
                    break;

                case ScheduleActionType.UseVendingMachine:
                case ScheduleActionType.UseATM:
                case ScheduleActionType.HandleDeal:
                case ScheduleActionType.EnsureDealSignal:
                    NoExtraFields.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
