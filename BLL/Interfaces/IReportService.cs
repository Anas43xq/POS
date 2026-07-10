using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IReportService
    {
        Task<List<TransactionReportDto>> GetTransactionReportAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate);

        Task<List<ProductReportDto>> GetProductSalesReportAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate,
            int? productId);
    }
}