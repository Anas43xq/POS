using POS.Contracts.Printing;
using POS.Contracts.Receipts;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using UI.Views;

namespace UI.Services
{
    public class ReceiptPrinterService : IReceiptPrinter
    {
        public void Print(ReceiptDetailsDto receipt)
        {
            var view = new ReceiptPrintView
            {
                DataContext = receipt
            };

            view.Measure(new Size(300, double.PositiveInfinity));
            view.Arrange(new Rect(0, 0, 300, view.DesiredSize.Height));
            view.UpdateLayout();

            var dialog = new PrintDialog();

            if (dialog.ShowDialog() == true)
            {
                dialog.PrintVisual(view, "Receipt");
            }
        }
    }
}