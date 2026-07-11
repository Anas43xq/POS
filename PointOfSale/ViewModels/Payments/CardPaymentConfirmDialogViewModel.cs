using System;
using UI.Commands;

namespace UI.ViewModels;

public class CardPaymentConfirmDialogViewModel : BaseViewModel
{
    private readonly BLL.Interfaces.ILocalizationService _localization;

    public decimal GrandTotal { get; }

    /// <summary>
    /// Localized confirmation message that includes the formatted amount
    /// (e.g. "Charge د.إ 35.71 to the card?").
    /// </summary>
    public string ConfirmMessage => _localization.GetString("Payment.Card.ConfirmCharge", $"د.إ {GrandTotal:N2}");

    public RelayCommand ConfirmCommand { get; }
    public RelayCommand CancelCommand { get; }

    public event Action<bool>? RequestClose;

    public CardPaymentConfirmDialogViewModel(decimal grandTotal, BLL.Interfaces.ILocalizationService localization)
    {
        _localization = localization;
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
