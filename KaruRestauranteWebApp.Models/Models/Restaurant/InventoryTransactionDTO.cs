using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.Restaurant
{
    public class InventoryTransactionDTO
    {
        public int IngredientID { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
