using System;
using System.Collections.ObjectModel;

namespace UI.ViewModels.Modifiers
{
    public class QuantityGroupViewModel : ModifierGroupViewModel
    {
        public ObservableCollection<QuantityOptionViewModel> Options { get; } = new();

        public QuantityGroupViewModel(Action onChanged)
        {
            GroupType = 3;
        }
    }
}