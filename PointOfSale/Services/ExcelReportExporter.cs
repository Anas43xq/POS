using System.Collections.Generic;
using System.IO;
using System.Linq;
using BLL.DTOs;
using ClosedXML.Excel;
using POS.Contracts.Receipts;

namespace UI.Services
{
    public class ExcelReportExporter
    {
        public byte[] Export(ExcelReportRequest request)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Report");

            // ================================================================
            // STYLES
            // ================================================================
            var titleFontColor = XLColor.FromArgb(21, 101, 192);
            var headerBg = XLColor.FromArgb(21, 101, 192);
            var headerFontColor = XLColor.White;
            var summaryLabelColor = XLColor.FromArgb(107, 114, 128);
            var summaryValueColor = XLColor.FromArgb(51, 51, 51);

            // ================================================================
            // COLUMN WIDTHS (uniform spacing)
            // ================================================================
            ws.Column(1).Width = 22;
            ws.Column(2).Width = 22;
            ws.Column(3).Width = 18;
            ws.Column(4).Width = 16;
            ws.Column(5).Width = 16;
            ws.Column(6).Width = 20;

            // ================================================================
            // ROW 1: TITLE (height: 30px for spacing)
            // ================================================================
            ws.Row(1).Height = 30;
            ws.Cell(1, 1).Value = request.Title;
            ws.Range(1, 1, 1, 6).Merge();
            ws.Cell(1, 1).Style.Font.FontSize = 18;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontColor = titleFontColor;
            ws.Cell(1, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            // ================================================================
            // ROW 2: DATE RANGE
            // ================================================================
            string dateLabel = request.FromDate.Date == request.ToDate.Date
                ? request.FromDate.ToString("dd/MM/yyyy")
                : $"From {request.FromDate:dd/MM/yyyy} → To {request.ToDate:dd/MM/yyyy}";
            ws.Cell(2, 1).Value = dateLabel;
            ws.Range(2, 1, 2, 6).Merge();
            ws.Cell(2, 1).Style.Font.FontSize = 11;
            ws.Cell(2, 1).Style.Font.FontColor = summaryLabelColor;

            // ================================================================
            // ROW 3: PRODUCT NAME (if product mode)
            // ================================================================
            if (request.ReportType == ReportType.ProductReport && !string.IsNullOrEmpty(request.ProductName))
            {
                ws.Cell(3, 1).Value = $"Product: {request.ProductName}";
                ws.Range(3, 1, 3, 6).Merge();
                ws.Cell(3, 1).Style.Font.FontSize = 12;
                ws.Cell(3, 1).Style.Font.Bold = true;
                ws.Cell(3, 1).Style.Font.FontColor = summaryLabelColor;
            }
            int currentRow = 4;

            // ================================================================
            // SUMMARY SECTION
            // ================================================================
            if (request.Summary != null)
            {
                if (request.ReportType == ReportType.Transactions)
                {
                    var summary = (TransactionsReportSummary)request.Summary;
                    WriteSummaryRow(ws, currentRow, "Total Orders", summary.TotalOrders, summaryLabelColor, summaryValueColor);
                    WriteSummaryRow(ws, currentRow, "Total Sales", summary.TotalSales, summaryLabelColor, summaryValueColor, col: 3);
                    WriteSummaryRow(ws, currentRow, "Cash Total", summary.CashTotal, summaryLabelColor, summaryValueColor, col: 5);
                    currentRow++;
                    WriteSummaryRow(ws, currentRow, "", "", summaryLabelColor, summaryValueColor);
                    WriteSummaryRow(ws, currentRow, "", "", summaryLabelColor, summaryValueColor, col: 3);
                    WriteSummaryRow(ws, currentRow, "Card Total", summary.CardTotal, summaryLabelColor, summaryValueColor, col: 5);
                    currentRow += 2;
                }
                else if (request.ReportType == ReportType.ProductReport)
                {
                    var summary = (ProductReportSummary)request.Summary;
                    WriteSummaryRow(ws, currentRow, "Total Quantity Sold", summary.TotalQuantitySold, summaryLabelColor, summaryValueColor);
                    WriteSummaryRow(ws, currentRow, "Total Revenue", summary.TotalRevenue, summaryLabelColor, summaryValueColor, col: 3);
                    WriteSummaryRow(ws, currentRow, "Average Price", summary.AveragePrice, summaryLabelColor, summaryValueColor, col: 5);
                    currentRow += 2;
                }
            }

            // ================================================================
            // DATA TABLE HEADERS
            // ================================================================
            WriteTableHeaders(ws, currentRow, request.ReportType, headerBg, headerFontColor);
            currentRow++;

            // ================================================================
            // DATA ROWS
            // ================================================================
            WriteDataRows(ws, currentRow, request.ReportType, request.Data);

            // ================================================================
            // FINAL FORMAT
            // ================================================================
            ws.Columns().AdjustToContents();

            // ================================================================
            // OUTPUT
            // ================================================================
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ================================================================
        // HELPERS
        // ================================================================

        private static void WriteSummaryRow(
            IXLWorksheet ws,
            int row,
            string label,
            string value,
            XLColor labelColor,
            XLColor valueColor,
            int col = 1)
        {
            ws.Cell(row, col).Value = label;
            ws.Cell(row, col).Style.Font.FontColor = labelColor;
            ws.Cell(row, col).Style.Font.FontSize = 11;

            ws.Cell(row, col + 1).Value = value;
            ws.Cell(row, col + 1).Style.Font.FontColor = valueColor;
            ws.Cell(row, col + 1).Style.Font.Bold = true;
            ws.Cell(row, col + 1).Style.Font.FontSize = 11;
        }

        private static void WriteTableHeaders(
            IXLWorksheet ws,
            int row,
            ReportType reportType,
            XLColor bg,
            XLColor fontColor)
        {
            string[] headers = BuildHeader(reportType);

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(row, i + 1);
                cell.Value = headers[i];
                cell.Style.Fill.BackgroundColor = bg;
                cell.Style.Font.FontColor = fontColor;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 11;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.BottomBorderColor = XLColor.FromArgb(229, 231, 235);
            }
        }

private static readonly string[] PurchaseHeaders =
{
    "Invoice No",
    "Supplier",
    "Invoice Date",
    "Amount",
    "VAT",
    "Total",
    "Note"
};

private static readonly string[] ProductReportHeaders =
{
    "Receipt Number",
    "Transaction Date",
    "Payment Method",
    "Quantity",
    "Line Total"
};

private static string[] BuildHeader(ReportType reportType)
{
    return reportType switch
    {
        ReportType.Transactions => new[]
        {
            "Receipt Number",
            "Transaction Date",
            "Payment Method",
            "Grand Total",
            "Note"
        },

        ReportType.VatPurchaseRegister => PurchaseHeaders,
        ReportType.NonVatPurchaseRegister => PurchaseHeaders,
        ReportType.ProductReport => ProductReportHeaders,

        _ => throw new ArgumentOutOfRangeException(nameof(reportType), reportType, null)
    };
}

