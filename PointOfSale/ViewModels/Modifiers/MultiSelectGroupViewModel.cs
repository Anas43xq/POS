using System;
using System.Collections.ObjectModel;

namespace UI.ViewModels.Modifiers
{
    public class MultiSelectGroupViewModel : ModifierGroupViewModel
    {
        public ObservableCollection<ModifierOptionViewModel> Options { get; } = new();

        public string SelectionHint { get; set; } = string.Empty;

        public MultiSelectGroupViewModel(Action<int, int> onOptionToggled)
        {
            GroupType = 2;
        }
    }
}