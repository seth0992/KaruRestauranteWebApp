using KaruRestauranteWebApp.Models.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.CashRegister
{
    public class CashRegisterSessionModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "La fecha de apertura es requerida")]
        public DateTime OpeningDate { get; set; } = DateTime.Now;

        public DateTime? ClosingDate { get; set; }

        [Required(ErrorMessage = "El usuario de apertura es requerido")]
        public int OpeningUserID { get; set; }

        public int? ClosingUserID { get; set; }

        [Required(ErrorMessage = "El monto inicial en colones es requerido")]
        public decimal InitialAmountCRC { get; set; } = 0;

        [Required(ErrorMessage = "El monto inicial en dólares es requerido")]
        public decimal InitialAmountUSD { get; set; } = 0;

        public decimal? FinalAmountCRC { get; set; }

        public decimal? FinalAmountUSD { get; set; }

        [Required(ErrorMessage = "El monto inicial en billetes de colones es requerido")]
        public decimal InitialBillsCRC { get; set; } = 0;

        [Required(ErrorMessage = "El monto inicial en monedas de colones es requerido")]
        public decimal InitialCoinsCRC { get; set; } = 0;

        [Required(ErrorMessage = "El monto inicial en billetes de dólares es requerido")]
        public decimal InitialBillsUSD { get; set; } = 0;

        [Required(ErrorMessage = "El monto inicial en monedas de dólares es requerido")]
        public decimal InitialCoinsUSD { get; set; } = 0;

        public decimal? FinalBillsCRC { get; set; }

        public decimal? FinalCoinsCRC { get; set; }

        public decimal? FinalBillsUSD { get; set; }

        public decimal? FinalCoinsUSD { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder los 20 caracteres")]
        public string Status { get; set; } = "Open"; // Open, Closed

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres")]
        public string Notes { get; set; } = string.Empty;

        // Navegación
        public virtual UserModel OpeningUser { get; set; } = null!;

        public virtual UserModel? ClosingUser { get; set; }

        public virtual List<CashRegisterTransactionModel> Transactions { get; set; } = new();
    }
}
