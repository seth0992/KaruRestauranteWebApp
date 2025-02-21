using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class IngredientModel
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal StockQuantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal MinimumStock { get; set; }
        public decimal Cost { get; set; }
        public DateTime? LastRestockDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
