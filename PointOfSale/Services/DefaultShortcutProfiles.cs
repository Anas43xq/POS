using Contracts.Enum;
using Contracts.Shortcuts;

namespace UI.Services;

public static class DefaultShortcutProfiles
{
    /// <summary>
    /// Cashier profile: max 2-key combos, single-key preferred.
    /// Designed for high-speed POS workflow.
    /// </summary>
    public static ShortcutProfile GetCashierProfile() => new()
    {
        ProfileType = ShortcutProfileType.Cashier,
        Bindings =
        [
            // ── General ──────────────────────────────────
            new() { Action = ShortcutAction.ShortcutHelp,       KeyGesture = "F1",   Description = "Show Shortcut Help" },
            new() { Action = ShortcutAction.SearchProducts,     KeyGesture = "Ctrl+F", Description = "Focus Product Search" },
            new() { Action = ShortcutAction.RefreshProducts,    KeyGesture = "F5",   Description = "Refresh Products" },
            new() { Action = ShortcutAction.Escape,             KeyGesture = "Escape", Description = "Cancel / Close Dialog" },

            // ── Navigation ───────────────────────────────
            new() { Action = ShortcutAction.FocusCategories,    KeyGesture = "F2",   Description = "Focus Categories" },
            new() { Action = ShortcutAction.FocusProducts,      KeyGesture = "F3",   Description = "Focus Products" },
            new() { Action = ShortcutAction.FocusCart,          KeyGesture = "F4",   Description = "Focus Cart" },

            // ── Cart ─────────────────────────────────────
            new() { Action = ShortcutAction.RemoveItem,         KeyGesture = "Delete", Description = "Remove Selected Item" },
            new() { Action = ShortcutAction.IncreaseQuantity,   KeyGesture = "Add",  Description = "Increase Quantity" },
            new() { Action = ShortcutAction.DecreaseQuantity,   KeyGesture = "Subtract", Description = "Decrease Quantity" },

            // ── Sale ─────────────────────────────────────
            new() { Action = ShortcutAction.CashPayment,        KeyGesture = "F8",   Description = "Cash Payment" },
            new() { Action = ShortcutAction.CardPayment,        KeyGesture = "F9",   Description = "Card Payment" },
            new() { Action = ShortcutAction.CompleteSale,       KeyGesture = "F12",  Description = "Complete Sale" },
            new() { Action = ShortcutAction.NewSale,            KeyGesture = "Ctrl+N", Description = "New Sale" },

            // ── Shift ────────────────────────────────────
            new() { Action = ShortcutAction.OpenCloseShift,     KeyGesture = "F10",  Description = "Open / Close Shift" },

            // ── Receipt ──────────────────────────────────
            new() { Action = ShortcutAction.ReprintLastReceipt, KeyGesture = "Ctrl+T", Description = "Reprint Last Receipt" },
        ]
    };

    /// <summary>
    /// Manager profile: 2–3 key combos acceptable.
    /// Covers navigation, CRUD, and admin operations.
    /// </summary>
    public static ShortcutProfile GetManagerProfile() => new()
    {
        ProfileType = ShortcutProfileType.Manager,
        Bindings =
        [
            // ── Global Navigation ────────────────────────
            new() { Action = ShortcutAction.NavigateHome,              KeyGesture = "Alt+H", Description = "Home" },
            new() { Action = ShortcutAction.NavigateProducts,          KeyGesture = "Alt+P", Description = "Products" },
            new() { Action = ShortcutAction.NavigateCategories,        KeyGesture = "Alt+C", Description = "Categories" },
            new() { Action = ShortcutAction.NavigateSizes,             KeyGesture = "Alt+V", Description = "Sizes" },
            new() { Action = ShortcutAction.NavigateTransactions,      KeyGesture = "Alt+T", Description = "Transactions" },
            new() { Action = ShortcutAction.NavigateReports,           KeyGesture = "Alt+R", Description = "Reports" },
            new() { Action = ShortcutAction.NavigateReceiptManagement, KeyGesture = "Alt+E", Description = "Receipt Management" },
            new() { Action = ShortcutAction.NavigateShiftManagement,   KeyGesture = "Alt+S", Description = "Shift Management" },
            new() { Action = ShortcutAction.NavigateSettings,          KeyGesture = "Alt+O", Description = "Settings" },
            new() { Action = ShortcutAction.Logout,                    KeyGesture = "Alt+L", Description = "Logout" },

            // ── General ──────────────────────────────────
            new() { Action = ShortcutAction.ShortcutHelp,  KeyGesture = "F1",     Description = "Shortcut Help" },
            new() { Action = ShortcutAction.FocusSearch,   KeyGesture = "Ctrl+F", Description = "Focus Search" },
            new() { Action = ShortcutAction.Refresh,       KeyGesture = "F5",     Description = "Refresh" },
            new() { Action = ShortcutAction.Escape,        KeyGesture = "Escape",  Description = "Close Dialog" },

            // ── CRUD ─────────────────────────────────────
            new() { Action = ShortcutAction.Add,             KeyGesture = "Ctrl+N", Description = "Add" },
            new() { Action = ShortcutAction.Edit,            KeyGesture = "Ctrl+E", Description = "Edit" },
            new() { Action = ShortcutAction.Delete,          KeyGesture = "Delete", Description = "Delete" },
            new() { Action = ShortcutAction.Save,            KeyGesture = "Ctrl+S", Description = "Save" },
            new() { Action = ShortcutAction.RestoreDefaults, KeyGesture = "Ctrl+Shift+R", Description = "Restore Defaults" },

            // ── Products ─────────────────────────────────
            new() { Action = ShortcutAction.ManageTranslations, KeyGesture = "Ctrl+T",      Description = "Manage Translations" },
            new() { Action = ShortcutAction.ManageVariants,     KeyGesture = "Ctrl+Shift+V", Description = "Manage Variants" },
            new() { Action = ShortcutAction.ManageSizes,        KeyGesture = "Ctrl+Shift+S", Description = "Manage Sizes" },

            // ── Categories ───────────────────────────────
            new() { Action = ShortcutAction.AddSubcategory,     KeyGesture = "Ctrl+Shift+N", Description = "Add Subcategory" },

            // ── Transactions ─────────────────────────────
            new() { Action = ShortcutAction.ViewDetails,      KeyGesture = "Enter",    Description = "View Details" },
            new() { Action = ShortcutAction.ReprintReceipt,   KeyGesture = "Ctrl+R",   Description = "Reprint Receipt" },
            new() { Action = ShortcutAction.VoidTransaction,  KeyGesture = "Ctrl+V",   Description = "Void Transaction" },

            // ── Reports ──────────────────────────────────
            new() { Action = ShortcutAction.GenerateReport,  KeyGesture = "F9",           Description = "Generate Report" },
            new() { Action = ShortcutAction.ExportToExcel,   KeyGesture = "Ctrl+Shift+E", Description = "Export to Excel" },
            new() { Action = ShortcutAction.PrintReport,     KeyGesture = "Ctrl+P",       Description = "Print Report" },
        ]
    };

    public static ShortcutProfile GetDefault(ShortcutProfileType profileType) => profileType switch
    {
        ShortcutProfileType.Cashier => GetCashierProfile(),
        ShortcutProfileType.Manager => GetManagerProfile(),
        _ => throw new ArgumentOutOfRangeException(nameof(profileType))
    };
}
