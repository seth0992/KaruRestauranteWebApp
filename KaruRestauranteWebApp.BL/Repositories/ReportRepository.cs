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
        Task<List<ProductSalesReportDTO>> GetTopSellingProductsAsync(
    DateTime startDate,
    DateTime endDate,
    int limit = 10,
    int? categoryId = null);
    }
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<ProductSalesReportDTO>> GetTopSellingProductsAsync(
    DateTime startDate,
    DateTime endDate,
    int limit = 10,
    int? categoryId = null)
        {
            try
            {
                // Calcular el total de ventas para el período
                var query = _context.OrderDetails
                    .Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate
                              && od.Order.OrderStatus != "Cancelled");

                // Aplicar filtro por categoría si existe
                if (categoryId.HasValue)
                {
                    query = query.Where(od =>
                        (od.ItemType == "Product" &&
                        _context.FastFoodItems.Any(p => p.ID == od.ItemID && p.CategoryID == categoryId.Value)) ||
                        (od.ItemType == "Combo" &&
                        _context.ComboItems.Any(ci => ci.ComboID == od.ItemID &&
                            _context.FastFoodItems.Any(p => p.ID == ci.FastFoodItemID && p.CategoryID == categoryId.Value))));
                }

                var totalSales = await query.SumAsync(od => od.SubTotal);

                // Obtenemos los datos de productos vendidos con el filtro aplicado
                var orderDetailsData = await query
                    .Select(od => new
                    {
                        od.ItemID,
                        od.ItemType,
                        od.Quantity,
                        od.SubTotal
                    })
                    .ToListAsync();

                // Agrupamos y calculamos en memoria
                var topProducts = orderDetailsData
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
                    .ToList();

                // Obtenemos los IDs de productos y combos que necesitamos
                var productIds = topProducts.Where(p => p.ItemType == "Product").Select(p => p.ItemID).ToList();
                var comboIds = topProducts.Where(p => p.ItemType == "Combo").Select(p => p.ItemID).ToList();

                // Obtenemos los datos de productos
                var products = await _context.FastFoodItems
                    .Where(p => productIds.Contains(p.ID))
                    .Select(p => new
                    {
                        p.ID,
                        p.Name,
                        CategoryName = p.Category.Name
                    })
                    .ToDictionaryAsync(p => p.ID, p => new { p.Name, p.CategoryName });

                // Obtenemos los datos de combos
                var combos = await _context.Combos
                    .Where(c => comboIds.Contains(c.ID))
                    .Select(c => new
                    {
                        c.ID,
                        c.Name
                    })
                    .ToDictionaryAsync(c => c.ID, c => c.Name);

                // Componemos el resultado final
                var result = new List<ProductSalesReportDTO>();
                foreach (var item in topProducts)
                {
                    string productName = "";
                    string categoryName = "";

                    if (item.ItemType == "Product" && products.TryGetValue(item.ItemID, out var product))
                    {
                        productName = product.Name;
                        categoryName = product.CategoryName;
                    }
                    else if (item.ItemType == "Combo" && combos.TryGetValue(item.ItemID, out var comboName))
                    {
                        productName = comboName;
                        categoryName = "Combo";
                    }

                    result.Add(new ProductSalesReportDTO
                    {
                        ProductID = item.ItemID,
                        ProductName = productName,
                        CategoryName = categoryName,
                        QuantitySold = item.QuantitySold,
                        TotalSales = item.TotalSales,
                        Percentage = totalSales > 0 ? (item.TotalSales / totalSales) * 100 : 0
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
               
                throw;
            }
        }
        public async Task<List<SalesReportDTO>> GetDailySalesAsync(DateTime startDate, DateTime endDate)
        {
            // Primero obtenemos los datos básicos
            var ordersData = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus != "Cancelled")
                .Select(o => new
                {
                    Date = o.CreatedAt.Date,
                    o.TotalAmount,
                    o.TaxAmount
                })
                .ToListAsync();

            // Luego agrupamos y calculamos en memoria
            var result = ordersData
                .GroupBy(o => o.Date)
                .Select(g => new SalesReportDTO
                {
                    Date = g.Key,
                    TotalSales = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    AverageTicket = g.Sum(o => o.TotalAmount) / (decimal)g.Count(),
                    TaxAmount = g.Sum(o => o.TaxAmount)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        public async Task<List<SalesReportDTO>> GetMonthlySalesAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            // Primero obtenemos los datos básicos
            var ordersData = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus != "Cancelled")
                .Select(o => new
                {
                    Year = o.CreatedAt.Year,
                    Month = o.CreatedAt.Month,
                    o.TotalAmount,
                    o.TaxAmount
                })
                .ToListAsync();

            // Luego agrupamos y calculamos en memoria
            var result = ordersData
                .GroupBy(o => new { o.Year, o.Month })
                .Select(g => new SalesReportDTO
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalSales = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    AverageTicket = g.Sum(o => o.TotalAmount) / (decimal)g.Count(),
                    TaxAmount = g.Sum(o => o.TaxAmount)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        public async Task<List<SalesReportDTO>> GetYearlySalesAsync(int startYear, int endYear)
        {
            var startDate = new DateTime(startYear, 1, 1);
            var endDate = new DateTime(endYear, 12, 31);

            // Primero obtenemos los datos básicos
            var ordersData = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus != "Cancelled")
                .Select(o => new
                {
                    Year = o.CreatedAt.Year,
                    o.TotalAmount,
                    o.TaxAmount
                })
                .ToListAsync();

            // Luego agrupamos y calculamos en memoria
            var result = ordersData
                .GroupBy(o => o.Year)
                .Select(g => new SalesReportDTO
                {
                    Date = new DateTime(g.Key, 1, 1),
                    TotalSales = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    AverageTicket = g.Sum(o => o.TotalAmount) / (decimal)g.Count(),
                    TaxAmount = g.Sum(o => o.TaxAmount)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        public async Task<List<ProductSalesReportDTO>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 10)
        {
            // Calculamos el total de ventas para el período
            var totalSales = await _context.OrderDetails
                .Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate
                          && od.Order.OrderStatus != "Cancelled")
                .SumAsync(od => od.SubTotal);

            // Obtenemos los datos de productos vendidos
            var orderDetailsData = await _context.OrderDetails
                .Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate
                          && od.Order.OrderStatus != "Cancelled")
                .Select(od => new
                {
                    od.ItemID,
                    od.ItemType,
                    od.Quantity,
                    od.SubTotal
                })
                .ToListAsync();

            // Agrupamos y calculamos en memoria
            var topProducts = orderDetailsData
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
                .ToList();

            // Obtenemos los IDs de productos y combos que necesitamos
            var productIds = topProducts.Where(p => p.ItemType == "Product").Select(p => p.ItemID).ToList();
            var comboIds = topProducts.Where(p => p.ItemType == "Combo").Select(p => p.ItemID).ToList();

            // Obtenemos los datos de productos
            var products = await _context.FastFoodItems
                .Where(p => productIds.Contains(p.ID))
                .Select(p => new
                {
                    p.ID,
                    p.Name,
                    CategoryName = p.Category.Name
                })
                .ToDictionaryAsync(p => p.ID, p => new { p.Name, p.CategoryName });

            // Obtenemos los datos de combos
            var combos = await _context.Combos
                .Where(c => comboIds.Contains(c.ID))
                .Select(c => new
                {
                    c.ID,
                    c.Name
                })
                .ToDictionaryAsync(c => c.ID, c => c.Name);

            // Componemos el resultado final
            var result = new List<ProductSalesReportDTO>();
            foreach (var item in topProducts)
            {
                string productName = "";
                string categoryName = "";

                if (item.ItemType == "Product" && products.TryGetValue(item.ItemID, out var product))
                {
                    productName = product.Name;
                    categoryName = product.CategoryName;
                }
                else if (item.ItemType == "Combo" && combos.TryGetValue(item.ItemID, out var comboName))
                {
                    productName = comboName;
                    categoryName = "Combo";
                }

                result.Add(new ProductSalesReportDTO
                {
                    ProductID = item.ItemID,
                    ProductName = productName,
                    CategoryName = categoryName,
                    QuantitySold = item.QuantitySold,
                    TotalSales = item.TotalSales,
                    Percentage = totalSales > 0 ? (item.TotalSales / totalSales) * 100 : 0
                });
            }

            return result;
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

            // Ordenar en memoria
            return result
                .OrderBy(r => r.Status == "Out" ? 0 : r.Status == "Low" ? 1 : 2)
                .ThenBy(r => r.ItemName)
                .ToList();
        }
    }
}
