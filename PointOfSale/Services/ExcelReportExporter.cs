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

            int currentRow = 3;

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
                else if (request.ReportType == ReportType.SalesAnalysis)
                {
                    var summary = (SalesAnalysisReportSummary)request.Summary;
                    WriteSummaryRow(ws, currentRow, "Categories Sold", summary.CategoriesSold, summaryLabelColor, summaryValueColor);
                    WriteSummaryRow(ws, currentRow, "Products Sold", summary.ProductsSold, summaryLabelColor, summaryValueColor, col: 3);
                    WriteSummaryRow(ws, currentRow, "Variants Sold", summary.VariantsSold, summaryLabelColor, summaryValueColor, col: 5);
                    currentRow++;
                    WriteSummaryRow(ws, currentRow, "Total Quantity Sold", summary.TotalQuantitySold, summaryLabelColor, summaryValueColor);
                    WriteSummaryRow(ws, currentRow, "Total Sales", summary.TotalSales, summaryLabelColor, summaryValueColor, col: 3);
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

private static readonly string[] SalesAnalysisHeaders =
{
    "Category / Product / Size",
    "Quantity Sold",
    "Total Sales"
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
        ReportType.SalesAnalysis => SalesAnalysisHeaders,

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
            else if (reportType == ReportType.SalesAnalysis && data is IEnumerable<SalesAnalysisDto> salesAnalysis)
            {
                WriteSalesAnalysisRows(ws, ref row, salesAnalysis.ToList());
            }
        }

        // ================================================================
        // SALES ANALYSIS — hierarchical Category -> Product -> Size
        // rendering with Excel Outline Grouping for expand/collapse.
        //
        // Perf: style templates are built once on a scratch row (10000) and
        // applied per cell via `cell.Style = templateCell.Style`, which copies
        // all properties in a single internal operation.  The old pattern of
        // setting Font.Bold / Font.FontColor / Alignment.Indent individually
        // on every cell triggered a separate ClosedXML style-table lookup
        // per property, which is O(rows × properties) and very slow.
        // ================================================================
        private static void WriteSalesAnalysisRows(
            IXLWorksheet ws,
            ref int row,
            List<SalesAnalysisDto> list)
        {
            var categoryColor = XLColor.FromArgb(21, 101, 192);
            var productColor = XLColor.FromArgb(51, 51, 51);
            var borderColor = XLColor.FromArgb(229, 231, 235);

            const int scratch = 10000;

            // ---------- Pre-build style templates (once) ----------
            // Category header  (col 1): bold + categoryColor
            var catHdrCell = ws.Cell(scratch, 1);
            catHdrCell.Style.Font.Bold = true;
            catHdrCell.Style.Font.FontColor = categoryColor;
            var catHdrStyle = catHdrCell.Style;

            // Product header  (col 1): bold + productColor + indent=1
            var prodHdrCell = ws.Cell(scratch, 2);
            prodHdrCell.Style.Font.Bold = true;
            prodHdrCell.Style.Font.FontColor = productColor;
            prodHdrCell.Style.Alignment.Indent = 1;
            var prodHdrStyle = prodHdrCell.Style;

            // Variant label  (col 1): indent=2, no bold
            var varLblCell = ws.Cell(scratch, 3);
            varLblCell.Style.Alignment.Indent = 2;
            var varLblStyle = varLblCell.Style;

            // Variant qty    (col 2): number format #,##0
            var varQtyCell = ws.Cell(scratch, 4);
            varQtyCell.Style.NumberFormat.Format = "#,##0";
            var varQtyStyle = varQtyCell.Style;

            // Variant total  (col 3): number format #,##0.00
            var varTotCell = ws.Cell(scratch, 5);
            varTotCell.Style.NumberFormat.Format = "#,##0.00";
            var varTotStyle = varTotCell.Style;

            // Product total label (col 1): bold + indent=1
            var prodTotLblCell = ws.Cell(scratch, 6);
            prodTotLblCell.Style.Font.Bold = true;
            prodTotLblCell.Style.Alignment.Indent = 1;
            var prodTotLblStyle = prodTotLblCell.Style;

            // Product total qty   (col 2): bold + #,##0
            var prodTotQtyCell = ws.Cell(scratch, 7);
            prodTotQtyCell.Style.Font.Bold = true;
            prodTotQtyCell.Style.NumberFormat.Format = "#,##0";
            var prodTotQtyStyle = prodTotQtyCell.Style;

            // Product total value (col 3): bold + #,##0.00
            var prodTotValCell = ws.Cell(scratch, 8);
            prodTotValCell.Style.Font.Bold = true;
            prodTotValCell.Style.NumberFormat.Format = "#,##0.00";
            var prodTotValStyle = prodTotValCell.Style;

            // Category total label (col 1): bold + categoryColor
            var catTotLblCell = ws.Cell(scratch, 9);
            catTotLblCell.Style.Font.Bold = true;
            catTotLblCell.Style.Font.FontColor = categoryColor;
            var catTotLblStyle = catTotLblCell.Style;

            // Category total qty   (col 2): bold + #,##0
            var catTotQtyCell = ws.Cell(scratch, 10);
            catTotQtyCell.Style.Font.Bold = true;
            catTotQtyCell.Style.NumberFormat.Format = "#,##0";
            var catTotQtyStyle = catTotQtyCell.Style;

            // Category total value (col 3): bold + #,##0.00
            var catTotValCell = ws.Cell(scratch, 11);
            catTotValCell.Style.Font.Bold = true;
            catTotValCell.Style.NumberFormat.Format = "#,##0.00";
            var catTotValStyle = catTotValCell.Style;

            // ---------- Build report ----------
            var categoryGroups = list
                .GroupBy(r => new { r.CategoryId, r.CategoryName })
                .OrderBy(g => g.Key.CategoryName);

            foreach (var categoryGroup in categoryGroups)
            {
                // ---- Category header row (always visible; level 0) ----
                var c = ws.Cell(row, 1);
                c.Value = categoryGroup.Key.CategoryName;
                c.Style = catHdrStyle;
                ws.Row(row).OutlineLevel = 0;
                ApplyRowBorder(ws, row, 3, borderColor);
                row++;

                int categoryQty = 0;
                decimal categoryTotal = 0m;

                var productGroups = categoryGroup
                    .GroupBy(r => new { r.ProductId, r.ProductName })
                    .OrderBy(g => g.Key.ProductName);

                foreach (var productGroup in productGroups)
                {
                    // ---- Product header row (level 1) ----
                    var pc = ws.Cell(row, 1);
                    pc.Value = productGroup.Key.ProductName;
                    pc.Style = prodHdrStyle;
                    ws.Row(row).OutlineLevel = 1;
                    ApplyRowBorder(ws, row, 3, borderColor);
                    row++;

                    int productQty = 0;
                    decimal productTotal = 0m;

                    // ---- Size / variant rows (level 2) ----
                    foreach (var variant in productGroup.OrderBy(v => v.SizeDisplayOrder))
                    {
                        var vc = ws.Cell(row, 1);
                        vc.Value = variant.SizeName;
                        vc.Style = varLblStyle;

                        var vq = ws.Cell(row, 2);
                        vq.Value = variant.Quantity;
                        vq.Style = varQtyStyle;

                        var vt = ws.Cell(row, 3);
                        vt.Value = variant.LineTotal;
                        vt.Style = varTotStyle;

                        ws.Row(row).OutlineLevel = 2;
                        ApplyRowBorder(ws, row, 3, borderColor);
                        row++;

                        productQty += variant.Quantity;
                        productTotal += variant.LineTotal;
                    }

                    // ---- Product Total row (level 1) ----
                    var ptc = ws.Cell(row, 1);
                    ptc.Value = $"{productGroup.Key.ProductName} Total";
                    ptc.Style = prodTotLblStyle;

                    var ptq = ws.Cell(row, 2);
                    ptq.Value = productQty;
                    ptq.Style = prodTotQtyStyle;

                    var ptv = ws.Cell(row, 3);
                    ptv.Value = productTotal;
                    ptv.Style = prodTotValStyle;

                    ws.Row(row).OutlineLevel = 1;
                    ApplyRowBorder(ws, row, 3, borderColor);
                    row++;

                    categoryQty += productQty;
                    categoryTotal += productTotal;
                }

                // ---- Category Total row (always visible; level 0) ----
                var ctc = ws.Cell(row, 1);
                ctc.Value = $"{categoryGroup.Key.CategoryName} Total";
                ctc.Style = catTotLblStyle;

                var ctq = ws.Cell(row, 2);
                ctq.Value = categoryQty;
                ctq.Style = catTotQtyStyle;

                var ctv = ws.Cell(row, 3);
                ctv.Value = categoryTotal;
                ctv.Style = catTotValStyle;

                ws.Row(row).OutlineLevel = 0;
                ApplyRowBorder(ws, row, 3, borderColor);
                row++;
            }

            // Summary rows sit below their detail rows (Product Total below
            // its Size rows, Category Total below its Product blocks), so
            // collapsing a group hides the detail rows above the summary.
            ws.Outline.SummaryVLocation = XLOutlineSummaryVLocation.Bottom;
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

    public class SalesAnalysisReportSummary
    {
        public string CategoriesSold { get; set; } = "0";
        public string ProductsSold { get; set; } = "0";
        public string VariantsSold { get; set; } = "0";
        public string TotalQuantitySold { get; set; } = "0";
        public string TotalSales { get; set; } = "AED 0.00";
    }
}
