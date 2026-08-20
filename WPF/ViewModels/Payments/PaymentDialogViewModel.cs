using BLL.Interfaces;
using Contracts.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels;

/// <summary>
/// Handles cash payment collection in a modal dialog.
/// </summary>
public class PaymentDialogViewModel : BaseViewModel
{
    private readonly ITransactionService _transactionService;
    private readonly ISessionService _sessionService;
    private readonly CashierDashboardViewModel _cashierDashboardViewModel;

    private decimal _paymentTotal;
    public decimal PaymentTotal
    {
        get => _paymentTotal;
        set
        {
            _paymentTotal = value;
            OnPropertyChanged();
        }
    }

    private string _cashReceivedText = string.Empty;
    public string CashReceivedText
    {
        get => _cashReceivedText;
        set
        {
            _cashReceivedText = value;
            OnPropertyChanged();

            if (decimal.TryParse(value, out decimal amount))
                CashReceived = amount;
            else
                CashReceived = 0m;
        }
    }

    private decimal _cashReceived;
    public decimal CashReceived
    {
        get => _cashReceived;
        set
        {
            _cashReceived = RoundMoney(value);
            OnPropertyChanged();
            OnPropertyChanged(nameof(ChangeAmount));
        }
    }

    public decimal ChangeAmount
    {
        get
        {
            decimal change = RoundMoney(CashReceived - PaymentTotal);
            return change < 0 ? 0 : change;
        }
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            _errorMessage = value ?? string.Empty;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ICommand ConfirmPaymentCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? DialogClosed;
    public event Action<int>? PaymentCompleted;

    public PaymentDialogViewModel(
        decimal paymentTotal,
        CashierDashboardViewModel cashierDashboardViewModel,
        ITransactionService transactionService,
        ISessionService sessionService,
        INotificationService notifications)
    {
        _paymentTotal = paymentTotal;
        _cashierDashboardViewModel = cashierDashboardViewModel;
        _transactionService = transactionService;
        _sessionService = sessionService;
        Notifications = notifications;

        ConfirmPaymentCommand = new AsyncRelayCommand(ConfirmPaymentAsync);
        CancelCommand = new RelayCommand(CancelPayment);
    }

    private async Task ConfirmPaymentAsync()
    {
        ErrorMessage = string.Empty;

        if (CashReceived < PaymentTotal)
        {
            ErrorMessage = System.Windows.Application.Current.TryFindResource("Payment.Underpaid") as string
                ?? "Cash received is less than the amount due.";
            return;
        }

        var result = await _transactionService.CreateTransactionAsync(BuildCreateTransactionRequest());
        if (result.IsSuccess)
        {
            PaymentCompleted?.Invoke(result.Value!);
            CloseDialog();
        }
        else
        {
            ErrorMessage = result.Error ?? "Payment failed. Please try again.";
        }
    }

    private void CancelPayment(object? obj)
    {
        CloseDialog();
    }

    private CreateTransactionRequest BuildCreateTransactionRequest()
    {
        return new CreateTransactionRequest
        {
            CashierId = _sessionService.CurrentUser?.UserId ?? 0,
            ShiftId = _sessionService.CurrentShift?.ShiftId ?? 0,
            Subtotal = _cashierDashboardViewModel.Subtotal,
            TaxTotal = _cashierDashboardViewModel.Tax,
            GrandTotal = _cashierDashboardViewModel.Total,
            PaymentMethod = "Cash",
            AmountTendered = CashReceived,
            ChangeGiven = ChangeAmount,
            ReferenceNumber = null,
            Items = BuildTransactionItems()
        };
    }

    private List<CreateTransactionItemRequest> BuildTransactionItems()
    {
        return _cashierDashboardViewModel.SaleItems
            .Select(item => new CreateTransactionItemRequest
            {
                VariantId = item.VariantId,
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

    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private void CloseDialog()
    {
        DialogClosed?.Invoke();
    }
}