using BLL.Interfaces;
using BLL.Models;
using Contracts.Sales;
using Contracts.Transactions;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using UI.Commands;
using UI.Services;
using UI.Views;
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

    private readonly ILogger<CashierDashboardViewModel> _logger;

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

    /// <summary>
    /// Gets whether a shift is currently open.
    /// </summary>
    public bool IsShiftOpen => _session.CurrentShift?.Status == DAL.Entities.ShiftStatus.Open;

    /// <summary>
    /// Gets whether the Start Day button should be enabled (no shift open).
    /// </summary>
    public bool CanStartDay => !IsShiftOpen;

    /// <summary>
    /// Gets whether the End Day button should be enabled (shift is open).
    /// </summary>
    public bool CanEndDay => IsShiftOpen;

    /// <summary>
    /// Top-of-window error banner. Always shown at the very top of the
    /// cashier dashboard so the user immediately sees failures
    /// (e.g. "session expired, please re-login").
    /// </summary>
    private string? _headerErrorMessage;
    public string? HeaderErrorMessage
    {
        get => _headerErrorMessage;
        private set
        {
            if (_headerErrorMessage == value) return;
            _headerErrorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasHeaderError));
        }
    }

    public bool HasHeaderError => !string.IsNullOrEmpty(_headerErrorMessage);

    public event Action? LogoutRequested;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();

            ProductsView.Refresh();
            UpdateNoProductsMessage();
        }
    }

    public int SaleItemsCount => SaleItems.Count;

    /// <summary>
    /// Localized count-badge text for the cart header
    /// (e.g. "0 items" / "3 items" / "0 عناصر" / "0 ഇനങ്ങൾ").
    /// Resolved via <see cref="ILocalizationService.GetString"/> so the
    /// <c>{0}</c> placeholder in the resource is properly substituted
    /// with <see cref="SaleItemsCount"/>. Re-evaluated on cart changes
    /// and on language switch.
    /// </summary>
    public string CartCountDisplay =>
        _localization.GetString("Common.ItemsCount", SaleItemsCount);

    public ObservableCollection<Product> Products { get; } = new();

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

    public ObservableCollection<Category> Categories { get; } = new();

    public ObservableCollection<Category> SubCategories { get; } = new();

    public IEnumerable<Category> VisibleSubCategories
    {
        get
        {
            if (SelectedCategory == null || SelectedCategory.CategoryId == 0)
                return Enumerable.Empty<Category>();

            if (SelectedCategory.ChildCategories?.Any() == true)
                return SelectedCategory.ChildCategories;

            return SelectedCategory.ParentCategory?.ChildCategories
                ?.Where(c => c.CategoryId != SelectedCategory.CategoryId)
                ?? Enumerable.Empty<Category>();
        }
    }

    public ObservableCollection<RecentTransactionDto> RecentSales { get; } = new();

    private Category? _selectedCategory;
    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {

            _selectedCategory = value;
            OnPropertyChanged();

            OnPropertyChanged(nameof(VisibleSubCategories));
            OnPropertyChanged(nameof(HasSubCategories));
            ProductsView.Refresh();
            UpdateNoProductsMessage();
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
        ILogger<CashierDashboardViewModel> logger)
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
        _logger = logger;

        // Cart count badge uses a localized format string ("{0} items"),
        // so it must re-evaluate when the language changes \u2014 not just
        // when the count changes.
        _localization.LanguageChanged += OnLocalizationLanguageChanged;

        SaleItems.CollectionChanged += SaleItems_CollectionChanged;

        AddProductCommand = new AsyncRelayCommand<Product>(
            AddProductAsync,
            product => product != null && product.IsActive && IsShiftOpen);

        RemoveSaleItemCommand = new AsyncRelayCommand<CartItem>(RemoveSaleItemAsync);

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

            ShowSetting = new AsyncRelayCommand(OpenSetting);

        SelectCategoryCommand = new AsyncRelayCommand<Category>(SelectParentCategoryAsync);

        SelectSubCategoryCommand = new AsyncRelayCommand<Category>(SelectSubCategoryAsync);

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

        StartDayCommand = new RelayCommand(
            ShowStartDayDialog,
            () => CanStartDay);

        EndDayCommand = new RelayCommand(
            ShowEndDayDialog,
            () => CanEndDay);

        ProductsView = CollectionViewSource.GetDefaultView(Products);
        ProductsView.Filter = FilterProduct;

        _ = InitializeAsync();

        SelectedCategory = null;
    }

    /// <summary>
    /// Sets the top-of-window error banner message. Pass
    /// <c>null</c> or empty to clear the banner.
    /// </summary>
    public void ShowHeaderError(string? message)
    {
        HeaderErrorMessage = string.IsNullOrWhiteSpace(message) ? null : message;
    }

    private void OnLocalizationLanguageChanged(object? sender, EventArgs e)
    {
        // The cart count badge binds to CartCountDisplay, which is a
        // computed string from ILocalizationService.GetString(...). The
        // underlying count (SaleItemsCount) hasn't changed, but the
        // resource value has, so raise PropertyChanged explicitly.
        OnPropertyChanged(nameof(CartCountDisplay));
    }
}
