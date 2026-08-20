using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BLL.DTOs;
using BLL.Interfaces;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels;

/// <summary>
/// Master-detail ViewModel for modifier group management.
/// </summary>
public partial class ModifierGroupManagementViewModel : BaseViewModel
{
    private readonly IModifierManagementService _managementService;
    private readonly ILocalizationService _localizationService;
    private readonly IDialogService _dialogService;

    private CancellationTokenSource? _groupLoadCts;
    private CancellationTokenSource? _detailLoadCts;
    private int _busyCount;
    private bool _hasLoadedOnce;

    private string _searchText = string.Empty;
    private ModifierGroupListItemViewModel? _selectedGroup;
    private ModifierOptionListItemViewModel? _selectedOption;
    private int? _pendingOptionSelectionId;

    private bool _isEditingGroup;
    private ModifierGroupListItemViewModel? _editingGroup;
    private string _editGroupName = string.Empty;
    private int _editGroupType = 1;
    private bool _editIsRequired;
    private bool _editIsActive = true;
    private string _editMinSelections = "0";
    private string _editMaxSelections = "1";
    private string _editSortOrder = "0";

    private bool _isEditingOption;
    private ModifierOptionListItemViewModel? _editingOption;
    private string _editOptionName = string.Empty;
    private string _editOptionPrice = "0.00";
    private bool _editOptionAllowQuantity;
    private bool _editOptionIsDefault;
    private bool _editOptionIsActive = true;
    private string _editOptionSortOrder = "0";

    private string _detailMinSelections = "0";
    private string _detailMaxSelections = "0";
    private string _detailSortOrder = "0";
    private string _errorMessage = string.Empty;
    private List<ModifierGroupSummaryDto> _allGroups = new();

    public ModifierGroupManagementViewModel(
        IModifierManagementService managementService,
        ILocalizationService localizationService,
        IDialogService dialogService)
    {
        _managementService = managementService;
        _localizationService = localizationService;
        _dialogService = dialogService;

        _localizationService.LanguageChanged += OnLanguageChanged;

        AddGroupCommand = new RelayCommand(OpenAddGroupDialog, () => !IsBusy);
        EditGroupCommand = new AsyncRelayCommand(OpenEditGroupDialogAsync, CanEditGroup);
        DeleteGroupCommand = new AsyncRelayCommand(DeleteGroupAsync, CanEditGroup);
        SaveGroupCommand = new AsyncRelayCommand(SaveGroupAsync, () => !IsBusy);
        CancelGroupEditCommand = new RelayCommand(CancelGroupEdit);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);

        AddOptionCommand = new RelayCommand(OpenAddOptionDialog, CanAddOption);
        EditOptionCommand = new RelayCommand(OpenEditOptionDialog, CanEditOption);
        DeleteOptionCommand = new AsyncRelayCommand(DeleteOptionAsync, CanEditOption);
        SaveOptionCommand = new AsyncRelayCommand(SaveOptionAsync, () => !IsBusy);
        CancelOptionEditCommand = new RelayCommand(CancelOptionEdit);

