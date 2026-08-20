using DAL.Entities;
using POS.Contracts.Receipts;

namespace DAL.Interfaces
{
    public interface IPurchaseReceiptRepository
    {
        Task<List<PurchaseReceipt>> GetAllAsync(PurchaseReceiptSearchRequest? request = null);
        Task<PurchaseReceipt?> GetByIdAsync(int receiptId);
        Task AddAsync(PurchaseReceipt receipt);
        Task UpdateAsync(PurchaseReceipt receipt);
        Task DeleteAsync(int receiptId);
    }
}
