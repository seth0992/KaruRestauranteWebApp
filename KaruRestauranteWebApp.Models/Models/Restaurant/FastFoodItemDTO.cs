using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Models.Restaurant
{
    public class FastFoodItemDTO
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "El precio de venta es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal SellingPrice { get; set; }

        [Required(ErrorMessage = "El costo estimado es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor a 0")]
        public decimal EstimatedCost { get; set; }

        [Required(ErrorMessage = "El tipo de producto es requerido")]
        public int ProductTypeID { get; set; }

        public bool IsAvailable { get; set; } = true;
        public string ImageUrl { get; set; } = string.Empty;
        public int? EstimatedPreparationTime { get; set; }

        public List<ItemIngredientDetailDTO> Ingredients { get; set; } = new();
    }

    public class ItemIngredientDetailDTO
    {
        public int IngredientID { get; set; }
        public decimal Quantity { get; set; }
        public bool IsOptional { get; set; }
        public bool CanBeExtra { get; set; }
        public decimal ExtraPrice { get; set; }
    }
}
