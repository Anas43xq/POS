using BLL.Interfaces;
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
using System.Windows;
using System.Windows.Input;
using UI.Commands;

namespace UI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;
    private readonly ISessionService _sessionService;
    private readonly IPrintingService _printingService;
    private readonly IReceiptFileWriter _receiptFileWriter;
    private readonly ILogger<SettingsViewModel> _logger;


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
                _ = SetLanguageAsync(value.Code);
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
                _ = SavePrinterSettingsAsync();
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
                _ = SavePrinterSettingsAsync();
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
                _ = SavePrinterSettingsAsync();
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
                _ = SavePrinterSettingsAsync();
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
                _ = SavePrinterSettingsAsync();
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
                _ = SavePrinterSettingsAsync();
            }
        }
    }

    public ICommand CloseCommand { get; }
    public ICommand RefreshPrintersCommand { get; }
    public ICommand PrintTestReceiptCommand { get; }

    public bool IsManager => string.Equals(_sessionService.CurrentUser?.RoleName, "Manager", StringComparison.OrdinalIgnoreCase);

    public SettingsViewModel(
        ILocalizationService localizationService,
        ISettingsService settingsService,
        ISessionService sessionService,
        IPrintingService printingService,
        IReceiptFileWriter receiptFileWriter,
        ILogger<SettingsViewModel> logger
        )
    {
        _localizationService = localizationService;
        _settingsService = settingsService;
        _sessionService = sessionService;
        _printingService = printingService;
        _receiptFileWriter = receiptFileWriter;
        _logger = logger;

        SupportedLanguages = new ObservableCollection<LanguageDto>(
            _localizationService.GetSupportedLanguages());

        _selectedLanguage = SupportedLanguages
            .FirstOrDefault(l => l.Code == _localizationService.CurrentLanguage.Code);

        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        RefreshPrintersCommand = new RelayCommand(_ => LoadPrinters());
        PrintTestReceiptCommand = new RelayCommand(_ => _ = PrintTestReceiptAsync());

        LoadPrinters();
        _ = LoadPrinterSettingsAsync();
    }

    public event System.Action? CloseRequested;

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

    private async System.Threading.Tasks.Task SavePrinterSettingsAsync()
    {
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
    }

    private async System.Threading.Tasks.Task SetLanguageAsync(LanguageCode code)
    {
        await _localizationService.SetLanguageAsync(code);
    }

    /// <summary>
    /// Generic "print any receipt" helper. Centralises the
    /// try / catch / log / MessageBox pattern so every print command
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
            MessageBox.Show(
                "Printing failed. Please check the printer and try again.",
                "Print Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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

            MessageBox.Show(
                $"Test receipt saved to:\n{filePath}",
                "Test Receipt Saved",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save test receipt as PDF");
            MessageBox.Show(
                $"Could not save the PDF.\n\n{ex.Message}",
                "Save Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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
