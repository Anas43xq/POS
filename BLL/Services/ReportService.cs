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

        public async Task<List<ProductReportDto>> GetProductSalesReportAsync(
            string periodType, DateTime? fromDate, DateTime? toDate, int? productId)
        {
            var entities = await _reportRepository.GetProductSalesReportAsync(periodType, fromDate, toDate, productId);
            return entities.Select(e => new ProductReportDto
            {
                ReceiptNumber = e.ReceiptNumber,
                TransactionDate = e.TransactionDate,
                PaymentMethod = e.PaymentMethod,
                Quantity = e.Quantity,
                LineTotal = e.LineTotal
            }).ToList();
        }
    }
}