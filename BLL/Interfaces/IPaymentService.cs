using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();

        Task<PaymentDto?> GetPaymentByIdAsync(int id);

        Task AddPaymentAsync(PaymentDto Payment);

        Task UpdatePaymentAsync(PaymentDto Payment);

        Task DeletePaymentAsync(int id);
    }
}