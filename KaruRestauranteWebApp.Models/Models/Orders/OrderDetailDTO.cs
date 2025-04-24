using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Models.Orders
{
    public class OrderDetailDTO
    {
        public int ID { get; set; }

        public int OrderID { get; set; }

        [Required(ErrorMessage = "El tipo de item es requerido")]
        public string ItemType { get; set; } = "Product"; // Product, Combo

        [Required(ErrorMessage = "El item es requerido")]
        public int ItemID { get; set; }

        public string? ItemName { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }

        public decimal SubTotal { get; set; }

        public string Notes { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";
        public decimal DiscountPercentage { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;

        public List<OrderItemCustomizationDTO> Customizations { get; set; } = new();
        public List<ComboItemDetail> ComboItems { get; set; } = new();
    }


}
