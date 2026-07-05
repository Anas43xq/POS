using POS.Contracts.Receipts;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public class ReceiptViewModel : BaseViewModel
    {
        private readonly ReceiptPrinterService _printerService;
        private readonly ILogger<UI.Services.ReceiptDisplayService> _logger;

        public ReceiptDetailsDto Receipt { get; }

        public string StoreName => Receipt.StoreName;
        public string ReceiptNumber => Receipt.ReceiptNumber;
        public string TransactionDate => Receipt.TransactionDate.ToString("yyyy-MM-dd HH:mm");
        public string CashierName => Receipt.CashierName;
        public List<ReceiptItemDto> Items => Receipt.Items;
        public decimal Subtotal => Receipt.Subtotal;
        public decimal TaxTotal => Receipt.TaxTotal;

        public decimal GrandTotal => Receipt.GrandTotal;
        public string PaymentMethod => Receipt.PaymentMethod;
        public decimal AmountTendered => Receipt.AmountTendered;
        public decimal ChangeGiven => Receipt.ChangeGiven;

        public ICommand PrintReceiptCommand { get; }

        public ReceiptViewModel(ReceiptDetailsDto receipt, ReceiptPrinterService printerService, ILogger<UI.Services.ReceiptDisplayService> logger)
        {
            Receipt = receipt;
            _printerService = printerService;
            _logger = logger;
            PrintReceiptCommand = new RelayCommand(PrintReceipt);
        }

        private void PrintReceipt(object? obj)
        {
            try
            {
                _printerService.Print(Receipt);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to print receipt preview for transaction {TransactionId}", Receipt.TransactionId);
                System.Windows.MessageBox.Show(
                    "Printing failed. Please check the printer and try again.",
                    "Print Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
