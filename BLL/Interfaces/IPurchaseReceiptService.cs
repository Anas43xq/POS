using BLL.Models;
using POS.Contracts.Receipts;

namespace BLL.Interfaces
{
    public interface IPurchaseReceiptService
    {
        Task<Result<List<PurchaseReceiptDto>>> GetAllAsync(PurchaseReceiptSearchRequest? request = null);
        Task<Result<PurchaseReceiptDto?>> GetByIdAsync(int receiptId);
        Task<Result<PurchaseReceiptDto>> CreateAsync(CreatePurchaseReceiptRequest request);
        Task<Result<PurchaseReceiptDto>> UpdateAsync(int receiptId, UpdatePurchaseReceiptRequest request);
        Task<Result<bool>> DeleteAsync(int receiptId);
    }
}
