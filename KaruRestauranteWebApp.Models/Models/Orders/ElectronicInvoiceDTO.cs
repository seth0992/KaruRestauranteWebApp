using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.Orders
{
    public class ElectronicInvoiceDTO
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "La orden es requerida")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "El cliente es requerido")]
        public int CustomerID { get; set; }

        public string? CustomerName { get; set; }

        public string? CustomerIdentification { get; set; }

        public string InvoiceStatus { get; set; } = "Generated";

        public DateTime CreationDate { get; set; }

        public string? HaciendaConfirmationNumber { get; set; }
    }
}
