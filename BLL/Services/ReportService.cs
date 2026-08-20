using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IReportRepository reportRepository,
            ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _logger = logger;
        }

        public async Task<List<TransactionReportDto>> GetTransactionReportAsync(
            string periodType, DateTime? fromDate, DateTime? toDate)
        {
            var entities = await _reportRepository.GetTransactionsReportAsync(periodType, fromDate, toDate);
            return entities.Select(e => new TransactionReportDto
            {
                TransactionId = e.TransactionId,
                ReceiptNumber = e.ReceiptNumber,
                TransactionDate = e.TransactionDate,
                PaymentMethod = e.PaymentMethod,
                GrandTotal = e.GrandTotal,
                Status = e.Status,
                Note = e.Note
            }).ToList();
        }

        public async Task<List<SalesAnalysisDto>> GetSalesAnalysisReportAsync(
            string periodType, DateTime? fromDate, DateTime? toDate)
        {
            var entities = await _reportRepository.GetSalesAnalysisReportAsync(periodType, fromDate, toDate);
            return entities.Select(e => new SalesAnalysisDto
            {
                CategoryId = e.CategoryId,
                CategoryName = e.CategoryName,
                ProductId = e.ProductId,
                ProductName = e.ProductName,
                SizeId = e.SizeId,
                SizeName = e.SizeName,
                SizeDisplayOrder = e.SizeDisplayOrder,
                Quantity = e.Quantity,
                LineTotal = e.LineTotal
            }).ToList();
        }

        public async Task<List<SalesChartBucketDto>> GetSalesChartAsync(
            string periodType, DateTime? fromDate, DateTime? toDate)
        {
            var rows = await _reportRepository.GetTransactionsReportAsync(periodType, fromDate, toDate);
            var mappedRows = rows.Select(e => new TransactionReportDto
            {
                TransactionId = e.TransactionId,
                ReceiptNumber = e.ReceiptNumber,
                TransactionDate = e.TransactionDate,
                PaymentMethod = e.PaymentMethod,
                GrandTotal = e.GrandTotal,
                Status = e.Status,
                Note = e.Note
            }).ToList();

            return BucketTransactionsForChart(mappedRows, periodType, fromDate, toDate);
        }

        public async Task<List<TopCategoryAggregateDto>> GetTopCategoriesAsync(
            string periodType, DateTime? fromDate, DateTime? toDate)
        {
            var rows = await _reportRepository.GetSalesAnalysisReportAsync(periodType, fromDate, toDate);

            return rows
                .GroupBy(r => new { r.CategoryId, r.CategoryName })
                .Select(g => new TopCategoryAggregateDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalSales = g.Sum(r => r.LineTotal),
                    Quantity = g.Sum(r => r.Quantity)
                })
                .OrderByDescending(c => c.TotalSales)
                .ToList();
        }

        public async Task<List<TopProductAggregateDto>> GetCategoryTopProductsAsync(
            int categoryId, string periodType, DateTime? fromDate, DateTime? toDate)
        {
            var rows = await _reportRepository.GetSalesAnalysisReportAsync(periodType, fromDate, toDate);

            return rows
                .Where(r => r.CategoryId == categoryId)
                .GroupBy(r => new { r.ProductId, r.ProductName })
                .Select(g => new TopProductAggregateDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalSales = g.Sum(r => r.LineTotal),
                    Quantity = g.Sum(r => r.Quantity)
                })
                .OrderByDescending(p => p.TotalSales)
                .ToList();
        }

        // Buckets stay ≤7 points regardless of period, per the Reports chart's server-side
        // aggregation requirement (docs/architecture.md Reporting Data Flow) — the mobile
        // client only ever receives already-summed points, never raw transaction rows.
        private static List<SalesChartBucketDto> BucketTransactionsForChart(
            List<TransactionReportDto> rows, string periodType, DateTime? fromDate, DateTime? toDate)
        {
            return periodType switch
            {
                "Today" => BucketByHourWindow(rows, windowHours: 4),
                "Month" => BucketByWeek(rows),
                "Custom" when fromDate.HasValue => BucketCustomRange(rows, fromDate.Value, toDate ?? fromDate.Value),
                _ => BucketByDay(rows) // "Week" and Custom-without-FromDate (falls back to Today server-side) default to daily buckets
            };
        }

        // A custom range can span anywhere from a day to a year+, so the bucket width scales
        // with the requested span to keep the chart at ≤7 bars instead of always bucketing by day.
        private static List<SalesChartBucketDto> BucketCustomRange(
            List<TransactionReportDto> rows, DateTime fromDate, DateTime toDate)
        {
            var spanDays = (toDate.Date - fromDate.Date).Days + 1;

            if (spanDays <= 7)
                return BucketByDay(rows);

            if (spanDays <= 49)
                return BucketByWeekFrom(rows, fromDate.Date);

            return BucketByMonthFrom(rows, fromDate.Date, toDate.Date);
        }

        private static List<SalesChartBucketDto> BucketByHourWindow(List<TransactionReportDto> rows, int windowHours)
        {
            return rows
                .GroupBy(r => r.TransactionDate.Hour / windowHours)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var startHour = g.Key * windowHours;
                    return new SalesChartBucketDto
                    {
                        Label = FormatHourWindowLabel(startHour),
                        TotalSales = g.Sum(r => r.GrandTotal)
                    };
                })
                .ToList();
        }

        private static List<SalesChartBucketDto> BucketByDay(List<TransactionReportDto> rows)
        {
            return rows
                .GroupBy(r => r.TransactionDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new SalesChartBucketDto
                {
                    Label = g.Key.ToString("ddd"),
                    TotalSales = g.Sum(r => r.GrandTotal)
                })
                .ToList();
        }

        private static List<SalesChartBucketDto> BucketByWeek(List<TransactionReportDto> rows)
        {
            if (rows.Count == 0)
                return new List<SalesChartBucketDto>();

            return BucketByWeekFrom(rows, rows.Min(r => r.TransactionDate.Date));
        }

        private static List<SalesChartBucketDto> BucketByWeekFrom(List<TransactionReportDto> rows, DateTime periodStart)
        {
            return rows
                .GroupBy(r => (r.TransactionDate.Date - periodStart).Days / 7)
                .OrderBy(g => g.Key)
                .Select(g => new SalesChartBucketDto
                {
                    Label = $"Wk {g.Key + 1}",
                    TotalSales = g.Sum(r => r.GrandTotal)
                })
                .ToList();
        }

        // Groups by a multi-month "block" (not a single calendar month) so a long range
        // (e.g. a year) still stays within the ≤7-bar cap instead of growing unbounded.
        private static List<SalesChartBucketDto> BucketByMonthFrom(
            List<TransactionReportDto> rows, DateTime periodStart, DateTime periodEnd)
        {
            const int MaxBars = 7;
            var totalMonths = ((periodEnd.Year - periodStart.Year) * 12) + periodEnd.Month - periodStart.Month + 1;
            var monthsPerBucket = Math.Max(1, (int)Math.Ceiling(totalMonths / (double)MaxBars));

            return rows
                .GroupBy(r =>
                {
                    var monthOffset = ((r.TransactionDate.Year - periodStart.Year) * 12) + r.TransactionDate.Month - periodStart.Month;
                    return monthOffset / monthsPerBucket;
                })
                .OrderBy(g => g.Key)
                .Select(g => new SalesChartBucketDto
                {
                    Label = FormatMonthBlockLabel(periodStart.AddMonths(g.Key * monthsPerBucket), monthsPerBucket),
                    TotalSales = g.Sum(r => r.GrandTotal)
                })
                .ToList();
        }

        private static string FormatMonthBlockLabel(DateTime blockStart, int monthsPerBucket)
        {
            if (monthsPerBucket == 1)
                return blockStart.ToString("MMM");

            var blockEnd = blockStart.AddMonths(monthsPerBucket - 1);
            return $"{blockStart:MMM}–{blockEnd:MMM}";
        }

        private static string FormatHourWindowLabel(int startHour)
        {
            var start = new DateTime(1, 1, 1, startHour % 24, 0, 0);
            return start.ToString("h tt").Replace(" ", "");
        }
    }
}
