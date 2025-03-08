namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class ItemIngredientModel
    {
        public int ID { get; set; }
        public int FastFoodItemID { get; set; }
        public int IngredientID { get; set; }
        public decimal Quantity { get; set; }
        public bool IsOptional { get; set; }
        public bool CanBeExtra { get; set; }
        public decimal ExtraPrice { get; set; }

        public virtual FastFoodItemModel FastFoodItem { get; set; } = null!;
        public virtual IngredientModel Ingredient { get; set; } = null!;
    }
}