        GroupTranslationsCommand = new RelayCommand(OpenGroupTranslations, () => SelectedGroup != null && SelectedGroup.ModifierGroupId > 0);
        OptionTranslationsCommand = new RelayCommand(OpenOptionTranslations, () => SelectedOption != null && SelectedOption.ModifierOptionId > 0);
    }

    public ObservableCollection<ModifierGroupListItemViewModel> Groups { get; } = new();

    public ObservableCollection<ModifierOptionListItemViewModel> Options { get; } = new();

    public Action? RequestClose { get; set; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

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
            if (_selectedGroup == value)
                return;

            if (_selectedGroup != null)
                _selectedGroup.IsSelected = false;

            _selectedGroup = value;

            if (_selectedGroup != null)
                _selectedGroup.IsSelected = true;

            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(HasDetail));
            OnPropertyChanged(nameof(IsOptionsEmpty));
            RaiseCommandStatesChanged();

            CancelGroupEdit();
            CancelOptionEdit();

            if (_selectedGroup == null)
            {
                SelectedOption = null;
                Options.Clear();
                ResetDetailSummary();
                return;
            }

            _ = LoadGroupDetailAsync(_selectedGroup.ModifierGroupId, _pendingOptionSelectionId);
            _pendingOptionSelectionId = null;
        }
    }

    public ModifierOptionListItemViewModel? SelectedOption
    {
        get => _selectedOption;
        set
        {
            if (_selectedOption == value)
                return;

            if (_selectedOption != null)
                _selectedOption.IsSelected = false;

            _selectedOption = value;

            if (_selectedOption != null)
                _selectedOption.IsSelected = true;

            OnPropertyChanged();
            OnPropertyChanged(nameof(HasOptionSelection));
            RaiseCommandStatesChanged();
        }
    }

    public bool HasSelection => SelectedGroup != null;

    public bool HasDetail => SelectedGroup != null;

    public bool HasOptionSelection => SelectedOption != null;

    public bool HasGroups => Groups.Count > 0;

    public bool IsGroupSearchEmpty => !IsBusy && Groups.Count == 0;

    public bool IsOptionsEmpty => HasDetail && !IsBusy && Options.Count == 0;

    public bool IsBusy => _busyCount > 0;

    public bool IsNotBusy => !IsBusy;

    public bool IsEditingGroup
    {
        get => _isEditingGroup;
        private set
        {
            if (_isEditingGroup == value)
                return;

            _isEditingGroup = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GroupDialogTitle));
        }
    }

    public bool IsEditingOption
    {
        get => _isEditingOption;
        private set
        {
            if (_isEditingOption == value)
                return;

            _isEditingOption = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(OptionDialogTitle));
        }
    }

    public string GroupDialogTitle => _editingGroup == null
        ? L("Manager.ModifierGroup.AddGroupTitle", "Add Group")
        : L("Manager.ModifierGroup.EditGroupTitle", "Edit Group");

    public string OptionDialogTitle => _editingOption == null
        ? L("Manager.ModifierGroup.AddOptionTitle", "Add Option")
        : L("Manager.ModifierGroup.EditOptionTitle", "Edit Option");

    public string EditGroupName
    {
        get => _editGroupName;
        set
        {
            if (_editGroupName == value)
                return;

            _editGroupName = value;
            OnPropertyChanged();
        }
    }

    public int EditGroupType
    {
        get => _editGroupType;
        set
        {
            if (_editGroupType == value)
                return;

            _editGroupType = value;
            OnPropertyChanged();
        }
    }

    public bool EditIsRequired
    {
        get => _editIsRequired;
        set
        {
            if (_editIsRequired == value)
                return;

            _editIsRequired = value;
            OnPropertyChanged();
        }
    }

    public bool EditIsActive
    {
        get => _editIsActive;
        set
        {
            if (_editIsActive == value)
                return;

            _editIsActive = value;
            OnPropertyChanged();
        }
    }

    public string EditMinSelections
    {
        get => _editMinSelections;
        set
        {
            if (_editMinSelections == value)
                return;

            _editMinSelections = value;
            OnPropertyChanged();
        }
    }

    public string EditMaxSelections
    {
        get => _editMaxSelections;
        set
        {
            if (_editMaxSelections == value)
                return;

            _editMaxSelections = value;
            OnPropertyChanged();
        }
    }

    public string EditSortOrder
    {
        get => _editSortOrder;
        set
        {
            if (_editSortOrder == value)
                return;

            _editSortOrder = value;
            OnPropertyChanged();
        }
    }

    public string EditOptionName
    {
        get => _editOptionName;
        set
        {
            if (_editOptionName == value)
                return;

            _editOptionName = value;
            OnPropertyChanged();
        }
    }

    public string EditOptionPrice
    {
        get => _editOptionPrice;
        set
        {
            if (_editOptionPrice == value)
                return;

            _editOptionPrice = value;
            OnPropertyChanged();
        }
    }

    public bool EditOptionAllowQuantity
    {
        get => _editOptionAllowQuantity;
        set
        {
            if (_editOptionAllowQuantity == value)
                return;

            _editOptionAllowQuantity = value;
            OnPropertyChanged();
        }
    }

    public bool EditOptionIsDefault
    {
        get => _editOptionIsDefault;
        set
        {
            if (_editOptionIsDefault == value)
                return;

            _editOptionIsDefault = value;
            OnPropertyChanged();
        }
    }

    public bool EditOptionIsActive
    {
        get => _editOptionIsActive;
        set
        {
            if (_editOptionIsActive == value)
                return;

            _editOptionIsActive = value;
            OnPropertyChanged();
        }
    }

    public string EditOptionSortOrder
    {
        get => _editOptionSortOrder;
        set
        {
            if (_editOptionSortOrder == value)
                return;

            _editOptionSortOrder = value;
            OnPropertyChanged();
        }
    }

    public string DetailMinSelections
    {
        get => _detailMinSelections;
        private set
        {
            if (_detailMinSelections == value)
                return;

            _detailMinSelections = value;
            OnPropertyChanged();
        }
    }

    public string DetailMaxSelections
    {
        get => _detailMaxSelections;
        private set
        {
            if (_detailMaxSelections == value)
                return;

            _detailMaxSelections = value;
            OnPropertyChanged();
        }
    }

    public string DetailSortOrder
    {
        get => _detailSortOrder;
        private set
        {
            if (_detailSortOrder == value)
                return;

            _detailSortOrder = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value)
                return;

            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public List<KeyValuePair<int, string>> GroupTypeOptions => new()
    {
        new(1, L("Manager.ModifierGroup.TypeSingle", "Single Select")),
        new(2, L("Manager.ModifierGroup.TypeMulti", "Multi Select")),
        new(3, L("Manager.ModifierGroup.TypeQuantity", "Quantity"))
    };

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

    public ICommand GroupTranslationsCommand { get; }

    public ICommand OptionTranslationsCommand { get; }

    public Task EnsureDataLoadedAsync()
    {
        if (_hasLoadedOnce)
            return Task.CompletedTask;

        _hasLoadedOnce = true;
        return LoadGroupsAsync();
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(GroupTypeOptions));
        OnPropertyChanged(nameof(GroupDialogTitle));
        OnPropertyChanged(nameof(OptionDialogTitle));
        _ = LoadGroupsAsync(SelectedGroup?.ModifierGroupId, SelectedOption?.ModifierOptionId);
    }

    private bool CanEditGroup() => !IsBusy && SelectedGroup != null;

    private bool CanAddOption() => !IsBusy && SelectedGroup != null;

    private bool CanEditOption() => !IsBusy && SelectedOption != null;

    private void RaiseCommandStatesChanged()
    {
        if (AddGroupCommand is RelayCommand addGroup)
            addGroup.RaiseCanExecuteChanged();
        if (EditGroupCommand is AsyncRelayCommand editGroup)
            editGroup.RaiseCanExecuteChanged();
        if (DeleteGroupCommand is AsyncRelayCommand deleteGroup)
            deleteGroup.RaiseCanExecuteChanged();
        if (SaveGroupCommand is AsyncRelayCommand saveGroup)
            saveGroup.RaiseCanExecuteChanged();
        if (RefreshCommand is AsyncRelayCommand refresh)
            refresh.RaiseCanExecuteChanged();

        if (AddOptionCommand is RelayCommand addOption)
            addOption.RaiseCanExecuteChanged();
        if (EditOptionCommand is RelayCommand editOption)
            editOption.RaiseCanExecuteChanged();
        if (DeleteOptionCommand is AsyncRelayCommand deleteOption)
            deleteOption.RaiseCanExecuteChanged();
        if (SaveOptionCommand is AsyncRelayCommand saveOption)
            saveOption.RaiseCanExecuteChanged();

        if (GroupTranslationsCommand is RelayCommand groupTranslations)
            groupTranslations.RaiseCanExecuteChanged();
        if (OptionTranslationsCommand is RelayCommand optionTranslations)
            optionTranslations.RaiseCanExecuteChanged();

        OnPropertyChanged(nameof(IsGroupSearchEmpty));
        OnPropertyChanged(nameof(IsOptionsEmpty));
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(IsNotBusy));
    }

    private void SetBusy(bool isBusy)
    {
        _busyCount = Math.Max(0, _busyCount + (isBusy ? 1 : -1));
        RaiseCommandStatesChanged();
    }

    private void ResetDetailSummary()
    {
        DetailMinSelections = "0";
        DetailMaxSelections = "0";
        DetailSortOrder = "0";
        OnPropertyChanged(nameof(IsOptionsEmpty));
    }

    private static CancellationTokenSource ReplaceCancellationToken(ref CancellationTokenSource? field)
    {
        try
        {
            field?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            field = null;
        }

        field?.Dispose();
        field = new CancellationTokenSource();
        return field;
    }

    private static bool IsActiveRequest(CancellationTokenSource? current, CancellationTokenSource candidate)
    {
        return ReferenceEquals(current, candidate) && !candidate.IsCancellationRequested;
    }

    private static void ReleaseCancellationToken(ref CancellationTokenSource? field, CancellationTokenSource candidate)
    {
        if (ReferenceEquals(field, candidate))
            field = null;

        candidate.Dispose();
    }

    private string L(string key, string fallback)
    {
        string value = _localizationService.GetString(key);
        return string.IsNullOrWhiteSpace(value) || string.Equals(value, key, StringComparison.Ordinal)
            ? fallback
            : value;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _localizationService.LanguageChanged -= OnLanguageChanged;

            try
            {
                _groupLoadCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            try
            {
                _detailLoadCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            _groupLoadCts?.Dispose();
            _detailLoadCts?.Dispose();
            _groupLoadCts = null;
            _detailLoadCts = null;
        }

        base.Dispose(disposing);
    }
}
