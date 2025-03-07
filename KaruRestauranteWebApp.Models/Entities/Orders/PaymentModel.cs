using KaruRestauranteWebApp.Models.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Entities.Orders
{
    public class PaymentModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "La orden es requerida")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "El método de pago es requerido")]
        [StringLength(50, ErrorMessage = "El método de pago no puede exceder los 50 caracteres")]
        public string PaymentMethod { get; set; } = string.Empty; // Cash, CreditCard, DebitCard, Transfer, Other

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        [StringLength(50, ErrorMessage = "El número de referencia no puede exceder los 50 caracteres")]
        public string ReferenceNumber { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El usuario que procesa es requerido")]
        public int ProcessedBy { get; set; }

        [StringLength(200, ErrorMessage = "Las notas no pueden exceder los 200 caracteres")]
        public string Notes { get; set; } = string.Empty;

        // Navegación
        public virtual OrderModel Order { get; set; } = null!;

        public virtual UserModel ProcessedByUser { get; set; } = null!;
    }
}
