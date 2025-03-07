using System.ComponentModel.DataAnnotations.Schema;

namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class ProductInventoryModel
    {
        public int ID { get; set; }
        public int FastFoodItemID { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SuggestedMarkup { get; set; }
        public DateTime? LastRestockDate { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;

        [ForeignKey("FastFoodItemID")]
        public virtual FastFoodItemModel FastFoodItem { get; set; } = null!;
    }
}
