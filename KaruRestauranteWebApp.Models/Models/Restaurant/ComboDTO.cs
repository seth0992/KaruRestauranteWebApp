using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Models.Restaurant
{
    public class ComboDTO
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio regular es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal RegularPrice { get; set; }

        [Required(ErrorMessage = "El precio de venta es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal SellingPrice { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public decimal DiscountPercentage { get; set; }

        public bool IsAvailable { get; set; } = true;

        public string ImageUrl { get; set; } = string.Empty;

        public List<ComboItemDetailDTO> Items { get; set; } = new();
    }

    public class ComboItemDetailDTO
    {
        public int FastFoodItemID { get; set; }
        public int Quantity { get; set; } = 1;
        public bool AllowCustomization { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}
