using DAL.Entities;
using DAL.Interfaces;
using BLL.Interfaces;

namespace BLL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentrepo;

        public PaymentService(IPaymentRepository PaymentRepo)
        {
            _paymentrepo = PaymentRepo;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync() =>
            await _paymentrepo.GetAllAsync();

        public async Task<Payment?> GetPaymentByIdAsync(int id) =>
            await _paymentrepo.GetByIdAsync(id);

        public async Task AddPaymentAsync(Payment Payment) =>
            await _paymentrepo.AddAsync(Payment);

        public async Task UpdatePaymentAsync(Payment Payment) =>
            await _paymentrepo.UpdateAsync(Payment);

        public async Task DeletePaymentAsync(int id) =>
            await _paymentrepo.DeleteAsync(id);
    }
}