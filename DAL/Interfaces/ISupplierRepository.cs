using DAL.Entities;

namespace DAL.Interfaces
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        Task<Supplier?> GetByCompanyNameAsync(string companyName);
    }
}
