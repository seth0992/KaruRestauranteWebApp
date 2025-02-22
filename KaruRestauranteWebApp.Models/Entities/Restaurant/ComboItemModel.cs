using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class ComboItemModel
    {
        public int ID { get; set; }
        public int ComboID { get; set; }
        public int FastFoodItemID { get; set; }
        public int Quantity { get; set; } = 1;
        public bool AllowCustomization { get; set; }
        public string? SpecialInstructions { get; set; }

        public virtual ComboModel Combo { get; set; } = null!;
        public virtual FastFoodItemModel FastFoodItem { get; set; } = null!;
    }
}
