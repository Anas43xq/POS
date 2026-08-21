using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using UI.Configuration;

namespace UI.Behaviors
{
    /// <summary>
    /// Attached behavior that wires keyboard shortcuts from <c>shortcuts.json</c>
    /// to commands on the view's view-model.
    /// </summary>
    public static class ShortcutBindingsBehavior
    {
        public static readonly DependencyProperty RoleProperty =
            DependencyProperty.RegisterAttached(
                "Role",
                typeof(ShortcutRole),
                typeof(ShortcutBindingsBehavior),
                new PropertyMetadata(ShortcutRole.None, OnChanged));

        public static void SetRole(DependencyObject obj, ShortcutRole value)
            => obj.SetValue(RoleProperty, value);

        public static ShortcutRole GetRole(DependencyObject obj)
            => (ShortcutRole)obj.GetValue(RoleProperty);

        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.RegisterAttached(
                "Actions",
                typeof(string),
                typeof(ShortcutBindingsBehavior),
                new PropertyMetadata(null, OnChanged));

        public static void SetActions(DependencyObject obj, string? value)
            => obj.SetValue(ActionsProperty, value);

        public static string? GetActions(DependencyObject obj)
            => (string?)obj.GetValue(ActionsProperty);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement fe)
                return;

            fe.InputBindings.Clear();

            var role = GetRole(fe);
            if (role == ShortcutRole.None) return;

            fe.Loaded -= WireBindings;
            fe.Loaded += WireBindings;
        }

        private static void WireBindings(object? sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe) return;
            fe.Loaded -= WireBindings;

            var role = GetRole(fe);
            if (role == ShortcutRole.None) return;

            var shortcuts = App.ServiceProvider.GetRequiredService<ShortcutSettings>();
            var entries = GetEntriesFor(role, shortcuts);
            if (entries.Count == 0) return;

            var allow = BuildAllowList(GetActions(fe));
            var dc = fe.DataContext;

            foreach (var (action, key, commandName) in entries)
            {
                if (allow is not null && !allow.Contains(action)) continue;
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (!TryParseKeyGesture(key, out var wpfKey, out var modifiers)) continue;

                var command = ResolveCommand(dc, commandName);
                if (command is null) continue;

                fe.InputBindings.Add(new KeyBinding(command, wpfKey, modifiers));
            }
        }

        private static bool TryParseKeyGesture(string gesture, out Key key, out ModifierKeys modifiers)
        {
            key = Key.None;
            modifiers = ModifierKeys.None;
            if (string.IsNullOrWhiteSpace(gesture)) return false;

            var parts = gesture.Split('+');
            var keyPart = parts[^1].Trim();
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var m = parts[i].Trim().ToLowerInvariant();
                modifiers |= m switch
                {
                    "ctrl" or "control" => ModifierKeys.Control,
                    "shift" => ModifierKeys.Shift,
                    "alt" => ModifierKeys.Alt,
                    _ => ModifierKeys.None
                };
            }
            return Enum.TryParse<Key>(keyPart, ignoreCase: true, out key) && key != Key.None;
        }

        private static ICommand? ResolveCommand(object? dataContext, string commandName)
        {
            if (dataContext is null || string.IsNullOrWhiteSpace(commandName))
                return null;

            var t = dataContext.GetType();

            var exact = t.GetProperty(commandName, BindingFlags.Public | BindingFlags.Instance);
            if (exact?.GetValue(dataContext) is ICommand cmd1)
                return cmd1;

            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!typeof(ICommand).IsAssignableFrom(p.PropertyType)) continue;
                var n = p.Name;
                if (n.Equals(commandName, StringComparison.OrdinalIgnoreCase) ||
                    n.Equals(commandName + "Command", StringComparison.OrdinalIgnoreCase) ||
                    n.EndsWith(commandName + "Command", StringComparison.OrdinalIgnoreCase))
                {
                    return p.GetValue(dataContext) as ICommand;
                }
            }
            return null;
        }

        private sealed record ShortcutEntry(string Action, string Key, string CommandName);

        private static IReadOnlyList<ShortcutEntry> GetEntriesFor(ShortcutRole role, ShortcutSettings s)
        {
            return role switch
            {
                ShortcutRole.Common => new List<ShortcutEntry>
                {
                    new("ShortcutHelp",   s.Common.ShortcutHelp,   "ShowShortcutHelpCommand"),
                    new("CloseDialog",   s.Common.CloseDialog,   "CloseCommand"),
                    new("ExecuteDefault",s.Common.ExecuteDefault,"AcceptCommand"),
                },
                ShortcutRole.Cashier => new List<ShortcutEntry>
                {
                    new("CashPayment",        s.Cashier.CashPayment,        "PayCashCommand"),
                    new("CardPayment",        s.Cashier.CardPayment,        "PayCardCommand"),
                    new("ShowRecentSales",    s.Cashier.ShowRecentSales,    "ShowRecentSalesCommand"),
                    new("ReprintLastReceipt", s.Cashier.ReprintLastReceipt, "ReprintLastReceiptCommand"),
                    new("ToggleShift",        s.Cashier.ToggleShift,        "ToggleShiftCommand"),
                    new("NewSale",            s.Cashier.NewSale,            "ClearSaleCommand"),
                    new("FocusSearch",        s.Cashier.FocusSearch,        "FocusSearchCommand"),
                    new("RemoveItem",         s.Cashier.RemoveItem,         "RemoveSelectedSaleItemCommand"),
                    new("IncreaseQuantity",   s.Cashier.IncreaseQuantity,   "IncreaseSelectedQuantityCommand"),
                    new("DecreaseQuantity",   s.Cashier.DecreaseQuantity,   "DecreaseSelectedQuantityCommand"),
                },
                ShortcutRole.Manager => new List<ShortcutEntry>
                {
                    new("Home",              s.Manager.Home,              "NavigateHomeCommand"),
                    new("Products",          s.Manager.Products,          "NavigateProductManagementCommand"),
                    new("Categories",        s.Manager.Categories,        "NavigateCategoryManagementCommand"),
                    new("Sizes",             s.Manager.Sizes,             "NavigateSizeManagementCommand"),
                    new("Transactions",      s.Manager.Transactions,      "NavigateTransactionsCommand"),
                    new("Reports",           s.Manager.Reports,           "NavigateReportsCommand"),
                    new("ReceiptManagement", s.Manager.ReceiptManagement, "NavigateReceiptManagementCommand"),
                    new("ShiftManagement",   s.Manager.ShiftManagement,   "NavigateShiftManagementCommand"),
                    new("Settings",                  s.Manager.Settings,                  "ShowSetting"),
                    new("ModifierGroupManagement",   s.Manager.ModifierGroupManagement,   "NavigateModifierGroupManagementCommand"),
                    new("Add",               s.Manager.Add,               "AddCommand"),
                    new("Edit",              s.Manager.Edit,              "EditCommand"),
                    new("Delete",            s.Manager.Delete,            "DeleteCommand"),
                    new("FocusSearch",       s.Manager.FocusSearch,       "FocusSearchCommand"),
                    new("Refresh",           s.Manager.Refresh,           "RefreshCommand"),
                },
                _ => Array.Empty<ShortcutEntry>()
            };
        }

        private static HashSet<string>? BuildAllowList(string? actionsFilter)
        {
            if (string.IsNullOrWhiteSpace(actionsFilter)) return null;
            return new HashSet<string>(
                actionsFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Identifies which subsection of <c>shortcuts.json</c> a view reads.
    /// </summary>
    public enum ShortcutRole
    {
        None,
        Common,
        Cashier,
        Manager
    }
}
