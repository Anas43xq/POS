using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BLL.DTOs;
using BLL.Interfaces;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels;

/// <summary>
/// Master-detail ViewModel for Modifier Group management.
/// Left panel: searchable list of modifier groups.
/// Right panel: group detail (general info, rules, modifier options).
/// Follows SizeManagementViewModel patterns.
/// </summary>
public class ModifierGroupManagementViewModel : BaseViewModel
{
    private readonly IModifierManagementService _managementService;
    private readonly ILocalizationService _localizationService;
    private readonly IDialogService _dialogService;

    // ── Group list state ────────────────────────────────
    private string _searchText = string.Empty;
    private ModifierGroupListItemViewModel? _selectedGroup;

    // ── Group edit state ────────────────────────────────
    private bool _isEditingGroup;
    private ModifierGroupListItemViewModel? _editingGroup;
    private string _editGroupName = string.Empty;
    private int _editGroupType = 1;
    private bool _editIsRequired;
    private bool _editIsActive = true;
    private string _editMinSelections = "0";
    private string _editMaxSelections = "1";
    private string _editSortOrder = "0";

    // ── Option list state ───────────────────────────────
    private ModifierOptionListItemViewModel? _selectedOption;

    // ── Option edit state ───────────────────────────────
    private bool _isEditingOption;
    private ModifierOptionListItemViewModel? _editingOption;
    private string _editOptionName = string.Empty;
    private string _editOptionPrice = "0";
    private bool _editOptionAllowQuantity;
    private bool _editOptionIsDefault;
    private bool _editOptionIsActive = true;
    private string _editOptionSortOrder = "0";

    // ── Error state ─────────────────────────────────────
    private string _errorMessage = string.Empty;

    public ModifierGroupManagementViewModel(
        IModifierManagementService managementService,
        ILocalizationService localizationService,
        IDialogService dialogService)
    {
        _managementService = managementService;
        _localizationService = localizationService;
        _dialogService = dialogService;

        // Group commands
        AddGroupCommand = new RelayCommand(StartAddGroup);
        EditGroupCommand = new RelayCommand(StartEditGroup, () => SelectedGroup != null);
        DeleteGroupCommand = new AsyncRelayCommand(DeleteGroupAsync, () => SelectedGroup != null);
        SaveGroupCommand = new AsyncRelayCommand(SaveGroupAsync);
        CancelGroupEditCommand = new RelayCommand(CancelGroupEdit);
        RefreshCommand = new AsyncRelayCommand(LoadGroupsAsync);

        // Option commands
        AddOptionCommand = new RelayCommand(StartAddOption, () => SelectedGroup != null);
        EditOptionCommand = new RelayCommand(StartEditOption, () => SelectedOption != null);
        DeleteOptionCommand = new AsyncRelayCommand(DeleteOptionAsync, () => SelectedOption != null);
        SaveOptionCommand = new AsyncRelayCommand(SaveOptionAsync);
        CancelOptionEditCommand = new RelayCommand(CancelOptionEdit);

        // NOTE: Data is intentionally NOT loaded here — see
        // ProductManagementViewModel for the rationale. Load is triggered
        // on first navigation via EnsureDataLoadedAsync(), called from
        // ManagerMainViewModel.NavigateToModifierGroupManagement().
    }

    private bool _hasLoadedOnce;

    /// <summary>
    /// Loads data the first time this page is navigated to; subsequent
    /// navigations are no-ops (use RefreshCommand to force a reload).
    /// </summary>
    public Task EnsureDataLoadedAsync()
    {
        if (_hasLoadedOnce)
            return Task.CompletedTask;

        _hasLoadedOnce = true;
        return LoadGroupsAsync();
    }

    // ── Collections ─────────────────────────────────────

    public ObservableCollection<ModifierGroupListItemViewModel> Groups { get; } = new();
    public ObservableCollection<ModifierOptionListItemViewModel> Options { get; } = new();

