using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.Orders
{
    public class OrderItemCustomizationDTO
    {
        public int ID { get; set; }

        public int OrderDetailID { get; set; }

        [Required(ErrorMessage = "El ingrediente es requerido")]
        public int IngredientID { get; set; }

        public string? IngredientName { get; set; }

        [Required(ErrorMessage = "El tipo de personalización es requerido")]
        public string CustomizationType { get; set; } = "Add"; // Add, Remove, Extra

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; } = 1;

        public decimal ExtraCharge { get; set; }
    }
}
