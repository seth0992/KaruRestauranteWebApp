using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Entities.Orders
{
    public class ElectronicInvoiceModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "La orden es requerida")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "El número de factura es requerido")]
        [StringLength(50, ErrorMessage = "El número de factura no puede exceder los 50 caracteres")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cliente es requerido")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "El monto total es requerido")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "El monto de impuestos es requerido")]
        public decimal TaxAmount { get; set; }

        public string InvoiceXML { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado de la factura es requerido")]
        [StringLength(20, ErrorMessage = "El estado de la factura no puede exceder los 20 caracteres")]
        public string InvoiceStatus { get; set; } = "Generated"; // Generated, Sent, Accepted, Rejected

        public DateTime CreationDate { get; set; } = DateTime.Now;

        public DateTime? ResponseDate { get; set; }

        [StringLength(500, ErrorMessage = "La descripción de error no puede exceder los 500 caracteres")]
        public string ErrorDescription { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "El número de confirmación no puede exceder los 100 caracteres")]
        public string HaciendaConfirmationNumber { get; set; } = string.Empty;

        // Navegación
        public virtual OrderModel Order { get; set; } = null!;

        public virtual CustomerModel Customer { get; set; } = null!;
    }
}
