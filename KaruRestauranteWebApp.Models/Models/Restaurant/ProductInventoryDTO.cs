using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.Restaurant
{
    public class ProductInventoryDTO
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El producto es requerido")]
        public int FastFoodItemID { get; set; }

        [Required(ErrorMessage = "El stock actual es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int CurrentStock { get; set; }

        [Required(ErrorMessage = "El stock mínimo es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int MinimumStock { get; set; }

        [Required(ErrorMessage = "El precio de compra es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PurchasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El margen sugerido no puede ser negativo")]
        public decimal SuggestedMarkup { get; set; }

        public DateTime? LastRestockDate { get; set; }

        [StringLength(50, ErrorMessage = "El SKU no puede exceder los 50 caracteres")]
        public string SKU { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "La unidad de medida no puede exceder los 20 caracteres")]
        public string UnitOfMeasure { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El código de ubicación no puede exceder los 50 caracteres")]
        public string LocationCode { get; set; } = string.Empty;

        public string? ProductName { get; set; }
    }

    public class StockMovementDTO
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductInventoryID { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es requerido")]
        public string MovementType { get; set; } = string.Empty; // Entrada, Salida, Ajuste

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; }

        public string? Notes { get; set; }
    }
}
