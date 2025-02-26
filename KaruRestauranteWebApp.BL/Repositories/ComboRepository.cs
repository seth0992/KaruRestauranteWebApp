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
    public interface IComboRepository
    {
        Task<List<ComboModel>> GetAllAsync(bool includeInactive = false);
        Task<ComboModel?> GetByIdAsync(int id);
        Task<ComboModel> CreateAsync(ComboModel combo);
        Task UpdateAsync(ComboModel combo);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string name, int? excludeId = null);
        Task AddComboItemsAsync(int comboId, List<ComboItemModel> items);
        Task UpdateComboItemsAsync(int comboId, List<ComboItemModel> items);
    }

    public class ComboRepository : IComboRepository
    {
        private readonly AppDbContext _context;

        public ComboRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ComboModel>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Combos
                .Include(c => c.Items)
                    .ThenInclude(i => i.FastFoodItem)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsAvailable);
            }

            return await query.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<ComboModel?> GetByIdAsync(int id)
        {
            return await _context.Combos
                .Include(c => c.Items)
                    .ThenInclude(i => i.FastFoodItem)
                .FirstOrDefaultAsync(c => c.ID == id);
        }

        public async Task<ComboModel> CreateAsync(ComboModel combo)
        {
            combo.CreatedAt = DateTime.UtcNow;
            combo.UpdatedAt = DateTime.UtcNow;

            // No incluir items en la creación inicial
            await _context.Combos.AddAsync(combo);
            await _context.SaveChangesAsync();
            return combo;
        }

        public async Task AddComboItemsAsync(int comboId, List<ComboItemModel> items)
        {
            foreach (var item in items)
            {
                item.ComboID = comboId;
                await _context.ComboItems.AddAsync(item);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ComboModel combo)
        {
            var local = _context.Combos
                .Local
                .FirstOrDefault(c => c.ID == combo.ID);

            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            combo.UpdatedAt = DateTime.UtcNow;

            _context.Entry(combo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateComboItemsAsync(int comboId, List<ComboItemModel> items)
        {
            // Eliminar items existentes
            var existingItems = await _context.ComboItems
                .Where(i => i.ComboID == comboId)
                .ToListAsync();

            _context.ComboItems.RemoveRange(existingItems);
            await _context.SaveChangesAsync();

            // Agregar nuevos items
            foreach (var item in items)
            {
                item.ComboID = comboId;
                await _context.ComboItems.AddAsync(item);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var combo = await _context.Combos.FindAsync(id);
            if (combo == null)
                return false;

            combo.IsAvailable = false;
            combo.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            return await _context.Combos
                .AnyAsync(c => c.Name.ToLower() == name.ToLower()
                            && (!excludeId.HasValue || c.ID != excludeId.Value));
        }
    }
}
