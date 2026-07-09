using BLL.Interfaces;
using POS.Contracts.Localization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using UI.Commands;

namespace UI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly ILocalizationService _localizationService;

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

    public ICommand CloseCommand { get; }

    public SettingsViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;

        SupportedLanguages = new ObservableCollection<LanguageDto>(
            _localizationService.GetSupportedLanguages());

        _selectedLanguage = SupportedLanguages
            .FirstOrDefault(l => l.Code == _localizationService.CurrentLanguage.Code);

        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
    }

    public event System.Action? CloseRequested;

    private async System.Threading.Tasks.Task SetLanguageAsync(LanguageCode code)
    {
        await _localizationService.SetLanguageAsync(code);
    }
}