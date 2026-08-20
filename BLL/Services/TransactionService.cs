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

        public async Task<IEnumerable<TransactionListItemDto>> SearchByReceiptNumberAsync(
            string receiptNumber,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(receiptNumber))
                return Enumerable.Empty<TransactionListItemDto>();

            return await _transactionRepository.SearchByReceiptNumberAsync(receiptNumber.Trim(), ct);
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

        public async Task<TransactionDetailDto?> GetTransactionDetailAsync(int id, CancellationToken ct = default)
        {
            var entity = await _transactionRepository.GetTransactionDetailAsync(id, ct);
            return entity is null ? null : MapToDetailDto(entity);
        }

        public async Task UpdateTransactionAsync(TransactionDto transaction)
        {
            await _transactionRepository.UpdateAsync(MapToEntity(transaction));
        }

        public async Task DeleteTransactionAsync(int id)
        {
            await _transactionRepository.DeleteAsync(id);
        }

        public async Task<Result<TransactionDto>> VoidTransactionAsync(int transactionId, string? voidReason)
        {
            if (transactionId <= 0)
                return Result<TransactionDto>.Failure("Invalid transaction id.");

            var normalizedReason = string.IsNullOrWhiteSpace(voidReason)
                ? null
                : voidReason.Trim();

            bool updated = await _transactionRepository.VoidTransactionAsync(transactionId, normalizedReason);
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

        public async Task<Result<int>> CreateTransactionAsync(CreateTransactionRequest request)
        {
            var validationError = ValidateCreateTransactionRequest(request);
            if (validationError != null)
            {
                return Result<int>.Failure(validationError);
            }

            try
            {
                int transactionId = await _transactionCommandRepository.CreateTransactionAsync(request);
              
                return Result<int>.Success(transactionId);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ex.Message);
            }
        }

        private static string? ValidateCreateTransactionRequest(CreateTransactionRequest request)
        {
            if (request == null)
                return "Request is required.";

            if (request.CashierId <= 0)
                return "Invalid cashier.";

            if (request.ShiftId <= 0)
                return "Invalid shift.";

            if (request.Items == null || !request.Items.Any())
                return "Cart is empty.";

            if (request.PaymentMethod != "Cash" && request.PaymentMethod != "Card")
                return "Invalid payment method.";

            if (request.PaymentMethod == "Cash" && request.AmountTendered < request.GrandTotal)
                return "Cash received is less than total.";

            return null;
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
            Notes = e.Notes,
            VoidReason = e.VoidReason
        };

        private static TransactionDetailDto MapToDetailDto(Transaction e) => new()
        {
            TransactionId = e.TransactionId,
            ReceiptNumber = e.ReceiptNumber,
            TransactionDate = e.TransactionDate,
            Subtotal = e.Subtotal,
            TaxTotal = e.TaxTotal,
            GrandTotal = e.GrandTotal,
            Status = (Contracts.Enum.TransactionStatus)(byte)e.Status,
            Notes = e.Notes,
            PaymentMethod = e.Payments.FirstOrDefault()?.PaymentMethod ?? string.Empty,
            Items = e.TransactionItems.Select(MapToDetailItemDto).ToList()
        };

        private static TransactionDetailItemDto MapToDetailItemDto(TransactionItem i) => new()
        {
            TransactionItemId = i.TransactionItemId,
            ProductName = i.ProductName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal,
            Modifiers = i.ModifierItems.Select(MapToDetailItemModifierDto).ToList()
        };

        private static TransactionDetailItemModifierDto MapToDetailItemModifierDto(TransactionItemModifier m) => new()
        {
            GroupName = m.GroupName,
            OptionName = m.OptionName,
            Quantity = m.Quantity,
            PriceAdd = m.PriceAdd,
            LineTotal = m.LineTotal
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
            Notes = d.Notes,
            VoidReason = d.VoidReason
        };
    }
}