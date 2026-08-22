using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using Contracts.Sales;
using POS.Contracts.Localization;
using Contracts.Transactions;
using Contracts.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using UI.Configuration;
using UI.Commands;
using UI.Services;
using UI.Views;
using UI.ViewModels.Modifiers;
using System.Diagnostics;

namespace UI.ViewModels;

public partial class CashierDashboardViewModel : BaseViewModel
{
    private readonly ISessionService _session;
    private readonly IShiftService _shiftService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IRecentSaleService _recentSale;
    private readonly IDialogService _dialogService;
    private readonly ITransactionService _transactionService;
    private readonly IReceiptDisplayService _receiptDisplayService;
    private readonly ILocalizationService _localization;
    private readonly IViewModelFactory _viewModelFactory;

    private readonly IModifierService _modifierService;
    private readonly ICartModifierService _cartModifierService;
    private readonly ICartPricingService _cartPricingService;

    private readonly ILogger<CashierDashboardViewModel> _logger;
    private readonly INotificationService _notifications;
    private bool _hasInitialized;
    private readonly ShortcutSettings _shortcuts;

    private readonly ModifierPanelViewModel _modifierPanel;
    public ModifierPanelViewModel ModifierPanel => _modifierPanel;

    private string _cashierName = string.Empty;
    public string CashierName
    {
        get => _cashierName;
        set
        {
            _cashierName = value;
            OnPropertyChanged();
        }
    }

