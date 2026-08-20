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
    private readonly INavigationService _navigationService;
    private readonly ISessionService _sessionService;

    public event Action? LogoutRequested;

    public object? CurrentViewModel => _navigationService.CurrentViewModel;

    public bool IsCashierView => _navigationService.CurrentViewModel is CashierDashboardViewModel;

    public bool IsManagerView => _navigationService.CurrentViewModel is ManagerMainViewModel;

    public MainViewModel(
        INavigationService navigationService,
        ITransactionService transactionService,
        ISessionService sessionService,
        IDialogService dialogService)
    {
        // NOTE: CashierDashboardViewModel is intentionally NOT a constructor
        // dependency here. It used to be a mandatory ctor parameter, which
        // meant DI eagerly constructed it (and its 4-query InitializeAsync
        // load) on EVERY login, including Manager logins that never see the
        // cashier dashboard. It is now resolved lazily/role-conditionally by
        // NavigateToDashboardByRole() below, via
        // _navigationService.NavigateTo<CashierDashboardViewModel>(), which
        // only runs on the Cashier branch. See login-performance-analysis.md.
        _navigationService = navigationService;
        _sessionService = sessionService;

        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;
        NavigateToDashboardByRole();
    }

    /// <summary>
    /// Detaches this view-model from the singleton
    /// <see cref="INavigationService"/>. MUST be called when the
    /// owning <c>MainWindow</c> is being torn down (e.g. on logout),
    /// otherwise <see cref="NavigationService.CurrentViewModelChanged"/>
    /// keeps a strong reference to this instance and to every
    /// <c>ManagerMainViewModel.LogoutRequested</c> it has wired up,
    /// which on the next login would cause the logout
    /// confirmation <c>MessageBox</c> to be shown N+1 times.
    /// </summary>
    public void UnloadFromNavigation()
    {
        _navigationService.CurrentViewModelChanged -= OnCurrentViewModelChanged;
        UnsubscribeManagerLogout();
        UnsubscribeCashierEvents();
        UnsubscribeManagerHomeEvents();
    }
}

