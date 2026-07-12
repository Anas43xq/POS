using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BLL.DTOs;
using BLL.Interfaces;
using POS.Contracts.Localization;
using UI.Commands;

namespace UI.ViewModels;

/// <summary>
/// Reusable ViewModel for managing translations of any entity type
/// (Product, Category, Size). Follows the architecture doc's translation
/// dialog pattern: Language dropdown + Name field + optional Description.
/// </summary>
public class TranslationDialogViewModel : BaseViewModel
{
    public enum EntityType { Product, Category, Size }

    private readonly EntityType _entityType;
    private readonly int _entityId;
    private readonly string _canonicalName;

    private readonly IProductTranslationService? _productTranslationService;
    private readonly ICategoryTranslationService? _categoryTranslationService;
    private readonly ISizeTranslationService? _sizeTranslationService;

    private TranslationItemViewModel? _selectedTranslation;
    private TranslationItemViewModel? _editItem;
    private bool _isEditing;

    public TranslationDialogViewModel(
        EntityType entityType,
        int entityId,
        string canonicalName,
        IProductTranslationService? productTranslationService = null,
        ICategoryTranslationService? categoryTranslationService = null,
        ISizeTranslationService? sizeTranslationService = null)
    {
        _entityType = entityType;
        _entityId = entityId;
        _canonicalName = canonicalName;
        _productTranslationService = productTranslationService;
        _categoryTranslationService = categoryTranslationService;
        _sizeTranslationService = sizeTranslationService;

        Title = entityType switch
        {
            EntityType.Product => "Product Translation",
            EntityType.Category => "Category Translation",
            EntityType.Size => "Size Translation",
            _ => "Translation"
        };

        HasDescription = entityType == EntityType.Product;

        // Non-English languages only (English is canonical, edited from main form)
        Languages = new ObservableCollection<LanguageDto>(
            SupportedLanguages.All.Where(l => l.FilePrefix != "en"));

        AddCommand = new RelayCommand(_ => StartAdd(), _ => !IsEditing);
        EditCommand = new RelayCommand(_ => StartEdit(), _ => !IsEditing && SelectedTranslation != null);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => !IsEditing && SelectedTranslation != null);
        CancelCommand = new RelayCommand(_ => CancelEdit());
        CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());

        _ = LoadTranslationsAsync();
    }

    public string Title { get; }
    public bool HasDescription { get; }
    public ObservableCollection<LanguageDto> Languages { get; }
    public ObservableCollection<TranslationItemViewModel> Translations { get; } = new();

    public TranslationItemViewModel? SelectedTranslation
    {
        get => _selectedTranslation;
        set
        {
            if (_selectedTranslation != value)
            {
                _selectedTranslation = value;
                OnPropertyChanged();
                if (AddCommand is RelayCommand addCmd) addCmd.RaiseCanExecuteChanged();
                if (EditCommand is RelayCommand editCmd) editCmd.RaiseCanExecuteChanged();
                if (DeleteCommand is AsyncRelayCommand deleteCmd) deleteCmd.RaiseCanExecuteChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public TranslationItemViewModel? EditItem
    {
        get => _editItem;
        set
        {
            if (_editItem != value)
            {
                _editItem = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (_isEditing != value)
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditing));
                if (AddCommand is RelayCommand addCmd) addCmd.RaiseCanExecuteChanged();
                if (EditCommand is RelayCommand editCmd) editCmd.RaiseCanExecuteChanged();
                if (DeleteCommand is AsyncRelayCommand deleteCmd) deleteCmd.RaiseCanExecuteChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsNotEditing => !IsEditing;

    public string? ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }
    private string? _errorMessage;
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand CloseCommand { get; }

    public Action? RequestClose { get; set; }

    private async Task LoadTranslationsAsync()
    {
        Translations.Clear();

        IEnumerable<object> dtos = _entityType switch
        {
            EntityType.Product when _productTranslationService != null
                => await _productTranslationService.GetByProductIdAsync(_entityId),
            EntityType.Category when _categoryTranslationService != null
                => await _categoryTranslationService.GetByCategoryIdAsync(_entityId),
            EntityType.Size when _sizeTranslationService != null
                => await _sizeTranslationService.GetBySizeIdAsync(_entityId),
            _ => Enumerable.Empty<object>()
        };

        foreach (var dto in dtos)
        {
            Translations.Add(MapToItem(dto));
        }
    }

    private TranslationItemViewModel MapToItem(object dto) => dto switch
    {
        ProductTranslationDto p => new TranslationItemViewModel
        {
            TranslationId = p.ProductTranslationId,
            LanguageCode = p.LanguageCode,
            LanguageName = Languages.FirstOrDefault(l => l.FilePrefix == p.LanguageCode)?.DisplayName ?? p.LanguageCode,
            TranslatedName = p.TranslatedName
        },
        CategoryTranslationDto c => new TranslationItemViewModel
        {
            TranslationId = c.CategoryTranslationId,
            LanguageCode = c.LanguageCode,
            LanguageName = Languages.FirstOrDefault(l => l.FilePrefix == c.LanguageCode)?.DisplayName ?? c.LanguageCode,
            TranslatedName = c.TranslatedName
        },
        SizeTranslationDto s => new TranslationItemViewModel
        {
            TranslationId = s.SizeTranslationId,
            LanguageCode = s.LanguageCode,
            LanguageName = Languages.FirstOrDefault(l => l.FilePrefix == s.LanguageCode)?.DisplayName ?? s.LanguageCode,
            TranslatedName = s.TranslatedName
        },
        _ => new TranslationItemViewModel()
    };

    private void StartAdd()
    {
        ErrorMessage = null;
        EditItem = new TranslationItemViewModel
        {
            EntityId = _entityId,
            SelectedLanguage = Languages.FirstOrDefault()
        };
        IsEditing = true;
    }

    private void StartEdit()
    {
        if (SelectedTranslation == null) return;

        ErrorMessage = null;
        EditItem = new TranslationItemViewModel
        {
            TranslationId = SelectedTranslation.TranslationId,
            EntityId = _entityId,
            LanguageCode = SelectedTranslation.LanguageCode,
            LanguageName = SelectedTranslation.LanguageName,
            TranslatedName = SelectedTranslation.TranslatedName,
            TranslatedDescription = SelectedTranslation.TranslatedDescription,
            SelectedLanguage = Languages.FirstOrDefault(l => l.FilePrefix == SelectedTranslation.LanguageCode)
        };
        IsEditing = true;
    }

    private void CancelEdit()
    {
        EditItem = null;
        IsEditing = false;
        ErrorMessage = null;
    }

    private async Task SaveAsync()
    {
        if (EditItem == null) return;

        if (EditItem.SelectedLanguage == null)
        {
            ErrorMessage = "Please select a language.";
            return;
        }

        if (string.IsNullOrWhiteSpace(EditItem.TranslatedName))
        {
            ErrorMessage = "Translated name is required.";
            return;
        }

        try
        {
            if (_entityType == EntityType.Product && _productTranslationService != null)
            {
                var dto = new ProductTranslationDto
                {
                    ProductTranslationId = EditItem.TranslationId,
                    ProductId = _entityId,
                    LanguageCode = EditItem.SelectedLanguage.FilePrefix,
                    TranslatedName = EditItem.TranslatedName.Trim()
                };

                if (EditItem.TranslationId > 0)
                    await _productTranslationService.UpdateAsync(dto);
                else
                    await _productTranslationService.AddAsync(dto);
            }
            else if (_entityType == EntityType.Category && _categoryTranslationService != null)
            {
                var dto = new CategoryTranslationDto
                {
                    CategoryTranslationId = EditItem.TranslationId,
                    CategoryId = _entityId,
                    LanguageCode = EditItem.SelectedLanguage.FilePrefix,
                    TranslatedName = EditItem.TranslatedName.Trim()
                };

                if (EditItem.TranslationId > 0)
                    await _categoryTranslationService.UpdateAsync(dto);
                else
                    await _categoryTranslationService.AddAsync(dto);
            }
            else if (_entityType == EntityType.Size && _sizeTranslationService != null)
            {
                var dto = new SizeTranslationDto
                {
                    SizeTranslationId = EditItem.TranslationId,
                    SizeId = _entityId,
                    LanguageCode = EditItem.SelectedLanguage.FilePrefix,
                    TranslatedName = EditItem.TranslatedName.Trim()
                };

                if (EditItem.TranslationId > 0)
                    await _sizeTranslationService.UpdateAsync(dto);
                else
                    await _sizeTranslationService.AddAsync(dto);
            }

            CancelEdit();
            await LoadTranslationsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save translation: {ex.Message}";
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedTranslation == null) return;

        try
        {
            if (_entityType == EntityType.Product && _productTranslationService != null)
                await _productTranslationService.DeleteAsync(SelectedTranslation.TranslationId);
            else if (_entityType == EntityType.Category && _categoryTranslationService != null)
                await _categoryTranslationService.DeleteAsync(SelectedTranslation.TranslationId);
            else if (_entityType == EntityType.Size && _sizeTranslationService != null)
                await _sizeTranslationService.DeleteAsync(SelectedTranslation.TranslationId);

            await LoadTranslationsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete translation: {ex.Message}";
        }
    }
}

public class TranslationItemViewModel : BaseViewModel
{
    private int _translationId;
    private int _entityId;
    private string _languageCode = string.Empty;
    private string _languageName = string.Empty;
    private string _translatedName = string.Empty;
    private string? _translatedDescription;
    private LanguageDto? _selectedLanguage;

    public int TranslationId { get => _translationId; set { _translationId = value; OnPropertyChanged(); } }
    public int EntityId { get => _entityId; set { _entityId = value; OnPropertyChanged(); } }
    public string LanguageCode { get => _languageCode; set { _languageCode = value; OnPropertyChanged(); } }
    public string LanguageName { get => _languageName; set { _languageName = value; OnPropertyChanged(); } }
    public string TranslatedName { get => _translatedName; set { _translatedName = value; OnPropertyChanged(); } }
    public string? TranslatedDescription { get => _translatedDescription; set { _translatedDescription = value; OnPropertyChanged(); } }

    public LanguageDto? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (_selectedLanguage != value)
            {
                _selectedLanguage = value;
                OnPropertyChanged();
            }
        }
    }
}