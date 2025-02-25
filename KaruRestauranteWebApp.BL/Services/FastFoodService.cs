using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface IFastFoodService
    {
        Task<List<FastFoodItemModel>> GetAllProductsAsync(bool includeInactive = false);
        Task<FastFoodItemModel?> GetProductByIdAsync(int id);
        Task<FastFoodItemModel> CreateProductAsync(FastFoodItemDTO productDto);
        Task UpdateProductAsync(int id, FastFoodItemDTO productDto);
        Task<bool> DeleteProductAsync(int id);
        Task<List<FastFoodItemModel>> GetProductsByCategoryAsync(int categoryId);
    }
    public class FastFoodService : IFastFoodService
    {
        private readonly IFastFoodRepository _fastFoodRepository;
        private readonly ILogger<FastFoodService> _logger;

        public FastFoodService(
            IFastFoodRepository fastFoodRepository,
            ILogger<FastFoodService> logger)
        {
            _fastFoodRepository = fastFoodRepository;
            _logger = logger;
        }

        public async Task<List<FastFoodItemModel>> GetAllProductsAsync(bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de productos. Incluir inactivos: {IncludeInactive}",
                    includeInactive);
                return await _fastFoodRepository.GetAllAsync(includeInactive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                throw;
            }
        }

        public async Task<FastFoodItemModel?> GetProductByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando producto con ID: {ProductId}", id);
                var product = await _fastFoodRepository.GetByIdAsync(id);

                if (product == null)
                {
                    _logger.LogWarning("No se encontró el producto con ID: {ProductId}", id);
                }

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el producto con ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<FastFoodItemModel> CreateProductAsync(FastFoodItemDTO productDto)
        {
            try
            {
                // Validar producto
                await ValidateProduct(productDto);

                // Crear modelo base sin relaciones
                var product = new FastFoodItemModel
                {
                    Name = productDto.Name,
                    Description = productDto.Description ?? string.Empty,
                    CategoryID = productDto.CategoryID,
                    SellingPrice = productDto.SellingPrice,
                    EstimatedCost = productDto.EstimatedCost,
                    ProductTypeID = productDto.ProductTypeID,
                    IsAvailable = productDto.IsAvailable,
                    ImageUrl = productDto.ImageUrl ?? string.Empty,
                    EstimatedPreparationTime = productDto.EstimatedPreparationTime,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Crear primero el producto (sin ingredientes)
                var createdProduct = await _fastFoodRepository.CreateAsync(product);

                // Ahora crear los ingredientes uno por uno
                if (productDto.Ingredients != null && productDto.Ingredients.Any())
                {
                    var ingredients = new List<ItemIngredientModel>();
                    foreach (var ingredientDto in productDto.Ingredients)
                    {
                        var itemIngredient = new ItemIngredientModel
                        {
                            FastFoodItemID = createdProduct.ID,
                            IngredientID = ingredientDto.IngredientID,
                            Quantity = ingredientDto.Quantity,
                            IsOptional = ingredientDto.IsOptional,
                            CanBeExtra = ingredientDto.CanBeExtra,
                            ExtraPrice = ingredientDto.ExtraPrice
                        };

                        ingredients.Add(itemIngredient);
                    }

                    // Agregar ingredientes al producto
                    await _fastFoodRepository.AddIngredientsAsync(createdProduct.ID, ingredients);
                }

                // Obtener producto completo con relaciones
                return await _fastFoodRepository.GetByIdAsync(createdProduct.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el producto {ProductName}", productDto.Name);
                throw;
            }
        }

        public async Task UpdateProductAsync(int id, FastFoodItemDTO productDto)
        {
            try
            {
                if (id != productDto.ID)
                {
                    throw new InvalidOperationException("ID no coincide");
                }

                await ValidateProduct(productDto, id);

                // Obtener producto existente
                var existingProduct = await _fastFoodRepository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    throw new InvalidOperationException($"No se encontró el producto con ID: {id}");
                }

                // Actualizar solo propiedades básicas
                existingProduct.Name = productDto.Name;
                existingProduct.Description = productDto.Description ?? string.Empty;
                existingProduct.CategoryID = productDto.CategoryID;
                existingProduct.SellingPrice = productDto.SellingPrice;
                existingProduct.EstimatedCost = productDto.EstimatedCost;
                existingProduct.ProductTypeID = productDto.ProductTypeID;
                existingProduct.IsAvailable = productDto.IsAvailable;
                existingProduct.ImageUrl = productDto.ImageUrl ?? string.Empty;
                existingProduct.EstimatedPreparationTime = productDto.EstimatedPreparationTime;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                // Actualizar el producto base
                await _fastFoodRepository.UpdateAsync(existingProduct);

                // Actualizar ingredientes por separado
                List<ItemIngredientModel> ingredients = productDto.Ingredients.Select(i => new ItemIngredientModel
                {
                    FastFoodItemID = id,
                    IngredientID = i.IngredientID,
                    Quantity = i.Quantity,
                    IsOptional = i.IsOptional,
                    CanBeExtra = i.CanBeExtra,
                    ExtraPrice = i.ExtraPrice
                }).ToList();

                await _fastFoodRepository.UpdateIngredientsAsync(id, ingredients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el producto {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                _logger.LogInformation("Intentando eliminar producto: {ProductId}", id);
                return await _fastFoodRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el producto {ProductId}", id);
                throw;
            }
        }

        public async Task<List<FastFoodItemModel>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                return await _fastFoodRepository.GetByCategoryAsync(categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoría {CategoryId}", categoryId);
                throw;
            }
        }

        private async Task ValidateProduct(FastFoodItemDTO product, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ValidationException("El nombre del producto es obligatorio");
            }

            if (product.SellingPrice <= 0)
            {
                throw new ValidationException("El precio de venta debe ser mayor a 0");
            }

            if (product.EstimatedCost <= 0)
            {
                throw new ValidationException("El costo estimado debe ser mayor a 0");
            }

            if (await _fastFoodRepository.ExistsAsync(product.Name, excludeId))
            {
                throw new ValidationException($"Ya existe un producto con el nombre: {product.Name}");
            }

            // Validar que haya ingredientes
            if (product.Ingredients == null || !product.Ingredients.Any())
            {
                throw new ValidationException("El producto debe tener al menos un ingrediente");
            }

            // Validar ingredientes
            foreach (var ingredient in product.Ingredients)
            {
                if (ingredient.IngredientID <= 0)
                {
                    throw new ValidationException("Todos los ingredientes deben ser seleccionados");
                }

                if (ingredient.Quantity <= 0)
                {
                    throw new ValidationException("La cantidad de cada ingrediente debe ser mayor a 0");
                }

                if (ingredient.CanBeExtra && ingredient.ExtraPrice <= 0)
                {
                    throw new ValidationException("Los ingredientes extras deben tener un precio extra");
                }
            }
        }
    }
}
