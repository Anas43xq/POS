using DAL.Entities;

namespace DAL.Interfaces
{
    public interface IReportRepository
    {
        Task<List<TransactionReportEntity>> GetTransactionsReportAsync(
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