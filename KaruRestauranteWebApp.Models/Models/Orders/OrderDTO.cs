using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Models.Orders
{
    public class OrderDTO
    {
        public int ID { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public int? CustomerID { get; set; }

        public string? CustomerName { get; set; }

        public int? TableID { get; set; }

        public int? TableNumber { get; set; }

        [Required(ErrorMessage = "El tipo de orden es requerido")]
        public string OrderType { get; set; } = "DineIn"; // DineIn, TakeOut, Delivery

        public string OrderStatus { get; set; } = "Pending";

        public string PaymentStatus { get; set; } = "Pending";

        public decimal TotalAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public string Notes { get; set; } = string.Empty;

        public List<OrderDetailDTO> OrderDetails { get; set; } = new();
    }
}
