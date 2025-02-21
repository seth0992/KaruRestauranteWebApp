using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class CategoryModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string Description { get; set; } = string.Empty;

        // Nos ayuda a mantener categorías que no queremos eliminar pero tampoco mostrar
        public bool IsActive { get; set; } = true;

        // Campos para auditoría
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Propiedad de navegación para los productos asociados
        public virtual ICollection<FastFoodItemModel> Items { get; set; } = new List<FastFoodItemModel>();
    }
}
