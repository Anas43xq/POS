using System.IO;
using POS.Contracts.Printing;
using POS.Contracts.Receipts;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace UI.Services;

/// <summary>
/// Renders a <see cref="ReceiptDetailsDto"/> as a real PDF file using
/// QuestPDF. Used by the "Print Test Receipt" action when the manager
/// picks "Save to file" on the Settings page. No dialog, no printer.
/// </summary>
public sealed class ReceiptPdfWriter : IReceiptFileWriter
{
    static ReceiptPdfWriter()
    {
        // Community license opt-in required once per process
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task SaveReceiptAsPdfAsync(
        ReceiptDetailsDto receipt,
        string filePath,
        int paperWidthMm,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        // Create directory if needed to avoid "directory not found"
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // Wrap sync Document.Create in Task.Run for proper async/cancellation support
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Flexible height allows natural receipt wrapping
                    page.Size(new PageSize(
                        width: paperWidthMm > 0 ? paperWidthMm : 80,
                        height: 297,    // A4 height; receipts are shorter
                        unit: Unit.Millimetre));
                    page.Margin(4, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(t => t.FontSize(9).FontFamily(Fonts.Calibri));

                    page.Content().Column(column =>
                    {
                        BuildHeader(column, receipt);
                        BuildItems(column, receipt);
                        BuildTotals(column, receipt);
                        BuildPayment(column, receipt);
                        BuildFooter(column);
                    });
                });
            });

            document.GeneratePdf(filePath);
        }, cancellationToken);
    }

    private static void BuildHeader(ColumnDescriptor column, ReceiptDetailsDto receipt)
    {
        if (!string.IsNullOrWhiteSpace(receipt.StoreName))
        {
            column.Item().AlignCenter().Text(receipt.StoreName)
                .FontSize(12).Bold();
        }

        column.Item().AlignCenter().Text("SALES RECEIPT")
            .FontSize(10).SemiBold();

        column.Item().PaddingTop(2).Row(row =>
        {
            row.RelativeItem().Text($"Receipt: {receipt.ReceiptNumber}");
            row.ConstantItem(40).AlignRight().Text("");
        });

        column.Item().Text($"Date: {receipt.TransactionDate:yyyy-MM-dd HH:mm}");

        if (!string.IsNullOrWhiteSpace(receipt.CashierName))
        {
            column.Item().Text($"Cashier: {receipt.CashierName}");
        }

        column.Item().PaddingTop(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
    }

    private static void BuildItems(ColumnDescriptor column, ReceiptDetailsDto receipt)
    {
        column.Item().PaddingTop(4).Text("ITEMS").Bold();

        if (receipt.Items is null || receipt.Items.Count == 0)
        {
            return;
        }

        foreach (var item in receipt.Items)
        {
            column.Item().PaddingTop(2).Column(itemCol =>
            {
                if (!string.IsNullOrWhiteSpace(item.ProductName))
                {
                    itemCol.Item().Text(item.ProductName).SemiBold();
                }

                // Non-default modifier lines (match the WPF visual).
                if (item.Modifiers is not null)
                {
                    foreach (var mod in item.Modifiers)
                    {
                        var qtyText = mod.Quantity > 1 ? $"{mod.Quantity} × " : string.Empty;
                        var priceText = mod.PriceAdd > 0
                            ? $" (+{mod.PriceAdd:0.00})"
                            : string.Empty;
                        itemCol.Item().PaddingLeft(2).Text(
                                $"• {qtyText}{mod.OptionName}{priceText}")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                    }
                }

                itemCol.Item().Row(qtyRow =>
                {
                    qtyRow.RelativeItem().Text(
                        $"{item.Quantity}  ×  {item.UnitPrice:0.00}");
                    qtyRow.ConstantItem(60).AlignRight().Text(
                        $"{item.LineTotal:0.00}").Bold();
                });
            });
        }

        column.Item().PaddingTop(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
    }

    private static void BuildTotals(ColumnDescriptor column, ReceiptDetailsDto receipt)
    {
        column.Item().PaddingTop(2).Row(row =>
        {
            row.RelativeItem().Text("Subtotal");
            row.ConstantItem(60).AlignRight().Text($"{receipt.Subtotal:0.00}");
        });
        column.Item().Row(row =>
        {
            row.RelativeItem().Text("Tax");
            row.ConstantItem(60).AlignRight().Text($"{receipt.TaxTotal:0.00}");
        });
        column.Item().Row(row =>
        {
            row.RelativeItem().Text("Discount");
            row.ConstantItem(60).AlignRight().Text($"{receipt.DiscountTotal:0.00}");
        });

        column.Item().PaddingTop(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

        column.Item().Row(row =>
        {
            row.RelativeItem().Text("GRAND TOTAL").Bold().FontSize(11);
            row.ConstantItem(70).AlignRight().Text($"{receipt.GrandTotal:0.00}")
                .Bold().FontSize(11);
        });

        column.Item().PaddingTop(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
    }

    private static void BuildPayment(ColumnDescriptor column, ReceiptDetailsDto receipt)
    {
        column.Item().PaddingTop(4).Row(row =>
        {
            row.RelativeItem().Text("Payment:");
            row.ConstantItem(60).AlignRight().Text(
                string.IsNullOrWhiteSpace(receipt.PaymentMethod)
                    ? string.Empty
                    : receipt.PaymentMethod);
        });

        column.Item().Row(row =>
        {
            row.RelativeItem().Text("Amount Tendered:");
            row.ConstantItem(60).AlignRight().Text($"{receipt.AmountTendered:0.00}");
        });

        column.Item().Row(row =>
        {
            row.RelativeItem().Text("Change:");
            row.ConstantItem(60).AlignRight().Text($"{receipt.ChangeGiven:0.00}");
        });
    }

    private static void BuildFooter(ColumnDescriptor column)
    {
        column.Item().PaddingTop(6).AlignCenter()
            .Text("Thank you!").SemiBold();
    }
}
