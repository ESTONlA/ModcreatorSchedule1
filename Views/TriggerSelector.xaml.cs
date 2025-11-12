using System.Windows;
using System.Windows.Controls;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Services;

namespace Schedule1ModdingTool.Views
{
    /// <summary>
    /// Interaction logic for TriggerSelector.xaml
    /// </summary>
    public partial class TriggerSelector : UserControl
    {
        public static readonly DependencyProperty SelectedTriggerMetadataProperty =
            DependencyProperty.Register(nameof(SelectedTriggerMetadata), typeof(TriggerMetadata), typeof(TriggerSelector),
                new PropertyMetadata((TriggerMetadata?)null, OnSelectedTriggerMetadataChanged));

        public static readonly DependencyProperty TriggerTypeFilterProperty =
            DependencyProperty.Register(nameof(TriggerTypeFilter), typeof(QuestTriggerType?), typeof(TriggerSelector),
                new PropertyMetadata(null, OnTriggerTypeFilterChanged));

        public static readonly DependencyProperty AvailableTriggersProperty =
            DependencyProperty.Register(nameof(AvailableTriggers), typeof(System.Collections.ObjectModel.ObservableCollection<TriggerMetadata>), typeof(TriggerSelector),
                new PropertyMetadata(null, OnAvailableTriggersChanged));

        public TriggerMetadata? SelectedTriggerMetadata
        {
            get => (TriggerMetadata?)GetValue(SelectedTriggerMetadataProperty);
            set => SetValue(SelectedTriggerMetadataProperty, value);
        }

        public QuestTriggerType? TriggerTypeFilter
        {
            get => (QuestTriggerType?)GetValue(TriggerTypeFilterProperty);
            set => SetValue(TriggerTypeFilterProperty, value);
        }

        public System.Collections.ObjectModel.ObservableCollection<TriggerMetadata> AvailableTriggers
        {
            get => (System.Collections.ObjectModel.ObservableCollection<TriggerMetadata>)GetValue(AvailableTriggersProperty);
            set => SetValue(AvailableTriggersProperty, value);
        }

        public TriggerSelector()
        {
            InitializeComponent();
            Loaded += TriggerSelector_Loaded;
            TriggerComboBox.SelectionChanged += TriggerComboBox_SelectionChanged;
        }

        private void TriggerSelector_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTriggerList();
        }

        private void TriggerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTriggerMetadata = TriggerComboBox.SelectedItem as TriggerMetadata;
        }

        private static void OnSelectedTriggerMetadataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TriggerSelector selector)
            {
                if (selector.TriggerComboBox.SelectedItem != e.NewValue)
                {
                    selector.TriggerComboBox.SelectedItem = e.NewValue;
                }
            }
        }

        private static void OnTriggerTypeFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TriggerSelector selector)
            {
                selector.UpdateTriggerList();
            }
        }

        private static void OnAvailableTriggersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TriggerSelector selector)
            {
                selector.UpdateTriggerList();
            }
        }

        private void UpdateTriggerList()
        {
            if (AvailableTriggers == null)
            {
                TriggerComboBox.ItemsSource = null;
                return;
            }

            if (TriggerTypeFilter.HasValue)
            {
                TriggerComboBox.ItemsSource = AvailableTriggers
                    .Where(t => t.TriggerType == TriggerTypeFilter.Value)
                    .ToList();
            }
            else
            {
                TriggerComboBox.ItemsSource = AvailableTriggers;
            }
        }
    }
}

