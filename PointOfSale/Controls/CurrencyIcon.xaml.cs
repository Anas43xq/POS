using System.Windows;
using System.Windows.Controls;

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

        public CurrencyIcon()
        {
            InitializeComponent();
        }
    }
}
