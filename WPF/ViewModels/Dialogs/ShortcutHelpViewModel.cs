using System;
using System.Collections.Generic;
using System.Windows.Input;
using UI.Commands;
using UI.Configuration;

namespace UI.ViewModels;

public record ShortcutRow(string Label, string Key);

public class ShortcutHelpViewModel : BaseViewModel
{
    public IReadOnlyList<ShortcutRow> CommonRows      { get; }
    public IReadOnlyList<ShortcutRow> CashierRows     { get; }
    public IReadOnlyList<ShortcutRow> ManagerNavRows  { get; }
    public IReadOnlyList<ShortcutRow> ManagerCrudRows { get; }

    public ICommand CloseCommand { get; }
    public event Action? DialogClosed;

    public ShortcutHelpViewModel(ShortcutSettings shortcuts)
    {
        var s = shortcuts;

        CommonRows = new List<ShortcutRow>
        {
            new("Keyboard shortcuts",   s.Common.ShortcutHelp),
            new("Close / Cancel",       s.Common.CloseDialog),
            new("Confirm",              s.Common.ExecuteDefault),
        };

        CashierRows = new List<ShortcutRow>
        {
            new("Pay — Cash",           s.Cashier.CashPayment),
            new("Pay — Card",           s.Cashier.CardPayment),
            new("Recent Sales",         s.Cashier.ShowRecentSales),
            new("Reprint Last Receipt", s.Cashier.ReprintLastReceipt),
            new("Toggle Shift",         s.Cashier.ToggleShift),
            new("New Sale",             s.Cashier.NewSale),
            new("Focus Search",         s.Cashier.FocusSearch),
            new("Remove Item",          s.Cashier.RemoveItem),
            new("Increase Quantity",    s.Cashier.IncreaseQuantity),
            new("Decrease Quantity",    s.Cashier.DecreaseQuantity),
        };

        ManagerNavRows = new List<ShortcutRow>
        {
            new("Home",            s.Manager.Home),
            new("Products",        s.Manager.Products),
            new("Categories",      s.Manager.Categories),
            new("Sizes",           s.Manager.Sizes),
            new("Transactions",    s.Manager.Transactions),
            new("Reports",         s.Manager.Reports),
            new("Receipts",        s.Manager.ReceiptManagement),
            new("Shifts",          s.Manager.ShiftManagement),
            new("Modifier Groups", s.Manager.ModifierGroupManagement),
            new("Settings",        s.Manager.Settings),
            new("Focus Search",    s.Manager.FocusSearch),
            new("Refresh",         s.Manager.Refresh),
        };

        ManagerCrudRows = new List<ShortcutRow>
        {
            new("Add",    s.Manager.Add),
            new("Edit",   s.Manager.Edit),
            new("Delete", s.Manager.Delete),
        };

        CloseCommand = new RelayCommand(_ => DialogClosed?.Invoke());
    }
}
