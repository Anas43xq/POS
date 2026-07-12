using BLL.Interfaces;
using BLL.Services;
using Contracts.Enum;
using Contracts.Shifts;
using Contracts.Transactions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public partial class ShiftManagementViewModel : BaseViewModel
    {
        private readonly IShiftManagementService _shiftManagementService;
        private readonly IReceiptDisplayService _receiptDisplayService;

        private readonly ObservableCollection<ShiftListItemDto> _shifts = new();
        private ICollectionView? _shiftsView;
        private string _errorMessage = string.Empty;
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _totalCount;
        private bool _isLoaded;

        public ShiftManagementViewModel(
            IShiftManagementService shiftManagementService,
            IReceiptDisplayService receiptDisplayService)
        {
            _shiftManagementService = shiftManagementService;
            _receiptDisplayService = receiptDisplayService;

            LoadDayCommand = new RelayCommand(_ => LoadDay());
            LoadWeekCommand = new RelayCommand(_ => LoadWeek());
            LoadMonthCommand = new RelayCommand(_ => LoadMonth());
            LoadPeriodCommand = new RelayCommand(_ => LoadPeriod());
            ApplyPeriodCommand = new RelayCommand(_ => ApplyPeriod());
            RefreshCommand = new AsyncRelayCommand(() => RefreshAsync());
            PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => CanGoPreviousPage);
            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CanGoNextPage);
            OpenShiftDetailCommand = new AsyncRelayCommand<ShiftListItemDto?>(OpenShiftDetailAsync);

            Shifts = _shifts;
            ShiftsView = CollectionViewSource.GetDefaultView(Shifts);
            ShiftsView.Filter = FilterShifts;

            CurrentFilterMode = ShiftFilterMode.Day;
            _selectedShiftStatusFilter = ShiftStatusFilterOptions[0];
            IsPeriodFilterVisible = false;
        }

        public ObservableCollection<ShiftListItemDto> Shifts { get; }

        public ICollectionView ShiftsView
        {
            get => _shiftsView!;
            private set
            {
                _shiftsView = value;
                OnPropertyChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (_errorMessage == value)
                    return;

                _errorMessage = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        // ── Filter mode ──

        private ShiftFilterMode _currentFilterMode;
        public ShiftFilterMode CurrentFilterMode
        {
            get => _currentFilterMode;
            private set
            {
                if (_currentFilterMode == value)
                    return;

                _currentFilterMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPeriodFilterVisible));
            }
        }

        public IReadOnlyList<ShiftStatusFilterOption> ShiftStatusFilterOptions { get; } = new[]
        {
            new ShiftStatusFilterOption("All", null),
            new ShiftStatusFilterOption("Open", "Open"),
            new ShiftStatusFilterOption("Closed", "Closed"),
        };

        private ShiftStatusFilterOption _selectedShiftStatusFilter;
        public ShiftStatusFilterOption SelectedShiftStatusFilter
        {
            get => _selectedShiftStatusFilter;
            set
            {
                if (value == null || _selectedShiftStatusFilter == value)
                    return;

                _selectedShiftStatusFilter = value;
                ErrorMessage = string.Empty;
                CurrentPage = 1;
                _ = LoadShifts();
            }
        }

        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                if (_fromDate == value)
                    return;

                _fromDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                if (_toDate == value)
                    return;

                _toDate = value;
                OnPropertyChanged();
            }
        }

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

        // ── Commands ──

        public ICommand LoadDayCommand { get; }
        public ICommand LoadWeekCommand { get; }
        public ICommand LoadMonthCommand { get; }
        public ICommand LoadPeriodCommand { get; }
        public ICommand ApplyPeriodCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand OpenShiftDetailCommand { get; }

        // ── Paging ──

        public int CurrentPage
        {
            get => _currentPage;
            private set
            {
                if (_currentPage == value)
                    return;

                _currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(CanGoPreviousPage));
                OnPropertyChanged(nameof(CanGoNextPage));
                RaisePagingCanExecuteChanged();
            }
        }

        public int PageSize
        {
            get => _pageSize;
            private set
            {
                if (_pageSize == value)
                    return;

                _pageSize = value;
                OnPropertyChanged();
            }
        }

        public int TotalCount
        {
            get => _totalCount;
            private set
            {
                if (_totalCount == value)
                    return;

                _totalCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(CanGoNextPage));
                RaisePagingCanExecuteChanged();
            }
        }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
        public string PageInfo => TotalPages == 0 ? "0 of 0" : $"{CurrentPage} of {TotalPages}";
        public bool CanGoPreviousPage => CurrentPage > 1;
        public bool CanGoNextPage => CurrentPage < TotalPages;

        public bool FilterShifts(object obj)
        {
            return true;
        }

        private void RaisePagingCanExecuteChanged()
        {
            if (PreviousPageCommand is RelayCommand prev)
                prev.RaiseCanExecuteChanged();

            if (NextPageCommand is RelayCommand next)
                next.RaiseCanExecuteChanged();
        }

        private void RefreshView()
        {
            if (ShiftsView == null)
                return;

            ShiftsView.Refresh();
        }
    }

    /// <summary>
    /// Filter option for the shift status ComboBox.
    /// </summary>
    public sealed class ShiftStatusFilterOption
    {
        public ShiftStatusFilterOption(string label, string? value)
        {
            Label = label;
            Value = value;
        }

        public string Label { get; }
        public string? Value { get; }

        public override string ToString() => Label;
    }

    public enum ShiftFilterMode
    {
        Day,
        Week,
        Month,
        Period
    }
}