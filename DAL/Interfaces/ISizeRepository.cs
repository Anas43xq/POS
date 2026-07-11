using DAL.Entities;

namespace DAL.Interfaces;

public interface ISizeRepository : IRepository<Size>
{
    Task<IEnumerable<Size>> GetAllOrderedAsync();
}