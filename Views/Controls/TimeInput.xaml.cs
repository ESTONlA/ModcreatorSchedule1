using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Schedule1ModdingTool.Views.Controls
{
    /// <summary>
    /// Reusable control for time input in HHMM format (0000-2359)
    /// </summary>
    public partial class TimeInput : UserControl
    {
        private static readonly Regex _numericRegex = new Regex("[^0-9]+");
        private bool _isUpdatingText = false;
        private bool _isUpdatingProperty = false;

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register(nameof(Time), typeof(int), typeof(TimeInput),
                new FrameworkPropertyMetadata(900, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTimeChanged));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(TimeInput),
                new PropertyMetadata("Time"));

        public int Time
        {
            get => (int)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public TimeInput()
        {
            InitializeComponent();
        }

        private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimeInput control && e.NewValue is int time && !control._isUpdatingProperty)
            {
                // Validate time range (0000-2359)
                if (time < 0)
                {
                    control.Time = 0;
                    return;
                }
                if (time > 2359)
                {
                    control.Time = 2359;
                    return;
                }

                // Validate minutes (00-59)
                int minutes = time % 100;
                if (minutes > 59)
                {
                    control.Time = (time / 100) * 100 + 59;
                    return;
                }

                // Update textbox if not focused
                if (!control.TimeTextBox.IsFocused)
                {
                    control._isUpdatingText = true;
                    control.TimeTextBox.Text = time.ToString("D4");
                    control._isUpdatingText = false;
                }
            }
        }

        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow numeric input
            e.Handled = _numericRegex.IsMatch(e.Text);
        }

        private void TimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText) return;

            // Update property when user types
            if (int.TryParse(TimeTextBox.Text, out int value))
            {
                _isUpdatingProperty = true;
                Time = value;
                _isUpdatingProperty = false;
            }
            else if (string.IsNullOrEmpty(TimeTextBox.Text))
            {
                // Allow empty for editing
                _isUpdatingProperty = true;
                Time = 0;
                _isUpdatingProperty = false;
            }
        }

        private void TimeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Select all text for easy editing
            TimeTextBox.SelectAll();
        }

        private void TimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Format to 4 digits when losing focus
            _isUpdatingText = true;
            TimeTextBox.Text = Time.ToString("D4");
            _isUpdatingText = false;
        }
    }
}
