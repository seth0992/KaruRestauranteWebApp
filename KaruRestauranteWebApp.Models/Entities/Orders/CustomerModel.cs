using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Orders
{
    public class CustomerModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El tipo de identificación no puede exceder los 20 caracteres")]
        public string IdentificationType { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "El número de identificación no puede exceder los 30 caracteres")]
        public string IdentificationNumber { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string Address { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navegación
        public virtual List<OrderModel> Orders { get; set; } = new();
    }
}
