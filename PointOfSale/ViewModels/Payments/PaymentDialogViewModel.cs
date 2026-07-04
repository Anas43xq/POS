using BLL.Interfaces;
using Contracts.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;

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
        set
        {
            _errorMessage = value;
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
        ISessionService sessionService)
    {
        _paymentTotal = paymentTotal;
        _cashierDashboardViewModel = cashierDashboardViewModel;
        _transactionService = transactionService;
        _sessionService = sessionService;

        ConfirmPaymentCommand = new AsyncRelayCommand(ConfirmPaymentAsync);
        CancelCommand = new RelayCommand(CancelPayment);
    }

    private async Task ConfirmPaymentAsync()
    {
        ErrorMessage = string.Empty;

        try
        {
            ValidatePaymentBeforeCreate();
            CreateTransactionRequest request = BuildCreateTransactionRequest();
            int transactionId = await _transactionService.CreateTransactionAsync(request);

            PaymentCompleted?.Invoke(transactionId);
            CloseDialog();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void CancelPayment(object? obj)
    {
        CloseDialog();
    }

    private void ValidatePaymentBeforeCreate()
    {
        if (_sessionService.CurrentUser == null)
            throw new InvalidOperationException("No logged-in cashier.");

        if (_sessionService.CurrentShift == null || _sessionService.CurrentShift.Status != DAL.Entities.ShiftStatus.Open)
            throw new InvalidOperationException("No open shift.");

        if (!_cashierDashboardViewModel.SaleItems.Any())
            throw new InvalidOperationException("Cart is empty.");

        if (CashReceived < PaymentTotal)
            throw new InvalidOperationException("Cash received is less than total.");
    }

    private CreateTransactionRequest BuildCreateTransactionRequest()
    {
        return new CreateTransactionRequest
        {
            CashierId = _sessionService.CurrentUser!.UserId,
            ShiftId = _sessionService.CurrentShift!.ShiftId,
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

    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private void CloseDialog()
    {
        DialogClosed?.Invoke();
    }
}
