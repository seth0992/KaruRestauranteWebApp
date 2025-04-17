using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.Reports
{
    public class InventoryStatusReportDTO
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty; // "Product" o "Ingredient"
        public decimal CurrentStock { get; set; }
        public decimal MinimumStock { get; set; }
        public string Status { get; set; } = string.Empty; // "Normal", "Low", "Out"
        public DateTime? LastRestockDate { get; set; }
    }
}
