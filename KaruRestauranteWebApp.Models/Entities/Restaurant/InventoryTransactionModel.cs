using KaruRestauranteWebApp.Models.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class InventoryTransactionModel
    {
        public int ID { get; set; }
        public int IngredientID { get; set; }
        public int UserID { get; set; }
        public string TransactionType { get; set; } = string.Empty; // Purchase, Consumption, Adjustment, Loss
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public virtual IngredientModel Ingredient { get; set; } = null!;
        public virtual UserModel User { get; set; } = null!;
    }
}
