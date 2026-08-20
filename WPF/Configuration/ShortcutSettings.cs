using Microsoft.Extensions.Configuration;

namespace UI.Configuration;

public sealed class ShortcutSettings
{
    private readonly IConfiguration _configuration;

    public ShortcutSettings(IConfiguration configuration)
    {
        _configuration = configuration;
        Common = new CommonShortcuts(configuration);
        Cashier = new CashierShortcuts(configuration);
        Manager = new ManagerShortcuts(configuration);
    }

    public CommonShortcuts Common { get; }
    public CashierShortcuts Cashier { get; }
    public ManagerShortcuts Manager { get; }
}

public sealed class CommonShortcuts
{
    private readonly IConfiguration _configuration;

    public CommonShortcuts(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string Get(string key, string fallback) =>
        _configuration[$"Shortcuts:Common:{key}"] ?? fallback;

    public string ShortcutHelp => Get(nameof(ShortcutHelp), "F1");
    public string CloseDialog => Get(nameof(CloseDialog), "Escape");
    public string ExecuteDefault => Get(nameof(ExecuteDefault), "Enter");
}

public sealed class CashierShortcuts
{
    private readonly IConfiguration _configuration;

    public CashierShortcuts(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string Get(string key, string fallback) =>
        _configuration[$"Shortcuts:Cashier:{key}"] ?? fallback;

    public string CashPayment => Get(nameof(CashPayment), "F8");
    public string CardPayment => Get(nameof(CardPayment), "F9");
    public string CompleteSale => Get(nameof(CompleteSale), "F12");
    public string NewSale => Get(nameof(NewSale), "Ctrl+N");
    public string RemoveItem => Get(nameof(RemoveItem), "Delete");
    public string IncreaseQuantity => Get(nameof(IncreaseQuantity), "Add");
    public string DecreaseQuantity => Get(nameof(DecreaseQuantity), "Subtract");
    public string FocusSearch => Get(nameof(FocusSearch), "Ctrl+F");
    public string FocusCategories => Get(nameof(FocusCategories), "F2");
    public string FocusProducts => Get(nameof(FocusProducts), "F3");
    public string FocusCart => Get(nameof(FocusCart), "F4");
    public string ReprintLastReceipt => Get(nameof(ReprintLastReceipt), "Ctrl+T");
    public string ToggleShift => Get(nameof(ToggleShift), "F10");
}

public sealed class ManagerShortcuts
{
    private readonly IConfiguration _configuration;

    public ManagerShortcuts(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string Get(string key, string fallback) =>
        _configuration[$"Shortcuts:Manager:{key}"] ?? fallback;

    // Navigation
    public string Home => Get(nameof(Home), "Alt+H");
    public string Products => Get(nameof(Products), "Alt+P");
    public string Categories => Get(nameof(Categories), "Alt+C");
    public string Sizes => Get(nameof(Sizes), "Alt+V");
    public string Transactions => Get(nameof(Transactions), "Alt+T");
    public string Reports => Get(nameof(Reports), "Alt+R");
    public string ReceiptManagement => Get(nameof(ReceiptManagement), "Alt+E");
    public string ShiftManagement => Get(nameof(ShiftManagement), "Alt+S");
    public string Settings => Get(nameof(Settings), "Alt+O");

    // CRUD
    public string Add => Get(nameof(Add), "Ctrl+N");
    public string Edit => Get(nameof(Edit), "Ctrl+E");
    public string Delete => Get(nameof(Delete), "Delete");

    // View
    public string FocusSearch => Get(nameof(FocusSearch), "Ctrl+F");
    public string Refresh => Get(nameof(Refresh), "F5");
}
