using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.Orders
{
    public class PaymentDTO
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "La orden es requerida")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "El método de pago es requerido")]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, CreditCard, DebitCard, Transfer, Other

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        public string ReferenceNumber { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}
