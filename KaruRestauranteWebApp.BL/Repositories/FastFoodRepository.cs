﻿using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IFastFoodRepository
    {
        Task<List<FastFoodItemModel>> GetAllAsync(bool includeInactive = false);
        Task<FastFoodItemModel?> GetByIdAsync(int id);
        Task<FastFoodItemModel> CreateAsync(FastFoodItemModel product);
        Task UpdateAsync(FastFoodItemModel product);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string name, int? excludeId = null);
        Task<List<FastFoodItemModel>> GetByCategoryAsync(int categoryId);
        Task AddIngredientsAsync(int productId, List<ItemIngredientModel> ingredients);
        Task UpdateIngredientsAsync(int productId, List<ItemIngredientModel> ingredients);

    }

    public class FastFoodRepository : IFastFoodRepository
    {
        private readonly AppDbContext _context;

        public FastFoodRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FastFoodItemModel>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.FastFoodItems
                .Include(f => f.Category)
                .Include(f => f.Ingredients)
                    .ThenInclude(i => i.Ingredient)
                .Include(f => f.Inventory)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(f => f.IsAvailable);
            }

            return await query.OrderBy(f => f.Name).ToListAsync();
        }

        public async Task<FastFoodItemModel?> GetByIdAsync(int id)
        {
            return await _context.FastFoodItems
                .Include(f => f.Category)
                .Include(f => f.Ingredients)
                    .ThenInclude(i => i.Ingredient)
                .Include(f => f.Inventory)
                .FirstOrDefaultAsync(f => f.ID == id);
        }

        public async Task<FastFoodItemModel> CreateAsync(FastFoodItemModel product)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            // No incluir ingredientes en la creación inicial
            await _context.FastFoodItems.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task AddIngredientsAsync(int productId, List<ItemIngredientModel> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                ingredient.FastFoodItemID = productId;
                await _context.ItemIngredients.AddAsync(ingredient);
            }

            await _context.SaveChangesAsync();
        }

        //public async Task UpdateAsync(FastFoodItemModel product)
        //{
        //    // Manejar el producto sin ingredientes primero
        //    var local = _context.FastFoodItems
        //        .Local
        //        .FirstOrDefault(f => f.ID == product.ID);

        //    if (local != null)
        //    {
        //        _context.Entry(local).State = EntityState.Detached;
        //    }

        //    product.UpdatedAt = DateTime.UtcNow;

        //    // Actualizar solo el producto principal
        //    _context.Entry(product).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();
        //}
        public async Task UpdateAsync(FastFoodItemModel product)
        {
            // Desconectar entidad local si existe
            var local = _context.FastFoodItems
                .Local
                .FirstOrDefault(f => f.ID == product.ID);

            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            product.UpdatedAt = DateTime.UtcNow;

            try
            {
                // Obtener la entidad original
                var originalEntity = await _context.FastFoodItems.FindAsync(product.ID);
                if (originalEntity == null)
                {
                    throw new InvalidOperationException($"No se encontró el producto con ID: {product.ID}");
                }

                // Actualizar cada propiedad individualmente
                _context.Entry(originalEntity).CurrentValues.SetValues(product);

                // No modificar relaciones
                _context.Entry(originalEntity).Property(x => x.CreatedAt).IsModified = false;

                // Guardar cambios
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Si hay error, intentamos un enfoque más básico
                var originalEntity = await _context.FastFoodItems
                    .Include(p => p.Category)
                    .Include(p => p.Ingredients)
                    .FirstOrDefaultAsync(p => p.ID == product.ID);

                if (originalEntity != null)
                {
                    // Actualizar propiedades manualmente
                    originalEntity.Name = product.Name;
                    originalEntity.Description = product.Description;
                    originalEntity.CategoryID = product.CategoryID;
                    originalEntity.SellingPrice = product.SellingPrice;
                    originalEntity.EstimatedCost = product.EstimatedCost;
                    originalEntity.ProductTypeID = product.ProductTypeID;
                    originalEntity.IsAvailable = product.IsAvailable;
                    originalEntity.ImageUrl = product.ImageUrl;
                    originalEntity.EstimatedPreparationTime = product.EstimatedPreparationTime;
                    originalEntity.UpdatedAt = DateTime.UtcNow;

                    // Guardar cambioe
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw; // No encontramos la entidad, relanzar la excepción
                }
            }
        }

        public async Task UpdateIngredientsAsync(int productId, List<ItemIngredientModel> ingredients)
        {
            // Eliminar ingredientes existentes
            var existingIngredients = await _context.ItemIngredients
                .Where(i => i.FastFoodItemID == productId)
                .ToListAsync();

            _context.ItemIngredients.RemoveRange(existingIngredients);
            await _context.SaveChangesAsync();

            // Agregar nuevos ingredientes
            foreach (var ingredient in ingredients)
            {
                ingredient.FastFoodItemID = productId;
                await _context.ItemIngredients.AddAsync(ingredient);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.FastFoodItems.FindAsync(id);
            if (product == null)
                return false;

            product.IsAvailable = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            return await _context.FastFoodItems
                .AnyAsync(f => f.Name.ToLower() == name.ToLower()
                            && (!excludeId.HasValue || f.ID != excludeId.Value));
        }

        public async Task<List<FastFoodItemModel>> GetByCategoryAsync(int categoryId)
        {
            return await _context.FastFoodItems
                .Include(f => f.Category)
                .Include(f => f.Ingredients)
                    .ThenInclude(i => i.Ingredient)
                .Include(f => f.Inventory)
                .Where(f => f.CategoryID == categoryId && f.IsAvailable)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }
    }
}
