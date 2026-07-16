using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;

namespace UI.ViewModels.Modifiers
{
    /// <summary>
    /// Self-contained ViewModel for the modifier selection panel.
    /// Owns modifier loading, selection state, validation, totals,
    /// and CartItem construction. Never touches SaleItems directly.
    /// </summary>
    public class ModifierPanelViewModel : BaseViewModel
    {
        private readonly IModifierService _modifierService;
        private readonly ICartModifierService _cartModifierService;
        private readonly ILocalizationService _localization;

        private Action<CartItem?>? _onCompleted;
        private ProductDto? _selectedProduct;
        private CartItem? _existingItem;
        private readonly List<CartItemModifier> _selections = new();
        private readonly List<ModifierGroupDto> _groups = new();

        // ─── Bound Properties ──────────────────────────────

        private bool _isModifierPanelOpen;
        public bool IsModifierPanelOpen
        {
            get => _isModifierPanelOpen;
            private set
            {
                if (_isModifierPanelOpen == value) return;
                _isModifierPanelOpen = value;
                OnPropertyChanged();
            }
        }

        private ProductDto? _selectedProductForModifier;
        public ProductDto? SelectedProductForModifier
        {
            get => _selectedProductForModifier;
            private set
            {
                _selectedProductForModifier = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ModifierGroupViewModel> ModifierGroups { get; } = new();

        private bool _allRequiredGroupsSatisfied;
        public bool AllRequiredGroupsSatisfied
        {
            get => _allRequiredGroupsSatisfied;
            private set
            {
                if (_allRequiredGroupsSatisfied == value) return;
                _allRequiredGroupsSatisfied = value;
                OnPropertyChanged();
            }
        }

        private decimal _modifierRunningTotal;
        public decimal ModifierRunningTotal
        {
            get => _modifierRunningTotal;
            private set
            {
                if (_modifierRunningTotal == value) return;
                _modifierRunningTotal = value;
                OnPropertyChanged();
                UpdateConfirmLabel();
            }
        }

        private string _modifierConfirmLabel = "Add to Cart";
        public string ModifierConfirmLabel
        {
            get => _modifierConfirmLabel;
            private set
            {
                if (_modifierConfirmLabel == value) return;
                _modifierConfirmLabel = value;
                OnPropertyChanged();
            }
        }

        // ─── Commands ──────────────────────────────────────

        public ICommand ConfirmModifierCommand { get; }
        public ICommand CloseModifierPanelCommand { get; }

        // ─── Ctor ──────────────────────────────────────────

        public ModifierPanelViewModel(
            IModifierService modifierService,
            ICartModifierService cartModifierService,
            ILocalizationService localization)
        {
            _modifierService = modifierService;
            _cartModifierService = cartModifierService;
            _localization = localization;

            ConfirmModifierCommand = new RelayCommand(Confirm);
            CloseModifierPanelCommand = new RelayCommand(Close);
        }

        // ─── Public API ────────────────────────────────────

        /// <summary>
        /// Opens the panel for a product. Callback is invoked exactly once
        /// when the user confirms (CartItem) or closes (null) the panel.
        /// </summary>
        public void Open(ProductDto product, CartItem? existingItem,
                         Action<CartItem?> onCompleted)
        {
            _onCompleted = onCompleted;
            _selectedProduct = product;
            _existingItem = existingItem;
            _selections.Clear();
            _groups.Clear();
            ModifierGroups.Clear();

            SelectedProductForModifier = product;
            ModifierConfirmLabel = existingItem != null
                ? "Update Cart"
                : "Add to Cart";
            ModifierRunningTotal = 0;
            AllRequiredGroupsSatisfied = false;
            IsModifierPanelOpen = true;

            _ = LoadModifierGroupsAsync(product, existingItem);
        }

        /// <summary>
        /// Indicates whether the panel is currently in edit mode
        /// (customizing an existing cart item rather than adding new).
        /// </summary>
        public bool IsEditing => _existingItem != null;

        /// <summary>
        /// The original cart item being edited, if any.
        /// The coordinator uses this to remove the old item after confirm.
        /// </summary>
        public CartItem? EditingOriginalItem => _existingItem;

        // ─── Internal Workflow ─────────────────────────────

        private async Task LoadModifierGroupsAsync(ProductDto product, CartItem? existingItem)
        {
            try
            {
                var languageCode = _localization.CurrentLanguage.FilePrefix;

                var result = await _modifierService.GetModifierGroupsForProductAsync(
                    product.ProductId, product.CategoryId, languageCode);

                if (!result.IsSuccess || result.Value == null)
                    return;

                _groups.Clear();
                _groups.AddRange(result.Value.OrderBy(g => g.SortOrder));

                // Pre-populate existing selections if editing
                if (existingItem?.Modifiers != null)
                {
                    _selections.AddRange(existingItem.Modifiers);
                }

                BuildGroupViewModels();

                Recalculate();
            }
            catch (Exception)
            {
                // Logging could be added here
            }
        }

        private void BuildGroupViewModels()
        {
            ModifierGroups.Clear();

            foreach (var group in _groups)
            {
                ModifierGroupViewModel vm = group.GroupType switch
                {
                    1 => new SingleSelectGroupViewModel(OnOptionToggled),
                    2 => new MultiSelectGroupViewModel(OnOptionToggled),
                    3 => new QuantityGroupViewModel(OnQuantityChanged),
                    _ => null!
                };

                if (vm == null!) continue;

                vm.ModifierGroupId = group.ModifierGroupId;
                vm.Name = group.Name;
                vm.IsRequired = group.IsRequired;
                vm.SortOrder = group.SortOrder;

                if (vm is SingleSelectGroupViewModel single)
                {
                    bool hasExistingSelection = _selections.Any(
                        s => s.ModifierGroupId == group.ModifierGroupId);

                    foreach (var opt in group.Options)
                    {
                        var existingSelection = _selections.FirstOrDefault(
                            s => s.ModifierGroupId == group.ModifierGroupId
                              && s.ModifierOptionId == opt.ModifierOptionId);

                        bool isSelected = existingSelection != null;
                        single.Options.Add(new ModifierOptionViewModel(() => OnOptionToggled(opt.ModifierOptionId, group.ModifierGroupId))
                        {
                            ModifierOptionId = opt.ModifierOptionId,
                            Name = opt.Name,
                            IsSelected = isSelected,
                            HasPriceAdd = opt.PriceAdd > 0,
                            PriceAddDisplay = opt.PriceAdd > 0 ? $"+{opt.PriceAdd:N2}" : string.Empty
                        });
                    }

                    // Auto-select the default option (IsDefault = true) for single-select
                    // groups when no pre-existing selection exists.
                    if (!hasExistingSelection && group.Options.Count > 0)
                    {
                        var defaultOption = group.Options.FirstOrDefault(o => o.IsDefault);
                        if (defaultOption != null)
                        {
                            var vmOption = single.Options.FirstOrDefault(
                                o => o.ModifierOptionId == defaultOption.ModifierOptionId);
                            if (vmOption != null)
                                vmOption.IsSelected = true;

                            _cartModifierService.ApplyModifier(group, defaultOption, 1, _selections);
                        }
                    }
                }
                else if (vm is MultiSelectGroupViewModel multi)
                {
                    multi.SelectionHint = $"select up to {group.MaxSelections}";

                    foreach (var opt in group.Options)
                    {
                        var existingSelection = _selections.FirstOrDefault(
                            s => s.ModifierGroupId == group.ModifierGroupId
                              && s.ModifierOptionId == opt.ModifierOptionId);

                        multi.Options.Add(new ModifierOptionViewModel(() => OnOptionToggled(opt.ModifierOptionId, group.ModifierGroupId))
                        {
                            ModifierOptionId = opt.ModifierOptionId,
                            Name = opt.Name,
                            IsSelected = existingSelection != null,
                            HasPriceAdd = opt.PriceAdd > 0,
                            PriceAddDisplay = opt.PriceAdd > 0 ? $"+{opt.PriceAdd:N2}" : string.Empty
                        });
                    }
                }
                else if (vm is QuantityGroupViewModel qty)
                {
                    foreach (var opt in group.Options)
                    {
                        var existingSelection = _selections.FirstOrDefault(
                            s => s.ModifierGroupId == group.ModifierGroupId
                              && s.ModifierOptionId == opt.ModifierOptionId);

                        qty.Options.Add(new QuantityOptionViewModel(() => OnQuantityChanged())
                        {
                            ModifierOptionId = opt.ModifierOptionId,
                            Name = opt.Name,
                            Quantity = existingSelection?.Quantity ?? 0,
                            UnitPrice = opt.PriceAdd
                        });
                    }
                }

                ModifierGroups.Add(vm);
            }
        }

        private void OnOptionToggled(int optionId, int groupId)
        {
            var group = _groups.FirstOrDefault(g => g.ModifierGroupId == groupId);
            var option = group?.Options.FirstOrDefault(o => o.ModifierOptionId == optionId);
            if (group == null || option == null) return;

            _cartModifierService.ApplyModifier(group, option, 1, _selections);
            Recalculate();
            SyncViewModelsToSelections();
        }

        private void OnQuantityChanged()
        {
            // Rebuild selections from quantity option ViewModels
            _selections.Clear();

            foreach (var groupVm in ModifierGroups.OfType<QuantityGroupViewModel>())
            {
                var group = _groups.FirstOrDefault(g => g.ModifierGroupId == groupVm.ModifierGroupId);
                if (group == null) continue;

                foreach (var optVm in groupVm.Options)
                {
                    if (optVm.Quantity > 0)
                    {
                        var option = group.Options.FirstOrDefault(o => o.ModifierOptionId == optVm.ModifierOptionId);
                        if (option == null) continue;

                        _selections.Add(new CartItemModifier
                        {
                            ModifierGroupId = group.ModifierGroupId,
                            GroupName = group.Name,
                            ModifierOptionId = option.ModifierOptionId,
                            OptionName = option.Name,
                            Quantity = optVm.Quantity,
                            PriceAdd = option.PriceAdd,
                            GroupType = group.GroupType
                        });
                    }
                }
            }

            Recalculate();
        }

        private void SyncViewModelsToSelections()
        {
            // Sync SingleSelect / MultiSelect checked states from _selections
            foreach (var groupVm in ModifierGroups)
            {
                if (groupVm is SingleSelectGroupViewModel single)
                {
                    foreach (var opt in single.Options)
                    {
                        opt.IsSelected = _selections.Any(
                            s => s.ModifierGroupId == groupVm.ModifierGroupId
                              && s.ModifierOptionId == opt.ModifierOptionId);
                    }
                }
                else if (groupVm is MultiSelectGroupViewModel multi)
                {
                    foreach (var opt in multi.Options)
                    {
                        opt.IsSelected = _selections.Any(
                            s => s.ModifierGroupId == groupVm.ModifierGroupId
                              && s.ModifierOptionId == opt.ModifierOptionId);
                    }
                }
            }
        }

        private void Recalculate()
        {
            ModifierRunningTotal = _cartModifierService.CalculateModifierTotal(_selections);

            var errors = _cartModifierService.ValidateModifierSelections(_groups, _selections);
            AllRequiredGroupsSatisfied = errors.Count == 0;
        }

        private void UpdateConfirmLabel()
        {
            var prefix = _existingItem != null ? "Update Cart" : "Add to Cart";
            ModifierConfirmLabel = _modifierRunningTotal > 0
                ? $"{prefix} — AED {_modifierRunningTotal:N2}"
                : prefix;
        }

        // ─── Confirm / Close ───────────────────────────────

        private void Confirm()
        {
            var errors = _cartModifierService.ValidateModifierSelections(_groups, _selections);
            if (errors.Count > 0)
            {
                AllRequiredGroupsSatisfied = false;
                return;
            }

            IsModifierPanelOpen = false;

            var callback = _onCompleted;
            _onCompleted = null;
            callback?.Invoke(BuildCartItem());
        }

        private void Close()
        {
            IsModifierPanelOpen = false;

            var callback = _onCompleted;
            _onCompleted = null;
            callback?.Invoke(null);
        }

        private CartItem BuildCartItem()
        {
            var modifierTotal = _cartModifierService.CalculateModifierTotal(_selections);

            return new CartItem
            {
                VariantId = _selectedProduct!.VariantId,
                ProductName = _selectedProduct.EnglishDisplayName,
                LocalizedProductName = _selectedProduct.DisplayName,
                Quantity = _existingItem?.Quantity ?? 1,
                UnitPrice = _selectedProduct.UnitPrice + modifierTotal,
                TaxRate = _selectedProduct.TaxRate,
                Modifiers = new List<CartItemModifier>(_selections),
                ModifierSummary = _cartModifierService.BuildModifierSummary(_selections)
            };
        }
    }
}