using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class ComboDetailModel
    {
        public int ID { get; set; }
        public int ComboID { get; set; }
        public int ItemID { get; set; }
        public int Quantity { get; set; }

        public virtual FastFoodItemModel Combo { get; set; } = null!;
        public virtual FastFoodItemModel Item { get; set; } = null!;
    }
}
