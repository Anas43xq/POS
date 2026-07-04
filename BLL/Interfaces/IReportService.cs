using DAL.Entities;

namespace BLL.Interfaces
{
    public interface IReportService
    {
        Task<List<TransactionReportEntity>> GetTransactionReportAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate);

        Task<List<ProductReportEntity>> GetProductSalesReportAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate,
            int? productId);
    }
}