        private static void WriteDataRows(
            IXLWorksheet ws,
            int startRow,
            ReportType reportType,
            object data)
        {
            int row = startRow;

            if (reportType == ReportType.Transactions && data is IEnumerable<TransactionReportDto> transactions)
            {
                foreach (var item in transactions)
                {
                    ws.Cell(row, 1).Value = item.ReceiptNumber;
                    ws.Cell(row, 2).Value = item.TransactionDate.ToString("g");
                    ws.Cell(row, 3).Value = item.PaymentMethod;
                    ws.Cell(row, 4).Value = item.GrandTotal;
                    ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 5).Value = item.Note;

                    ApplyRowBorder(ws, row, 5, borderColor: XLColor.FromArgb(229, 231, 235));
                    row++;
                }
            }
            else if ((reportType == ReportType.VatPurchaseRegister || reportType == ReportType.NonVatPurchaseRegister)
                     && data is IEnumerable<PurchaseReceiptDto> receipts)
            {
                var list = receipts.ToList();
                bool isVat = reportType == ReportType.VatPurchaseRegister;

                foreach (var item in list)
                {
                    // 1:1 with PurchaseHeaders: Invoice No, Supplier, Invoice Date, Amount, VAT, Total, Note
                    ws.Cell(row, 1).Value = item.InvoiceNumber;
                    ws.Cell(row, 2).Value = item.SupplierName;
                    ws.Cell(row, 3).Value = item.InvoiceDate.ToString("dd/MM/yyyy");
                    ws.Cell(row, 4).Value = item.Subtotal;
                    ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 5).Value = item.VatAmount;
                    ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 6).Value = item.GrandTotal;
                    ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 7).Value = item.Notes;

