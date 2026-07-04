using System;
using UI.Commands;

namespace UI.ViewModels;

public class CardPaymentConfirmDialogViewModel : BaseViewModel
{
    public decimal GrandTotal { get; }

    public RelayCommand ConfirmCommand { get; }
    public RelayCommand CancelCommand { get; }

    public event Action<bool>? RequestClose;

    public CardPaymentConfirmDialogViewModel(decimal grandTotal)
    {
        GrandTotal = grandTotal;
        ConfirmCommand = new RelayCommand(Confirm);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void Confirm()
    {
        RequestClose?.Invoke(true);
    }

    private void Cancel()
    {
        RequestClose?.Invoke(false);
    }
}
