namespace UI.ViewModels.Modifiers
{
    public abstract class ModifierGroupViewModel : BaseViewModel
    {
        public int ModifierGroupId { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        /// <summary>
        /// 1 = SingleSelect, 2 = MultiSelect, 3 = Quantity
        /// </summary>
        public int GroupType { get; set; }

        public int SortOrder { get; set; }
    }
}