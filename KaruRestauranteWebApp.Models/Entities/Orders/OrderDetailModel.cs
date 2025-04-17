using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Entities.Orders
{
    public class OrderDetailModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "La orden es requerida")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "El tipo de item es requerido")]
        [StringLength(20, ErrorMessage = "El tipo de item no puede exceder los 20 caracteres")]
        public string ItemType { get; set; } = string.Empty; // Product, Combo

        [Required(ErrorMessage = "El item es requerido")]
        public int ItemID { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "El precio unitario es requerido")]
        public decimal UnitPrice { get; set; } = 0;

        [Required(ErrorMessage = "El subtotal es requerido")]
        public decimal SubTotal { get; set; } = 0;

        [StringLength(200, ErrorMessage = "Las notas no pueden exceder los 200 caracteres")]
        public string Notes { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder los 20 caracteres")]
        public string Status { get; set; } = "Pending"; // Pending, InPreparation, Ready, Delivered, Cancelled

        [Required(ErrorMessage = "El porcentaje de descuento es requerido")]
        public decimal DiscountPercentage { get; set; } = 0;

        public decimal DiscountAmount { get; set; } = 0;

        // Navegación
        public virtual OrderModel Order { get; set; } = null!;

        public virtual List<OrderItemCustomizationModel> Customizations { get; set; } = new();
    }
}
