namespace Contracts.Enum;

public enum ShortcutAction : byte
{
    // ── Common ──────────────────────────────────────────────
    ShortcutHelp = 0,
    FocusSearch = 60,
    Escape = 61,

    // ── Cashier: General ────────────────────────────────────
    SearchProducts = 3,
    RefreshProducts = 62,

    // ── Cashier: Navigation ─────────────────────────────────
    FocusCategories = 70,
    FocusProducts = 71,
    FocusCart = 72,

    // ── Cashier: Cart ───────────────────────────────────────
    RemoveItem = 4,
    IncreaseQuantity = 80,
    DecreaseQuantity = 81,

    // ── Cashier: Sale ───────────────────────────────────────
    CashPayment = 90,
    CardPayment = 91,
    CompleteSale = 92,
    NewSale = 93,

    // ── Cashier: Shift ──────────────────────────────────────
    OpenCloseShift = 100,

    // ── Cashier: Receipt ────────────────────────────────────
    ReprintLastReceipt = 110,

    // ── Cashier: Legacy (kept for compat) ───────────────────
    ConfirmPayment = 1,
    HoldSale = 2,

    // ── Manager: Navigation ─────────────────────────────────
    NavigateHome = 20,
    NavigateProducts = 21,
    NavigateCategories = 22,
    NavigateReports = 23,
    NavigateTransactions = 24,
    NavigateSizes = 25,
    NavigateReceiptManagement = 26,
    NavigateShiftManagement = 27,
    NavigateSettings = 28,
    Logout = 29,

    // ── Manager: CRUD ───────────────────────────────────────
    Add = 40,
    Edit = 41,
    Delete = 42,
    Refresh = 43,
    Export = 44,
    Save = 45,
    Duplicate = 46,
    RestoreDefaults = 47,

    // ── Manager: Module-specific ────────────────────────────
    ManageTranslations = 120,
    ManageVariants = 121,
    ManageSizes = 122,
    AddSubcategory = 123,

    // ── Manager: Transactions ───────────────────────────────
    ViewDetails = 130,
    ReprintReceipt = 131,
    VoidTransaction = 132,

    // ── Manager: Reports ────────────────────────────────────
    GenerateReport = 140,
    ExportToExcel = 141,
    PrintReport = 142,

    // ── Manager: Dialogs ────────────────────────────────────
    Confirm = 150,
    NextField = 151,
    PreviousField = 152,
}
