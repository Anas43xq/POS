using BLL.Interfaces;
using Contracts.Enum;
using Contracts.Transactions;
using POS.Contracts.Receipts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public class ReceiptManagementViewModel : BaseViewModel
    {
        private readonly ITransactionService _transactionService;
        private readonly IReceiptDisplayService _receiptDisplayService;
        private readonly IPurchaseReceiptService _purchaseReceiptService;
        private readonly ISupplierService _supplierService;
        private readonly ExcelReportExporter _excelExporter;

        /// <summary>When true, property-change-triggered auto-loads are suppressed
        /// (used during programmatic refresh to avoid re-entrant cycles).</summary>
        private bool _isLoading;

        private string _searchText = string.Empty;
        private DateTime? _dateFrom;
        private DateTime? _dateTo;
        private string _categoryFilter = string.Empty;
        private SupplierDto? _selectedSupplier;
        private string _selectedDateRange = "Day";
        private string _selectedReceiptTypeFilter = "All";
        private string _statusMessage = string.Empty;
        private string _formTitle = "Add Purchase Receipt";

        private string _invoiceNumber = string.Empty;
        private string _category = string.Empty;
        private string _description = string.Empty;
        private string _notes = string.Empty;
        private string _imagePath = string.Empty;
        private DateTime _invoiceDate = DateTime.Today;
        private decimal _subtotal;
        private decimal _vatRate;
        private decimal _vatAmount;
        private decimal _grandTotal;
        private int? _editingReceiptId;
        private byte _activeReceiptTypeId = 1;


        public ReceiptManagementViewModel(
            ITransactionService transactionService,
            IReceiptDisplayService receiptDisplayService,
            IPurchaseReceiptService purchaseReceiptService,
            ISupplierService supplierService,
            ExcelReportExporter excelExporter)
        {
            _transactionService = transactionService;
            _receiptDisplayService = receiptDisplayService;
            _purchaseReceiptService = purchaseReceiptService;
            _supplierService = supplierService;
            _excelExporter = excelExporter;

            SalesReceipts = new ObservableCollection<TransactionListItemDto>();
            VatReceipts = new ObservableCollection<PurchaseReceiptDto>();
            NonVatReceipts = new ObservableCollection<PurchaseReceiptDto>();
            Suppliers = new ObservableCollection<SupplierDto>();

            RefreshCommand = new RelayCommand(_ => { _ = LoadAllAsync(); });
            ApplyFiltersCommand = new RelayCommand(_ => { _ = LoadAllAsync(); });
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());

            // Quick-range chips bound by the shared DateRangeFilterControl.
            // Day / Week / Month set the date range immediately; Period just
            // reveals the From / To pickers, the user then clicks Apply.
            LoadDayCommand = new RelayCommand(_ => { SelectedDateRange = "Day"; ApplyDateRange(DateTime.Today, DateTime.Today); });
            LoadWeekCommand = new RelayCommand(_ => { SelectedDateRange = "Week"; ApplyDateRange(StartOfWeek(DateTime.Today), DateTime.Today); });
            LoadMonthCommand = new RelayCommand(_ => { SelectedDateRange = "Month"; ApplyDateRange(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today); });
            LoadPeriodCommand = new RelayCommand(_ => { SelectedDateRange = "Period"; IsPeriodFilterVisible = true; });
            ApplyPeriodCommand = new RelayCommand(_ => { _ = LoadAllAsync(); });
            OpenSalesReceiptCommand = new AsyncRelayCommand(OpenSelectedSalesReceipt);
            AddVatReceiptCommand = new RelayCommand(_ => NavigateToForm(1, false));
            EditVatReceiptCommand = new RelayCommand(_ => NavigateToForm(1, true));
            DeleteVatReceiptCommand = new RelayCommand(_ => { _ = DeleteSelectedReceipt(1); });
            ViewVatReceiptCommand = new RelayCommand(_ => ViewSelectedReceipt(1));
            AddNonVatReceiptCommand = new RelayCommand(_ => NavigateToForm(2, false));
            EditNonVatReceiptCommand = new RelayCommand(_ => NavigateToForm(2, true));
            DeleteNonVatReceiptCommand = new RelayCommand(_ => { _ = DeleteSelectedReceipt(2); });
            ViewNonVatReceiptCommand = new RelayCommand(_ => ViewSelectedReceipt(2));
            SaveReceiptCommand = new RelayCommand(_ => { _ = SaveReceiptAsync(); });
            CancelReceiptCommand = new RelayCommand(_ => NavigateToList());
            ExportVatPurchasesCommand = new RelayCommand(_ => { _ = ExportVatPurchasesAsync(); });
            ExportNonVatPurchasesCommand = new RelayCommand(_ => { _ = ExportNonVatPurchasesAsync(); });

            _ = LoadAllAsync();
        }

        public event Action? NavigateToFormRequested;
        public event Action? NavigateToListRequested;

        public ObservableCollection<TransactionListItemDto> SalesReceipts { get; }
        public ObservableCollection<PurchaseReceiptDto> VatReceipts { get; }
        public ObservableCollection<PurchaseReceiptDto> NonVatReceipts { get; }
        public ObservableCollection<SupplierDto> Suppliers { get; }

        public ICommand RefreshCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand LoadDayCommand { get; }
        public ICommand LoadWeekCommand { get; }
        public ICommand LoadMonthCommand { get; }
        public ICommand LoadPeriodCommand { get; }
        public ICommand ApplyPeriodCommand { get; }
        public ICommand OpenSalesReceiptCommand { get; }
        public ICommand AddVatReceiptCommand { get; }
        public ICommand EditVatReceiptCommand { get; }
        public ICommand DeleteVatReceiptCommand { get; }
        public ICommand ViewVatReceiptCommand { get; }
        public ICommand AddNonVatReceiptCommand { get; }
        public ICommand EditNonVatReceiptCommand { get; }
        public ICommand DeleteNonVatReceiptCommand { get; }
        public ICommand ViewNonVatReceiptCommand { get; }
        public ICommand SaveReceiptCommand { get; }
        public ICommand CancelReceiptCommand { get; }
        public ICommand ExportVatPurchasesCommand { get; }
        public ICommand ExportNonVatPurchasesCommand { get; }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? string.Empty; OnPropertyChanged(); if (!_isLoading) _ = LoadAllAsync(); }
        }

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set { _dateFrom = value; OnPropertyChanged(); if (!_isLoading && IsPeriodFilterVisible) _ = LoadAllAsync(); }
        }

        public DateTime? DateTo
        {
            get => _dateTo;
            set { _dateTo = value; OnPropertyChanged(); if (!_isLoading && IsPeriodFilterVisible) _ = LoadAllAsync(); }
        }

        /// <summary>
        /// Whether the custom From / To date pickers are visible inside
        /// the shared <see cref="UI.Controls.DateRangeFilterControl"/>.
        /// Toggled by the Period chip on the filter bar.
        /// </summary>
        private bool _isPeriodFilterVisible;
        public bool IsPeriodFilterVisible
        {
            get => _isPeriodFilterVisible;
            private set
            {
                if (_isPeriodFilterVisible == value)
                    return;

                _isPeriodFilterVisible = value;
                OnPropertyChanged();
            }
        }

        public string CategoryFilter
        {
            get => _categoryFilter;
            set { _categoryFilter = value ?? string.Empty; OnPropertyChanged(); if (!_isLoading) _ = LoadAllAsync(); }
        }

        /// <summary>
        /// Tracks the currently active quick-range chip so the
        /// DateRangeFilterControl RadioButtons can reflect the
        /// correct checked state (Day / Week / Month / Period).
        /// </summary>
        public string SelectedDateRange
        {
            get => _selectedDateRange;
            set { _selectedDateRange = value ?? "Day"; OnPropertyChanged(); }
        }

        public SupplierDto? SelectedSupplier
        {
            get => _selectedSupplier;
            set { _selectedSupplier = value; OnPropertyChanged(); if (!_isLoading) _ = LoadAllAsync(); }
        }

        public string SelectedReceiptTypeFilter
        {
            get => _selectedReceiptTypeFilter;
            set { _selectedReceiptTypeFilter = value ?? "All"; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ReceiptTypeFilterOptions { get; } = new() { "All", "Sales", "VAT", "Non-VAT" };

        private TransactionListItemDto? _selectedSalesReceipt;
        public TransactionListItemDto? SelectedSalesReceipt
        {
            get => _selectedSalesReceipt;
            set { if (_selectedSalesReceipt == value) return; _selectedSalesReceipt = value; OnPropertyChanged(); }
        }

        private PurchaseReceiptDto? _selectedVatReceipt;
        public PurchaseReceiptDto? SelectedVatReceipt
        {
            get => _selectedVatReceipt;
            set
            {
                if (_selectedVatReceipt == value) return;
                _selectedVatReceipt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasVatSelection));
            }
        }

        private PurchaseReceiptDto? _selectedNonVatReceipt;
        public PurchaseReceiptDto? SelectedNonVatReceipt
        {
            get => _selectedNonVatReceipt;
            set
            {
                if (_selectedNonVatReceipt == value) return;
                _selectedNonVatReceipt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNonVatSelection));
            }
        }

        /// <summary>True when a VAT row is selected; used by the
        /// purchase-receipt toolbar to gate Edit / View / Delete.</summary>
        public bool HasVatSelection => SelectedVatReceipt is not null;

        /// <summary>True when a Non-VAT row is selected; used by the
        /// purchase-receipt toolbar to gate Edit / View / Delete.</summary>
        public bool HasNonVatSelection => SelectedNonVatReceipt is not null;

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string FormTitle
        {
            get => _formTitle;
            private set { _formTitle = value; OnPropertyChanged(); }
        }

        public bool IsFormEditable { get; private set; } = true;

        public bool IsVatReceiptMode => _activeReceiptTypeId == 1;
        public bool IsNonVatReceiptMode => _activeReceiptTypeId == 2;

        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set { _invoiceNumber = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string ImagePath
        {
            get => _imagePath;
            set { _imagePath = value ?? string.Empty; OnPropertyChanged(); }
        }

        public DateTime InvoiceDate
        {
            get => _invoiceDate;
            set { _invoiceDate = value; OnPropertyChanged(); }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set { _subtotal = value; OnPropertyChanged(); RecalculateTotals(); }
        }

        public decimal VatRate
        {
            get => _vatRate;
            set { _vatRate = value; OnPropertyChanged(); RecalculateTotals(); }
        }

        public decimal VatAmount
        {
            get => _vatAmount;
            private set { _vatAmount = value; OnPropertyChanged(); }
        }

        public decimal GrandTotal
        {
            get => _grandTotal;
            private set { _grandTotal = value; OnPropertyChanged(); }
        }

        private async Task LoadAllAsync()
        {
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                StatusMessage = string.Empty;
                await LoadSuppliersAsync();
                await LoadSalesReceiptsAsync();
                await LoadPurchaseReceiptsAsync();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadSalesReceiptsAsync()
        {
            if (!ShouldShowReceiptType("Sales"))
            {
                SalesReceipts.Clear();
                return;
            }

            try
            {
                if (DateFrom is null && DateTo is null)
                {
                    // No dates selected, use default period (Today) instead of Custom
                    var defaultRequest = new GetTransactionsListRequest
                    {
                        PageNumber = 1,
                        PageSize = 100
                    };
                    var defaultResult = await _transactionService.GetTransactionsListAsync(defaultRequest);
                    SalesReceipts.Clear();
                    foreach (var item in defaultResult.Items.Where(MatchesFilters))
                    {
                        SalesReceipts.Add(item);
                    }
                }
                else if (DateFrom is not null && DateTo is not null)
                {
                    // Both dates selected, use Custom period
                    var request = new GetTransactionsListRequest
                    {
                        PeriodType = "Custom",
                        FromDate = DateFrom,
                        ToDate = DateTo,
                        PageNumber = 1,
                        PageSize = 100
                    };
                    var result = await _transactionService.GetTransactionsListAsync(request);
                    SalesReceipts.Clear();
                    foreach (var item in result.Items.Where(MatchesFilters))
                    {
                        SalesReceipts.Add(item);
                    }
                }
                else
                {
                    // One date is null but not both - request both dates to use Custom period
                    StatusMessage = "Please select both From and To dates, or clear both to use default period.";
                    SalesReceipts.Clear();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private async Task LoadPurchaseReceiptsAsync()
        {
            if (!ShouldShowReceiptType("VAT") && !ShouldShowReceiptType("Non-VAT"))
            {
                VatReceipts.Clear();
                NonVatReceipts.Clear();
                return;
            }

            try
            {
                if (ShouldShowReceiptType("VAT"))
                {
                    var request = BuildPurchaseReceiptSearchRequest(1);
                    var vatResult = await _purchaseReceiptService.GetAllAsync(request);
                    VatReceipts.Clear();
                    if (vatResult.IsSuccess && vatResult.Value is not null)
                    {
                        foreach (var receipt in vatResult.Value.Where(MatchesFilters))
                        {
                            VatReceipts.Add(receipt);
                        }
                    }
                }
                else
                {
                    VatReceipts.Clear();
                }

                if (ShouldShowReceiptType("Non-VAT"))
                {
                    var request = BuildPurchaseReceiptSearchRequest(2);
                    var nonVatResult = await _purchaseReceiptService.GetAllAsync(request);
                    NonVatReceipts.Clear();
                    if (nonVatResult.IsSuccess && nonVatResult.Value is not null)
                    {
                        foreach (var receipt in nonVatResult.Value.Where(MatchesFilters))
                        {
                            NonVatReceipts.Add(receipt);
                        }
                    }
                }
                else
                {
                    NonVatReceipts.Clear();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private async Task LoadSuppliersAsync()
        {
            try
            {
                // Preserve the current selection across the reload so the
                // TwoWay binding on the Supplier ComboBox isn't destroyed
                // when Suppliers.Clear() removes the selected item.
                var savedSupplierId = SelectedSupplier?.SupplierId;

                var result = await _supplierService.GetAllAsync();
                Suppliers.Clear();
                if (result.IsSuccess && result.Value is not null)
                {
                    foreach (var supplier in result.Value)
                    {
                        Suppliers.Add(supplier);
                    }
                }

                // Restore the previously selected supplier, if it still exists.
                SelectedSupplier = savedSupplierId.HasValue
                    ? Suppliers.FirstOrDefault(s => s.SupplierId == savedSupplierId.Value)
                    : null;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void ResetFilters()
        {
            SearchText = string.Empty;
            DateFrom = DateTime.Today;
            DateTo = DateTime.Today;
            CategoryFilter = string.Empty;
            SelectedSupplier = null;
            SelectedReceiptTypeFilter = "All";
            IsPeriodFilterVisible = false;
            SelectedDateRange = "Day";
            _ = LoadAllAsync();
        }

        /// <summary>
        /// Sets the active date range and immediately reloads the
        /// visible records.  Used by the quick-range chips on the
        /// shared <see cref="UI.Controls.DateRangeFilterControl"/>.
        /// </summary>
        private void ApplyDateRange(DateTime from, DateTime to)
        {
            DateFrom = from;
            DateTo = to;
            _ = LoadAllAsync();
        }

        /// <summary>
        /// Returns the Monday on or before <paramref name="date"/>.
        /// The UI uses a Monday-anchored week to match the rest of
        /// the Manager module.
        /// </summary>
        private static DateTime StartOfWeek(DateTime date)
        {
            int diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return date.Date.AddDays(-diff);
        }

        private async Task OpenSelectedSalesReceipt()
        {
            if (SelectedSalesReceipt is null)
            {
                StatusMessage = "Select a sales receipt to open.";
                return;
            }

            await _receiptDisplayService.ShowReceiptAsync(SelectedSalesReceipt.TransactionId);
        }

        private void NavigateToForm(byte receiptTypeId, bool isEditing)
        {
            _activeReceiptTypeId = receiptTypeId;
            OnPropertyChanged(nameof(IsVatReceiptMode));
            OnPropertyChanged(nameof(IsNonVatReceiptMode));
            IsFormEditable = true;
            OnPropertyChanged(nameof(IsFormEditable));
            FormTitle = isEditing ? $"Edit {(receiptTypeId == 1 ? "VAT" : "Non-VAT")} Purchase Receipt" : $"Add {(receiptTypeId == 1 ? "VAT" : "Non-VAT")} Purchase Receipt";
            StatusMessage = string.Empty;
            _editingReceiptId = null;

            if (!isEditing)
            {
                PopulateForm(null, receiptTypeId);
                RecalculateTotals();
                NavigateToFormRequested?.Invoke();
                return;
            }

            var selectedReceipt = receiptTypeId == 1 ? SelectedVatReceipt : SelectedNonVatReceipt;
            if (selectedReceipt is null)
            {
                StatusMessage = "Select a receipt to edit.";
                return;
            }

            PopulateForm(selectedReceipt, receiptTypeId);
            _editingReceiptId = selectedReceipt.ReceiptId;
            RecalculateTotals();
            NavigateToFormRequested?.Invoke();
        }

        private void ViewSelectedReceipt(byte receiptTypeId)
        {
            _activeReceiptTypeId = receiptTypeId;
            var selectedReceipt = receiptTypeId == 1 ? SelectedVatReceipt : SelectedNonVatReceipt;
            if (selectedReceipt is null)
            {
                StatusMessage = "Select a receipt to view.";
                return;
            }

            PopulateForm(selectedReceipt, receiptTypeId);
            IsFormEditable = false;
            OnPropertyChanged(nameof(IsFormEditable));
            FormTitle = $"View {(receiptTypeId == 1 ? "VAT" : "Non-VAT")} Purchase Receipt";
            StatusMessage = string.Empty;
            RecalculateTotals();
            NavigateToFormRequested?.Invoke();
        }

        private void NavigateToList()
        {
            FormTitle = "Add Purchase Receipt";
            StatusMessage = string.Empty;
            _editingReceiptId = null;
            OnPropertyChanged(nameof(IsVatReceiptMode));
            OnPropertyChanged(nameof(IsNonVatReceiptMode));
            NavigateToListRequested?.Invoke();
        }

        private void PopulateForm(PurchaseReceiptDto? receipt, byte receiptTypeId)
        {
            if (receipt is null)
            {
                InvoiceNumber = string.Empty;
                Category = string.Empty;
                Description = string.Empty;
                Notes = string.Empty;
                ImagePath = string.Empty;
                InvoiceDate = DateTime.Today;
                Subtotal = 0m;
                VatRate = 0m;
                VatAmount = 0m;
                GrandTotal = 0m;
                SelectedSupplier = null;
                return;
            }

            InvoiceNumber = receipt.InvoiceNumber;
            Category = receipt.Category;
            Description = receipt.Description ?? string.Empty;
            Notes = receipt.Notes ?? string.Empty;
            ImagePath = receipt.ImagePath ?? string.Empty;
            InvoiceDate = receipt.InvoiceDate;
            Subtotal = receipt.Subtotal;
            VatRate = receipt.VatRate;
            VatAmount = receipt.VatAmount;
            GrandTotal = receipt.GrandTotal;
            SelectedSupplier = receipt.SupplierId is null ? null : Suppliers.FirstOrDefault(s => s.SupplierId == receipt.SupplierId);
        }

        private async Task SaveReceiptAsync()
        {
            if (!ValidateForm())
                return;

            try
            {
                var vatRate = IsVatReceiptMode ? VatRate : 0m;
                var vatAmount = IsVatReceiptMode ? VatAmount : 0m;
                var grandTotal = IsVatReceiptMode ? GrandTotal : Subtotal;

                if (_editingReceiptId.HasValue)
                {
                    var request = new UpdatePurchaseReceiptRequest
                    {
                        ReceiptId = _editingReceiptId.Value,
                        ReceiptTypeId = _activeReceiptTypeId,
                        SupplierId = SelectedSupplier?.SupplierId,
                        InvoiceNumber = InvoiceNumber,
                        InvoiceDate = InvoiceDate,
                        Category = Category,
                        Description = Description,
                        Subtotal = Subtotal,
                        VatRate = vatRate,
                        VatAmount = vatAmount,
                        GrandTotal = grandTotal,
                        Notes = Notes,
                        ImagePath = ImagePath
                    };

                    var result = await _purchaseReceiptService.UpdateAsync(_editingReceiptId.Value, request);
                    if (!result.IsSuccess)
                    {
                        StatusMessage = result.Error ?? "Unable to update receipt.";
                        return;
                    }
                }
                else
                {
                    var request = new CreatePurchaseReceiptRequest
                    {
                        ReceiptTypeId = _activeReceiptTypeId,
                        SupplierId = SelectedSupplier?.SupplierId,
                        InvoiceNumber = InvoiceNumber,
                        InvoiceDate = InvoiceDate,
                        Category = Category,
                        Description = Description,
                        Subtotal = Subtotal,
                        VatRate = VatRate,
                        VatAmount = VatAmount,
                        GrandTotal = GrandTotal,
                        Notes = Notes,
                        ImagePath = ImagePath,
                        CreatedBy = 1
                    };

                    var result = await _purchaseReceiptService.CreateAsync(request);
                    if (!result.IsSuccess)
                    {
                        StatusMessage = result.Error ?? "Unable to create receipt.";
                        return;
                    }
                }

                StatusMessage = string.Empty;
                NavigateToList();
                await LoadPurchaseReceiptsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(InvoiceNumber))
            {
                StatusMessage = "Receipt number is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                StatusMessage = "Category is required.";
                return false;
            }

            if (Subtotal <= 0)
            {
                StatusMessage = "Amount must be greater than zero.";
                return false;
            }

            if (IsVatReceiptMode && VatRate < 0)
            {
                StatusMessage = "VAT rate cannot be negative.";
                return false;
            }

            if (IsVatReceiptMode && GrandTotal < Subtotal)
            {
                StatusMessage = "Grand total must be greater than or equal to subtotal.";
                return false;
            }

            StatusMessage = string.Empty;
            return true;
        }

        private async Task DeleteSelectedReceipt(byte receiptTypeId)
        {
            var selectedReceipt = receiptTypeId == 1 ? SelectedVatReceipt : SelectedNonVatReceipt;
            if (selectedReceipt is null)
            {
                StatusMessage = "Select a receipt to delete.";
                return;
            }

            var confirm = MessageBox.Show($"Delete receipt {selectedReceipt.InvoiceNumber}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                var result = await _purchaseReceiptService.DeleteAsync(selectedReceipt.ReceiptId);
                if (!result.IsSuccess)
                {
                    StatusMessage = result.Error ?? "Unable to delete receipt.";
                    return;
                }

                StatusMessage = string.Empty;
                await LoadPurchaseReceiptsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void RecalculateTotals()
        {
            VatAmount = IsVatReceiptMode ? Subtotal * VatRate / 100m : 0m;
            GrandTotal = IsVatReceiptMode ? Subtotal + VatAmount : Subtotal;
        }

        private async Task ExportVatPurchasesAsync()
        {
            if (VatReceipts.Count == 0)
            {
                StatusMessage = "No VAT purchase receipts match the current filters.";
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"VAT_Purchase_Register_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                var request = new ExcelReportRequest
                {
                    ReportType = ReportType.VatPurchaseRegister,
                    Title = "UAE PURCHASE VAT REGISTER",
                    FromDate = DateFrom ?? DateTime.Today,
                    ToDate = DateTo ?? DateTime.Today,
                    Data = VatReceipts.ToList()
                };

                var bytes = _excelExporter.Export(request);
                await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                // Never fail silently: surface the error to the UI.
                StatusMessage = $"VAT register export failed: {ex.Message}";
            }
        }

        private async Task ExportNonVatPurchasesAsync()
        {
            if (NonVatReceipts.Count == 0)
            {
                StatusMessage = "No non-VAT purchase receipts match the current filters.";
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Business_Expense_Register_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                var request = new ExcelReportRequest
                {
                    ReportType = ReportType.NonVatPurchaseRegister,
                    Title = "BUSINESS EXPENSE REGISTER",
                    FromDate = DateFrom ?? DateTime.Today,
                    ToDate = DateTo ?? DateTime.Today,
                    Data = NonVatReceipts.ToList()
                };

                var bytes = _excelExporter.Export(request);
                await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                // Never fail silently: surface the error to the UI.
                StatusMessage = $"Non-VAT register export failed: {ex.Message}";
            }
        }

        private bool ShouldShowReceiptType(string receiptType)
        {
            return SelectedReceiptTypeFilter is "All" or "Sales" or "VAT" or "Non-VAT"
                && (SelectedReceiptTypeFilter == "All" || string.Equals(SelectedReceiptTypeFilter, receiptType, StringComparison.OrdinalIgnoreCase));
        }

        private PurchaseReceiptSearchRequest BuildPurchaseReceiptSearchRequest(byte receiptTypeId)
        {
            return new PurchaseReceiptSearchRequest
            {
                SearchText = SearchText,
                FromDate = DateFrom,
                ToDate = DateTo,
                SupplierId = SelectedSupplier?.SupplierId,
                Category = string.IsNullOrWhiteSpace(CategoryFilter) ? null : CategoryFilter.Trim(),
                ReceiptTypeId = receiptTypeId
            };
        }

        private bool MatchesFilters(TransactionListItemDto transaction)
        {
            return MatchesFilters(
                transaction.ReceiptNumber,
                transaction.TransactionDate,
                category: null,
                supplierName: null,
                searchText: transaction.ReceiptNumber + " " + transaction.PaymentMethod + " " + (transaction.Status ?? string.Empty) + " " + (transaction.Notes ?? string.Empty));
        }

        private bool MatchesFilters(PurchaseReceiptDto receipt)
        {
            return MatchesFilters(
                receipt.InvoiceNumber,
                receipt.InvoiceDate,
                receipt.Category,
                receipt.SupplierName,
                receipt.InvoiceNumber + " " + receipt.Category + " " + receipt.Description + " " + receipt.Notes + " " + receipt.SupplierName);
        }

        private bool MatchesFilters(string? receiptNumber, DateTime? receiptDate, string? category, string? supplierName, string? searchText)
        {
            var normalizedSearch = SearchText?.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                var search = searchText ?? string.Empty;
                if (!search.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            if (DateFrom.HasValue && receiptDate is not null && receiptDate.Value.Date < DateFrom.Value.Date)
            {
                return false;
            }

            if (DateTo.HasValue && receiptDate is not null && receiptDate.Value.Date > DateTo.Value.Date)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                if (string.IsNullOrWhiteSpace(category) || !category.Contains(CategoryFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            if (SelectedSupplier is not null)
            {
                if (string.IsNullOrWhiteSpace(supplierName) || !string.Equals(supplierName, SelectedSupplier.CompanyName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}