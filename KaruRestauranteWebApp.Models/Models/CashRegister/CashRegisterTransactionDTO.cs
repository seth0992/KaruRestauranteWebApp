using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.CashRegister
{
    public class CashRegisterTransactionDTO
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "La sesión de caja es requerida")]
        public int SessionID { get; set; }

        public int UserID { get; set; }

        public string? UserName { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El tipo de transacción es requerido")]
        public string TransactionType { get; set; } = "Income"; // Income, Expense

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto en colones es requerido")]
        public decimal AmountCRC { get; set; } = 0;

        [Required(ErrorMessage = "El monto en dólares es requerido")]
        public decimal AmountUSD { get; set; } = 0;

        [Required(ErrorMessage = "El método de pago es requerido")]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Transfer

        public string ReferenceNumber { get; set; } = string.Empty;

        public int? RelatedOrderID { get; set; }

        public string? RelatedOrderNumber { get; set; }
    }
}
