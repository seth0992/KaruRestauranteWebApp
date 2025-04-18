using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.Reports
{
    public class SalesReportDTO
    {
        public string MonthName { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal TotalSales { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageTicket { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
