using BLL.Interfaces;
using Contracts.Enum;
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
        private readonly IPurchaseReceiptService _purchaseReceiptService;
        private readonly ISupplierService _supplierService;
        private readonly ExcelReportExporter _excelExporter;
        private readonly ILocalizationService _localizationService;

        /// <summary>
        /// Sentinel item shown at the top of the Supplier filter ComboBox
        /// meaning "no supplier filter applied". SupplierId 0 is reserved
        /// for this purpose since real suppliers use an identity column
        /// starting at 1.
        /// </summary>
        private readonly SupplierDto _allSuppliersOption;

        /// <summary>When true, property-change-triggered auto-loads are suppressed
        /// (used during programmatic refresh to avoid re-entrant cycles).</summary>
        private bool _isLoading;

        private string _searchText = string.Empty;
        private DateTime? _dateFrom;
        private DateTime? _dateTo;
        private string _categoryFilter = string.Empty;
        private SupplierDto? _selectedSupplier;
        private string _selectedDateRange = "Day";
        private string _statusMessage = string.Empty;
        private string _formTitle = string.Empty;

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
        private bool _hasLoadedOnce;


        public ReceiptManagementViewModel(
            IPurchaseReceiptService purchaseReceiptService,
            ISupplierService supplierService,
            ExcelReportExporter excelExporter,
            ILocalizationService localizationService)
        {
            _purchaseReceiptService = purchaseReceiptService;
            _supplierService = supplierService;
            _excelExporter = excelExporter;
            _localizationService = localizationService;

            _allSuppliersOption = new SupplierDto
            {
                SupplierId = 0,
                CompanyName = _localizationService.GetString("Manager.Receipt.AllSuppliers")
            };
            _formTitle = _localizationService.GetString("Manager.Receipt.FormTitleAddPlain");

            ActiveReceipts = new ObservableCollection<PurchaseReceiptDto>();
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
            SwitchToVatModeCommand = new RelayCommand(_ => SetActiveReceiptType(1));
            SwitchToNonVatModeCommand = new RelayCommand(_ => SetActiveReceiptType(2));
            AddReceiptCommand = new RelayCommand(_ => NavigateToForm(false));
            EditReceiptCommand = new RelayCommand(_ => NavigateToForm(true));
            DeleteReceiptCommand = new RelayCommand(_ => { _ = DeleteSelectedReceipt(); });
            ViewReceiptCommand = new RelayCommand(_ => ViewSelectedReceipt());
            SaveReceiptCommand = new RelayCommand(_ => { _ = SaveReceiptAsync(); });
            CancelReceiptCommand = new RelayCommand(_ => NavigateToList());
            ExportReceiptsCommand = new RelayCommand(_ => { _ = ExportActiveReceiptsAsync(); });
            CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());

            // NOTE: Data is intentionally NOT loaded here — see
            // ProductManagementViewModel for the rationale. Load is triggered
            // on first navigation via EnsureDataLoadedAsync(), called from
            // ManagerMainViewModel.NavigateToReceiptManagement().
        }

        public event Action? NavigateToFormRequested;
        public event Action? NavigateToListRequested;
        public event Action? CloseRequested;

        public ObservableCollection<PurchaseReceiptDto> ActiveReceipts { get; }
        public ObservableCollection<SupplierDto> Suppliers { get; }

        public ICommand RefreshCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand LoadDayCommand { get; }
        public ICommand LoadWeekCommand { get; }
        public ICommand LoadMonthCommand { get; }
        public ICommand LoadPeriodCommand { get; }
        public ICommand ApplyPeriodCommand { get; }
        public ICommand SwitchToVatModeCommand { get; }
        public ICommand SwitchToNonVatModeCommand { get; }
        public ICommand AddReceiptCommand { get; }
        public ICommand EditReceiptCommand { get; }
        public ICommand DeleteReceiptCommand { get; }
        public ICommand ViewReceiptCommand { get; }
        public ICommand SaveReceiptCommand { get; }
        public ICommand CancelReceiptCommand { get; }
        public ICommand ExportReceiptsCommand { get; }
        public ICommand CloseCommand { get; }

        /// <summary>
        /// Loads data the first time this page is navigated to; subsequent
        /// navigations are no-ops (use RefreshCommand to force a reload).
        /// </summary>
        public Task EnsureDataLoadedAsync()
        {
            if (_hasLoadedOnce)
                return Task.CompletedTask;

            _hasLoadedOnce = true;
            return LoadAllAsync();
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? string.Empty; OnPropertyChanged(); if (!_isLoading) _ = LoadAllAsync(); }
        }

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set { _dateFrom = value; OnPropertyChanged(); if (!_isLoading) _ = LoadAllAsync(); }
        }

        public DateTime? DateTo
        {
            get => _dateTo;
            set { _dateTo = value; OnPropertyChanged(); if (!_isLoading) _ = LoadAllAsync(); }
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

        private PurchaseReceiptDto? _selectedReceipt;
        public PurchaseReceiptDto? SelectedReceipt
        {
            get => _selectedReceipt;
            set
            {
                if (_selectedReceipt == value) return;
                _selectedReceipt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
            }
        }

        /// <summary>True when a row is selected; used by the
        /// purchase-receipt toolbar to gate Edit / View / Delete.</summary>
        public bool HasSelection => SelectedReceipt is not null;

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
                await LoadActiveReceiptsAsync();
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Switches the VAT / Non-VAT mode toggle and reloads — one query
        /// per toggle rather than holding both collections in memory.
        /// </summary>
        private void SetActiveReceiptType(byte receiptTypeId)
        {
            if (_activeReceiptTypeId == receiptTypeId)
                return;

            _activeReceiptTypeId = receiptTypeId;
            OnPropertyChanged(nameof(IsVatReceiptMode));
            OnPropertyChanged(nameof(IsNonVatReceiptMode));
            SelectedReceipt = null;
            _ = LoadActiveReceiptsAsync();
        }

        private async Task LoadActiveReceiptsAsync()
        {
            try
            {
                var request = BuildPurchaseReceiptSearchRequest(_activeReceiptTypeId);
                var result = await _purchaseReceiptService.GetAllAsync(request);
                ActiveReceipts.Clear();
                if (result.IsSuccess && result.Value is not null)
                {
                    foreach (var receipt in result.Value.Where(MatchesFilters))
                    {
                        ActiveReceipts.Add(receipt);
                    }
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
                var savedSupplierId = SelectedSupplier?.SupplierId ?? _allSuppliersOption.SupplierId;

                var result = await _supplierService.GetAllAsync();
                Suppliers.Clear();
                Suppliers.Add(_allSuppliersOption);
                if (result.IsSuccess && result.Value is not null)
                {
                    foreach (var supplier in result.Value)
                    {
                        Suppliers.Add(supplier);
                    }
                }

                // Restore the previously selected supplier, if it still
                // exists; otherwise fall back to "All Suppliers" rather
                // than null so the ComboBox always shows a valid item.
                SelectedSupplier = Suppliers.FirstOrDefault(s => s.SupplierId == savedSupplierId)
                    ?? _allSuppliersOption;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void ResetFilters()
        {
            // Suppress property-change-triggered auto-loads while we reset the
            // filter fields programmatically. Otherwise the first setter (e.g.
            // SelectedSupplier) synchronously kicks off LoadAllAsync, which
            // captures the *old* supplier selection before this method has
            // reset it, and LoadSuppliersAsync then restores that stale
            // selection — so Reset would not leave the Supplier filter on
            // "All Suppliers". With the guard on, SelectedSupplier becomes
            // the "All Suppliers" sentinel first, then a single explicit
            // reload runs once below.
            _isLoading = true;
            try
            {
                SearchText = string.Empty;
                DateFrom = DateTime.Today;
                DateTo = DateTime.Today;
                CategoryFilter = string.Empty;
                SelectedSupplier = _allSuppliersOption;
                IsPeriodFilterVisible = false;
                SelectedDateRange = "Day";
            }
            finally
            {
                _isLoading = false;
            }
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

        private void NavigateToForm(bool isEditing)
        {
            OnPropertyChanged(nameof(IsVatReceiptMode));
            OnPropertyChanged(nameof(IsNonVatReceiptMode));
            IsFormEditable = true;
            OnPropertyChanged(nameof(IsFormEditable));
            FormTitle = isEditing
                ? _localizationService.GetString("Manager.Receipt.FormTitleEdit", IsVatReceiptMode ? "VAT" : "Non-VAT")
                : _localizationService.GetString("Manager.Receipt.FormTitleAdd", IsVatReceiptMode ? "VAT" : "Non-VAT");
            StatusMessage = string.Empty;
            _editingReceiptId = null;

            if (!isEditing)
            {
                PopulateForm(null);
                RecalculateTotals();
                NavigateToFormRequested?.Invoke();
                return;
            }

            if (SelectedReceipt is null)
            {
                StatusMessage = _localizationService.GetString("Manager.Receipt.SelectReceiptToEdit");
                return;
            }

            PopulateForm(SelectedReceipt);
            _editingReceiptId = SelectedReceipt.ReceiptId;
            RecalculateTotals();
            NavigateToFormRequested?.Invoke();
        }

        private void ViewSelectedReceipt()
        {
            if (SelectedReceipt is null)
            {
                StatusMessage = _localizationService.GetString("Manager.Receipt.SelectReceiptToView");
                return;
            }

            PopulateForm(SelectedReceipt);
            IsFormEditable = false;
            OnPropertyChanged(nameof(IsFormEditable));
            FormTitle = _localizationService.GetString("Manager.Receipt.FormTitleView", IsVatReceiptMode ? "VAT" : "Non-VAT");
            StatusMessage = string.Empty;
            RecalculateTotals();
            NavigateToFormRequested?.Invoke();
        }

        private void NavigateToList()
        {
            FormTitle = _localizationService.GetString("Manager.Receipt.FormTitleAddPlain");
            StatusMessage = string.Empty;
            _editingReceiptId = null;
            OnPropertyChanged(nameof(IsVatReceiptMode));
            OnPropertyChanged(nameof(IsNonVatReceiptMode));
            NavigateToListRequested?.Invoke();
        }

        private void PopulateForm(PurchaseReceiptDto? receipt)
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
                StatusMessage = result.Error ?? _localizationService.GetString("Manager.Receipt.UpdateFailed");
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
                StatusMessage = result.Error ?? _localizationService.GetString("Manager.Receipt.CreateFailed");
                return;
            }
        }

        StatusMessage = string.Empty;
        // Refresh grid before returning to list to avoid showing stale rows
        await LoadActiveReceiptsAsync();
        NavigateToList();
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
                StatusMessage = _localizationService.GetString("Manager.Receipt.NumberRequired");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                StatusMessage = _localizationService.GetString("Manager.Receipt.CategoryRequired");
                return false;
            }

            if (Subtotal <= 0)
            {
                StatusMessage = _localizationService.GetString("Manager.Receipt.AmountGreaterThanZero");
                return false;
            }

            if (IsVatReceiptMode && VatRate < 0)
            {
                StatusMessage = _localizationService.GetString("Manager.Receipt.VatRateNegative");
                return false;
            }

            if (IsVatReceiptMode && GrandTotal < Subtotal)
            {
                StatusMessage = _localizationService.GetString("Manager.Receipt.GrandTotalLessThanSubtotal");
                return false;
            }

            StatusMessage = string.Empty;
            return true;
        }

        private async Task DeleteSelectedReceipt()
        {
            if (SelectedReceipt is null)
            {
                StatusMessage = _localizationService.GetString("Manager.Receipt.SelectReceiptToDelete");
                return;
            }

            var confirm = MessageBox.Show($"Delete receipt {SelectedReceipt.InvoiceNumber}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                var result = await _purchaseReceiptService.DeleteAsync(SelectedReceipt.ReceiptId);
                if (!result.IsSuccess)
                {
                    StatusMessage = result.Error ?? _localizationService.GetString("Manager.Receipt.DeleteFailed");
                    return;
                }

                StatusMessage = string.Empty;
                await LoadActiveReceiptsAsync();
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

        private async Task ExportActiveReceiptsAsync()
        {
            if (ActiveReceipts.Count == 0)
            {
                StatusMessage = _localizationService.GetString(IsVatReceiptMode ? "Manager.Receipt.NoVatMatch" : "Manager.Receipt.NoNonVatMatch");
                return;
            }

            var filePrefix = IsVatReceiptMode ? "VAT_Purchase_Register" : "Business_Expense_Register";
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"{filePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                var request = new ExcelReportRequest
                {
                    ReportType = IsVatReceiptMode ? ReportType.VatPurchaseRegister : ReportType.NonVatPurchaseRegister,
                    Title = IsVatReceiptMode ? "UAE PURCHASE VAT REGISTER" : "BUSINESS EXPENSE REGISTER",
                    FromDate = DateFrom ?? DateTime.Today,
                    ToDate = DateTo ?? DateTime.Today,
                    Data = ActiveReceipts.ToList()
                };

                var bytes = _excelExporter.Export(request);
                await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                // Never fail silently: surface the error to the UI.
                StatusMessage = _localizationService.GetString(
                    IsVatReceiptMode ? "Manager.Receipt.VatExportFailed" : "Manager.Receipt.NonVatExportFailed",
                    ex.Message);
            }
        }

        private PurchaseReceiptSearchRequest BuildPurchaseReceiptSearchRequest(byte receiptTypeId)
        {
            return new PurchaseReceiptSearchRequest
            {
                SearchText = SearchText,
                FromDate = DateFrom,
                ToDate = DateTo,
                SupplierId = SelectedSupplierIdOrNull,
                Category = string.IsNullOrWhiteSpace(CategoryFilter) ? null : CategoryFilter.Trim(),
                ReceiptTypeId = receiptTypeId
            };
        }

        /// <summary>
        /// The real supplier id to send to the API, or null when the
        /// "All Suppliers" sentinel (SupplierId 0) is selected — that
        /// entry only exists to clear the filter and is never a valid
        /// supplier to save against a receipt.
        /// </summary>
        private int? SelectedSupplierIdOrNull =>
            SelectedSupplier is null || SelectedSupplier.SupplierId == 0
                ? null
                : SelectedSupplier.SupplierId;

        private bool MatchesFilters(PurchaseReceiptDto receipt)
        {
            return MatchesFilters(
                receipt.InvoiceNumber,
                receipt.InvoiceDate,
                receipt.Category,
                receipt.SupplierName,
                receipt.InvoiceNumber + " " + receipt.Category + " " + receipt.Description + " " + receipt.Notes + " " + receipt.SupplierName,
                applySupplierFilter: true);
        }

        private bool MatchesFilters(string? receiptNumber, DateTime? receiptDate, string? category, string? supplierName, string? searchText, bool applySupplierFilter)
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

            // SupplierId 0 is the "All Suppliers" sentinel — treat it the
            // same as no filter selected.
            if (applySupplierFilter && SelectedSupplier is not null && SelectedSupplier.SupplierId != 0)
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
