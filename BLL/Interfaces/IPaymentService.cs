using DAL.Entities;

namespace BLL.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();

        Task<Payment?> GetPaymentByIdAsync(int id);

        Task AddPaymentAsync(Payment Payment);

        Task UpdatePaymentAsync(Payment Payment);

        Task DeletePaymentAsync(int id);
    }
}