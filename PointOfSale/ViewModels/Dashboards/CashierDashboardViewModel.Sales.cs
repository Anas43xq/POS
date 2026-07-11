using BLL.DTOs;
using BLL.Models;
using Contracts.Transactions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UI.ViewModels;
using UI.Views;

namespace UI.ViewModels
{
    public partial class CashierDashboardViewModel
    {
        private async Task AddProductAsync(ProductDto? product)
        {
            if (product == null)
                return;

            CartItem? existingItem = SaleItems
                .FirstOrDefault(item => item.VariantId == product.VariantId
                                    && item.UnitPrice == product.UnitPrice);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                CartItem item = new CartItem
                {
                    VariantId = product.VariantId,
                    // English-only snapshot for receipts (TransactionItems.ProductName).
                    ProductName = product.EnglishDisplayName,
                    // Localized name for the cart UI.
                    LocalizedProductName = product.DisplayName,
                    Quantity = 1,
                    UnitPrice = product.UnitPrice,
                    TaxRate = product.TaxRate
                };
                SaleItems.Add(item);
            }

            await Task.CompletedTask;
        }

        private async Task RemoveSaleItemAsync(CartItem? item)
        {
            if (item == null)
                return;

            SaleItems.Remove(item);

            await Task.CompletedTask;
        }

        private void ClearSales()
        {
            if (!SaleItems.Any())
                return;

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to clear the current sale?",
                "Confirm Clear Sale",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            SaleItems.Clear();
        }

        private void RefreshTotals()
        {
            Subtotal = SaleItems.Sum(item => item.LineSubtotal);
            Tax = SaleItems.Sum(item => item.LineTax);
            Total = Subtotal + Tax;
        }

        private async Task PayCashAsync()
        {
            if (!TryValidatePaymentPrerequisites())
                return;

            await ShowPaymentDialogAsync();
        }

        private async Task PayCardAsync()
        {
            if (!TryValidatePaymentPrerequisites())
                return;

            var confirmViewModel = new CardPaymentConfirmDialogViewModel(Total, _localization);
            bool? confirmed = _dialogService.ShowDialogWithResult<CardPaymentConfirmDialog>(confirmViewModel);

            if (confirmed != true)
                return;

            await CreateCardPaymentAsync();
        }

        private async Task ShowPaymentDialogAsync()
        {
            var paymentViewModel = new PaymentDialogViewModel(
                Total,
                this,
                _transactionService,
                _session,
                _logger);

            paymentViewModel.PaymentCompleted += async (transactionId) =>
            {
                ClearCurrentSale();
                await LoadRecentSalesAsync();
                _ = _receiptDisplayService.PrintReceiptAsync(transactionId);
                _receiptDisplayService.ShowReceipt(transactionId);
            };

            _dialogService.ShowDialog<PaymentDialog>(paymentViewModel);
        }

        private async Task CreateCardPaymentAsync()
        {
            int transactionId = await _transactionService.CreateTransactionAsync(
                BuildCreatePaymentRequest(
                    paymentMethod: "Card",
                    amountTendered: Total,
                    changeGiven: 0m,
                    referenceNumber: null));

            ClearCurrentSale();
            await LoadRecentSalesAsync();
            _ = _receiptDisplayService.PrintReceiptAsync(transactionId);
            _receiptDisplayService.ShowReceipt(transactionId);
        }

        private bool TryValidatePaymentPrerequisites()
        {
            if (_session.CurrentUser == null)
                return ShowPrerequisiteError("No active user. Please sign in first.", "User Required");

            if (!IsShiftOpen)
                return ShowPrerequisiteError("No active shift. Please start a shift first.", "Shift Required");

            if (!SaleItems.Any())
                return ShowPrerequisiteError("Cart is empty.", "Sale Required");

            return true;
        }

        private static bool ShowPrerequisiteError(string message, string title)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        private CreateTransactionRequest BuildCreatePaymentRequest(
            string paymentMethod,
            decimal amountTendered,
            decimal changeGiven,
            string? referenceNumber)
        {
            return new CreateTransactionRequest
            {
                CashierId = _session.CurrentUser!.UserId,
                ShiftId = _session.CurrentShift!.ShiftId,
                Subtotal = Subtotal,
                TaxTotal = Tax,
                GrandTotal = Total,
                PaymentMethod = paymentMethod,
                AmountTendered = amountTendered,
                ChangeGiven = changeGiven,
                ReferenceNumber = referenceNumber,
                Items = BuildTransactionItems()
            };
        }

        private List<CreateTransactionItemRequest> BuildTransactionItems()
        {
            return SaleItems
                .Select(item => new CreateTransactionItemRequest
                {
                    VariantId = item.VariantId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TaxRate = item.TaxRate,
                    LineSubtotal = item.LineSubtotal,
                    LineTax = item.LineTax,
                    LineTotal = item.LineTotal
                })
                .ToList();
        }

        private async Task ShowRecentSalesAsync()
        {
            await LoadRecentSalesAsync();

            var recentSalesViewModel = new RecentSalesDialogViewModel(this, _receiptDisplayService);

            _dialogService.ShowDialog<RecentSalesDialog>(recentSalesViewModel);
        }

        private void LogoutAsync()
        {
            LogoutRequested?.Invoke();
        }

        public void ClearCurrentSale()
        {
            SaleItems.Clear();
        }

        private void IncreaseSelectedQuantity()
        {
            if (SelectedCartItem == null)
                return;

            SelectedCartItem.Quantity++;
            RefreshTotals();
        }

        private void DecreaseSelectedQuantity()
        {
            if (SelectedCartItem == null)
                return;

            if (SelectedCartItem.Quantity <= 1)
            {
                RemoveSelectedSaleItem();
                return;
            }

            SelectedCartItem.Quantity--;
            RefreshTotals();
        }

        private void RemoveSelectedSaleItem()
        {
            if (SelectedCartItem == null)
                return;

            SaleItems.Remove(SelectedCartItem);
            SelectedCartItem = SaleItems.LastOrDefault();
        }
    }
}
