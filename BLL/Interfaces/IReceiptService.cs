using POS.Contracts.Receipts;

namespace BLL.Interfaces
{
    public interface IReceiptService
    {
        Task<ReceiptDetailsDto?> GetReceiptByTransactionIdAsync(int transactionId);
    }
}