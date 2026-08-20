using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentrepo;

        public PaymentService(IPaymentRepository PaymentRepo)
        {
            _paymentrepo = PaymentRepo;
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            var entities = await _paymentrepo.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
        {
            var entity = await _paymentrepo.GetByIdAsync(id);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task AddPaymentAsync(PaymentDto Payment) =>
            await _paymentrepo.AddAsync(MapToEntity(Payment));

        public async Task UpdatePaymentAsync(PaymentDto Payment) =>
            await _paymentrepo.UpdateAsync(MapToEntity(Payment));

        public async Task DeletePaymentAsync(int id) =>
            await _paymentrepo.DeleteAsync(id);

        private static PaymentDto MapToDto(Payment e) => new()
        {
            PaymentId = e.PaymentId,
            TransactionId = e.TransactionId,
            PaymentMethod = e.PaymentMethod,
            AmountTendered = e.AmountTendered,
            ChangeGiven = e.ChangeGiven,
            ReferenceNumber = e.ReferenceNumber,
            PaidAt = e.PaidAt
        };

        private static Payment MapToEntity(PaymentDto d) => new()
        {
            PaymentId = d.PaymentId,
            TransactionId = d.TransactionId,
            PaymentMethod = d.PaymentMethod,
            AmountTendered = d.AmountTendered,
            ChangeGiven = d.ChangeGiven,
            ReferenceNumber = d.ReferenceNumber,
            PaidAt = d.PaidAt
        };
    }
}