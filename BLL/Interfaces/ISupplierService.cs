using BLL.Models;
using POS.Contracts.Receipts;

namespace BLL.Interfaces
{
    public interface ISupplierService
    {
        Task<Result<List<SupplierDto>>> GetAllAsync();
        Task<Result<SupplierDto?>> GetByIdAsync(int supplierId);
        Task<Result<SupplierDto>> CreateAsync(CreateSupplierRequest request);
        Task<Result<SupplierDto>> UpdateAsync(int supplierId, UpdateSupplierRequest request);
        Task<Result<bool>> DeleteAsync(int supplierId);
    }
}
