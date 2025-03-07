using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IProductInventoryRepository
    {
        Task<List<ProductInventoryModel>> GetAllAsync();
        Task<ProductInventoryModel?> GetByIdAsync(int id);
        Task<ProductInventoryModel?> GetByProductIdAsync(int productId);
        Task<ProductInventoryModel> CreateAsync(ProductInventoryModel productInventory);
        Task UpdateAsync(ProductInventoryModel productInventory);
        Task<bool> DeleteAsync(int id);
        Task<List<ProductInventoryModel>> GetLowStockAsync();
    }

    public class ProductInventoryRepository : IProductInventoryRepository
    {
        private readonly AppDbContext _context;

        public ProductInventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductInventoryModel>> GetAllAsync()
        {
            return await _context.ProductInventory
                .Include(p => p.FastFoodItem)
                .OrderBy(p => p.FastFoodItem.Name)
                .ToListAsync();
        }

        public async Task<ProductInventoryModel?> GetByIdAsync(int id)
        {
            return await _context.ProductInventory
                .Include(p => p.FastFoodItem)
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<ProductInventoryModel?> GetByProductIdAsync(int productId)
        {
            return await _context.ProductInventory
                .Include(p => p.FastFoodItem)
                .FirstOrDefaultAsync(p => p.FastFoodItemID == productId);
        }

        public async Task<ProductInventoryModel> CreateAsync(ProductInventoryModel productInventory)
        {
            _context.ProductInventory.Add(productInventory);
            await _context.SaveChangesAsync();
            return productInventory;
        }

        public async Task UpdateAsync(ProductInventoryModel productInventory)
        {
            var local = _context.ProductInventory
                .Local
                .FirstOrDefault(p => p.ID == productInventory.ID);

            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(productInventory).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var productInventory = await _context.ProductInventory.FindAsync(id);
            if (productInventory == null)
                return false;

            _context.ProductInventory.Remove(productInventory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProductInventoryModel>> GetLowStockAsync()
        {
            return await _context.ProductInventory
                .Include(p => p.FastFoodItem)
                .Where(p => p.CurrentStock <= p.MinimumStock)
                .OrderBy(p => p.CurrentStock)
                .ToListAsync();
        }
    }
}
