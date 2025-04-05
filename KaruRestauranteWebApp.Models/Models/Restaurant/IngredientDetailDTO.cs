using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.Restaurant
{
    public class IngredientDetailDTO
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal StockQuantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
    }
}
