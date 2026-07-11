using POS.Contracts.Printing;
using POS.Contracts.Receipts;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public class ReceiptViewModel : BaseViewModel
    {
        private readonly IPrintingService _printingService;
        private readonly ILogger<ReceiptDisplayService> _logger;

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

        public ReceiptViewModel(ReceiptDetailsDto receipt, IPrintingService printingService, ILogger<ReceiptDisplayService> logger)
        {
            Receipt = receipt;
            _printingService = printingService;
            _logger = logger;
            PrintReceiptCommand = new AsyncRelayCommand(PrintReceiptAsync);
        }

        private async Task PrintReceiptAsync()
        {
            try
            {
                await _printingService.PrintReceiptDirectAsync(Receipt, showDialog: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print receipt for transaction {TransactionId}", Receipt.TransactionId);
                MessageBox.Show(
                    "Printing failed. Please check the printer and try again.",
                    "Print Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
