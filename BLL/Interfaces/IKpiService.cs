using Contracts.Sales;
using Contracts.Transactions;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IKpiService
    {
        Task<KpiDto> GetKpisAsync(GetTransactionKpisRequest request, CancellationToken ct = default);
    }
}
