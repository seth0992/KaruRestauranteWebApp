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
    public interface ICategoryRepository
    {
        Task<List<CategoryModel>> GetAllAsync(bool includeInactive = false);
        Task<CategoryModel?> GetByIdAsync(int id);
        Task<CategoryModel> CreateAsync(CategoryModel category);
        Task UpdateAsync(CategoryModel category);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string name, int? excludeId = null);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryModel>> GetAllAsync(bool includeInactive = false)
        {
            // Si includeInactive es false, solo retornamos las categorías activas
            var query = _context.Categories.AsQueryable();
            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            // Incluimos los items relacionados para tener la información completa
            return await query
                .Include(c => c.Items)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<CategoryModel?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ID == id);
        }

        public async Task<CategoryModel> CreateAsync(CategoryModel category)
        {
            // Aseguramos que las fechas estén correctamente establecidas
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(CategoryModel category)
        {
            try
            {
                // Desactivamos el tracking de la entidad actual
                var local = _context.Categories
                    .Local
                    .FirstOrDefault(entry => entry.ID == category.ID);

                if (local != null)
                {
                    _context.Entry(local).State = EntityState.Detached;
                }

                // Marcamos la entidad como modificada
                _context.Entry(category).State = EntityState.Modified;

                // Aseguramos que la fecha de creación no se modifique
                _context.Entry(category).Property(x => x.CreatedAt).IsModified = false;

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (category == null)
                return false;

            // Si la categoría tiene items asociados, la desactivamos en lugar de eliminarla
            if (category.Items.Any())
            {
                category.IsActive = false;
                await UpdateAsync(category);
                return true;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            // Verificamos si existe otra categoría con el mismo nombre
            return await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == name.ToLower()
                              && (!excludeId.HasValue || c.ID != excludeId.Value));
        }
    }

}