                    ApplyRowBorder(ws, row, 7, borderColor: XLColor.FromArgb(229, 231, 235));
                    row++;
                }

                var totalAmount = list.Sum(r => r.Subtotal);
                var totalVat = list.Sum(r => r.VatAmount);
                var totalGrand = list.Sum(r => r.GrandTotal);

                if (isVat)
                    WritePurchaseTotals(ws, row, totalAmount, totalVat, totalGrand, XLColor.FromArgb(21, 101, 192));
                else
                    WriteNonVatTotals(ws, row, totalAmount, XLColor.FromArgb(21, 101, 192));
            }
            else if (reportType == ReportType.ProductReport && data is IEnumerable<ProductReportDto> products)
            {
                var list = products.ToList();
                foreach (var item in list)
                {
                    // 1:1 with ProductReportHeaders: Receipt Number, Transaction Date, Payment Method, Quantity, Line Total
                    ws.Cell(row, 1).Value = item.ReceiptNumber;
                    ws.Cell(row, 2).Value = item.TransactionDate.ToString("g");
                    ws.Cell(row, 3).Value = item.PaymentMethod;
                    ws.Cell(row, 4).Value = item.Quantity;
                    ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, 5).Value = item.LineTotal;
                    ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

                    ApplyRowBorder(ws, row, 5, borderColor: XLColor.FromArgb(229, 231, 235));
                    row++;
                }

                var totalQty = list.Sum(p => p.Quantity);
                var totalLine = list.Sum(p => p.LineTotal);
                WriteProductTotals(ws, row, totalQty, totalLine, XLColor.FromArgb(21, 101, 192));
            }
        }

        private static void ApplyRowBorder(IXLWorksheet ws, int row, int columns, XLColor borderColor)
        {
            for (int c = 1; c <= columns; c++)
            {
                ws.Cell(row, c).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell(row, c).Style.Border.BottomBorderColor = borderColor;
            }
        }

        private static void WritePurchaseTotals(IXLWorksheet ws, int row, decimal taxableAmount, decimal vatAmount, decimal grandTotal, XLColor headerColor)
        {
            ws.Cell(row, 1).Value = "Totals";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontColor = headerColor;
            ws.Cell(row, 4).Value = taxableAmount;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 5).Value = vatAmount;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 6).Value = grandTotal;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Style.Font.Bold = true;
        }

        private static void WriteNonVatTotals(IXLWorksheet ws, int row, decimal totalExpenses, XLColor headerColor)
        {
            ws.Cell(row, 1).Value = "Total Expenses";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontColor = headerColor;
            ws.Cell(row, 4).Value = totalExpenses;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 6).Value = totalExpenses;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Style.Font.Bold = true;
        }

        private static void WriteProductTotals(IXLWorksheet ws, int row, int totalQuantity, decimal totalLineTotal, XLColor headerColor)
        {
            ws.Cell(row, 1).Value = "Totals";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontColor = headerColor;
            ws.Cell(row, 4).Value = totalQuantity;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 5).Value = totalLineTotal;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Style.Font.Bold = true;
        }
    }

    // ================================================================
    // SUMMARY DTOs
    // ================================================================
    public class TransactionsReportSummary
    {
        public string TotalOrders { get; set; } = "0";
        public string TotalSales { get; set; } = "AED 0.00";
        public string CashTotal { get; set; } = "AED 0.00";
        public string CardTotal { get; set; } = "AED 0.00";
    }

    public class ProductReportSummary
    {
        public string TotalQuantitySold { get; set; } = "0";
        public string TotalRevenue { get; set; } = "AED 0.00";
        public string AveragePrice { get; set; } = "AED 0.00";
    }
}