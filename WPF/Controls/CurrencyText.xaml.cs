using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UI.Controls
{
    public partial class CurrencyText : UserControl
    {
        public static readonly DependencyProperty AmountProperty =
            DependencyProperty.Register(
                nameof(Amount),
                typeof(decimal),
                typeof(CurrencyText),
                new PropertyMetadata(0m));

        public decimal Amount
        {
            get => (decimal)GetValue(AmountProperty);
            set => SetValue(AmountProperty, value);
        }

        public static readonly DependencyProperty AmountForegroundProperty =
            DependencyProperty.Register(
                nameof(AmountForeground),
                typeof(Brush),
                typeof(CurrencyText),
                new PropertyMetadata(System.Windows.Media.Brushes.Black));

        public Brush AmountForeground
        {
            get => (Brush)GetValue(AmountForegroundProperty);
            set => SetValue(AmountForegroundProperty, value);
        }

        public static readonly DependencyProperty IconSizeProperty =
          DependencyProperty.Register(
              nameof(IconSize),
              typeof(double?),
              typeof(CurrencyText),
              new PropertyMetadata(null));

        public double? IconSize
        {
            get => (double?)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register(
                nameof(ShowIcon),
                typeof(bool),
                typeof(CurrencyText),
                new PropertyMetadata(true));

        public bool ShowIcon
        {
            get => (bool)GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        public CurrencyText()
        {
            InitializeComponent();
        }
    }
}
