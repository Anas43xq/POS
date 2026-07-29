using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI.Controls;

/// <summary>
/// Shared Add / Edit / Delete / Refresh button group used by the
/// management pages (Products, Categories, Sizes, Modifier Groups).
///
/// Generic and technology-agnostic: it does NOT own a DataContext.
/// Each command/enabled-flag/label is exposed as a DependencyProperty
/// and bound by the consumer to that page's own ViewModel members —
/// no ViewModel, command, or existing binding is changed by adopting
/// this control; it only centralizes the button markup.
///
/// Consumers: ProductManagementView, CategoryManagementView,
/// SizeManagementView, ModifierGroupManagementView.
/// </summary>
public partial class CrudActionButtons : UserControl
{
    // ── Add ────────────────────────────────────────────────────────────
    public static readonly DependencyProperty AddCommandProperty =
        DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(CrudActionButtons), new PropertyMetadata(null));

    public static readonly DependencyProperty AddEnabledProperty =
        DependencyProperty.Register(nameof(AddEnabled), typeof(bool), typeof(CrudActionButtons), new PropertyMetadata(true));

    public static readonly DependencyProperty AddLabelProperty =
        DependencyProperty.Register(nameof(AddLabel), typeof(string), typeof(CrudActionButtons), new PropertyMetadata("Add"));

    // ── Edit ───────────────────────────────────────────────────────────
    public static readonly DependencyProperty EditCommandProperty =
        DependencyProperty.Register(nameof(EditCommand), typeof(ICommand), typeof(CrudActionButtons), new PropertyMetadata(null));

    public static readonly DependencyProperty EditEnabledProperty =
        DependencyProperty.Register(nameof(EditEnabled), typeof(bool), typeof(CrudActionButtons), new PropertyMetadata(true));

    public static readonly DependencyProperty EditLabelProperty =
        DependencyProperty.Register(nameof(EditLabel), typeof(string), typeof(CrudActionButtons), new PropertyMetadata("Edit"));

    // ── Delete ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(CrudActionButtons), new PropertyMetadata(null));

    public static readonly DependencyProperty DeleteEnabledProperty =
        DependencyProperty.Register(nameof(DeleteEnabled), typeof(bool), typeof(CrudActionButtons), new PropertyMetadata(true));

    public static readonly DependencyProperty DeleteLabelProperty =
        DependencyProperty.Register(nameof(DeleteLabel), typeof(string), typeof(CrudActionButtons), new PropertyMetadata("Delete"));

    // ── Refresh ────────────────────────────────────────────────────────
    public static readonly DependencyProperty RefreshCommandProperty =
        DependencyProperty.Register(nameof(RefreshCommand), typeof(ICommand), typeof(CrudActionButtons), new PropertyMetadata(null));

    public static readonly DependencyProperty RefreshLabelProperty =
        DependencyProperty.Register(nameof(RefreshLabel), typeof(string), typeof(CrudActionButtons), new PropertyMetadata("Refresh"));

    // ShowRefresh — when False, the divider and Refresh button are
    // collapsed. Lets the same UC serve both the page toolbar
    // (Refresh visible) and inline uses such as the Modifier
    // Options section header (no Refresh — the page toolbar owns it).
    public static readonly DependencyProperty ShowRefreshProperty =
        DependencyProperty.Register(nameof(ShowRefresh), typeof(bool), typeof(CrudActionButtons), new PropertyMetadata(true));

    // ── Translations (optional, opt-in) ────────────────────────────────
    // Hidden by default; consumers that need a Translations button
    // (e.g. SizeManagementView) set ShowTranslations="True" and bind
    // the command/enabled/label just like the other actions.
    public static readonly DependencyProperty ShowTranslationsProperty =
        DependencyProperty.Register(nameof(ShowTranslations), typeof(bool), typeof(CrudActionButtons), new PropertyMetadata(false));

    public static readonly DependencyProperty TranslationsCommandProperty =
        DependencyProperty.Register(nameof(TranslationsCommand), typeof(ICommand), typeof(CrudActionButtons), new PropertyMetadata(null));

    public static readonly DependencyProperty TranslationsEnabledProperty =
        DependencyProperty.Register(nameof(TranslationsEnabled), typeof(bool), typeof(CrudActionButtons), new PropertyMetadata(true));

    public static readonly DependencyProperty TranslationsLabelProperty =
        DependencyProperty.Register(nameof(TranslationsLabel), typeof(string), typeof(CrudActionButtons), new PropertyMetadata("Translations"));

    public CrudActionButtons()
    {
        InitializeComponent();
    }

    public ICommand? AddCommand
    {
        get => (ICommand?)GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public string AddLabel
    {
        get => (string)GetValue(AddLabelProperty);
        set => SetValue(AddLabelProperty, value);
    }

    public bool AddEnabled
    {
        get => (bool)GetValue(AddEnabledProperty);
        set => SetValue(AddEnabledProperty, value);
    }

    public ICommand? EditCommand
    {
        get => (ICommand?)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public bool EditEnabled
    {
        get => (bool)GetValue(EditEnabledProperty);
        set => SetValue(EditEnabledProperty, value);
    }

    public string EditLabel
    {
        get => (string)GetValue(EditLabelProperty);
        set => SetValue(EditLabelProperty, value);
    }

    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public bool DeleteEnabled
    {
        get => (bool)GetValue(DeleteEnabledProperty);
        set => SetValue(DeleteEnabledProperty, value);
    }

    public string DeleteLabel
    {
        get => (string)GetValue(DeleteLabelProperty);
        set => SetValue(DeleteLabelProperty, value);
    }

    public ICommand? RefreshCommand
    {
        get => (ICommand?)GetValue(RefreshCommandProperty);
        set => SetValue(RefreshCommandProperty, value);
    }

    public string RefreshLabel
    {
        get => (string)GetValue(RefreshLabelProperty);
        set => SetValue(RefreshLabelProperty, value);
    }

    public bool ShowRefresh
    {
        get => (bool)GetValue(ShowRefreshProperty);
        set => SetValue(ShowRefreshProperty, value);
    }

    public bool ShowTranslations
    {
        get => (bool)GetValue(ShowTranslationsProperty);
        set => SetValue(ShowTranslationsProperty, value);
    }

    public ICommand? TranslationsCommand
    {
        get => (ICommand?)GetValue(TranslationsCommandProperty);
        set => SetValue(TranslationsCommandProperty, value);
    }

    public bool TranslationsEnabled
    {
        get => (bool)GetValue(TranslationsEnabledProperty);
        set => SetValue(TranslationsEnabledProperty, value);
    }

    public string TranslationsLabel
    {
        get => (string)GetValue(TranslationsLabelProperty);
        set => SetValue(TranslationsLabelProperty, value);
    }
}
