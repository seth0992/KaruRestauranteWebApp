using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IInventoryRepository
    {
        Task<List<IngredientModel>> GetAllIngredientsAsync();
        Task<IngredientModel?> GetIngredientByIdAsync(int id);
        Task<IngredientModel> CreateIngredientAsync(IngredientModel ingredient);
        Task<bool> UpdateIngredientAsync(IngredientModel ingredient);
        Task<List<InventoryTransactionModel>> GetTransactionsAsync(DateTime? fromDate, DateTime? toDate);
        Task<InventoryTransactionModel> CreateTransactionAsync(InventoryTransactionModel transaction);
        Task<List<IngredientModel>> GetLowStockItemsAsync();

    }
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<IngredientModel>> GetAllIngredientsAsync()
        {
            return await _context.Ingredients
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<IngredientModel?> GetIngredientByIdAsync(int id)
        {
            return await _context.Ingredients
                .FirstOrDefaultAsync(i => i.ID == id);
        }

        public async Task<IngredientModel> CreateIngredientAsync(IngredientModel ingredient)
        {
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
            return ingredient;
        }

        public async Task<bool> UpdateIngredientAsync(IngredientModel ingredient)
        {
            var local = _context.Ingredients
                .Local
                .FirstOrDefault(e => e.ID == ingredient.ID);

            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(ingredient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<InventoryTransactionModel>> GetTransactionsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.InventoryTransactions
                .Include(t => t.Ingredient)
                .Include(t => t.User)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.TransactionDate <= toDate.Value);

            return await query
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<InventoryTransactionModel> CreateTransactionAsync(InventoryTransactionModel transaction)
        {
            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<List<IngredientModel>> GetLowStockItemsAsync()
        {
            return await _context.Ingredients
                .Where(i => i.IsActive && i.StockQuantity <= i.MinimumStock)
                .OrderBy(i => i.StockQuantity)
                .ToListAsync();
        }
    }

}
