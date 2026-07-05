using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using POS.Contracts.Receipts;

namespace DAL.Repositories
{
    public class ReceiptRepository : IReceiptRepository
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public ReceiptRepository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<ReceiptDetailsDto?> GetReceiptByTransactionIdAsync(int transactionId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            Transaction? transaction = await context.Transactions
                .AsNoTracking()
                .Include(t => t.Cashier)
                .Include(t => t.TransactionItems)
                .Include(t => t.Payments)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
                return null;

            return CreateReceiptDetailsDto(transaction);
        }

        private static ReceiptDetailsDto CreateReceiptDetailsDto(Transaction transaction)
        {
            Payment? payment = transaction.Payments.FirstOrDefault();

            return new ReceiptDetailsDto
            {
                TransactionId = transaction.TransactionId,
                ReceiptNumber = transaction.ReceiptNumber,
                TransactionDate = transaction.TransactionDate,

                CashierName = transaction.Cashier?.FullName ?? string.Empty,

                StoreName = "POS SYS",

                Subtotal = transaction.Subtotal,
                TaxTotal = transaction.TaxTotal,
                GrandTotal = transaction.GrandTotal,

                PaymentMethod = payment?.PaymentMethod ?? string.Empty,
                AmountTendered = payment?.AmountTendered ?? 0m,
                ChangeGiven = payment?.ChangeGiven ?? 0m,
                ReferenceNumber = payment?.ReferenceNumber,

                Items = CreateReceiptItemDtos(transaction.TransactionItems.ToList())
            };
        }

        private static List<ReceiptItemDto> CreateReceiptItemDtos(
            List<TransactionItem> transactionItems)
        {
            return transactionItems
                .Select(item => new ReceiptItemDto
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal
                })
                .ToList();
        }
    }
}

