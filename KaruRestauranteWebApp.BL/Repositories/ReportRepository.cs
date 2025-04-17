using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IReportRepository
    {
        Task<List<SalesReportDTO>> GetDailySalesAsync(DateTime startDate, DateTime endDate);
        Task<List<SalesReportDTO>> GetMonthlySalesAsync(int year);
        Task<List<SalesReportDTO>> GetYearlySalesAsync(int startYear, int endYear);
        Task<List<ProductSalesReportDTO>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 10);
        Task<List<InventoryStatusReportDTO>> GetInventoryStatusReportAsync();
    }
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SalesReportDTO>> GetDailySalesAsync(DateTime startDate, DateTime endDate)
        {
            var result = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus != "Cancelled")
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new SalesReportDTO
                {
                    Date = g.Key,
                    TotalSales = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    AverageTicket = g.Sum(o => o.TotalAmount) / g.Count(),
                    TaxAmount = g.Sum(o => o.TaxAmount)
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            return result;
        }

        public async Task<List<SalesReportDTO>> GetMonthlySalesAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            var result = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus != "Cancelled")
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new SalesReportDTO
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalSales = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    AverageTicket = g.Sum(o => o.TotalAmount) / g.Count(),
                    TaxAmount = g.Sum(o => o.TaxAmount)
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            return result;
        }

        public async Task<List<SalesReportDTO>> GetYearlySalesAsync(int startYear, int endYear)
        {
            var startDate = new DateTime(startYear, 1, 1);
            var endDate = new DateTime(endYear, 12, 31);

            var result = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus != "Cancelled")
                .GroupBy(o => o.CreatedAt.Year)
                .Select(g => new SalesReportDTO
                {
                    Date = new DateTime(g.Key, 1, 1),
                    TotalSales = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    AverageTicket = g.Sum(o => o.TotalAmount) / g.Count(),
                    TaxAmount = g.Sum(o => o.TaxAmount)
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            return result;
        }

        public async Task<List<ProductSalesReportDTO>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 10)
        {
            var totalSales = await _context.OrderDetails
                .Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate
                         && od.Order.OrderStatus != "Cancelled")
                .SumAsync(od => od.SubTotal);

            var result = await _context.OrderDetails
                .Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate
                         && od.Order.OrderStatus != "Cancelled")
                .GroupBy(od => new { od.ItemID, od.ItemType })
                .Select(g => new
                {
                    ItemID = g.Key.ItemID,
                    ItemType = g.Key.ItemType,
                    QuantitySold = g.Sum(od => od.Quantity),
                    TotalSales = g.Sum(od => od.SubTotal)
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(limit)
                .ToListAsync();

            var products = new List<ProductSalesReportDTO>();

            foreach (var item in result)
            {
                string productName = "";
                string categoryName = "";

                if (item.ItemType == "Product")
                {
                    var product = await _context.FastFoodItems
                        .Include(f => f.Category)
                        .FirstOrDefaultAsync(f => f.ID == item.ItemID);

                    if (product != null)
                    {
                        productName = product.Name;
                        categoryName = product.Category?.Name ?? "Sin categoría";
                    }
                }
                else if (item.ItemType == "Combo")
                {
                    var combo = await _context.Combos
                        .FirstOrDefaultAsync(c => c.ID == item.ItemID);

                    if (combo != null)
                    {
                        productName = combo.Name;
                        categoryName = "Combo";
                    }
                }

                products.Add(new ProductSalesReportDTO
                {
                    ProductID = item.ItemID,
                    ProductName = productName,
                    CategoryName = categoryName,
                    QuantitySold = item.QuantitySold,
                    TotalSales = item.TotalSales,
                    Percentage = totalSales > 0 ? (item.TotalSales / totalSales) * 100 : 0
                });
            }

            return products;
        }

        public async Task<List<InventoryStatusReportDTO>> GetInventoryStatusReportAsync()
        {
            var result = new List<InventoryStatusReportDTO>();

            // Obtener ingredientes
            var ingredients = await _context.Ingredients
                .Where(i => i.IsActive)
                .Select(i => new InventoryStatusReportDTO
                {
                    ItemID = i.ID,
                    ItemName = i.Name,
                    ItemType = "Ingredient",
                    CurrentStock = i.StockQuantity,
                    MinimumStock = i.MinimumStock,
                    Status = i.StockQuantity <= 0 ? "Out" :
                             i.StockQuantity <= i.MinimumStock ? "Low" : "Normal",
                    LastRestockDate = i.LastRestockDate
                })
                .ToListAsync();

            // Obtener productos de inventario
            var products = await _context.ProductInventory
                .Include(p => p.FastFoodItem)
                .Where(p => p.FastFoodItem.IsAvailable)
                .Select(p => new InventoryStatusReportDTO
                {
                    ItemID = p.FastFoodItemID,
                    ItemName = p.FastFoodItem.Name,
                    ItemType = "Product",
                    CurrentStock = p.CurrentStock,
                    MinimumStock = p.MinimumStock,
                    Status = p.CurrentStock <= 0 ? "Out" :
                             p.CurrentStock <= p.MinimumStock ? "Low" : "Normal",
                    LastRestockDate = p.LastRestockDate
                })
                .ToListAsync();

            result.AddRange(ingredients);
            result.AddRange(products);

            return result.OrderBy(r => r.Status == "Out" ? 0 : r.Status == "Low" ? 1 : 2)
                         .ThenBy(r => r.ItemName)
                         .ToList();
        }
    }
}
