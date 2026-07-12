using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BLL.DTOs;
using BLL.Interfaces;
using Contracts.Enum;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels;

public class SizeManagementViewModel : BaseViewModel
{
    private readonly ISizeService _sizeService;
    private readonly IProductTranslationService _productTranslationService;
    private readonly ICategoryTranslationService _categoryTranslationService;
    private readonly ISizeTranslationService _sizeTranslationService;
    private readonly IDialogService _dialogService;
    private readonly IKeyboardShortcutService _shortcutService;

    private SizeRowViewModel? _selectedSize;
    private string _errorMessage = string.Empty;
    private bool _isEditing;
    private string _editName = string.Empty;
    private string _editDisplayOrder = "0";
    private bool _editIsActive = true;
    private SizeRowViewModel? _editingSize;

    public string AddGesture => GetShortcutGesture(ShortcutAction.Add);
    public string EditGesture => GetShortcutGesture(ShortcutAction.Edit);
    public string DeleteGesture => GetShortcutGesture(ShortcutAction.Delete);

    public SizeManagementViewModel(
        ISizeService sizeService,
        IProductTranslationService productTranslationService,
        ICategoryTranslationService categoryTranslationService,
        ISizeTranslationService sizeTranslationService,
        IDialogService dialogService,
        IKeyboardShortcutService shortcutService)
    {
        _sizeService = sizeService;
        _productTranslationService = productTranslationService;
        _categoryTranslationService = categoryTranslationService;
        _sizeTranslationService = sizeTranslationService;
        _dialogService = dialogService;
        _shortcutService = shortcutService;

        AddCommand = new RelayCommand(StartAdd);
        EditCommand = new RelayCommand(StartEdit, () => SelectedSize != null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedSize != null);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);
        TranslationsCommand = new RelayCommand(OpenTranslations, () => SelectedSize != null);
        RefreshCommand = new AsyncRelayCommand(LoadDataAsync);

        _ = LoadDataAsync();
    }

    public ObservableCollection<SizeRowViewModel> Sizes { get; } = new();

    public SizeRowViewModel? SelectedSize
    {
        get => _selectedSize;
        set
        {
            if (_selectedSize != value)
            {
                _selectedSize = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
                if (EditCommand is RelayCommand e) e.RaiseCanExecuteChanged();
                if (DeleteCommand is RelayCommand d) d.RaiseCanExecuteChanged();
                if (TranslationsCommand is RelayCommand t) t.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasSelection => SelectedSize != null;

    public bool IsEditing
    {
        get => _isEditing;
        set { _isEditing = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotEditing)); }
    }
    public bool IsNotEditing => !IsEditing;

    public string EditName { get => _editName; set { _editName = value; OnPropertyChanged(); } }
    public string EditDisplayOrder { get => _editDisplayOrder; set { _editDisplayOrder = value; OnPropertyChanged(); } }
    public bool EditIsActive { get => _editIsActive; set { _editIsActive = value; OnPropertyChanged(); } }

    public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand TranslationsCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task LoadDataAsync()
    {
        var result = await _sizeService.GetAllSizesAsync();
        Sizes.Clear();
        if (result.IsSuccess && result.Value != null)
        {
            foreach (var s in result.Value)
            {
                Sizes.Add(new SizeRowViewModel
                {
                    Id = s.SizeId,
                    Name = s.Name,
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive,
                    Status = s.IsActive ? "Active" : "Inactive"
                });
            }
        }
    }

    private void StartAdd()
    {
        ErrorMessage = string.Empty;
        _editingSize = null;
        EditName = string.Empty;
        EditDisplayOrder = "0";
        EditIsActive = true;
        IsEditing = true;
    }

    private void StartEdit()
    {
        if (SelectedSize == null) return;
        ErrorMessage = string.Empty;
        _editingSize = SelectedSize;
        EditName = SelectedSize.Name;
        EditDisplayOrder = SelectedSize.DisplayOrder.ToString();
        EditIsActive = SelectedSize.IsActive;
        IsEditing = true;
    }

    private void CancelEdit()
    {
        IsEditing = false;
        _editingSize = null;
        ErrorMessage = string.Empty;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            ErrorMessage = "Size name is required.";
            return;
        }

        if (!int.TryParse(EditDisplayOrder, out int displayOrder))
        {
            ErrorMessage = "Display order must be a number.";
            return;
        }

        try
        {
            var dto = new SizeDto
            {
                SizeId = _editingSize?.Id ?? 0,
                Name = EditName.Trim(),
                DisplayOrder = displayOrder,
                IsActive = EditIsActive
            };

            if (_editingSize == null)
                await _sizeService.AddSizeAsync(dto);
            else
                await _sizeService.UpdateSizeAsync(dto);

            CancelEdit();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save size: {ex.Message}";
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedSize == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete \"{SelectedSize.Name}\"?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sizeService.DeleteSizeAsync(SelectedSize.Id);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete size: {ex.Message}";
        }
    }

    private void OpenTranslations()
    {
        if (SelectedSize == null) return;

        var vm = new TranslationDialogViewModel(
            TranslationDialogViewModel.EntityType.Size,
            SelectedSize.Id,
            SelectedSize.Name,
            _productTranslationService,
            _categoryTranslationService,
            _sizeTranslationService);

        var dialog = new Views.TranslationDialogView { DataContext = vm, Owner = Application.Current.MainWindow };
        vm.RequestClose = () => dialog.Close();
        dialog.ShowDialog();
    }

    private string GetShortcutGesture(ShortcutAction action)
    {
        var bindings = _shortcutService.GetActiveBindings();
        var binding = bindings.FirstOrDefault(b => b.Action == action);
        return binding?.KeyGesture ?? string.Empty;
    }
}

public class SizeRowViewModel : BaseViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
}