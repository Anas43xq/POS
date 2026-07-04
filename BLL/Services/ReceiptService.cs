using BLL.Interfaces;
using DAL.Interfaces;
using POS.Contracts.Receipts;

namespace BLL.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IReceiptRepository _receiptRepository;

        public ReceiptService(IReceiptRepository receiptRepository)
        {
            _receiptRepository = receiptRepository;
        }

        public async Task<ReceiptDetailsDto?> GetReceiptByTransactionIdAsync(int transactionId)
        {
            ValidateTransactionId(transactionId);

            return await _receiptRepository.GetReceiptByTransactionIdAsync(transactionId);
        }

        private static void ValidateTransactionId(int transactionId)
        {
            if (transactionId <= 0)
                throw new InvalidOperationException("Invalid transaction id.");
        }
    }
}