using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI.Controls
{
    public partial class CurrencyIcon : UserControl
    {
        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(
                nameof(IconSize),
                typeof(double),
                typeof(CurrencyIcon),
                new PropertyMetadata(14.0));

        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public static readonly DependencyProperty IconColorProperty =
            DependencyProperty.Register(
                nameof(IconColor),
                typeof(Brush),
                typeof(CurrencyIcon),
                new PropertyMetadata(Brushes.Black));

        /// <summary>Tint for the dirham glyph. Defaults to black; bind to match adjacent text color.</summary>
        public Brush IconColor
        {
            get => (Brush)GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }

        public CurrencyIcon()
        {
            InitializeComponent();
        }
    }
}
