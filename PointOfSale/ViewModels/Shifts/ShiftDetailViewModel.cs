using Contracts.Shifts;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public class ShiftDetailViewModel : BaseViewModel
    {
        private readonly IReceiptDisplayService _receiptDisplayService;

        public ShiftDetailViewModel(ShiftDetailDto detail, IReceiptDisplayService receiptDisplayService)
        {
            _receiptDisplayService = receiptDisplayService;
            Detail = detail;

            Transactions = new ObservableCollection<ShiftTransactionDto>(detail.Transactions);
            OpenReceiptCommand = new RelayCommand<ShiftTransactionDto?>(OpenReceipt);
            CloseCommand = new RelayCommand(_ => Close());
        }

        public ShiftDetailDto Detail { get; }

        public ObservableCollection<ShiftTransactionDto> Transactions { get; }

        public ICommand OpenReceiptCommand { get; }
        public ICommand CloseCommand { get; }

        private void OpenReceipt(ShiftTransactionDto? transaction)
        {
            if (transaction == null)
                return;

            _receiptDisplayService.ShowReceipt(transaction.TransactionId);
        }

        private void Close()
        {
            var window = Application.Current.Windows
                .OfType<System.Windows.Window>()
                .FirstOrDefault(w => w.DataContext == this);

            window?.Close();
        }
    }
}
