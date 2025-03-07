using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class FastFoodItemModel
    {
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public int CategoryID { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal EstimatedCost { get; set; }
        public int ProductTypeID { get; set; } // Usamos solo el ID, sin navegación
        public bool IsAvailable { get; set; } = true;
        public string ImageUrl { get; set; } = string.Empty;
        public int? EstimatedPreparationTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual CategoryModel Category { get; set; } = null!;
        public virtual List<ItemIngredientModel> Ingredients { get; set; } = new();
        public virtual ProductInventoryModel? Inventory { get; set; }
    }
}
