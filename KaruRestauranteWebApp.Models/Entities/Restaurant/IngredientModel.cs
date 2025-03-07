namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class IngredientModel
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal StockQuantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal MinimumStock { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime? LastRestockDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual List<ItemIngredientModel> Items { get; set; } = new();
        public virtual List<InventoryTransactionModel> Transactions { get; set; } = new();
    }
}
