namespace UI.ViewModels;

/// <summary>
/// Row ViewModel for a modifier option in the management list.
/// </summary>
public class ModifierOptionListItemViewModel : BaseViewModel
{
    private bool _isSelected;

    public int ModifierOptionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceAdd { get; set; }
    public bool AllowQuantity { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }
}