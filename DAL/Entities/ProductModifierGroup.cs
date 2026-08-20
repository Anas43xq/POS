namespace DAL.Entities
{
    public class ProductModifierGroup
    {
        public int ProductId { get; set; }

        public int ModifierGroupId { get; set; }

        public Product? Product { get; set; }

        public ModifierGroup? ModifierGroup { get; set; }
    }
}