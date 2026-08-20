using Contracts.Sales;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Interfaces
{
    public interface IRecentSaleRepository
    {
        public Task<List<RecentTransactionDto>> GetRecentTransactionsByCashierId(int cashierId, int shiftId, int take = 10, CancellationToken ct = default);
    }
}
