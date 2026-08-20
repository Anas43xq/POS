using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI.Controls;

public partial class RecentSalesList : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(RecentSalesList),
            new PropertyMetadata(null));

    public static readonly DependencyProperty OpenCommandProperty =
        DependencyProperty.Register(
            nameof(OpenCommand),
            typeof(ICommand),
            typeof(RecentSalesList),
            new PropertyMetadata(null));

    public RecentSalesList()
    {
        InitializeComponent();
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ICommand? OpenCommand
    {
        get => (ICommand?)GetValue(OpenCommandProperty);
        set => SetValue(OpenCommandProperty, value);
    }
}
