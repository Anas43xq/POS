using Contracts.Sales;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels;

/// <summary>
/// Displays recent sales in a modal dialog.
/// Allows user to select a sale and view its receipt.
/// </summary>
public class RecentSalesDialogViewModel : BaseViewModel
{
    private readonly CashierDashboardViewModel _cashierDashboardViewModel;
    private readonly IReceiptDisplayService _receiptDisplayService;

    /// <summary>
    /// Proxies recent sales from CashierDashboardViewModel
    /// </summary>
    public ObservableCollection<RecentTransactionDto> RecentSales 
        => _cashierDashboardViewModel.RecentSales;

    // Commands
    public ICommand CloseDialogCommand { get; }
    public ICommand OpenReceiptCommand { get; }

    // Event to notify when dialog should close
    public event Action? DialogClosed;

    public RecentSalesDialogViewModel(
        CashierDashboardViewModel cashierDashboardViewModel,
        IReceiptDisplayService receiptDisplayService
    )
    {
        _cashierDashboardViewModel = cashierDashboardViewModel;
        _receiptDisplayService = receiptDisplayService;

        CloseDialogCommand = new RelayCommand(CloseDialog);
        OpenReceiptCommand = new RelayCommand<int>(OpenReceipt);
    }

    private void CloseDialog(object? obj)
    {
        DialogClosed?.Invoke();
    }

    private void OpenReceipt(int transactionId)
    {
        if (transactionId <= 0)
            return;

        // Show receipt directly via independent service
        _receiptDisplayService.ShowReceipt(transactionId);

        // Close this dialog
        CloseDialog(null);
    }
}
