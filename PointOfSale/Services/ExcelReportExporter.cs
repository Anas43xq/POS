using System.IO;
using ClosedXML.Excel;
using DAL.Entities;

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
            // ROW 3: BLANK
            // ================================================================
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
                }
                else if (request.ReportType == ReportType.Product)
                {
                    var summary = (ProductReportSummary)request.Summary;
                    WriteSummaryRow(ws, currentRow, "Total Qty Sold", summary.TotalQuantitySold, summaryLabelColor, summaryValueColor);
                    WriteSummaryRow(ws, currentRow, "Total Revenue", summary.TotalRevenue, summaryLabelColor, summaryValueColor, col: 3);
                    WriteSummaryRow(ws, currentRow, "Average Price", summary.AveragePrice, summaryLabelColor, summaryValueColor, col: 5);
                }

                currentRow += 2;
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
            string[] headers = reportType == ReportType.Transactions
                ? new[] { "Receipt Number", "Transaction Date", "Payment Method", "Grand Total", "Note" }
                : new[] { "Receipt Number", "Transaction Date", "Payment Method", "Quantity", "Line Total" };

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

        private static void WriteDataRows(
            IXLWorksheet ws,
            int startRow,
            ReportType reportType,
            object data)
        {
            int row = startRow;

            if (reportType == ReportType.Transactions && data is List<TransactionReportEntity> transactions)
            {
                foreach (var item in transactions)
                {
                    ws.Cell(row, 1).Value = item.ReceiptNumber;
                    ws.Cell(row, 2).Value = item.TransactionDate.ToString("g");
                    ws.Cell(row, 3).Value = item.PaymentMethod;
                    ws.Cell(row, 4).Value = item.GrandTotal;
                    ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 6).Value = item.Note;

                    ApplyRowBorder(ws, row, 6, borderColor: XLColor.FromArgb(229, 231, 235));
                    row++;
                }
            }
            else if (reportType == ReportType.Product && data is List<ProductReportEntity> products)
            {
                foreach (var item in products)
                {
                    ws.Cell(row, 1).Value = item.ReceiptNumber;
                    ws.Cell(row, 2).Value = item.TransactionDate.ToString("g");
                    ws.Cell(row, 3).Value = item.PaymentMethod;
                    ws.Cell(row, 4).Value = item.Quantity;
                    ws.Cell(row, 5).Value = item.LineTotal;
                    ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

                    ApplyRowBorder(ws, row, 5, borderColor: XLColor.FromArgb(229, 231, 235));
                    row++;
                }
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