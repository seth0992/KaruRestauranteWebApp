using KaruRestauranteWebApp.Models.Entities.Restaurant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.Restaurant
{
    public class ComboItemViewModel
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public FastFoodItemModel? Item { get; set; }
        public decimal Subtotal => Item?.Price * Quantity ?? 0;
    }
}
