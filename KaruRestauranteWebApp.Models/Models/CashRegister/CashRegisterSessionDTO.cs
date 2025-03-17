using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.CashRegister
{
    public class CashRegisterSessionDTO
    {
        public int ID { get; set; }

        public DateTime OpeningDate { get; set; } = DateTime.Now;

        public DateTime? ClosingDate { get; set; }

        public int OpeningUserID { get; set; }

        public string? OpeningUserName { get; set; }

        public int? ClosingUserID { get; set; }

        public string? ClosingUserName { get; set; }

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

        public string Status { get; set; } = "Open";

        public string Notes { get; set; } = string.Empty;

        public decimal CurrentBalanceCRC { get; set; } = 0;

        public decimal CurrentBalanceUSD { get; set; } = 0;
    }
}
