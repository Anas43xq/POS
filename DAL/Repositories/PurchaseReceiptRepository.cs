using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using POS.Contracts.Receipts;

namespace DAL.Repositories
{
    public class PurchaseReceiptRepository : IPurchaseReceiptRepository
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public PurchaseReceiptRepository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<PurchaseReceipt>> GetAllAsync(PurchaseReceiptSearchRequest? request = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            IQueryable<PurchaseReceipt> query = context.PurchaseReceipts
                .AsNoTracking()
                .Include(p => p.ReceiptType)
                .Include(p => p.Supplier)
                .Include(p => p.CreatedByUser);

            if (request is not null)
            {
                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    var search = request.SearchText.Trim();
                    query = query.Where(p =>
                        p.InvoiceNumber.Contains(search) ||
                        (p.Description != null && p.Description.Contains(search)) ||
                        (p.Supplier != null && p.Supplier.CompanyName.Contains(search)) ||
                        p.Category.Contains(search));
                }

                if (request.FromDate.HasValue)
                {
                    query = query.Where(p => p.InvoiceDate >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(p => p.InvoiceDate <= request.ToDate.Value);
                }

                if (request.SupplierId.HasValue)
                {
                    query = query.Where(p => p.SupplierId == request.SupplierId.Value);
                }

                if (!string.IsNullOrWhiteSpace(request.Category))
                {
                    query = query.Where(p => p.Category == request.Category);
                }

                if (request.ReceiptTypeId.HasValue)
                {
                    query = query.Where(p => p.ReceiptTypeId == request.ReceiptTypeId.Value);
                }
            }

            return await query
                .OrderByDescending(p => p.InvoiceDate)
                .ThenByDescending(p => p.ReceiptId)
                .ToListAsync();
        }

        public async Task<PurchaseReceipt?> GetByIdAsync(int receiptId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.PurchaseReceipts
                .AsNoTracking()
                .Include(p => p.ReceiptType)
                .Include(p => p.Supplier)
                .Include(p => p.CreatedByUser)
                .FirstOrDefaultAsync(p => p.ReceiptId == receiptId);
        }

        public async Task AddAsync(PurchaseReceipt receipt)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.PurchaseReceipts.Add(receipt);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PurchaseReceipt receipt)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.PurchaseReceipts.Update(receipt);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int receiptId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var receipt = await context.PurchaseReceipts.FindAsync(receiptId);
            if (receipt is not null)
            {
                context.PurchaseReceipts.Remove(receipt);
                await context.SaveChangesAsync();
            }
        }
    }
}
