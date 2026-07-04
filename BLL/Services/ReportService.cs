using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<TransactionReportEntity>> GetTransactionReportAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate)
        {
            return await _reportRepository.GetTransactionsReportAsync(
                periodType, fromDate, toDate);
        }

        public async Task<List<ProductReportEntity>> GetProductSalesReportAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate,
            int? productId)
        {
            return await _reportRepository.GetProductSalesReportAsync(
                periodType, fromDate, toDate, productId);
        }
    }
}