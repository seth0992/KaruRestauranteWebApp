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
    public interface IProductInventoryService
    {
        Task<List<ProductInventoryModel>> GetAllProductInventoryAsync();
        Task<ProductInventoryModel?> GetProductInventoryByIdAsync(int id);
        Task<ProductInventoryModel?> GetProductInventoryByProductIdAsync(int productId);
        Task<ProductInventoryModel> CreateProductInventoryAsync(ProductInventoryDTO dto);
        Task UpdateProductInventoryAsync(ProductInventoryDTO dto);
        Task<bool> DeleteProductInventoryAsync(int id);
        Task<List<ProductInventoryModel>> GetLowStockAsync();
        Task<ProductInventoryModel> ProcessStockMovementAsync(StockMovementDTO movement);
    }

    public class ProductInventoryService : IProductInventoryService
    {
        private readonly IProductInventoryRepository _productInventoryRepository;
        private readonly IFastFoodRepository _fastFoodRepository;
        private readonly ILogger<ProductInventoryService> _logger;

        public ProductInventoryService(
            IProductInventoryRepository productInventoryRepository,
            IFastFoodRepository fastFoodRepository,
            ILogger<ProductInventoryService> logger)
        {
            _productInventoryRepository = productInventoryRepository;
            _fastFoodRepository = fastFoodRepository;
            _logger = logger;
        }

        public async Task<List<ProductInventoryModel>> GetAllProductInventoryAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de inventarios de productos");
                return await _productInventoryRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventarios de productos");
                throw;
            }
        }

        public async Task<ProductInventoryModel?> GetProductInventoryByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando inventario de producto con ID: {InventoryId}", id);
                return await _productInventoryRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario con ID: {InventoryId}", id);
                throw;
            }
        }

        public async Task<ProductInventoryModel?> GetProductInventoryByProductIdAsync(int productId)
        {
            try
            {
                _logger.LogInformation("Buscando inventario para el producto con ID: {ProductId}", productId);
                return await _productInventoryRepository.GetByProductIdAsync(productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario para el producto con ID: {ProductId}", productId);
                throw;
            }
        }

        public async Task<ProductInventoryModel> CreateProductInventoryAsync(ProductInventoryDTO dto)
        {
            try
            {
                // Validar que el producto existe
                var product = await _fastFoodRepository.GetByIdAsync(dto.FastFoodItemID);
                if (product == null)
                {
                    throw new ValidationException($"El producto con ID {dto.FastFoodItemID} no existe");
                }

                // Validar que el producto no tenga ya un inventario asociado
                var existingInventory = await _productInventoryRepository.GetByProductIdAsync(dto.FastFoodItemID);
                if (existingInventory != null)
                {
                    throw new ValidationException($"El producto ya tiene un inventario asociado");
                }

                var inventoryModel = new ProductInventoryModel
                {
                    FastFoodItemID = dto.FastFoodItemID,
                    CurrentStock = dto.CurrentStock,
                    MinimumStock = dto.MinimumStock,
                    PurchasePrice = dto.PurchasePrice,
                    SuggestedMarkup = dto.SuggestedMarkup,
                    LastRestockDate = dto.LastRestockDate,
                    SKU = dto.SKU,
                    UnitOfMeasure = dto.UnitOfMeasure,
                    LocationCode = dto.LocationCode
                };

                return await _productInventoryRepository.CreateAsync(inventoryModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear inventario para el producto {ProductId}", dto.FastFoodItemID);
                throw;
            }
        }

        public async Task UpdateProductInventoryAsync(ProductInventoryDTO dto)
        {
            try
            {
                var existingInventory = await _productInventoryRepository.GetByIdAsync(dto.ID);
                if (existingInventory == null)
                {
                    throw new ValidationException($"El inventario con ID {dto.ID} no existe");
                }

                // Validar que el producto no fue cambiado
                if (existingInventory.FastFoodItemID != dto.FastFoodItemID)
                {
                    throw new ValidationException("No se puede cambiar el producto asociado al inventario");
                }

                existingInventory.CurrentStock = dto.CurrentStock;
                existingInventory.MinimumStock = dto.MinimumStock;
                existingInventory.PurchasePrice = dto.PurchasePrice;
                existingInventory.SuggestedMarkup = dto.SuggestedMarkup;
                existingInventory.SKU = dto.SKU;
                existingInventory.UnitOfMeasure = dto.UnitOfMeasure;
                existingInventory.LocationCode = dto.LocationCode;

                await _productInventoryRepository.UpdateAsync(existingInventory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar inventario con ID {InventoryId}", dto.ID);
                throw;
            }
        }

        public async Task<bool> DeleteProductInventoryAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando inventario con ID: {InventoryId}", id);
                return await _productInventoryRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar inventario con ID: {InventoryId}", id);
                throw;
            }
        }

        public async Task<List<ProductInventoryModel>> GetLowStockAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo productos con bajo stock");
                return await _productInventoryRepository.GetLowStockAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con bajo stock");
                throw;
            }
        }

        public async Task<ProductInventoryModel> ProcessStockMovementAsync(StockMovementDTO movement)
        {
            try
            {
                var inventory = await _productInventoryRepository.GetByIdAsync(movement.ProductInventoryID);
                if (inventory == null)
                {
                    throw new ValidationException($"El inventario con ID {movement.ProductInventoryID} no existe");
                }

                switch (movement.MovementType.ToUpper())
                {
                    case "ENTRADA":
                        inventory.CurrentStock += movement.Quantity;
                        inventory.LastRestockDate = DateTime.UtcNow;
                        break;
                    case "SALIDA":
                        if (inventory.CurrentStock < movement.Quantity)
                        {
                            throw new ValidationException("No hay suficiente stock disponible");
                        }
                        inventory.CurrentStock -= movement.Quantity;
                        break;
                    case "AJUSTE":
                        inventory.CurrentStock = movement.Quantity;
                        break;
                    default:
                        throw new ValidationException("Tipo de movimiento no válido. Usar: Entrada, Salida o Ajuste");
                }

                await _productInventoryRepository.UpdateAsync(inventory);
                return inventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar movimiento de stock para el inventario {InventoryId}", movement.ProductInventoryID);
                throw;
            }
        }
    }
}