    // ── Group list properties ───────────────────────────

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public ModifierGroupListItemViewModel? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (_selectedGroup != value)
            {
                // Deselect previous
                if (_selectedGroup != null) _selectedGroup.IsSelected = false;
                _selectedGroup = value;
                if (_selectedGroup != null) _selectedGroup.IsSelected = true;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(HasDetail));
                RaiseGroupCommandCanExecute();
                CancelGroupEdit();
                CancelOptionEdit();
                _ = LoadGroupDetailAsync();
            }
        }
    }

    public bool HasSelection => SelectedGroup != null;
    public bool HasDetail => SelectedGroup != null;

    // ── Group edit properties ───────────────────────────

    public bool IsEditingGroup
    {
        get => _isEditingGroup;
        set { _isEditingGroup = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotEditingGroup)); }
    }
    public bool IsNotEditingGroup => !IsEditingGroup;

    public string EditGroupName { get => _editGroupName; set { _editGroupName = value; OnPropertyChanged(); } }
    public int EditGroupType { get => _editGroupType; set { _editGroupType = value; OnPropertyChanged(); } }
    public bool EditIsRequired { get => _editIsRequired; set { _editIsRequired = value; OnPropertyChanged(); } }
    public bool EditIsActive { get => _editIsActive; set { _editIsActive = value; OnPropertyChanged(); } }
    public string EditMinSelections { get => _editMinSelections; set { _editMinSelections = value; OnPropertyChanged(); } }
    public string EditMaxSelections { get => _editMaxSelections; set { _editMaxSelections = value; OnPropertyChanged(); } }
    public string EditSortOrder { get => _editSortOrder; set { _editSortOrder = value; OnPropertyChanged(); } }

    // ── Option list properties ──────────────────────────

    public ModifierOptionListItemViewModel? SelectedOption
    {
        get => _selectedOption;
        set
        {
            if (_selectedOption != value)
            {
                if (_selectedOption != null) _selectedOption.IsSelected = false;
                _selectedOption = value;
                if (_selectedOption != null) _selectedOption.IsSelected = true;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasOptionSelection));
                RaiseOptionCommandCanExecute();
            }
        }
    }

    public bool HasOptionSelection => SelectedOption != null;

    // ── Option edit properties ──────────────────────────

    public bool IsEditingOption
    {
        get => _isEditingOption;
        set { _isEditingOption = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotEditingOption)); }
    }
    public bool IsNotEditingOption => !IsEditingOption;

    public string EditOptionName { get => _editOptionName; set { _editOptionName = value; OnPropertyChanged(); } }
    public string EditOptionPrice { get => _editOptionPrice; set { _editOptionPrice = value; OnPropertyChanged(); } }
    public bool EditOptionAllowQuantity { get => _editOptionAllowQuantity; set { _editOptionAllowQuantity = value; OnPropertyChanged(); } }
    public bool EditOptionIsDefault { get => _editOptionIsDefault; set { _editOptionIsDefault = value; OnPropertyChanged(); } }
    public bool EditOptionIsActive { get => _editOptionIsActive; set { _editOptionIsActive = value; OnPropertyChanged(); } }
    public string EditOptionSortOrder { get => _editOptionSortOrder; set { _editOptionSortOrder = value; OnPropertyChanged(); } }

    // ── Error ───────────────────────────────────────────

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    // ── Group type options for ComboBox ──────────────────

    public List<KeyValuePair<int, string>> GroupTypeOptions => new()
    {
        new(1, "Single Select"),
        new(2, "Multi Select"),
        new(3, "Quantity")
    };

    // ── Commands ────────────────────────────────────────

    public ICommand AddGroupCommand { get; }
    public ICommand EditGroupCommand { get; }
    public ICommand DeleteGroupCommand { get; }
    public ICommand SaveGroupCommand { get; }
    public ICommand CancelGroupEditCommand { get; }
    public ICommand RefreshCommand { get; }

    public ICommand AddOptionCommand { get; }
    public ICommand EditOptionCommand { get; }
    public ICommand DeleteOptionCommand { get; }
    public ICommand SaveOptionCommand { get; }
    public ICommand CancelOptionEditCommand { get; }

    // ── Group CRUD ──────────────────────────────────────

    private List<ModifierGroupSummaryDto> _allGroups = new();

    private async Task LoadGroupsAsync()
    {
        var result = await _managementService.GetAllGroupsAsync();
        Groups.Clear();
        if (result.IsSuccess && result.Value != null)
        {
            _allGroups = result.Value;
            foreach (var g in result.Value)
            {
                Groups.Add(MapToGroupListItem(g));
            }
        }
        else if (!result.IsSuccess)
        {
            ErrorMessage = result.Error ?? "Failed to load groups.";
        }
    }

    private void ApplyFilter()
    {
        Groups.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allGroups
            : _allGroups.Where(g => g.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var g in filtered)
            Groups.Add(MapToGroupListItem(g));
    }

    private async Task LoadGroupDetailAsync()
    {
        Options.Clear();
        if (SelectedGroup == null) return;

        var result = await _managementService.GetGroupDetailAsync(SelectedGroup.ModifierGroupId);
        if (result.IsSuccess && result.Value != null)
        {
            foreach (var o in result.Value.Options)
            {
                Options.Add(MapToOptionListItem(o));
            }
        }
    }

    private void StartAddGroup()
    {
        ErrorMessage = string.Empty;
        _editingGroup = null;
        EditGroupName = string.Empty;
        EditGroupType = 1;
        EditIsRequired = false;
        EditIsActive = true;
        EditMinSelections = "0";
        EditMaxSelections = "1";
        EditSortOrder = "0";
        IsEditingGroup = true;
    }

    private void StartEditGroup()
    {
        if (SelectedGroup == null) return;
        ErrorMessage = string.Empty;
        _editingGroup = SelectedGroup;
        EditGroupName = SelectedGroup.Name;
        EditGroupType = SelectedGroup.GroupType;
        EditIsRequired = SelectedGroup.IsRequired;
        EditIsActive = SelectedGroup.IsActive;
        EditMinSelections = "0"; // Will be loaded from detail
        EditMaxSelections = "1";
        EditSortOrder = SelectedGroup.SortOrder.ToString();

        // Load full detail to get min/max
        _ = LoadGroupEditFieldsAsync(SelectedGroup.ModifierGroupId);
        IsEditingGroup = true;
    }

    private async Task LoadGroupEditFieldsAsync(int groupId)
    {
        var result = await _managementService.GetGroupDetailAsync(groupId);
        if (result.IsSuccess && result.Value != null)
        {
            EditMinSelections = result.Value.MinSelections.ToString();
            EditMaxSelections = result.Value.MaxSelections.ToString();
        }
    }

    private void CancelGroupEdit()
    {
        IsEditingGroup = false;
        _editingGroup = null;
        ErrorMessage = string.Empty;
    }

    private async Task SaveGroupAsync()
    {
        if (string.IsNullOrWhiteSpace(EditGroupName))
        {
            ErrorMessage = "Group name is required.";
            return;
        }

        if (!int.TryParse(EditMinSelections, out int minSel) || minSel < 0)
        {
            ErrorMessage = "Min selections must be a non-negative number.";
            return;
        }

        if (!int.TryParse(EditMaxSelections, out int maxSel) || maxSel < 0)
        {
            ErrorMessage = "Max selections must be a non-negative number.";
            return;
        }

        if (maxSel < minSel)
        {
            ErrorMessage = "Max selections must be >= Min selections.";
            return;
        }

        if (EditIsRequired && minSel < 1)
        {
            ErrorMessage = "Required groups must have Min selections >= 1.";
            return;
        }

        if (!int.TryParse(EditSortOrder, out int sortOrder))
        {
            ErrorMessage = "Sort order must be a number.";
            return;
        }

        var dto = new ModifierGroupWriteDto
        {
            ModifierGroupId = _editingGroup?.ModifierGroupId,
            Name = EditGroupName.Trim(),
            GroupType = (byte)EditGroupType,
            IsRequired = EditIsRequired,
            IsActive = EditIsActive,
            MinSelections = minSel,
            MaxSelections = maxSel,
            SortOrder = sortOrder
        };

        BLL.Models.Result<int> createResult;
        BLL.Models.Result<bool> updateResult;

        if (_editingGroup == null)
        {
            createResult = await _managementService.CreateGroupAsync(dto);
            if (!createResult.IsSuccess)
            {
                ErrorMessage = createResult.Error ?? "Failed to create group.";
                return;
            }
        }
        else
        {
            updateResult = await _managementService.UpdateGroupAsync(dto);
            if (!updateResult.IsSuccess)
            {
                ErrorMessage = updateResult.Error ?? "Failed to update group.";
                return;
            }
        }

        CancelGroupEdit();
        await LoadGroupsAsync();
    }

    private async Task DeleteGroupAsync()
    {
        if (SelectedGroup == null) return;

        var confirm = MessageBox.Show(
            $"Are you sure you want to delete \"{SelectedGroup.Name}\"?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        var result = await _managementService.DeleteGroupAsync(SelectedGroup.ModifierGroupId);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error ?? "Failed to delete group.";
            return;
        }

        SelectedGroup = null;
        await LoadGroupsAsync();
    }

    // ── Option CRUD ─────────────────────────────────────

    private void StartAddOption()
    {
        if (SelectedGroup == null) return;
        ErrorMessage = string.Empty;
        _editingOption = null;
        EditOptionName = string.Empty;
        EditOptionPrice = "0";
        EditOptionAllowQuantity = false;
        EditOptionIsDefault = false;
        EditOptionIsActive = true;
        EditOptionSortOrder = Options.Count.ToString();
        IsEditingOption = true;
    }

    private void StartEditOption()
    {
        if (SelectedOption == null) return;
        ErrorMessage = string.Empty;
        _editingOption = SelectedOption;
        EditOptionName = SelectedOption.Name;
        EditOptionPrice = SelectedOption.PriceAdd.ToString("0.00");
        EditOptionAllowQuantity = SelectedOption.AllowQuantity;
        EditOptionIsDefault = SelectedOption.IsDefault;
        EditOptionIsActive = SelectedOption.IsActive;
        EditOptionSortOrder = SelectedOption.SortOrder.ToString();
        IsEditingOption = true;
    }

    private void CancelOptionEdit()
    {
        IsEditingOption = false;
        _editingOption = null;
        ErrorMessage = string.Empty;
    }

    private async Task SaveOptionAsync()
    {
        if (SelectedGroup == null) return;

        if (string.IsNullOrWhiteSpace(EditOptionName))
        {
            ErrorMessage = "Option name is required.";
            return;
        }

        if (!decimal.TryParse(EditOptionPrice, out decimal price) || price < 0)
        {
            ErrorMessage = "Price must be a non-negative number.";
            return;
        }

        if (!int.TryParse(EditOptionSortOrder, out int sortOrder))
        {
            ErrorMessage = "Sort order must be a number.";
            return;
        }

        var dto = new ModifierOptionWriteDto
        {
            ModifierOptionId = _editingOption?.ModifierOptionId,
            ModifierGroupId = SelectedGroup.ModifierGroupId,
            Name = EditOptionName.Trim(),
            PriceAdd = price,
            AllowQuantity = EditOptionAllowQuantity,
            IsDefault = EditOptionIsDefault,
            IsActive = EditOptionIsActive,
            SortOrder = sortOrder
        };

        if (_editingOption == null)
        {
            var createResult = await _managementService.CreateOptionAsync(dto);
            if (!createResult.IsSuccess)
            {
                ErrorMessage = createResult.Error ?? "Failed to create option.";
                return;
            }
        }
        else
        {
            var updateResult = await _managementService.UpdateOptionAsync(dto);
            if (!updateResult.IsSuccess)
            {
                ErrorMessage = updateResult.Error ?? "Failed to update option.";
                return;
            }
        }

        CancelOptionEdit();
        await LoadGroupDetailAsync();
        // Refresh group list to update option count
        await LoadGroupsAsync();
    }

    private async Task DeleteOptionAsync()
    {
        if (SelectedOption == null || SelectedGroup == null) return;

        var confirm = MessageBox.Show(
            $"Are you sure you want to delete option \"{SelectedOption.Name}\"?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        var result = await _managementService.DeleteOptionAsync(SelectedOption.ModifierOptionId);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error ?? "Failed to delete option.";
            return;
        }

        SelectedOption = null;
        await LoadGroupDetailAsync();
        await LoadGroupsAsync();
    }

    // ── CanExecute helpers ──────────────────────────────

    private void RaiseGroupCommandCanExecute()
    {
        if (EditGroupCommand is RelayCommand e) e.RaiseCanExecuteChanged();
        if (DeleteGroupCommand is RelayCommand d) d.RaiseCanExecuteChanged();
        if (AddOptionCommand is RelayCommand a) a.RaiseCanExecuteChanged();
    }

    private void RaiseOptionCommandCanExecute()
    {
        if (EditOptionCommand is RelayCommand e) e.RaiseCanExecuteChanged();
        if (DeleteOptionCommand is RelayCommand d) d.RaiseCanExecuteChanged();
    }

    // ── Mapping helpers ─────────────────────────────────

    private static ModifierGroupListItemViewModel MapToGroupListItem(ModifierGroupSummaryDto dto) => new()
    {
        ModifierGroupId = dto.ModifierGroupId,
        Name = dto.Name,
        GroupType = dto.GroupType,
        GroupTypeDisplay = dto.GroupTypeDisplay,
        IsRequired = dto.IsRequired,
        IsActive = dto.IsActive,
        OptionCount = dto.OptionCount,
        SortOrder = dto.SortOrder
    };

    private static ModifierOptionListItemViewModel MapToOptionListItem(ModifierOptionDetailDto dto) => new()
    {
        ModifierOptionId = dto.ModifierOptionId,
        Name = dto.Name,
        PriceAdd = dto.PriceAdd,
        AllowQuantity = dto.AllowQuantity,
        IsDefault = dto.IsDefault,
        IsActive = dto.IsActive,
        SortOrder = dto.SortOrder
    };
}