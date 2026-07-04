using BLL.Interfaces;
using Contracts.Sales;
using Contracts.Transactions;
using DAL.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class KpiService : IKpiService
    {
        private readonly IKpiRepository _kpiRepository;

        public KpiService(IKpiRepository kpiRepository)
        {
            _kpiRepository = kpiRepository;
        }

        public async Task<KpiDto> GetKpisAsync(GetTransactionKpisRequest request, CancellationToken ct = default)
        {
            var dto = await _kpiRepository.GetKpisAsync(request, ct);

            // apply any business validation/rounding here if needed
            return dto;
        }
    }
}
