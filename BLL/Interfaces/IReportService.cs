using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IReportService
    {
        Task<List<TransactionReportDto>> GetTransactionReportAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate);

        Task<List<SalesAnalysisDto>> GetSalesAnalysisReportAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate);

        /// <summary>
        /// Sales-by-period-bucket series for the mobile Reports chart, capped at ~7 points
        /// (by day for Week, by weekly bucket for Month, by ~4hr window for Today).
        /// </summary>
        Task<List<SalesChartBucketDto>> GetSalesChartAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate);

        /// <summary>
        /// Sales analysis rows grouped by category, ranked by sales total, for the mobile
        /// Reports top-categories list.
        /// </summary>
        Task<List<TopCategoryAggregateDto>> GetTopCategoriesAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate);

        /// <summary>
        /// Sales analysis rows for one category, grouped by product, ranked by sales total,
        /// for the mobile category drill-down screen.
        /// </summary>
        Task<List<TopProductAggregateDto>> GetCategoryTopProductsAsync(
            int categoryId,
            string periodType,
            DateTime? fromDate,
            DateTime? toDate);
    }
}
