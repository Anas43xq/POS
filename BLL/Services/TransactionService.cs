using BLL.Interfaces;
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

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return await _transactionRepository.GetAllAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            await _transactionRepository.UpdateAsync(transaction);
        }

        public async Task DeleteTransactionAsync(int id)
        {
            await _transactionRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Creates a new transaction with strict shift validation.
        /// Rule: Transactions can only be created if the shift is OPEN.
        /// </summary>
        public async Task<int> CreateTransactionAsync(CreateTransactionRequest request)
        {
            ValidateCreateTransactionRequest(request);

            // Validate shift is open
            await ValidateShiftIsOpenAsync(request.ShiftId);

            return await _transactionCommandRepository.CreateTransactionAsync(request);
        }

        /// <summary>
        /// Validates the transaction request has all required fields.
        /// </summary>
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

        /// <summary>
        /// Validates that the shift exists and is in Open status.
        /// Throws an exception if shift is not open.
        /// </summary>
        private async Task ValidateShiftIsOpenAsync(int shiftId)
        {
            var shift = await _shiftRepository.GetByIdAsync(shiftId);
            
            if (shift == null)
                throw new InvalidOperationException(
                    "No active shift. Please start a shift first.");

            if (shift.Status != ShiftStatus.Open)
                throw new InvalidOperationException(
                    "No active shift. Please start a shift first.");
        }
    }
}
