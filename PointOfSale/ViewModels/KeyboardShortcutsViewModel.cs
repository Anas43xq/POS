using System.Collections.ObjectModel;
using System.Windows.Input;
using Contracts.Enum;
using Contracts.Shortcuts;
using Microsoft.Extensions.Logging;
using UI.Commands;
using UI.Services;
using UI.Views;

namespace UI.ViewModels;

public class KeyboardShortcutsViewModel : BaseViewModel
{
    private readonly IKeyboardShortcutService _shortcutService;
    private readonly ShortcutManager _shortcutManager;
    private readonly IDialogService _dialogService;
    private readonly ILogger<KeyboardShortcutsViewModel>? _logger;

    private ShortcutProfileType _selectedProfile = ShortcutProfileType.Cashier;
    private string _searchText = string.Empty;
    private bool _isDirty;
    private ShortcutItemViewModel? _selectedShortcut;

    public KeyboardShortcutsViewModel(
        IKeyboardShortcutService shortcutService,
        ShortcutManager shortcutManager,
        IDialogService dialogService,
        ILogger<KeyboardShortcutsViewModel>? logger)
    {
        _shortcutService = shortcutService;
        _shortcutManager = shortcutManager;
        _dialogService = dialogService;
        _logger = logger;

        Shortcuts = [];
        FilteredShortcuts = [];

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => IsDirty);
        CancelCommand = new RelayCommand(_ => Cancel());
        RestoreDefaultCommand = new RelayCommand(_ => RestoreDefault(), _ => SelectedShortcut != null);
        RestoreAllCommand = new RelayCommand(_ => RestoreAll());
        EditCommand = new RelayCommand(_ => EditShortcut(), _ => SelectedShortcut != null);
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());

        LoadShortcuts();
    }

    public ObservableCollection<ShortcutItemViewModel> Shortcuts { get; }
    public ObservableCollection<ShortcutItemViewModel> FilteredShortcuts { get; }

    public ShortcutProfileType SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (_selectedProfile != value)
            {
                _selectedProfile = value;
                OnPropertyChanged();
                LoadShortcuts();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
    }

    public bool IsDirty
    {
        get => _isDirty;
        set { _isDirty = value; OnPropertyChanged(); }
    }

    public ShortcutItemViewModel? SelectedShortcut
    {
        get => _selectedShortcut;
        set { _selectedShortcut = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RestoreDefaultCommand { get; }
    public ICommand RestoreAllCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand CloseCommand { get; }

    public event Action? CloseRequested;

    public void LoadShortcuts()
    {
        Shortcuts.Clear();
        FilteredShortcuts.Clear();

        var bindings = _shortcutService.GetBindings(_selectedProfile);

        var grouped = bindings
            .GroupBy(b => GetCategory(b.Action))
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            foreach (var binding in group.OrderBy(b => b.Description))
            {
                var item = new ShortcutItemViewModel
                {
                    Action = binding.Action,
                    ActionName = binding.Action.ToString(),
                    KeyGesture = binding.KeyGesture,
                    Description = binding.Description,
                    Category = group.Key,
                    OriginalKeyGesture = binding.KeyGesture
                };
                Shortcuts.Add(item);
                FilteredShortcuts.Add(item);
            }
        }

        IsDirty = false;
    }

    private void ApplyFilter()
    {
        FilteredShortcuts.Clear();

        var query = string.IsNullOrWhiteSpace(SearchText)
            ? Shortcuts.AsEnumerable()
            : Shortcuts.Where(s =>
                s.ActionName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || s.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || s.KeyGesture.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || s.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var item in query)
        {
            FilteredShortcuts.Add(item);
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            await _shortcutService.SaveCustomBindingsAsync(_selectedProfile);
            IsDirty = false;
            _logger?.LogDebug("Keyboard shortcuts saved for profile {Profile}", _selectedProfile);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save keyboard shortcuts");
        }
    }

    private void Cancel()
    {
        LoadShortcuts();
    }

    private void RestoreDefault()
    {
        if (SelectedShortcut == null) return;

        var defaults = DefaultShortcutProfiles.GetDefault(_selectedProfile);
        var defaultBinding = defaults.Bindings.FirstOrDefault(b => b.Action == SelectedShortcut.Action);
        if (defaultBinding != null)
        {
            SelectedShortcut.KeyGesture = defaultBinding.KeyGesture;
            MarkDirty();
        }
    }

    private void RestoreAll()
    {
        var defaults = DefaultShortcutProfiles.GetDefault(_selectedProfile);
        foreach (var item in Shortcuts)
        {
            var defaultBinding = defaults.Bindings.FirstOrDefault(b => b.Action == item.Action);
            if (defaultBinding != null)
            {
                item.KeyGesture = defaultBinding.KeyGesture;
            }
        }
        MarkDirty();
    }

    private void EditShortcut()
    {
        if (SelectedShortcut == null) return;

        var binding = new ShortcutBinding
        {
            Action = SelectedShortcut.Action,
            KeyGesture = SelectedShortcut.KeyGesture,
            Description = SelectedShortcut.Description
        };

        var dialogViewModel = new EditShortcutDialogViewModel(binding, _selectedProfile, _shortcutService);
        var result = _dialogService.ShowDialogWithResult<EditShortcutDialogWindow>(dialogViewModel);

        if (result == true)
        {
            var updatedBindings = _shortcutService.GetBindings(_selectedProfile);
            var updated = updatedBindings.FirstOrDefault(b => b.Action == SelectedShortcut.Action);
            if (updated != null)
            {
                SelectedShortcut.KeyGesture = updated.KeyGesture;
                MarkDirty();
            }
        }
    }

    private void MarkDirty()
    {
        IsDirty = true;
    }

    private static string GetCategory(ShortcutAction action)
    {
        return action switch
        {
            // Cashier
            ShortcutAction.ConfirmPayment or
            ShortcutAction.HoldSale or
            ShortcutAction.SearchProducts or
            ShortcutAction.RefreshProducts or
            ShortcutAction.RemoveItem or
            ShortcutAction.IncreaseQuantity or
            ShortcutAction.DecreaseQuantity or
            ShortcutAction.CashPayment or
            ShortcutAction.CardPayment or
            ShortcutAction.CompleteSale or
            ShortcutAction.NewSale or
            ShortcutAction.OpenCloseShift or
            ShortcutAction.ReprintLastReceipt => "Cashier Operations",

            // Navigation
            ShortcutAction.NavigateHome or
            ShortcutAction.NavigateProducts or
            ShortcutAction.NavigateCategories or
            ShortcutAction.NavigateSizes or
            ShortcutAction.NavigateReports or
            ShortcutAction.NavigateTransactions or
            ShortcutAction.NavigateReceiptManagement or
            ShortcutAction.NavigateShiftManagement or
            ShortcutAction.NavigateSettings or
            ShortcutAction.Logout => "Navigation",

            // Cashier Navigation
            ShortcutAction.FocusCategories or
            ShortcutAction.FocusProducts or
            ShortcutAction.FocusCart => "Cashier Navigation",

            // CRUD
            ShortcutAction.Add or
            ShortcutAction.Edit or
            ShortcutAction.Delete or
            ShortcutAction.Save or
            ShortcutAction.Duplicate or
            ShortcutAction.AddSubcategory => "CRUD Operations",

            // Module-specific
            ShortcutAction.ManageTranslations or
            ShortcutAction.ManageVariants or
            ShortcutAction.ManageSizes => "Module Operations",

            // Data Operations
            ShortcutAction.Refresh or
            ShortcutAction.Export or
            ShortcutAction.GenerateReport or
            ShortcutAction.ExportToExcel or
            ShortcutAction.PrintReport => "Data Operations",

            // Transaction Operations
            ShortcutAction.ViewDetails or
            ShortcutAction.ReprintReceipt or
            ShortcutAction.VoidTransaction => "Transaction Operations",

            // Common
            ShortcutAction.ShortcutHelp or
            ShortcutAction.FocusSearch or
            ShortcutAction.Escape or
            ShortcutAction.Confirm or
            ShortcutAction.NextField or
            ShortcutAction.PreviousField or
            ShortcutAction.RestoreDefaults => "Common",

            _ => "Other"
        };
    }
}

public class ShortcutItemViewModel : BaseViewModel
{
    private string _keyGesture = string.Empty;

    public ShortcutAction Action { get; init; }
    public string ActionName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string OriginalKeyGesture { get; init; } = string.Empty;

    public string KeyGesture
    {
        get => _keyGesture;
        set { _keyGesture = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsModified)); }
    }

    public bool IsModified => !string.Equals(KeyGesture, OriginalKeyGesture, StringComparison.OrdinalIgnoreCase);
}
