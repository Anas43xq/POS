namespace UI.ViewModels;

/// <summary>
/// Row ViewModel for a modifier group in the management list.
/// </summary>
public class ModifierGroupListItemViewModel : BaseViewModel
{
    private bool _isSelected;

    public int ModifierGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int GroupType { get; set; }
    public string GroupTypeDisplay { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    public int OptionCount { get; set; }
    public int SortOrder { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }
}