using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BLL.DTOs;

namespace UI.ViewModels;

public partial class ModifierGroupManagementViewModel
{
    private async Task RefreshAsync()
    {
        await LoadGroupsAsync(SelectedGroup?.ModifierGroupId, SelectedOption?.ModifierOptionId);
    }

    private async Task LoadGroupsAsync(int? preserveSelectedGroupId = null, int? preserveSelectedOptionId = null)
    {
        CancellationTokenSource cts = ReplaceCancellationToken(ref _groupLoadCts);
        SetBusy(true);

        try
        {
            string languageCode = _localizationService.CurrentLanguage.FilePrefix;
            var result = await _managementService.GetAllGroupsAsync(languageCode);

            if (!IsActiveRequest(_groupLoadCts, cts))
                return;

            if (!result.IsSuccess || result.Value == null)
            {
                ErrorMessage = result.Error ?? L("Modifiers.LoadGroupsFailed", "Failed to load groups.");
                return;
            }

            ErrorMessage = string.Empty;
            _allGroups = result.Value;
            ApplyFilter();
            RestoreSelectedGroup(preserveSelectedGroupId, preserveSelectedOptionId);
        }
        finally
        {
            ReleaseCancellationToken(ref _groupLoadCts, cts);
            SetBusy(false);
        }
    }

    private void ApplyFilter()
    {
        int? selectedGroupId = _selectedGroup?.ModifierGroupId;
        int? selectedOptionId = _selectedOption?.ModifierOptionId;
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allGroups
            : _allGroups.Where(group => group.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

        Groups.Clear();

        foreach (ModifierGroupSummaryDto group in filtered)
            Groups.Add(MapToGroupListItem(group));

        if (selectedGroupId.HasValue)
        {
            ModifierGroupListItemViewModel? refreshedSelection = Groups.FirstOrDefault(group => group.ModifierGroupId == selectedGroupId.Value);
            if (refreshedSelection == null && SelectedGroup != null)
            {
                SelectedGroup = null;
            }
            else if (refreshedSelection != null && !ReferenceEquals(refreshedSelection, SelectedGroup))
            {
                if (_selectedGroup != null)
                    _selectedGroup.IsSelected = false;

                _selectedGroup = refreshedSelection;
                _selectedGroup.IsSelected = true;
                _pendingOptionSelectionId = selectedOptionId;
                OnPropertyChanged(nameof(SelectedGroup));
                RaiseCommandStatesChanged();
            }
        }

        OnPropertyChanged(nameof(HasGroups));
        OnPropertyChanged(nameof(IsGroupSearchEmpty));
    }

    private void RestoreSelectedGroup(int? preserveSelectedGroupId, int? preserveSelectedOptionId)
    {
        if (!preserveSelectedGroupId.HasValue)
        {
            if (SelectedGroup != null)
                SelectedGroup = null;

            return;
        }

        ModifierGroupListItemViewModel? nextSelection = Groups.FirstOrDefault(group => group.ModifierGroupId == preserveSelectedGroupId.Value);
        _pendingOptionSelectionId = preserveSelectedOptionId;
        SelectedGroup = nextSelection;
    }

    private async Task LoadGroupDetailAsync(int groupId, int? preserveSelectedOptionId = null)
    {
        CancellationTokenSource cts = ReplaceCancellationToken(ref _detailLoadCts);
        SetBusy(true);

        try
        {
            string languageCode = _localizationService.CurrentLanguage.FilePrefix;
            var result = await _managementService.GetGroupDetailAsync(groupId, languageCode);

            if (!IsActiveRequest(_detailLoadCts, cts))
                return;

            if (!result.IsSuccess || result.Value == null)
            {
                ErrorMessage = result.Error ?? L("Modifiers.LoadGroupsFailed", "Failed to load groups.");
                return;
            }

            if (SelectedGroup == null || SelectedGroup.ModifierGroupId != groupId)
                return;

            ErrorMessage = string.Empty;
            DetailMinSelections = result.Value.MinSelections.ToString();
            DetailMaxSelections = result.Value.MaxSelections.ToString();
            DetailSortOrder = result.Value.SortOrder.ToString();

            Options.Clear();
            foreach (ModifierOptionDetailDto option in result.Value.Options)
                Options.Add(MapToOptionListItem(option));

            SelectedOption = preserveSelectedOptionId.HasValue
                ? Options.FirstOrDefault(option => option.ModifierOptionId == preserveSelectedOptionId.Value)
                : null;

            OnPropertyChanged(nameof(IsOptionsEmpty));
        }
        finally
        {
            ReleaseCancellationToken(ref _detailLoadCts, cts);
            SetBusy(false);
        }
    }
}
