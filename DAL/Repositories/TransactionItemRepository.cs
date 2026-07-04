using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories
{
    public class TransactionItemRepository : Repository<TransactionItem>, ITransactionItemRepository
    {
        public TransactionItemRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
        {
        }
    }
}

