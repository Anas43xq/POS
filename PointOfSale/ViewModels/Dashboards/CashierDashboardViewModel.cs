using BLL.Interfaces;
using BLL.Models;
using Contracts.Sales;
using Contracts.Transactions;
using DAL.Entities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.Windows;
using System.Windows.Input;
using UI.Commands;
using UI.Services;
using UI.Views;

namespace UI.ViewModels
{
    public class CashierDashboardViewModel : BaseViewModel
    {
        private readonly ISessionService _session;
        private readonly IShiftService _shiftService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IRecentSaleService _recentSale;
        private readonly IDialogService _dialogService;
        private readonly ITransactionService _transactionService;
        private readonly IReceiptDisplayService _receiptDisplayService;

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

        public ObservableCollection<RecentTransactionDto> RecentSales { get; } = new();

        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();

                UpdateSubCategories();
                OnPropertyChanged(nameof(HasSubCategories));
                ProductsView.Refresh();
                UpdateNoProductsMessage();
            }
        }

        public bool HasSubCategories => SubCategories.Any();

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

        public CashierDashboardViewModel(
            ISessionService session,
            IShiftService shiftService,
            IProductService productService,
            ICategoryService categoryService,
            IRecentSaleService recentSale,
            IDialogService dialogService,
            ITransactionService transactionService,
            IReceiptDisplayService receiptDisplayService)
        {
            _session = session;
            _shiftService = shiftService;
            _productService = productService;
            _categoryService = categoryService;
            _recentSale = recentSale;
            _dialogService = dialogService;
            _transactionService = transactionService;
            _receiptDisplayService = receiptDisplayService;

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

            SelectCategoryCommand = new AsyncRelayCommand<Category>(SelectCategoryAsync);

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
        /// Shows the Start Day dialog.
        /// </summary>
        private void ShowStartDayDialog()
        {
            var viewModel = new StartDayDialogViewModel(_shiftService, _session);
            _dialogService.ShowDialog<StartDayDialog>(viewModel);
            
            // Refresh shift status after dialog closes
            LoadTopBar();
            RefreshCommandStates();
        }



        /// <summary>
        /// Shows the End Day dialog.
        /// </summary>
        private void ShowEndDayDialog()
        {
            var viewModel = new EndDayDialogViewModel(_shiftService, _session);
            _dialogService.ShowDialog<EndDayDialog>(viewModel);
            
            // Refresh shift status and clear sale after dialog closes
            LoadTopBar();
            RefreshCommandStates();
            
            if (_session.CurrentShift == null)
            {
                SaleItems.Clear();
                RecentSales.Clear();
            }
        }

        /// <summary>
        /// Refreshes the can-execute state of all shift-related commands.
        /// </summary>
        private void RefreshCommandStates()
        {
            ((RelayCommand)StartDayCommand).RaiseCanExecuteChanged();
            ((RelayCommand)EndDayCommand).RaiseCanExecuteChanged();
            PayCashCommand.RaiseCanExecuteChanged();
            PayCardCommand.RaiseCanExecuteChanged();
            if (AddProductCommand is AsyncRelayCommand addProductCmd)
            {
                addProductCmd.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Loads initial dashboard data and shift information.
        /// </summary>
        private async Task InitializeAsync()
        {
            LoadTopBar();

            await LoadCategoriesAsync();
            await LoadProductsAsync();
            await LoadRecentSalesAsync();
            ProductsView.Refresh();
            UpdateNoProductsMessage();
        }

        /// <summary>
        /// Loads and displays the current shift status.
        /// </summary>
        private void LoadTopBar()
        {
            CashierName = _session.CurrentUser?.FullName ?? "Unknown Cashier";

            if (_session.CurrentShift != null && _session.CurrentShift.Status == DAL.Entities.ShiftStatus.Open)
            {
                ShiftStatus = $"Shift Open ({_session.CurrentShift.OpeningCash:C2})";
            }
            else if (_session.CurrentShift != null)
            {
                ShiftStatus = "Shift Closed";
            }
            else
            {
                ShiftStatus = "No Shift";
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                Result<List<Category>> result =
                    await _categoryService.GetAllCategoriesWithChildrenAsync();

                Categories.Clear();
                SubCategories.Clear();

                Categories.Add(new Category
                {
                    CategoryId = 0,
                    Name = "All"
                });

                if (result.IsSuccess && result.Value != null)
                {
                    foreach (Category category in result.Value.Where(c => !c.ParentCategoryId.HasValue))
                    {
                        Categories.Add(category);
                    }
                }

                SelectedCategory = Categories.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    private async Task LoadProductsAsync()
        {
            try
            {
                Result<List<Product>> result =
                    await _productService.GetAllProductsWithTaxRateAsync();

                Products.Clear();

                if (result.IsSuccess && result.Value != null)
                {
                    foreach (Product product in result.Value)
                    {
                        Products.Add(product);
                    }
                }

                ProductsView.Refresh();
                UpdateNoProductsMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task LoadRecentSalesAsync()
        {
            RecentSales.Clear();

            if (_session.CurrentUser == null || _session.CurrentShift == null)
                return;

            try
            {
                List<RecentTransactionDto> result =
                    await _recentSale.GetRecentTransactionsByCashierId(
                        _session.CurrentUser.UserId,
                        _session.CurrentShift.ShiftId);

                if (result != null)
                {
                    foreach (RecentTransactionDto recentSale in result)
                    {
                        RecentSales.Add(recentSale);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task SelectCategoryAsync(Category? category)
        {
            if (category == null)
                return;

            SelectedCategory = category;
            ProductsView.Refresh();
            UpdateNoProductsMessage();

            await Task.CompletedTask;
        }

        private async Task SelectSubCategoryAsync(Category? subCategory)
        {
            if (subCategory == null)
                return;

            SelectedCategory = subCategory;
            ProductsView.Refresh();
            UpdateNoProductsMessage();

            await Task.CompletedTask;
        }

        private void UpdateSubCategories()
        {
            SubCategories.Clear();

            if (SelectedCategory == null || SelectedCategory.CategoryId == 0)
                return;

            if (SelectedCategory.ChildCategories != null && SelectedCategory.ChildCategories.Any())
            {
                foreach (Category child in SelectedCategory.ChildCategories)
                    SubCategories.Add(child);
            }
            else if (SelectedCategory.ParentCategory != null)
            {
                foreach (Category sibling in SelectedCategory.ParentCategory.ChildCategories)
                {
                    if (sibling.CategoryId != SelectedCategory.CategoryId)
                        SubCategories.Add(sibling);
                }
            }
        }

        private bool FilterProduct(object item)
        {
            if (item is not Product product)
                return false;

            if (!IsProductInSelectedCategory(product))
                return false;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim();
                if (!product.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsProductInSelectedCategory(Product product)
        {
            if (SelectedCategory == null || SelectedCategory.CategoryId == 0)
                return true;

            if (product.CategoryId == SelectedCategory.CategoryId)
                return true;

            if (SelectedCategory.ChildCategories != null && SelectedCategory.ChildCategories.Any())
            {
                return SelectedCategory.ChildCategories.Any(child => child.CategoryId == product.CategoryId);
            }

            return SelectedCategory.ParentCategory != null &&
                   SelectedCategory.ParentCategory.ChildCategories.Any(child => child.CategoryId == product.CategoryId);
        }

        private void UpdateNoProductsMessage()
        {
            ShowNoProductsMessage = ProductsView.IsEmpty && !string.IsNullOrWhiteSpace(SearchText);
        }

        private void SaleItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CartItem item in e.NewItems)
                {
                    item.PropertyChanged += CartItem_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CartItem item in e.OldItems)
                    item.PropertyChanged -= CartItem_PropertyChanged;
            }

            RefreshTotals();
            OnPropertyChanged(nameof(SaleItemsCount));
            ClearSaleCommand.RaiseCanExecuteChanged();
            PayCashCommand.RaiseCanExecuteChanged();
            PayCardCommand.RaiseCanExecuteChanged();
            IncreaseSelectedQuantityCommand.RaiseCanExecuteChanged();
            DecreaseSelectedQuantityCommand.RaiseCanExecuteChanged();
            RemoveSelectedSaleItemCommand.RaiseCanExecuteChanged();
        }

        private void CartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItem.Quantity))
            {
                RefreshTotals();
            }
        }

        private async Task AddProductAsync(Product? product)
        {
            if (product == null)
                return;

            CartItem? existingItem = SaleItems
                .FirstOrDefault(item => item.ProductId == product.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                CartItem item = new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Quantity = 1,
                    UnitPrice = product.UnitPrice,
                    TaxRate = product.TaxRate?.Rate ?? 0m
                };
                SaleItems.Add(item);
            }

            await Task.CompletedTask;
        }

        private async Task RemoveSaleItemAsync(CartItem? item)
        {
            if (item == null)
                return;

            SaleItems.Remove(item);

            await Task.CompletedTask;
        }

        private void ClearSales()
        {
            if (!SaleItems.Any())
                return;

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to clear the current sale?",
                "Confirm Clear Sale",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            SaleItems.Clear();
        }

        private void RefreshTotals()
        {
            Subtotal = SaleItems.Sum(item => item.LineSubtotal);
            Tax = SaleItems.Sum(item => item.LineTax);
            Total = Subtotal + Tax;
        }

        /// <summary>
        /// Starts the payment flow and opens the payment dialog.
        /// </summary>
        private async Task PayCashAsync()
        {
            if (!TryValidatePaymentPrerequisites())
                return;

            await ShowPaymentDialogAsync();
        }

        private async Task PayCardAsync()
        {
            if (!TryValidatePaymentPrerequisites())
                return;

            var confirmViewModel = new CardPaymentConfirmDialogViewModel(Total);
            bool? confirmed = _dialogService.ShowDialogWithResult<CardPaymentConfirmDialog>(confirmViewModel);

            if (confirmed != true)
                return;

            await CreateCardPaymentAsync();
        }

        private async Task ShowPaymentDialogAsync()
        {
            var paymentViewModel = new PaymentDialogViewModel(
                Total,
                this,
                _transactionService,
                _session);

            paymentViewModel.PaymentCompleted += async (transactionId) =>
            {
                ClearCurrentSale();
                await LoadRecentSalesAsync();
                _receiptDisplayService.ShowReceipt(transactionId);
            };

            _dialogService.ShowDialog<PaymentDialog>(paymentViewModel);
        }

        private async Task CreateCardPaymentAsync()
        {
            int transactionId = await _transactionService.CreateTransactionAsync(
                BuildCreatePaymentRequest(
                    paymentMethod: "Card",
                    amountTendered: Total,
                    changeGiven: 0m,
                    referenceNumber: null));

            ClearCurrentSale();
            await LoadRecentSalesAsync();
            _receiptDisplayService.ShowReceipt(transactionId);
        }

        private bool TryValidatePaymentPrerequisites()
        {
            if (_session.CurrentUser == null)
                return ShowPrerequisiteError("No active user. Please sign in first.", "User Required");

            if (!IsShiftOpen)
                return ShowPrerequisiteError("No active shift. Please start a shift first.", "Shift Required");

            if (!SaleItems.Any())
                return ShowPrerequisiteError("Cart is empty.", "Sale Required");

            return true;
        }

        private static bool ShowPrerequisiteError(string message, string title)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        private CreateTransactionRequest BuildCreatePaymentRequest(
            string paymentMethod,
            decimal amountTendered,
            decimal changeGiven,
            string? referenceNumber)
        {
            return new CreateTransactionRequest
            {
                CashierId = _session.CurrentUser!.UserId,
                ShiftId = _session.CurrentShift!.ShiftId,
                Subtotal = Subtotal,
                TaxTotal = Tax,
                GrandTotal = Total,
                PaymentMethod = paymentMethod,
                AmountTendered = amountTendered,
                ChangeGiven = changeGiven,
                ReferenceNumber = referenceNumber,
                Items = BuildTransactionItems()
            };
        }

        private List<CreateTransactionItemRequest> BuildTransactionItems()
        {
            return SaleItems
                .Select(item => new CreateTransactionItemRequest
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TaxRate = item.TaxRate,
                    LineSubtotal = item.LineSubtotal,
                    LineTax = item.LineTax,
                    LineTotal = item.LineTotal
                })
                .ToList();
        }

        private async Task ShowRecentSalesAsync()
        {
            await LoadRecentSalesAsync();

            var recentSalesViewModel = new RecentSalesDialogViewModel(this, _receiptDisplayService);

            _dialogService.ShowDialog<RecentSalesDialog>(recentSalesViewModel);
        }


        private void LogoutAsync()
        {
            LogoutRequested?.Invoke();

        }

        public void ClearCurrentSale()
        {
            SaleItems.Clear();
        }

        private void IncreaseSelectedQuantity()
        {
            if (SelectedCartItem == null)
                return;

            SelectedCartItem.Quantity++;
            RefreshTotals();
        }

        private void DecreaseSelectedQuantity()
        {
            if (SelectedCartItem == null)
                return;

            if (SelectedCartItem.Quantity <= 1)
            {
                RemoveSelectedSaleItem();
                return;
            }

            SelectedCartItem.Quantity--;
            RefreshTotals();
        }

        private void RemoveSelectedSaleItem()
        {
            if (SelectedCartItem == null)
                return;

            SaleItems.Remove(SelectedCartItem);
            SelectedCartItem = SaleItems.LastOrDefault();
        }
    }
}
