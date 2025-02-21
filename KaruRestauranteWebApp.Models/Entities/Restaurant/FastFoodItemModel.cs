using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class FastFoodItemModel
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public decimal Price { get; set; }
        public bool IsCombo { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string ImageUrl { get; set; } = string.Empty;
        public int EstimatedPreparationTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual CategoryModel Category { get; set; } = null!;
        public virtual List<ItemIngredientModel> Ingredients { get; set; } = new();
        public virtual List<ComboDetailModel> ComboItems { get; set; } = new();
    }
}
