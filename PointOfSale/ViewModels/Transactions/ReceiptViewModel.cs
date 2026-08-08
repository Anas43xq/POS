using POS.Contracts.Printing;
using POS.Contracts.Receipts;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public class ReceiptViewModel : BaseViewModel
    {
        private readonly IPrintingService _printingService;
        private readonly ILogger<ReceiptDisplayService> _logger;
        private readonly INotificationService _notifications;

        public ReceiptDetailsDto Receipt { get; }

        public string StoreName => Receipt.StoreName;
        public string ReceiptNumber => Receipt.ReceiptNumber;
        public string TransactionDate => Receipt.TransactionDate.ToString("yyyy-MM-dd HH:mm");
        public string CashierName => Receipt.CashierName;
        public List<ReceiptItemDto> Items => Receipt.Items;
        public decimal Subtotal => Receipt.Subtotal;
        public decimal TaxTotal => Receipt.TaxTotal;
        public decimal GrandTotal => Receipt.GrandTotal;
        public decimal DiscountTotal => Receipt.DiscountTotal;
        public string PaymentMethod => Receipt.PaymentMethod;
        public decimal AmountTendered => Receipt.AmountTendered;
        public decimal ChangeGiven => Receipt.ChangeGiven;

        public ICommand PrintReceiptCommand { get; }

        public ReceiptViewModel(
            ReceiptDetailsDto receipt,
            IPrintingService printingService,
            ILogger<ReceiptDisplayService> logger,
            INotificationService notifications)
        {
            Receipt = receipt;
            _printingService = printingService;
            _logger = logger;
            _notifications = notifications;
            PrintReceiptCommand = new AsyncRelayCommand(PrintReceiptAsync, onError: ex =>
            {
                _logger.LogError(ex, "Failed to print receipt for transaction {TransactionId}", Receipt.TransactionId);
                _notifications.ShowError("Printing failed. Please check the printer and try again.");
            });
        }

        private async Task PrintReceiptAsync()
        {
            await _printingService.PrintReceiptDirectAsync(Receipt, showDialog: true);
        }
    }
}
