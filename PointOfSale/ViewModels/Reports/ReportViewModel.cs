using BLL.Interfaces;
using BLL.DTOs;
using Contracts.Enum;
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
        private readonly ILocalizationService _localization;
        private readonly ExcelReportExporter _excelExporter;
        private readonly IReceiptDisplayService _receiptDisplayService;

        private ReportFilterMode _selectedPeriodType = ReportFilterMode.Today;
        private DateTime? _fromDate;
        private DateTime? _toDate;
        private bool _isSalesMode = true;
        private bool _isSalesAnalysisMode;
        private bool _isPeriodFilterVisible;
        private bool _isLoading;
        private string _totalOrders = "0";
        private string _totalSales = "AED 0.00";
        private string _cashTotal = "AED 0.00";
        private string _cardTotal = "AED 0.00";
        private string _categoriesSold = "0";
        private string _productsSold = "0";
        private string _variantsSold = "0";
        private string _salesAnalysisTotalQuantitySold = "0";
        private string _salesAnalysisTotalSales = "AED 0.00";

        public ReportViewModel(IReportService reportService, ILocalizationService localization, ExcelReportExporter excelExporter, IReceiptDisplayService receiptDisplayService)
        {
            _reportService = reportService;
            _localization = localization;
            _excelExporter = excelExporter;
            _receiptDisplayService = receiptDisplayService;

            ReportCommand = new RelayCommand<string>(OnReportAction, _ => !_isLoading);
            OpenReceiptCommand = new RelayCommand<TransactionReportDto?>(OpenReceipt);

            TransactionReports = new ObservableCollection<TransactionReportDto>();
            SalesAnalysisReports = new ObservableCollection<SalesAnalysisDto>();

            IsSalesMode = true;
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
                if (_fromDate == value) return;
                _fromDate = value;
                OnPropertyChanged();
                if (SelectedPeriodType == ReportFilterMode.Period && !_isLoading)
                    _ = LoadReportAsync();
            }
        }

        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                if (_toDate == value) return;
                _toDate = value;
                OnPropertyChanged();
                if (SelectedPeriodType == ReportFilterMode.Period && !_isLoading)
                    _ = LoadReportAsync();
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
                if (_isSalesMode == value) return;
                _isSalesMode = value;
                if (value) { _isSalesAnalysisMode = false; }
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSalesAnalysisMode));
                RaiseModeVisibilityChanged();
            }
        }

        public bool IsSalesAnalysisMode
        {
            get => _isSalesAnalysisMode;
            set
            {
                if (_isSalesAnalysisMode == value) return;
                _isSalesAnalysisMode = value;
                if (value) { _isSalesMode = false; }
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSalesMode));
                RaiseModeVisibilityChanged();
            }
        }

        private void RaiseModeVisibilityChanged()
        {
            OnPropertyChanged(nameof(SalesSummaryVisibility));
            OnPropertyChanged(nameof(SalesGridVisibility));
            OnPropertyChanged(nameof(SalesAnalysisSummaryVisibility));
            OnPropertyChanged(nameof(SalesAnalysisGridVisibility));
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
        // SALES ANALYSIS SUMMARY
        // ================================================================
        public string CategoriesSold
        {
            get => _categoriesSold;
            set { _categoriesSold = value; OnPropertyChanged(); }
        }

        public string ProductsSold
        {
            get => _productsSold;
            set { _productsSold = value; OnPropertyChanged(); }
        }

        public string VariantsSold
        {
            get => _variantsSold;
            set { _variantsSold = value; OnPropertyChanged(); }
        }

        public string SalesAnalysisTotalQuantitySold
        {
            get => _salesAnalysisTotalQuantitySold;
            set { _salesAnalysisTotalQuantitySold = value; OnPropertyChanged(); }
        }

        public string SalesAnalysisTotalSales
        {
            get => _salesAnalysisTotalSales;
            set { _salesAnalysisTotalSales = value; OnPropertyChanged(); }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // ================================================================
        // VISIBILITY (MODE TOGGLING)
        // ================================================================
        public System.Windows.Visibility SalesSummaryVisibility =>
            IsSalesMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility SalesGridVisibility =>
            IsSalesMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility SalesAnalysisSummaryVisibility =>
            IsSalesAnalysisMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility SalesAnalysisGridVisibility =>
            IsSalesAnalysisMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        // ================================================================
        // COLLECTIONS
        // ================================================================
        public ObservableCollection<TransactionReportDto> TransactionReports { get; }
        public ObservableCollection<SalesAnalysisDto> SalesAnalysisReports { get; }
    }
}
