using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Schedule1ModdingTool.Models;

namespace Schedule1ModdingTool.Views.Controls
{
    /// <summary>
    /// Timeline visualization control for schedule actions
    /// </summary>
    public partial class ScheduleTimelineView : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<NpcScheduleAction> _scheduleActions = new ObservableCollection<NpcScheduleAction>();
        private NpcScheduleAction? _selectedAction;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<NpcScheduleAction>? ActionSelected;
        public event EventHandler? AddActionRequested;

        public ObservableCollection<NpcScheduleAction> ScheduleActions
        {
            get => _scheduleActions;
            set
            {
                if (_scheduleActions != null)
                {
                    _scheduleActions.CollectionChanged -= ScheduleActions_CollectionChanged;
                }
                _scheduleActions = value;
                if (_scheduleActions != null)
                {
                    _scheduleActions.CollectionChanged += ScheduleActions_CollectionChanged;
                }
                OnPropertyChanged(nameof(ScheduleActions));
                UpdateTimeline();
            }
        }

        public NpcScheduleAction? SelectedAction
        {
            get => _selectedAction;
            set
            {
                _selectedAction = value;
                OnPropertyChanged(nameof(SelectedAction));
                UpdateTimeline();
            }
        }

        public ObservableCollection<HourLabel> HourLabels { get; } = new ObservableCollection<HourLabel>();

        public ScheduleTimelineView()
        {
            InitializeComponent();
            DataContext = this;
            InitializeHourLabels();
            
            Loaded += (s, e) => UpdateTimeline();
            if (ScheduleActionsItemsControl != null)
            {
                ScheduleActionsItemsControl.ItemContainerGenerator.StatusChanged += (s, e) =>
                {
                    if (ScheduleActionsItemsControl.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        UpdateTimeline();
                    }
                };
            }
        }

        private void InitializeHourLabels()
        {
            HourLabels.Clear();
            for (int hour = 0; hour < 24; hour++)
            {
                int x = hour * 60; // 60 pixels per hour (1 pixel per minute)
                HourLabels.Add(new HourLabel
                {
                    X = x,
                    Label = $"{hour:D2}:00"
                });
            }
        }

        private void UpdateTimeline()
        {
            if (ScheduleActionsItemsControl == null || TimelineCanvas == null)
                return;

            // Force refresh of item containers
            ScheduleActionsItemsControl.UpdateLayout();

            // Sort actions by time
            var sortedActions = ScheduleActions.OrderBy(a => a.StartTime).ToList();

            foreach (var action in sortedActions)
            {
                var container = ScheduleActionsItemsControl.ItemContainerGenerator.ContainerFromItem(action) as FrameworkElement;
                if (container != null)
                {
                    // Position based on start time (minutes from midnight)
                    double x = action.StartTime; // 1 pixel per minute
                    double y = 30; // Top margin

                    Canvas.SetLeft(container, x);
                    Canvas.SetTop(container, y);

                    // Highlight selected action
                    if (action == SelectedAction)
                    {
                        container.Opacity = 1.0;
                        if (container is Border border)
                        {
                            border.BorderBrush = System.Windows.Media.Brushes.White;
                            border.BorderThickness = new Thickness(2);
                        }
                    }
                    else
                    {
                        container.Opacity = 0.8;
                        if (container is Border border)
                        {
                            border.BorderBrush = Application.Current.Resources["DarkBorderBrush"] as System.Windows.Media.Brush;
                            border.BorderThickness = new Thickness(1);
                        }
                    }

                    // Warning for time 0
                    if (action.StartTime == 0)
                    {
                        if (container is Border border)
                        {
                            border.BorderBrush = System.Windows.Media.Brushes.Orange;
                            border.BorderThickness = new Thickness(2);
                        }
                    }
                }
            }
        }

        private void ScheduleActions_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTimeline();
        }

        private void ActionBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is NpcScheduleAction action)
            {
                SelectedAction = action;
                ActionSelected?.Invoke(this, action);
            }
        }

        private void SortByTime_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleActions == null)
                return;

            var sorted = ScheduleActions.OrderBy(a => a.StartTime).ToList();
            ScheduleActions.Clear();
            foreach (var action in sorted)
            {
                ScheduleActions.Add(action);
            }
            UpdateTimeline();
        }

        private void AddAction_Click(object sender, RoutedEventArgs e)
        {
            AddActionRequested?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HourLabel
    {
        public double X { get; set; }
        public string Label { get; set; } = "";
    }
}

