using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.Restaurant
{
    public class FastFoodItemDetailDTO
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public decimal EstimatedCost { get; set; }
        public int ProductTypeID { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string ImageUrl { get; set; } = string.Empty;
        public int? EstimatedPreparationTime { get; set; }
        public List<ItemIngredientDetailExpandedDTO> Ingredients { get; set; } = new();
    }
}
