using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.CashRegister
{
    public class CashRegisterTransactionModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "La sesión de caja es requerida")]
        public int SessionID { get; set; }

        [Required(ErrorMessage = "El usuario es requerido")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "La fecha de transacción es requerida")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El tipo de transacción es requerido")]
        [StringLength(20, ErrorMessage = "El tipo de transacción no puede exceder los 20 caracteres")]
        public string TransactionType { get; set; } = string.Empty; // Income, Expense

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto en colones es requerido")]
        public decimal AmountCRC { get; set; } = 0;

        [Required(ErrorMessage = "El monto en dólares es requerido")]
        public decimal AmountUSD { get; set; } = 0;

        [Required(ErrorMessage = "El método de pago es requerido")]
        [StringLength(20, ErrorMessage = "El método de pago no puede exceder los 20 caracteres")]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Transfer

        [StringLength(50, ErrorMessage = "El número de referencia no puede exceder los 50 caracteres")]
        public string ReferenceNumber { get; set; } = string.Empty;

        public int? RelatedOrderID { get; set; }

        // Navegación
        public virtual CashRegisterSessionModel Session { get; set; } = null!;

        public virtual UserModel User { get; set; } = null!;

        public virtual OrderModel? RelatedOrder { get; set; }
    }
}
