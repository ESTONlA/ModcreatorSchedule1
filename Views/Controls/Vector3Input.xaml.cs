using System.Windows;
using System.Windows.Controls;

namespace Schedule1ModdingTool.Views.Controls
{
    /// <summary>
    /// Reusable control for Vector3 input (X, Y, Z coordinates)
    /// </summary>
    public partial class Vector3Input : UserControl
    {
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register(nameof(X), typeof(float), typeof(Vector3Input),
                new FrameworkPropertyMetadata(0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register(nameof(Y), typeof(float), typeof(Vector3Input),
                new FrameworkPropertyMetadata(0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ZProperty =
            DependencyProperty.Register(nameof(Z), typeof(float), typeof(Vector3Input),
                new FrameworkPropertyMetadata(0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public float X
        {
            get => (float)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        public float Y
        {
            get => (float)GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        public float Z
        {
            get => (float)GetValue(ZProperty);
            set => SetValue(ZProperty, value);
        }

        public Vector3Input()
        {
            InitializeComponent();
        }
    }
}
