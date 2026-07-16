using System;
using System.Collections.ObjectModel;

namespace UI.ViewModels.Modifiers
{
    public class SingleSelectGroupViewModel : ModifierGroupViewModel
    {
        public ObservableCollection<ModifierOptionViewModel> Options { get; } = new();

        public SingleSelectGroupViewModel(Action<int, int> onOptionToggled)
        {
            GroupType = 1;
        }
    }
}