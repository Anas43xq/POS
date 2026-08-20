using Contracts;
using POS.Contracts.Receipts;


namespace DAL.Interfaces
{
    public interface IReceiptRepository
    {
        Task<ReceiptDetailsDto?> GetReceiptByTransactionIdAsync(int transactionId);
        

    }
}
