using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using UI.Commands;
using UI.Configuration;
using UI.ViewModels;

namespace UI.Views.Cashier;

public partial class CashierDashboardView : UserControl
{
    // private readonly ShortcutSettings _shortcuts = App.ServiceProvider.GetRequiredService<ShortcutSettings>();

    public CashierDashboardView()
    {
        InitializeComponent();
        // Loaded += OnLoaded;
    }

    // private void OnLoaded(object sender, RoutedEventArgs e)
    // {
    //     if (DataContext is not CashierDashboardViewModel vm)
    //         return;
    //
    //     var k = _shortcuts.Cashier;
    //
    //     // Command shortcuts
    //     BindKey(k.CashPayment, vm.PayCashCommand);
    //     BindKey(k.CardPayment, vm.PayCardCommand);
    //     BindKey(k.CompleteSale, vm.CompleteSaleCommand);
    //     BindKey(k.NewSale, vm.NewSaleCommand);
    //     BindKey(k.RemoveItem, vm.RemoveSelectedSaleItemCommand);
    //     BindKey(k.IncreaseQuantity, vm.IncreaseSelectedQuantityCommand);
    //     BindKey(k.DecreaseQuantity, vm.DecreaseSelectedQuantityCommand);
    //     BindKey(k.ToggleShift, vm.ToggleShiftCommand);
    //     BindKey(k.ReprintLastReceipt, vm.ReprintLastReceiptCommand);
    //
    //     // Focus shortcuts
    //     BindKey(k.FocusSearch, () => SearchTextBox.Focus());
    //     BindKey(k.FocusCategories, () => CategoriesScrollViewer.Focus());
    //     BindKey(k.FocusProducts, () => ProductsItemsControl.Focus());
    //     BindKey(k.FocusCart, () => CartDataGrid.Focus());
    // }

    // private void BindKey(string keyGesture, ICommand command)
    // {
    //     if (TryParseKeyGesture(keyGesture, out var key, out var modifiers))
    //         InputBindings.Add(new KeyBinding(command, key, modifiers));
    // }

    // private void BindKey(string keyGesture, Action focusAction)
    // {
    //     if (TryParseKeyGesture(keyGesture, out var key, out var modifiers))
    //         InputBindings.Add(new KeyBinding(
    //             new RelayCommand(focusAction), key, modifiers));
    // }

    // private static bool TryParseKeyGesture(string gesture, out Key key, out ModifierKeys modifiers)
    // {
    //     key = Key.None;
    //     modifiers = ModifierKeys.None;
    //
    //     if (string.IsNullOrWhiteSpace(gesture))
    //         return false;
    //
    //     var parts = gesture.Split('+');
    //     var keyPart = parts[^1].Trim();
    //
    //     for (int i = 0; i < parts.Length - 1; i++)
    //     {
    //         var mod = parts[i].Trim().ToLowerInvariant();
    //         modifiers |= mod switch
    //         {
    //             "ctrl" or "control" => ModifierKeys.Control,
    //             "shift" => ModifierKeys.Shift,
    //             "alt" => ModifierKeys.Alt,
    //             _ => ModifierKeys.None
    //         };
    //     }
    //
    //     return Enum.TryParse<Key>(keyPart, ignoreCase: true, out key) && key != Key.None;
    // }
}