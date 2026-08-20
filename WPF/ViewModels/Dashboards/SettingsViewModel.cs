using BLL.Interfaces;
using BLL.DTOs;
using Microsoft.Extensions.Logging;
using POS.Contracts.Localization;
using POS.Contracts.Printing;
using POS.Contracts.Receipts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;
    private readonly ISessionService _sessionService;
    private readonly IPrintingService _printingService;
    private readonly IReceiptFileWriter _receiptFileWriter;
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly INotificationService _notifications;
    private readonly IPinService _pinService;

    private bool _hasPin;
    private string _pinEntry = string.Empty;
    private string _pinConfirmEntry = string.Empty;
    private string? _pinErrorMessage;
    private bool _isPinBusy;


    public ObservableCollection<LanguageDto> SupportedLanguages { get; }

    private LanguageDto? _selectedLanguage;
    public LanguageDto? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value is not null)
            {
                _selectedLanguage = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<string> AvailablePrinters { get; } = new();
    public ObservableCollection<int> PaperWidthOptions { get; } = new() { 58, 80 };

    public IReadOnlyList<TestPrintActionOption> TestPrintActionOptions { get; } = new[]
    {
        new TestPrintActionOption(TestPrintAction.Print, "Print to printer"),
        new TestPrintActionOption(TestPrintAction.SaveToFile, "Save to PDF file"),
    };

    private string? _selectedReceiptPrinter;
    public string? SelectedReceiptPrinter
    {
        get => _selectedReceiptPrinter;
        set
        {
            if (_selectedReceiptPrinter != value)
            {
                _selectedReceiptPrinter = value;
                OnPropertyChanged();
            }
        }
    }

    private int _paperWidth = 80;
    public int PaperWidth
    {
        get => _paperWidth;
        set
        {
            if (_paperWidth != value)
            {
                _paperWidth = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _autoPrint = true;
    public bool AutoPrint
    {
        get => _autoPrint;
        set
        {
            if (_autoPrint != value)
            {
                _autoPrint = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _showPrintDialog;
    public bool ShowPrintDialog
    {
        get => _showPrintDialog;
        set
        {
            if (_showPrintDialog != value)
            {
                _showPrintDialog = value;
                OnPropertyChanged();
            }
        }
    }

    private int _copies = 1;
    public int Copies
    {
        get => _copies;
        set
        {
            if (_copies != value)
            {
                _copies = value;
                OnPropertyChanged();
            }
        }
    }

    private TestPrintAction _testPrintAction = TestPrintAction.Print;
    public TestPrintAction TestPrintAction
    {
        get => _testPrintAction;
        set
        {
            if (_testPrintAction != value)
            {
                _testPrintAction = value;
                OnPropertyChanged();
            }
        }
    }

        public ICommand CloseCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RefreshPrintersCommand { get; }
        public ICommand PrintTestReceiptCommand { get; }
        public ICommand SetPinCommand { get; }

    public bool IsManager => string.Equals(_sessionService.CurrentUser?.RoleName, "Manager", StringComparison.OrdinalIgnoreCase);

    public bool HasPin
    {
        get => _hasPin;
        private set { if (_hasPin != value) { _hasPin = value; OnPropertyChanged(); } }
    }

    // Pushed from code-behind PasswordBox handlers (WPF PasswordBox doesn't support binding)
    public string PinEntry
    {
        get => _pinEntry;
        set { _pinEntry = value ?? string.Empty; PinErrorMessage = null; }
    }

    public string PinConfirmEntry
    {
        get => _pinConfirmEntry;
        set { _pinConfirmEntry = value ?? string.Empty; PinErrorMessage = null; }
    }

    public string? PinErrorMessage
    {
        get => _pinErrorMessage;
        private set
        {
            if (_pinErrorMessage != value)
            {
                _pinErrorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPinError));
            }
        }
    }

    public bool HasPinError => !string.IsNullOrWhiteSpace(_pinErrorMessage);

    public SettingsViewModel(
        ILocalizationService localizationService,
        ISettingsService settingsService,
        ISessionService sessionService,
        IPrintingService printingService,
        IReceiptFileWriter receiptFileWriter,
        ILogger<SettingsViewModel> logger,
        INotificationService notifications,
        IPinService pinService
        )
    {
        _localizationService = localizationService;
        _settingsService = settingsService;
        _sessionService = sessionService;
        _printingService = printingService;
        _receiptFileWriter = receiptFileWriter;
        _logger = logger;
        _notifications = notifications;
        _pinService = pinService;

        SupportedLanguages = new ObservableCollection<LanguageDto>(
            _localizationService.GetSupportedLanguages());

        _selectedLanguage = SupportedLanguages
            .FirstOrDefault(l => l.Code == _localizationService.CurrentLanguage.Code);

        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        SaveCommand = new AsyncRelayCommand(SaveAndCloseAsync);
        RefreshPrintersCommand = new RelayCommand(_ => LoadPrinters());
        PrintTestReceiptCommand = new RelayCommand(_ => _ = PrintTestReceiptAsync());
        SetPinCommand = new AsyncRelayCommand(SetPinAsync, () => !_isPinBusy);

        LoadPrinters();
        _ = LoadPrinterSettingsAsync();
        _ = LoadPinStatusAsync();
    }

    public event System.Action? CloseRequested;

    private async System.Threading.Tasks.Task LoadPinStatusAsync()
    {
        if (!IsManager) return;
        try
        {
            var userId = _sessionService.CurrentUser?.UserId;
            if (userId is null) return;
            HasPin = await _pinService.HasPinAsync(userId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load PIN status");
        }
    }

    private async System.Threading.Tasks.Task SetPinAsync()
    {
        if (_isPinBusy) return;

        PinErrorMessage = null;

        if (string.IsNullOrWhiteSpace(_pinEntry) || _pinEntry.Length != 4)
        {
            PinErrorMessage = "PIN must be exactly 4 digits.";
            return;
        }

        if (_pinEntry != _pinConfirmEntry)
        {
            PinErrorMessage = "PINs do not match.";
            return;
        }

        var userId = _sessionService.CurrentUser?.UserId;
        if (userId is null)
        {
            PinErrorMessage = "No active session.";
            return;
        }

        _isPinBusy = true;
        if (SetPinCommand is AsyncRelayCommand cmd) cmd.RaiseCanExecuteChanged();

        try
        {
            var result = await _pinService.SetPinAsync(userId.Value, _pinEntry);
            if (!result.IsSuccess)
            {
                PinErrorMessage = result.Error;
                return;
            }

            HasPin = true;
            _pinEntry = string.Empty;
            _pinConfirmEntry = string.Empty;
            PinResetRequested?.Invoke();
            _notifications.ShowSuccess("Override PIN updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set PIN for user {UserId}", userId);
            PinErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            _isPinBusy = false;
            if (SetPinCommand is AsyncRelayCommand c) c.RaiseCanExecuteChanged();
        }
    }

    /// <summary>Raised when the PIN is successfully saved so the code-behind can clear the PasswordBoxes.</summary>
    public event System.Action? PinResetRequested;

    private void LoadPrinters()
    {
        AvailablePrinters.Clear();

        try
        {
            var printServer = new LocalPrintServer();
            var printQueues = printServer.GetPrintQueues();

            foreach (var queue in printQueues)
            {
                AvailablePrinters.Add(queue.Name);
            }
        }
        catch
        {
            // Printer enumeration is best-effort.
        }

        if (SelectedReceiptPrinter is null && AvailablePrinters.Count > 0)
        {
            SelectedReceiptPrinter = AvailablePrinters.FirstOrDefault();
        }
    }

    private async System.Threading.Tasks.Task LoadPrinterSettingsAsync()
    {
        var settings = await _settingsService.GetPrinterSettingsAsync();

        _paperWidth = settings.PaperWidth;
        _autoPrint = settings.AutoPrint;
        _showPrintDialog = settings.ShowPrintDialog;
        _copies = settings.Copies;
        _testPrintAction = settings.TestPrintAction;

        OnPropertyChanged(nameof(PaperWidth));
        OnPropertyChanged(nameof(AutoPrint));
        OnPropertyChanged(nameof(ShowPrintDialog));
        OnPropertyChanged(nameof(Copies));
        OnPropertyChanged(nameof(TestPrintAction));

        if (!string.IsNullOrWhiteSpace(settings.ReceiptPrinterName)
            && AvailablePrinters.Contains(settings.ReceiptPrinterName))
        {
            _selectedReceiptPrinter = settings.ReceiptPrinterName;
            OnPropertyChanged(nameof(SelectedReceiptPrinter));
        }
    }

        private async System.Threading.Tasks.Task SaveAndCloseAsync()
        {
            // Persist printer settings
            var settings = new PrinterSettings
            {
                ReceiptPrinterName = SelectedReceiptPrinter ?? string.Empty,
                PaperWidth = PaperWidth,
                AutoPrint = AutoPrint,
                ShowPrintDialog = ShowPrintDialog,
                Copies = Copies,
                TestPrintAction = TestPrintAction
            };
            await _settingsService.SetPrinterSettingsAsync(settings);

            // Persist language
            if (_selectedLanguage is not null)
                await _localizationService.SetLanguageAsync(_selectedLanguage.Code);

            CloseRequested?.Invoke();
        }

    /// <summary>
    /// Generic "print any receipt" helper. Centralises the
    /// try / catch / log / toast pattern so every print command
    /// in this view-model behaves the same way and reuses the same
    /// error contract. Reused by the test-print command and any
    /// future receipt-printing command added here.
    /// </summary>
    private async System.Threading.Tasks.Task PrintReceiptAsync(ReceiptDetailsDto receipt, bool showDialog)
    {
        try
        {
            await _printingService.PrintReceiptDirectAsync(receipt, showDialog: showDialog);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to print receipt {ReceiptNumber}",
                receipt?.ReceiptNumber);
            _notifications.ShowError("Printing failed. Please check the printer and try again.");
        }
    }

    /// <summary>
    /// Builds a synthetic <see cref="ReceiptDetailsDto"/> (today's
    /// date, HWA-{yyyyMMdd}-XXXX, zero totals) and either prints it
    /// to the configured thermal printer (no dialog) or saves it as
    /// a PDF on disk, depending on the user's <see cref="TestPrintAction"/>
    /// choice. Per the test-print spec:
    ///   • Date label uses today's local date+time (yyyy-MM-dd HH:mm).
    ///   • Receipt number is HWA-{today yyyyMMdd}-{4-digit suffix}.
    ///   • Totals are 0 and payment is Cash so the manager can verify
    ///     the printer/width/dialog settings without a real transaction.
    /// </summary>
    private System.Threading.Tasks.Task PrintTestReceiptAsync()
    {
        var receipt = BuildTestReceipt();
        return _testPrintAction switch
        {
            TestPrintAction.SaveToFile => SaveTestReceiptAsPdfAsync(receipt),
            _                            => PrintReceiptAsync(receipt, showDialog: false),
        };
    }

    /// <summary>
    /// Saves the test receipt to a PDF file under
    /// %USERPROFILE%\Documents\Hawa Receipts\ with the file name
    /// "hawa-receipt-{yyyy-MM-dd}-{4-digit}.pdf". The folder is
    /// created if missing. No printer, no dialog.
    /// </summary>
    private async System.Threading.Tasks.Task SaveTestReceiptAsPdfAsync(ReceiptDetailsDto receipt)
    {
        try
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folder = Path.Combine(docs, "Hawa Receipts");
            var fileName = $"{receipt.ReceiptNumber}.pdf";
            var filePath = Path.Combine(folder, fileName);

            var paperWidth = _paperWidth > 0 ? _paperWidth : 80;
            await _receiptFileWriter.SaveReceiptAsPdfAsync(
                receipt,
                filePath,
                paperWidth);

            _logger.LogInformation(
                "Test receipt {ReceiptNumber} saved as PDF at {FilePath}",
                receipt.ReceiptNumber,
                filePath);

            _notifications.ShowSuccess($"Test receipt saved to:\n{filePath}");        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save test receipt as PDF");
            _notifications.ShowError($"Could not save the PDF.\n\n{ex.Message}");
        }
    }

    private static ReceiptDetailsDto BuildTestReceipt()
    {
        var now = DateTime.Now;
        int suffix = Random.Shared.Next(1000, 10000); // 1000..9999 (4 digits)

        return new ReceiptDetailsDto
        {
            TransactionId = 0,
            ReceiptNumber = $"HWA-{now:yyyyMMdd}-{suffix:D4}",
            TransactionDate = now,
            StoreName = "Cafeteria Hawa",
            CashierName = "Test Cashier",
            Subtotal = 0m,
            TaxTotal = 0m,
            DiscountTotal = 0m,
            GrandTotal = 0m,
            PaymentMethod = "Cash",
            AmountTendered = 0m,
            ChangeGiven = 0m,
            Items = new List<ReceiptItemDto>
            {
                new ReceiptItemDto
                {
                    ProductName = "Sample Product",
                    Quantity = 1,
                    UnitPrice = 0m,
                    LineTotal = 0m,
                    Modifiers = new List<ReceiptModifierDto>()
                }
            }
        };
    }
}

/// <summary>
/// Display wrapper used to bind the "Test Print Action" ComboBox.
/// Carries the enum value and a user-visible label; DataTemplate
/// renders <see cref="Label"/>.
/// </summary>
public sealed record TestPrintActionOption(TestPrintAction Value, string Label)
{
    public override string ToString() => Label;
}
