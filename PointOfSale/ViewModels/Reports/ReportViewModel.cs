using BLL.Interfaces;
using BLL.DTOs;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public enum ReportFilterMode
    {
        Today,
        Week,
        Month,
        Period
    }

    public partial class ReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IProductService _productService;
        private readonly ExcelReportExporter _excelExporter;

        private ReportFilterMode _selectedPeriodType = ReportFilterMode.Today;
        private DateTime? _fromDate;
        private DateTime? _toDate;
        private object? _selectedProduct;
        private bool _isSalesMode = true;
        private bool _isProductMode;
        private bool _isPeriodFilterVisible;
        private bool _isLoading;
        private string _totalOrders = "0";
        private string _totalSales = "AED 0.00";
        private string _cashTotal = "AED 0.00";
        private string _cardTotal = "AED 0.00";
        private string _totalQuantitySold = "0";
        private string _totalRevenue = "AED 0.00";
        private string _averagePrice = "AED 0.00";

        public ReportViewModel(IReportService reportService, IProductService productService, ExcelReportExporter excelExporter)
        {
            _reportService = reportService;
            _productService = productService;
            _excelExporter = excelExporter;

            ReportCommand = new RelayCommand<string>(OnReportAction, _ => !_isLoading);

            TransactionReports = new ObservableCollection<TransactionReportDto>();
            ProductReports = new ObservableCollection<ProductReportDto>();
            Products = new ObservableCollection<object>();

            IsSalesMode = true;
            _ = LoadProductsAsync();
        }

        // ================================================================
        // FILTER MODE
        // ================================================================
        public ReportFilterMode SelectedPeriodType
        {
            get => _selectedPeriodType;
            set
            {
                if (_selectedPeriodType == value) return;
                _selectedPeriodType = value;
                OnPropertyChanged();
            }
        }

        public bool IsPeriodFilterVisible
        {
            get => _isPeriodFilterVisible;
            set
            {
                _isPeriodFilterVisible = value;
                OnPropertyChanged();
            }
        }

        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged();
            }
        }

        public object? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
            }
        }

        // ================================================================
        // FILTER COMMANDS
        // ================================================================
        public ICommand FilterTodayCommand =>
            new RelayCommand(async _ => { SelectedPeriodType = ReportFilterMode.Today; IsPeriodFilterVisible = false; await LoadReportAsync(); }, _ => !_isLoading);
        public ICommand FilterThisWeekCommand =>
            new RelayCommand(async _ => { SelectedPeriodType = ReportFilterMode.Week; IsPeriodFilterVisible = false; await LoadReportAsync(); }, _ => !_isLoading);
        public ICommand FilterMonthCommand =>
            new RelayCommand(async _ => { SelectedPeriodType = ReportFilterMode.Month; IsPeriodFilterVisible = false; await LoadReportAsync(); }, _ => !_isLoading);
        public ICommand ShowPeriodFilterCommand =>
            new RelayCommand(_ => { SelectedPeriodType = ReportFilterMode.Period; IsPeriodFilterVisible = true; });
        public ICommand ApplyPeriodFilterCommand =>
            new RelayCommand(async _ => { SelectedPeriodType = ReportFilterMode.Period; IsPeriodFilterVisible = true; await LoadReportAsync(); }, _ => !_isLoading);

        // ================================================================
        // MODE PROPERTIES
        // ================================================================
        public bool IsSalesMode
        {
            get => _isSalesMode;
            set
            {
                _isSalesMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProductMode));
                OnPropertyChanged(nameof(SalesSummaryVisibility));
                OnPropertyChanged(nameof(ProductSummaryVisibility));
                OnPropertyChanged(nameof(SalesGridVisibility));
                OnPropertyChanged(nameof(ProductGridVisibility));
            }
        }

        public bool IsProductMode
        {
            get => _isProductMode;
            set
            {
                _isProductMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSalesMode));
                OnPropertyChanged(nameof(SalesSummaryVisibility));
                OnPropertyChanged(nameof(ProductSummaryVisibility));
                OnPropertyChanged(nameof(SalesGridVisibility));
                OnPropertyChanged(nameof(ProductGridVisibility));
            }
        }

        // ================================================================
        // SALES SUMMARY
        // ================================================================
        public string TotalOrders
        {
            get => _totalOrders;
            set { _totalOrders = value; OnPropertyChanged(); }
        }

        public string TotalSales
        {
            get => _totalSales;
            set { _totalSales = value; OnPropertyChanged(); }
        }

        public string CashTotal
        {
            get => _cashTotal;
            set { _cashTotal = value; OnPropertyChanged(); }
        }

        public string CardTotal
        {
            get => _cardTotal;
            set { _cardTotal = value; OnPropertyChanged(); }
        }

        // ================================================================
        // PRODUCT SUMMARY
        // ================================================================
        public string TotalQuantitySold
        {
            get => _totalQuantitySold;
            set { _totalQuantitySold = value; OnPropertyChanged(); }
        }

        public string TotalRevenue
        {
            get => _totalRevenue;
            set { _totalRevenue = value; OnPropertyChanged(); }
        }

        public string AveragePrice
        {
            get => _averagePrice;
            set { _averagePrice = value; OnPropertyChanged(); }
        }

        // ================================================================
        // VISIBILITY (MODE TOGGLING)
        // ================================================================
        public System.Windows.Visibility SalesSummaryVisibility =>
            IsSalesMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility ProductSummaryVisibility =>
            IsProductMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility SalesGridVisibility =>
            IsSalesMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility ProductGridVisibility =>
            IsProductMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        // ================================================================
        // COLLECTIONS
        // ================================================================
        public ObservableCollection<TransactionReportDto> TransactionReports { get; }
        public ObservableCollection<ProductReportDto> ProductReports { get; }
        public ObservableCollection<object> Products { get; }
    }
}
