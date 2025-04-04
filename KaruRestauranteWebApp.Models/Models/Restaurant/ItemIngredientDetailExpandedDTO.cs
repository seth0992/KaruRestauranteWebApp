using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.Restaurant
{
    public class ItemIngredientDetailExpandedDTO
    {
        public int ID { get; set; }
        public int IngredientID { get; set; }
        public IngredientDetailDTO Ingredient { get; set; } = new();
        public decimal Quantity { get; set; }
        public bool IsOptional { get; set; }
        public bool CanBeExtra { get; set; }
        public decimal ExtraPrice { get; set; }
    }
}
