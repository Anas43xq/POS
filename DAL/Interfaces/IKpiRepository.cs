using Contracts.Sales;
using Contracts.Transactions;
using System.Threading;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IKpiRepository
    {
        Task<KpiDto> GetKpisAsync(GetTransactionKpisRequest request, CancellationToken ct = default);
    }
}
