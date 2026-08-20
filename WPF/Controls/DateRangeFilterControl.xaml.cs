using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI.Controls;

/// <summary>
/// Shared date-range filter for the main screens. Consumers bind the control to
/// their own ViewModel properties; the period section is shown only when requested.
/// </summary>
public partial class DateRangeFilterControl : UserControl
{
    public static readonly DependencyProperty DayCommandProperty =
        DependencyProperty.Register(nameof(DayCommand), typeof(ICommand), typeof(DateRangeFilterControl), new PropertyMetadata(null));

    public static readonly DependencyProperty WeekCommandProperty =
        DependencyProperty.Register(nameof(WeekCommand), typeof(ICommand), typeof(DateRangeFilterControl), new PropertyMetadata(null));

    public static readonly DependencyProperty MonthCommandProperty =
        DependencyProperty.Register(nameof(MonthCommand), typeof(ICommand), typeof(DateRangeFilterControl), new PropertyMetadata(null));

    public static readonly DependencyProperty PeriodCommandProperty =
        DependencyProperty.Register(nameof(PeriodCommand), typeof(ICommand), typeof(DateRangeFilterControl), new PropertyMetadata(null));

    public static readonly DependencyProperty ApplyCommandProperty =
        DependencyProperty.Register(nameof(ApplyCommand), typeof(ICommand), typeof(DateRangeFilterControl), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedDateRangeProperty =
        DependencyProperty.Register(
            nameof(SelectedDateRange),
            typeof(string),
            typeof(DateRangeFilterControl),
            new FrameworkPropertyMetadata("Day", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsPeriodFilterVisibleProperty =
        DependencyProperty.Register(
            nameof(IsPeriodFilterVisible),
            typeof(bool),
            typeof(DateRangeFilterControl),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty FromDateProperty =
        DependencyProperty.Register(
            nameof(FromDate),
            typeof(DateTime?),
            typeof(DateRangeFilterControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ToDateProperty =
        DependencyProperty.Register(
            nameof(ToDate),
            typeof(DateTime?),
            typeof(DateRangeFilterControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty DayLabelProperty =
        DependencyProperty.Register(nameof(DayLabel), typeof(string), typeof(DateRangeFilterControl), new PropertyMetadata("Day"));

    public static readonly DependencyProperty WeekLabelProperty =
        DependencyProperty.Register(nameof(WeekLabel), typeof(string), typeof(DateRangeFilterControl), new PropertyMetadata("Week"));

    public static readonly DependencyProperty MonthLabelProperty =
        DependencyProperty.Register(nameof(MonthLabel), typeof(string), typeof(DateRangeFilterControl), new PropertyMetadata("Month"));

    public static readonly DependencyProperty PeriodLabelProperty =
        DependencyProperty.Register(nameof(PeriodLabel), typeof(string), typeof(DateRangeFilterControl), new PropertyMetadata("Period"));

    public static readonly DependencyProperty FromLabelProperty =
        DependencyProperty.Register(nameof(FromLabel), typeof(string), typeof(DateRangeFilterControl), new PropertyMetadata("From:"));

    public static readonly DependencyProperty ToLabelProperty =
        DependencyProperty.Register(nameof(ToLabel), typeof(string), typeof(DateRangeFilterControl), new PropertyMetadata("To:"));

    public static readonly DependencyProperty ApplyButtonTextProperty =
        DependencyProperty.Register(nameof(ApplyButtonText), typeof(string), typeof(DateRangeFilterControl), new PropertyMetadata("Apply"));

    public DateRangeFilterControl()
    {
        InitializeComponent();
    }

    public ICommand? DayCommand
    {
        get => (ICommand?)GetValue(DayCommandProperty);
        set => SetValue(DayCommandProperty, value);
    }

    public ICommand? WeekCommand
    {
        get => (ICommand?)GetValue(WeekCommandProperty);
        set => SetValue(WeekCommandProperty, value);
    }

    public ICommand? MonthCommand
    {
        get => (ICommand?)GetValue(MonthCommandProperty);
        set => SetValue(MonthCommandProperty, value);
    }

    public ICommand? PeriodCommand
    {
        get => (ICommand?)GetValue(PeriodCommandProperty);
        set => SetValue(PeriodCommandProperty, value);
    }

    public ICommand? ApplyCommand
    {
        get => (ICommand?)GetValue(ApplyCommandProperty);
        set => SetValue(ApplyCommandProperty, value);
    }

    public string SelectedDateRange
    {
        get => (string)GetValue(SelectedDateRangeProperty);
        set => SetValue(SelectedDateRangeProperty, value);
    }

    public bool IsPeriodFilterVisible
    {
        get => (bool)GetValue(IsPeriodFilterVisibleProperty);
        set => SetValue(IsPeriodFilterVisibleProperty, value);
    }

    public DateTime? FromDate
    {
        get => (DateTime?)GetValue(FromDateProperty);
        set => SetValue(FromDateProperty, value);
    }

    public DateTime? ToDate
    {
        get => (DateTime?)GetValue(ToDateProperty);
        set => SetValue(ToDateProperty, value);
    }

    public string DayLabel
    {
        get => (string)GetValue(DayLabelProperty);
        set => SetValue(DayLabelProperty, value);
    }

    public string WeekLabel
    {
        get => (string)GetValue(WeekLabelProperty);
        set => SetValue(WeekLabelProperty, value);
    }

    public string MonthLabel
    {
        get => (string)GetValue(MonthLabelProperty);
        set => SetValue(MonthLabelProperty, value);
    }

    public string PeriodLabel
    {
        get => (string)GetValue(PeriodLabelProperty);
        set => SetValue(PeriodLabelProperty, value);
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
