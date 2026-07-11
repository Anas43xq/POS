using BLL.Interfaces;
using POS.Contracts.Localization;
using POS.Contracts.Printing;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Windows.Input;
using UI.Commands;

namespace UI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;

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

    public ICommand CloseCommand { get; }
    public ICommand RefreshPrintersCommand { get; }

    public SettingsViewModel(ILocalizationService localizationService, ISettingsService settingsService)
    {
        _localizationService = localizationService;
        _settingsService = settingsService;

        SupportedLanguages = new ObservableCollection<LanguageDto>(
            _localizationService.GetSupportedLanguages());

        _selectedLanguage = SupportedLanguages
            .FirstOrDefault(l => l.Code == _localizationService.CurrentLanguage.Code);

        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        RefreshPrintersCommand = new RelayCommand(_ => LoadPrinters());

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

        OnPropertyChanged(nameof(PaperWidth));
        OnPropertyChanged(nameof(AutoPrint));
        OnPropertyChanged(nameof(ShowPrintDialog));
        OnPropertyChanged(nameof(Copies));

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
            Copies = Copies
        };

        await _settingsService.SetPrinterSettingsAsync(settings);
    }

    private async System.Threading.Tasks.Task SetLanguageAsync(LanguageCode code)
    {
        await _localizationService.SetLanguageAsync(code);
    }
}
