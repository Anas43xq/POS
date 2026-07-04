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
    /// <summary>
    /// Interaction logic for CurrencyText.xaml
    /// </summary>
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

        public static readonly DependencyProperty IconSizeProperty =
          DependencyProperty.Register(
              nameof(IconSize),
              typeof(double?),
              typeof(CurrencyText),
              new PropertyMetadata(null));

        /// <summary>
        /// Leave unset to auto-scale the icon with FontSize. Set explicitly to override.
        /// </summary>
        public double? IconSize
        {
            get => (double?)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public CurrencyText()
        {
            InitializeComponent();
        }
    }
}
