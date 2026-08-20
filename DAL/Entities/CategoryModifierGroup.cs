namespace DAL.Entities
{
    public class CategoryModifierGroup
    {
        public int CategoryId { get; set; }

        public int ModifierGroupId { get; set; }

        public Category? Category { get; set; }

        public ModifierGroup? ModifierGroup { get; set; }
    }
}