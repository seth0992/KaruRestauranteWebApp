using KaruRestauranteWebApp.Models.Entities.Restaurant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Orders
{
    public class OrderItemCustomizationModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El detalle de la orden es requerido")]
        public int OrderDetailID { get; set; }

        [Required(ErrorMessage = "El ingrediente es requerido")]
        public int IngredientID { get; set; }

        [Required(ErrorMessage = "El tipo de personalización es requerido")]
        [StringLength(20, ErrorMessage = "El tipo de personalización no puede exceder los 20 caracteres")]
        public string CustomizationType { get; set; } = string.Empty; // Add, Remove, Extra

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "El cargo extra es requerido")]
        public decimal ExtraCharge { get; set; } = 0;

        // Navegación
        public virtual OrderDetailModel OrderDetail { get; set; } = null!;

        public virtual IngredientModel Ingredient { get; set; } = null!;
    }
}
