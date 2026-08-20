using System.Threading.Tasks;
using System.Windows;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using UI.Views;
using UI.Views.Modifiers;

namespace UI.ViewModels;

public partial class ModifierGroupManagementViewModel
{
    private void OpenAddGroupDialog()
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

        _dialogService.ShowDialog<ModifierGroupEditDialogView>(this);
    }

    private async Task OpenEditGroupDialogAsync()
    {
        if (SelectedGroup == null || IsBusy)
            return;

        ErrorMessage = string.Empty;
        _editingGroup = SelectedGroup;
        EditGroupName = SelectedGroup.Name;
        EditGroupType = SelectedGroup.GroupType;
        EditIsRequired = SelectedGroup.IsRequired;
        EditIsActive = SelectedGroup.IsActive;
        EditMinSelections = DetailMinSelections;
        EditMaxSelections = DetailMaxSelections;
        EditSortOrder = DetailSortOrder;

        await LoadGroupEditFieldsAsync(SelectedGroup.ModifierGroupId, _localizationService.CurrentLanguage.FilePrefix);

        IsEditingGroup = true;
        _dialogService.ShowDialog<ModifierGroupEditDialogView>(this);
    }

    private async Task LoadGroupEditFieldsAsync(int groupId, string languageCode)
    {
        var result = await _managementService.GetGroupDetailAsync(groupId, languageCode);
        if (result.IsSuccess && result.Value != null)
        {
            EditMinSelections = result.Value.MinSelections.ToString();
            EditMaxSelections = result.Value.MaxSelections.ToString();
            EditSortOrder = result.Value.SortOrder.ToString();
        }
    }

    private void CancelGroupEdit()
    {
        IsEditingGroup = false;
        _editingGroup = null;
        ErrorMessage = string.Empty;
        RequestClose?.Invoke();
    }

    private async Task SaveGroupAsync()
    {
        if (!int.TryParse(EditMinSelections, out int minSelections))
        {
            ErrorMessage = L("Modifiers.MinSelectionsMustBeNumber", "Min selections must be a number.");
            return;
        }

        if (!int.TryParse(EditMaxSelections, out int maxSelections))
        {
            ErrorMessage = L("Modifiers.MaxSelectionsMustBeNumber", "Max selections must be a number.");
            return;
        }

        if (!int.TryParse(EditSortOrder, out int sortOrder))
        {
            ErrorMessage = L("Modifiers.SortOrderMustBeNumber", "Sort order must be a number.");
            return;
        }

        var dto = new ModifierGroupWriteDto
        {
            ModifierGroupId = _editingGroup?.ModifierGroupId,
            Name = EditGroupName.Trim(),
            GroupType = (byte)EditGroupType,
            IsRequired = EditIsRequired,
            IsActive = EditIsActive,
            MinSelections = minSelections,
            MaxSelections = maxSelections,
            SortOrder = sortOrder
        };

        SetBusy(true);

        try
        {
            int targetGroupId;

            if (_editingGroup == null)
            {
                var createResult = await _managementService.CreateGroupAsync(dto);
                if (!createResult.IsSuccess)
                {
                    ErrorMessage = createResult.Error ?? L("Modifiers.CreateGroupFailed", "Failed to create group.");
                    return;
                }

                targetGroupId = createResult.Value;
            }
            else
            {
                var updateResult = await _managementService.UpdateGroupAsync(dto);
                if (!updateResult.IsSuccess)
                {
                    ErrorMessage = updateResult.Error ?? L("Modifiers.UpdateGroupFailed", "Failed to update group.");
                    return;
                }

                targetGroupId = _editingGroup.ModifierGroupId;
            }

            ErrorMessage = string.Empty;
            IsEditingGroup = false;
            _editingGroup = null;
            RequestClose?.Invoke();
            await LoadGroupsAsync(targetGroupId, null);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task DeleteGroupAsync()
    {
        if (SelectedGroup == null)
            return;

        var confirm = MessageBox.Show(
            $"Are you sure you want to delete \"{SelectedGroup.Name}\"?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
            return;

        SetBusy(true);

        try
        {
            var result = await _managementService.DeleteGroupAsync(SelectedGroup.ModifierGroupId);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error ?? L("Modifiers.DeleteGroupFailed", "Failed to delete group.");
                return;
            }

            ErrorMessage = string.Empty;
            SelectedGroup = null;
            await LoadGroupsAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OpenAddOptionDialog()
    {
        if (SelectedGroup == null)
            return;

        ErrorMessage = string.Empty;
        _editingOption = null;
        EditOptionName = string.Empty;
        EditOptionPrice = "0.00";
        EditOptionAllowQuantity = false;
        EditOptionIsDefault = false;
        EditOptionIsActive = true;
        EditOptionSortOrder = Options.Count.ToString();
        IsEditingOption = true;

        _dialogService.ShowDialog<ModifierOptionEditDialogView>(this);
    }

    private void OpenEditOptionDialog()
    {
        if (SelectedOption == null)
            return;

        ErrorMessage = string.Empty;
        _editingOption = SelectedOption;
        EditOptionName = SelectedOption.Name;
        EditOptionPrice = SelectedOption.PriceAdd.ToString("0.00");
        EditOptionAllowQuantity = SelectedOption.AllowQuantity;
        EditOptionIsDefault = SelectedOption.IsDefault;
        EditOptionIsActive = SelectedOption.IsActive;
        EditOptionSortOrder = SelectedOption.SortOrder.ToString();
        IsEditingOption = true;

        _dialogService.ShowDialog<ModifierOptionEditDialogView>(this);
    }

    private void CancelOptionEdit()
    {
        IsEditingOption = false;
        _editingOption = null;
        ErrorMessage = string.Empty;
        RequestClose?.Invoke();
    }

    private async Task SaveOptionAsync()
    {
        if (SelectedGroup == null)
            return;

        if (!decimal.TryParse(EditOptionPrice, out decimal price))
        {
            ErrorMessage = L("Modifiers.PriceMustBeNumber", "Price must be a number.");
            return;
        }

        if (!int.TryParse(EditOptionSortOrder, out int sortOrder))
        {
            ErrorMessage = L("Modifiers.SortOrderMustBeNumber", "Sort order must be a number.");
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

        SetBusy(true);

        try
        {
            if (_editingOption == null)
            {
                var createResult = await _managementService.CreateOptionAsync(dto);
                if (!createResult.IsSuccess)
                {
                    ErrorMessage = createResult.Error ?? L("Modifiers.CreateOptionFailed", "Failed to create option.");
                    return;
                }
            }
            else
            {
                var updateResult = await _managementService.UpdateOptionAsync(dto);
                if (!updateResult.IsSuccess)
                {
                    ErrorMessage = updateResult.Error ?? L("Modifiers.UpdateOptionFailed", "Failed to update option.");
                    return;
                }
            }

            int currentGroupId = SelectedGroup.ModifierGroupId;
            ErrorMessage = string.Empty;
            IsEditingOption = false;
            _editingOption = null;
            RequestClose?.Invoke();
            await LoadGroupsAsync(currentGroupId, null);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task DeleteOptionAsync()
    {
        if (SelectedGroup == null || SelectedOption == null)
            return;

        var confirm = MessageBox.Show(
            $"Are you sure you want to delete option \"{SelectedOption.Name}\"?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
            return;

        SetBusy(true);

        try
        {
            int currentGroupId = SelectedGroup.ModifierGroupId;
            var result = await _managementService.DeleteOptionAsync(SelectedOption.ModifierOptionId);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error ?? L("Modifiers.DeleteOptionFailed", "Failed to delete option.");
                return;
            }

            ErrorMessage = string.Empty;
            SelectedOption = null;
            await LoadGroupsAsync(currentGroupId, null);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OpenGroupTranslations()
    {
        if (SelectedGroup == null || SelectedGroup.ModifierGroupId <= 0)
            return;

        var vm = new TranslationDialogViewModel(
            TranslationDialogViewModel.EntityType.ModifierGroup,
            SelectedGroup.ModifierGroupId,
            SelectedGroup.Name,
            modifierGroupTranslationService: App.ServiceProvider.GetRequiredService<IModifierGroupTranslationService>());

        ShowTranslationDialog(vm);
    }

    private void OpenOptionTranslations()
    {
        if (SelectedOption == null || SelectedOption.ModifierOptionId <= 0)
            return;

        var vm = new TranslationDialogViewModel(
            TranslationDialogViewModel.EntityType.ModifierOption,
            SelectedOption.ModifierOptionId,
            SelectedOption.Name,
            modifierOptionTranslationService: App.ServiceProvider.GetRequiredService<IModifierOptionTranslationService>());

        ShowTranslationDialog(vm);
    }

    private static void ShowTranslationDialog(TranslationDialogViewModel vm)
    {
        var dialog = new TranslationDialogView { DataContext = vm };
        var owner = Application.Current?.MainWindow;
        if (owner != null && !ReferenceEquals(owner, dialog))
            dialog.Owner = owner;
        vm.RequestClose = () => dialog.Close();
        dialog.ShowDialog();
    }

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
