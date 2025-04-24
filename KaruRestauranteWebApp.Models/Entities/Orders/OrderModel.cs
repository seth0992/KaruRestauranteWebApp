using KaruRestauranteWebApp.Models.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Entities.Orders
{
    public class OrderModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El número de orden es requerido")]
        [StringLength(20, ErrorMessage = "El número de orden no puede exceder los 20 caracteres")]
        public string OrderNumber { get; set; } = string.Empty;

        public int? CustomerID { get; set; }

        public int? TableID { get; set; }

        [Required(ErrorMessage = "El usuario es requerido")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "El tipo de orden es requerido")]
        [StringLength(20, ErrorMessage = "El tipo de orden no puede exceder los 20 caracteres")]
        public string OrderType { get; set; } = string.Empty; // DineIn, TakeOut, Delivery

        [Required(ErrorMessage = "El estado de la orden es requerido")]
        [StringLength(20, ErrorMessage = "El estado de la orden no puede exceder los 20 caracteres")]
        public string OrderStatus { get; set; } = "Pending"; // Pending, InProgress, Ready, Delivered, Cancelled

        [Required(ErrorMessage = "El estado de pago es requerido")]
        [StringLength(20, ErrorMessage = "El estado de pago no puede exceder los 20 caracteres")]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Partially Paid, Cancelled

        [Required(ErrorMessage = "El monto total es requerido")]
        public decimal TotalAmount { get; set; } = 0;

        [Required(ErrorMessage = "El monto de impuestos es requerido")]
        public decimal TaxAmount { get; set; } = 0;

        [Required(ErrorMessage = "El monto de descuento es requerido")]
        public decimal DiscountAmount { get; set; } = 0;

        public decimal DiscountPercentage { get; set; } = 0;

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres")]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navegación
        public virtual CustomerModel? Customer { get; set; }

        public virtual TableModel? Table { get; set; }

        public virtual UserModel User { get; set; } = null!;

        public virtual List<OrderDetailModel> OrderDetails { get; set; } = new();

        public virtual List<PaymentModel> Payments { get; set; } = new();

        public virtual ElectronicInvoiceModel? ElectronicInvoice { get; set; }
    }
}