    private string _shiftStatus = "No Shift";
    public string ShiftStatus
    {
        get => _shiftStatus;
        set
        {
            _shiftStatus = value ?? "No Shift";
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsShiftOpen));
            OnPropertyChanged(nameof(CanStartDay));
            OnPropertyChanged(nameof(CanEndDay));
        }
    }

    public bool IsShiftOpen => _session.CurrentShift?.Status == Contracts.Enum.ShiftStatus.Open;

    public bool CanStartDay => !IsShiftOpen;

    public bool CanEndDay => IsShiftOpen;

    public event Action? LogoutRequested;
    private string _searchText = string.Empty;
    private readonly DispatcherTimer _searchRefreshTimer;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

            _searchText = value;
            OnPropertyChanged();
            ScheduleProductRefresh();
        }
    }

    public int SaleItemsCount => SaleItems.Count;

    public string CartCountDisplay =>
        _localization.GetString("Common.ItemsCount", SaleItemsCount);

    public ObservableCollection<ProductDto> Products { get; } = new();

    public ICollectionView ProductsView { get; }

    private bool _showNoProductsMessage;
    public bool ShowNoProductsMessage
    {
        get => _showNoProductsMessage;
        private set
        {
            _showNoProductsMessage = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<CartItem> SaleItems { get; } = new();

    public ObservableCollection<CategoryDto> Categories { get; } = new();

    public ObservableCollection<CategoryDto> SubCategories { get; } = new();

    public IEnumerable<CategoryDto> VisibleSubCategories
    {
        get
        {
            if (SelectedCategory == null || SelectedCategory.CategoryId == 0)
                return Enumerable.Empty<CategoryDto>();

            if (SelectedCategory.ChildCategories?.Any() == true)
                return SelectedCategory.ChildCategories;

            if (SelectedCategory.ParentCategoryId.HasValue)
            {
                var parent = Categories.FirstOrDefault(c => c.CategoryId == SelectedCategory.ParentCategoryId.Value);
                if (parent?.ChildCategories != null)
                    return parent.ChildCategories.Where(c => c.CategoryId != SelectedCategory.CategoryId);
            }

            return Enumerable.Empty<CategoryDto>();
        }
    }

    public ObservableCollection<RecentTransactionDto> RecentSales { get; } = new();

    private CategoryDto? _selectedCategory;
    public CategoryDto? SelectedCategory
    {
        get => _selectedCategory;
        set
        {

            _selectedCategory = value;
            OnPropertyChanged();

            OnPropertyChanged(nameof(VisibleSubCategories));
            OnPropertyChanged(nameof(HasSubCategories));
            RefreshProductView();
        }
    }
    public bool HasSubCategories => VisibleSubCategories.Any();

    private decimal _subtotal;
    public decimal Subtotal
    {
        get => _subtotal;
        set
        {
            _subtotal = value;
            OnPropertyChanged();
        }
    }

    private decimal _tax;
    public decimal Tax
    {
        get => _tax;
        set
        {
            _tax = value;
            OnPropertyChanged();
        }
    }

    private decimal _total;
    public decimal Total
    {
        get => _total;
        set
        {
            _total = value;
            OnPropertyChanged();
        }
    }

    private CartItem? _selectedCartItem;
    public CartItem? SelectedCartItem
    {
        get => _selectedCartItem;
        set
        {
            _selectedCartItem = value;
            OnPropertyChanged();
            IncreaseSelectedQuantityCommand.RaiseCanExecuteChanged();
            DecreaseSelectedQuantityCommand.RaiseCanExecuteChanged();
            RemoveSelectedSaleItemCommand.RaiseCanExecuteChanged();
        }
    }

    public ICommand AddProductCommand { get; }

    public ICommand SelectCategoryCommand { get; }

    public ICommand SelectSubCategoryCommand { get; }

    public ICommand RemoveSaleItemCommand { get; }

    public RelayCommand IncreaseSelectedQuantityCommand { get; }

    public RelayCommand DecreaseSelectedQuantityCommand { get; }

    public RelayCommand RemoveSelectedSaleItemCommand { get; }

    public RelayCommand ClearSaleCommand { get; }

    public AsyncRelayCommand PayCashCommand { get; }

    public AsyncRelayCommand PayCardCommand { get; }

    public ICommand LogoutCommand { get; }

    public ICommand ShowRecentSalesCommand { get; }

    public ICommand StartDayCommand { get; }

    public ICommand EndDayCommand { get; }

    public ICommand ShowSetting { get; }

    public ICommand ShowShortcutHelpCommand { get; }

    public RelayCommand CompleteSaleCommand { get; }

    public RelayCommand NewSaleCommand { get; }

    public RelayCommand ToggleShiftCommand { get; }

    public AsyncRelayCommand ReprintLastReceiptCommand { get; }

    public AsyncRelayCommand<CartItem> EditCartLineCommand { get; }

    public IReadOnlyList<LanguageDto> SupportedLanguages { get; }
    public string CashPaymentShortcut => _shortcuts.Cashier.CashPayment;
    public string CardPaymentShortcut => _shortcuts.Cashier.CardPayment;
    public string ShowRecentSalesShortcut => _shortcuts.Cashier.ShowRecentSales;
    public string ReprintLastReceiptShortcut => _shortcuts.Cashier.ReprintLastReceipt;
    public ICommand OpenLanguagePickerCommand { get; }
    public ICommand CloseLanguagePickerCommand { get; }
    public ICommand SelectLanguageCommand { get; }

    private bool _isLanguagePickerOpen;
    public bool IsLanguagePickerOpen
    {
        get => _isLanguagePickerOpen;
        set
        {
            if (_isLanguagePickerOpen != value)
            {
                _isLanguagePickerOpen = value;
                OnPropertyChanged();
            }
        }
    }

    public CashierDashboardViewModel(
        ISessionService session,
        IShiftService shiftService,
        IProductService productService,
        ICategoryService categoryService,
        IRecentSaleService recentSale,
        IDialogService dialogService,
        ITransactionService transactionService,
        IReceiptDisplayService receiptDisplayService,
        ILocalizationService localization,
        IModifierService modifierService,
        ICartModifierService cartModifierService,
        ICartPricingService cartPricingService,
        IViewModelFactory viewModelFactory,
        ILogger<CashierDashboardViewModel> logger,
        INotificationService notifications,
        ShortcutSettings shortcuts)
    {
        _session = session;
        _shiftService = shiftService;
        _productService = productService;
        _categoryService = categoryService;
        _recentSale = recentSale;
        _dialogService = dialogService;
        _transactionService = transactionService;
        _receiptDisplayService = receiptDisplayService;
        _localization = localization;
        _modifierService = modifierService;
        _cartModifierService = cartModifierService;
        _cartPricingService = cartPricingService;
        _viewModelFactory = viewModelFactory;
        _logger = logger;
        _notifications = notifications;
        Notifications = notifications;
        _shortcuts = shortcuts;

        _modifierPanel = _viewModelFactory.Create<ModifierPanelViewModel>();

        _localization.LanguageChanged += OnLocalizationLanguageChanged;

        SaleItems.CollectionChanged += SaleItems_CollectionChanged;

        AddProductCommand = new AsyncRelayCommand<ProductDto>(
            AddProductAsync,
            product => product != null && product.IsActive && IsShiftOpen);

        RemoveSaleItemCommand = new AsyncRelayCommand<CartItem>(RemoveSaleItemAsync);

        EditCartLineCommand = new AsyncRelayCommand<CartItem>(
            EditCartLineAsync,
            item => item != null);

        IncreaseSelectedQuantityCommand = new RelayCommand(
            IncreaseSelectedQuantity,
            () => SelectedCartItem != null);

        DecreaseSelectedQuantityCommand = new RelayCommand(
            DecreaseSelectedQuantity,
            () => SelectedCartItem != null);

        RemoveSelectedSaleItemCommand = new RelayCommand(
            RemoveSelectedSaleItem,
            () => SelectedCartItem != null);

        ClearSaleCommand = new RelayCommand(
            ClearSales,
            () => SaleItems.Any());

        ShowSetting = new AsyncRelayCommand(OpenSetting, onError: ex =>
        {
            _logger.LogError(ex, "Failed to open Settings dialog");
            _notifications.ShowError(_localization.GetString("Common.UnableToOpenSettings"));
        });

        SelectCategoryCommand = new AsyncRelayCommand<CategoryDto>(SelectParentCategoryAsync);

        SelectSubCategoryCommand = new AsyncRelayCommand<CategoryDto>(SelectSubCategoryAsync);

        PayCashCommand = new AsyncRelayCommand(
            PayCashAsync,
            () => IsShiftOpen && SaleItems.Any());

        PayCardCommand = new AsyncRelayCommand(
            PayCardAsync,
            () => IsShiftOpen && SaleItems.Any());

        LogoutCommand = new RelayCommand(
            LogoutAsync,
            () => _session.CurrentUser != null);

        ShowRecentSalesCommand = new AsyncRelayCommand(ShowRecentSalesAsync);

        ShowShortcutHelpCommand = new RelayCommand(_ =>
        {
            var vm = _viewModelFactory.Create<ShortcutHelpViewModel>();
            _dialogService.ShowDialog<ShortcutHelpView>(vm);
        });

        StartDayCommand = new RelayCommand(
            ShowStartDayDialog,
            () => CanStartDay);

        EndDayCommand = new RelayCommand(
            ShowEndDayDialog,
            () => CanEndDay);

        CompleteSaleCommand = new RelayCommand(
            () => SaleItems.Clear(),
            () => SaleItems.Any());

        NewSaleCommand = new RelayCommand(
            () => SaleItems.Clear());

        ToggleShiftCommand = new RelayCommand(
            () =>
            {
                if (IsShiftOpen)
                    ((ICommand)EndDayCommand).Execute(null);
                else
                    ((ICommand)StartDayCommand).Execute(null);
            });

        ReprintLastReceiptCommand = new AsyncRelayCommand(
            ReprintLastReceiptAsync,
            () => RecentSales.Any());

        _searchRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(150)
        };
        _searchRefreshTimer.Tick += (_, _) =>
        {
            _searchRefreshTimer.Stop();
            RefreshProductView();
        };

        ProductsView = CollectionViewSource.GetDefaultView(Products);
        ProductsView.Filter = FilterProduct;

        SelectedCategory = null;

        SupportedLanguages = _localization.GetSupportedLanguages();

        OpenLanguagePickerCommand  = new RelayCommand(_ => IsLanguagePickerOpen = true);
        CloseLanguagePickerCommand = new RelayCommand(_ => IsLanguagePickerOpen = false);
        SelectLanguageCommand = new AsyncRelayCommand<LanguageDto>(async lang =>
        {
            if (lang is null) return;
            await _localization.SetLanguageAsync(lang.Code);
            IsLanguagePickerOpen = false;
        });
    }

    public async Task EnsureInitializedAsync()
    {
        if (_hasInitialized)
            return;

        _hasInitialized = true;

        await Task.Yield();
        await InitializeAsync();
    }

    public async Task RefreshAfterShiftHydrationAsync()
    {
        LoadTopBar();
        RefreshCommandStates();
        await LoadRecentSalesAsync();
        RefreshProductView();
    }

    private void ScheduleProductRefresh()
    {
        _searchRefreshTimer.Stop();
        _searchRefreshTimer.Start();
    }

    private void RefreshProductView()
    {
        ProductsView.Refresh();
        UpdateNoProductsMessage();
    }

        private void OnLocalizationLanguageChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CartCountDisplay));
            _ = ReloadLocalizedDataAsync();
        }

        /// <summary>
        /// Reloads categories and products using the current language, then
        /// updates in-cart display names without touching the English-only
        /// receipt snapshots stored in <see cref="CartItem.ProductName"/>.
        /// </summary>
        private async Task ReloadLocalizedDataAsync()
        {
            try
            {
                await LoadCategoriesAsync();
                await LoadProductsAsync();

                UpdateCartItemLocalizedNames();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reload localized data after language change");
                _notifications.ShowError("Failed to refresh display language.");
            }
        }

        /// <summary>
        /// Updates <see cref="CartItem.LocalizedProductName"/> for every item
        /// currently in the cart using the freshly-loaded <see cref="Products"/>
        /// collection. <see cref="CartItem.ProductName"/> (English) is never
        /// touched so receipt snapshots remain English-only.
        /// </summary>
        private void UpdateCartItemLocalizedNames()
        {
            var displayNameByVariantId = Products.ToDictionary(p => p.VariantId, p => p.DisplayName);

            foreach (var item in SaleItems)
            {
                if (displayNameByVariantId.TryGetValue(item.VariantId, out var localized))
                    item.LocalizedProductName = localized;
                else
                    item.LocalizedProductName = item.ProductName;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _localization.LanguageChanged -= OnLocalizationLanguageChanged;

                _modifierPanel.Dispose();
            }

            base.Dispose(disposing);
        }
}
