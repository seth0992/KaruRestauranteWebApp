using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class ProductInventoryModel
    {
        public int ID { get; set; }

        [Required]
        public int FastFoodItemID { get; set; }

        [Required]
        public int CurrentStock { get; set; }

        [Required]
        public int MinimumStock { get; set; }

        public DateTime? LastRestockDate { get; set; }

        // Propiedad de navegación
        [ForeignKey("FastFoodItemID")]
        public virtual FastFoodItemModel FastFoodItem { get; set; } = null!;
    }
}
