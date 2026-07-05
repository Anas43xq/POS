using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI.Controls;

public partial class PeriodFilterPanel : UserControl
{
    public static readonly DependencyProperty FilterVisibilityProperty =
        DependencyProperty.Register(nameof(FilterVisibility), typeof(Visibility), typeof(PeriodFilterPanel), new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty FromDateProperty =
        DependencyProperty.Register(nameof(FromDate), typeof(object), typeof(PeriodFilterPanel), new PropertyMetadata(null));

    public static readonly DependencyProperty ToDateProperty =
        DependencyProperty.Register(nameof(ToDate), typeof(object), typeof(PeriodFilterPanel), new PropertyMetadata(null));

    public static readonly DependencyProperty ApplyCommandProperty =
        DependencyProperty.Register(nameof(ApplyCommand), typeof(ICommand), typeof(PeriodFilterPanel), new PropertyMetadata(null));

    public static readonly DependencyProperty FromLabelProperty =
        DependencyProperty.Register(nameof(FromLabel), typeof(string), typeof(PeriodFilterPanel), new PropertyMetadata("From:"));

    public static readonly DependencyProperty ToLabelProperty =
        DependencyProperty.Register(nameof(ToLabel), typeof(string), typeof(PeriodFilterPanel), new PropertyMetadata("To:"));

    public static readonly DependencyProperty ApplyButtonTextProperty =
        DependencyProperty.Register(nameof(ApplyButtonText), typeof(string), typeof(PeriodFilterPanel), new PropertyMetadata("Apply"));

    public PeriodFilterPanel()
    {
        InitializeComponent();
    }

    public Visibility FilterVisibility
    {
        get => (Visibility)GetValue(FilterVisibilityProperty);
        set => SetValue(FilterVisibilityProperty, value);
    }

    public object? FromDate
    {
        get => GetValue(FromDateProperty);
        set => SetValue(FromDateProperty, value);
    }

    public object? ToDate
    {
        get => GetValue(ToDateProperty);
        set => SetValue(ToDateProperty, value);
    }

    public ICommand? ApplyCommand
    {
        get => (ICommand?)GetValue(ApplyCommandProperty);
        set => SetValue(ApplyCommandProperty, value);
    }

    public string FromLabel
    {
        get => (string)GetValue(FromLabelProperty);
        set => SetValue(FromLabelProperty, value);
    }

    public string ToLabel
    {
        get => (string)GetValue(ToLabelProperty);
        set => SetValue(ToLabelProperty, value);
    }

    public string ApplyButtonText
    {
        get => (string)GetValue(ApplyButtonTextProperty);
        set => SetValue(ApplyButtonTextProperty, value);
    }
}
