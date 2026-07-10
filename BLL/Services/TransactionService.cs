using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using Contracts.Transactions;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionCommandRepository _transactionCommandRepository;
        private readonly IShiftRepository _shiftRepository;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ITransactionCommandRepository transactionCommandRepository,
            IShiftRepository shiftRepository)
        {
            _transactionRepository = transactionRepository;
            _transactionCommandRepository = transactionCommandRepository;
            _shiftRepository = shiftRepository;
        }

        public async Task<PagedResult<TransactionListItemDto>> GetTransactionsListAsync(
            GetTransactionsListRequest request,
            CancellationToken ct = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return await _transactionRepository.GetTransactionsListAsync(request, ct);
        }

        public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
        {
            var entities = await _transactionRepository.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<TransactionDto?> GetTransactionByIdAsync(int id)
        {
            var entity = await _transactionRepository.GetByIdAsync(id);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task UpdateTransactionAsync(TransactionDto transaction)
        {
            await _transactionRepository.UpdateAsync(MapToEntity(transaction));
        }

        public async Task DeleteTransactionAsync(int id)
        {
            await _transactionRepository.DeleteAsync(id);
        }

        public async Task<Result<TransactionDto>> VoidTransactionAsync(int transactionId)
        {
            if (transactionId <= 0)
                return Result<TransactionDto>.Failure("Invalid transaction id.");

            bool updated = await _transactionRepository.VoidTransactionAsync(transactionId);
            if (!updated)
            {
                var current = await _transactionRepository.GetByIdAsync(transactionId);
                if (current == null)
                    return Result<TransactionDto>.Failure("Transaction not found.");

                return Result<TransactionDto>.Failure(
                    $"Only completed transactions can be voided (current status: {current.Status}).");
            }

            var voided = await _transactionRepository.GetByIdAsync(transactionId);
            return Result<TransactionDto>.Success(MapToDto(voided!));
        }

        public async Task<int> CreateTransactionAsync(CreateTransactionRequest request)
        {
            ValidateCreateTransactionRequest(request);
            await ValidateShiftIsOpenAsync(request.ShiftId);
            return await _transactionCommandRepository.CreateTransactionAsync(request);
        }

        private static void ValidateCreateTransactionRequest(CreateTransactionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.CashierId <= 0)
                throw new InvalidOperationException("Invalid cashier.");

            if (request.ShiftId <= 0)
                throw new InvalidOperationException("Invalid shift.");

            if (request.Items == null || !request.Items.Any())
                throw new InvalidOperationException("Cart is empty.");

            if (request.PaymentMethod != "Cash" && request.PaymentMethod != "Card")
                throw new InvalidOperationException("Invalid payment method.");

            if (request.PaymentMethod == "Cash" && request.AmountTendered < request.GrandTotal)
                throw new InvalidOperationException("Cash received is less than total.");
        }

        private async Task ValidateShiftIsOpenAsync(int shiftId)
        {
            var shift = await _shiftRepository.GetByIdAsync(shiftId);

            if (shift == null)
                throw new InvalidOperationException("No active shift. Please start a shift first.");

            if (shift.Status != ShiftStatus.Open)
                throw new InvalidOperationException("No active shift. Please start a shift first.");
        }

        private static TransactionDto MapToDto(Transaction e) => new()
        {
            TransactionId = e.TransactionId,
            ReceiptNumber = e.ReceiptNumber,
            ShiftId = e.ShiftId,
            CashierId = e.CashierId,
            TransactionDate = e.TransactionDate,
            Subtotal = e.Subtotal,
            TaxTotal = e.TaxTotal,
            GrandTotal = e.GrandTotal,
            Status = (Contracts.Enum.TransactionStatus)(byte)e.Status,
            Notes = e.Notes
        };

        private static Transaction MapToEntity(TransactionDto d) => new()
        {
            TransactionId = d.TransactionId,
            ReceiptNumber = d.ReceiptNumber,
            ShiftId = d.ShiftId,
            CashierId = d.CashierId,
            TransactionDate = d.TransactionDate,
            Subtotal = d.Subtotal,
            TaxTotal = d.TaxTotal,
            GrandTotal = d.GrandTotal,
            Status = (DAL.Entities.TransactionStatus)(byte)d.Status,
            Notes = d.Notes
        };
    }
}