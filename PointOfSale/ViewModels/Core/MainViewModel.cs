using BLL.Interfaces;
using Contracts.Sales;
using Contracts.Transactions;
using DAL.Entities;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UI.Commands;
using UI.Services;
using UI.Views;

namespace UI.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly CashierDashboardViewModel _cashierDashboardViewModel;
    private readonly INavigationService _navigationService;
    private readonly ISessionService _sessionService;

    public event Action? LogoutRequested;

    /// <summary>
    /// Exposes CurrentViewModel from NavigationService as read-only.
    /// </summary>
    public object? CurrentViewModel => _navigationService.CurrentViewModel;

    public bool IsCashierView => _navigationService.CurrentViewModel is CashierDashboardViewModel;

    public bool IsManagerView => _navigationService.CurrentViewModel is ManagerMainViewModel;

    public MainViewModel(
        CashierDashboardViewModel cashierDashboardViewModel,
        INavigationService navigationService,
        ITransactionService transactionService,
        ISessionService sessionService,
        IDialogService dialogService)
    {
        _cashierDashboardViewModel = cashierDashboardViewModel;
        _navigationService = navigationService;
        _sessionService = sessionService;

        // Subscribe to navigation changes and refresh UI
        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

        // Navigate to dashboard based on user role
        NavigateToDashboardByRole();
    }
}

