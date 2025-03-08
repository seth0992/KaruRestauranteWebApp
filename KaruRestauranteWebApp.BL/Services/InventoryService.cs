using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using Microsoft.Extensions.Logging;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface IInventoryService
    {
        Task<List<IngredientModel>> GetAllIngredientsAsync();
        Task<List<InventoryTransactionModel>> GetTransactionsAsync(DateTime? fromDate, DateTime? toDate);
        Task<IngredientModel> CreateIngredientAsync(IngredientModel ingredient);
        Task<InventoryTransactionModel> RegisterTransactionAsync(InventoryTransactionModel transaction);
        Task<List<IngredientModel>> GetLowStockItemsAsync();
        Task<bool> UpdateIngredientStockAsync(int ingredientId, decimal quantity, string transactionType);

        Task<IngredientModel?> GetIngredientByIdAsync(int id);
        Task UpdateIngredientAsync(IngredientModel ingredient);
    }

    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(IInventoryRepository inventoryRepository, ILogger<InventoryService> logger)
        {
            _inventoryRepository = inventoryRepository;
            _logger = logger;
        }

        public async Task<List<IngredientModel>> GetAllIngredientsAsync()
        {
            try
            {
                return await _inventoryRepository.GetAllIngredientsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ingredientes");
                throw;
            }
        }

        public async Task<IngredientModel> CreateIngredientAsync(IngredientModel ingredient)
        {
            try
            {
                ValidateIngredient(ingredient);
                ingredient.CreatedAt = DateTime.UtcNow;
                ingredient.IsActive = true;
                return await _inventoryRepository.CreateIngredientAsync(ingredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ingrediente");
                throw;
            }
        }

        public async Task<List<InventoryTransactionModel>> GetTransactionsAsync(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                return await _inventoryRepository.GetTransactionsAsync(fromDate, toDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones");
                throw;
            }
        }

        public async Task<InventoryTransactionModel> RegisterTransactionAsync(InventoryTransactionModel transaction)
        {
            try
            {
                ValidateTransaction(transaction);

                // Registrar la transacción
                transaction.TransactionDate = DateTime.UtcNow;
                var registeredTransaction = await _inventoryRepository.CreateTransactionAsync(transaction);

                // Actualizar el stock del ingrediente
                await UpdateIngredientStockAsync(
                    transaction.IngredientID,
                    transaction.Quantity,
                    transaction.TransactionType);

                return registeredTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar transacción");
                throw;
            }
        }

        public async Task<List<IngredientModel>> GetLowStockItemsAsync()
        {
            try
            {
                return await _inventoryRepository.GetLowStockItemsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener items con bajo stock");
                throw;
            }
        }

        public async Task<IngredientModel?> GetIngredientByIdAsync(int id)
        {
            try
            {
                return await _inventoryRepository.GetIngredientByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ingrediente por ID {IngredientId}", id);
                throw;
            }
        }

        public async Task UpdateIngredientAsync(IngredientModel ingredient)
        {
            try
            {
                ValidateIngredient(ingredient);

                var existingIngredient = await _inventoryRepository.GetIngredientByIdAsync(ingredient.ID);
                if (existingIngredient == null)
                {
                    throw new InvalidOperationException($"No se encontró el ingrediente con ID: {ingredient.ID}");
                }

                // Actualizar solo las propiedades permitidas
                existingIngredient.Name = ingredient.Name;
                existingIngredient.Description = ingredient.Description;
                existingIngredient.UnitOfMeasure = ingredient.UnitOfMeasure;
                existingIngredient.MinimumStock = ingredient.MinimumStock;
                existingIngredient.PurchasePrice = ingredient.PurchasePrice;
                existingIngredient.IsActive = ingredient.IsActive;
                existingIngredient.UpdatedAt = DateTime.UtcNow;

                await _inventoryRepository.UpdateIngredientAsync(existingIngredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar ingrediente {IngredientId}", ingredient.ID);
                throw;
            }
        }

        public async Task<bool> UpdateIngredientStockAsync(int ingredientId, decimal quantity, string transactionType)
        {
            try
            {
                var ingredient = await _inventoryRepository.GetIngredientByIdAsync(ingredientId);
                if (ingredient == null)
                    throw new InvalidOperationException("Ingrediente no encontrado");

                switch (transactionType.ToUpper())
                {
                    case "PURCHASE":
                        ingredient.StockQuantity += quantity;
                        ingredient.LastRestockDate = DateTime.UtcNow;
                        break;
                    case "CONSUMPTION":
                    case "LOSS":
                        if (ingredient.StockQuantity < quantity)
                            throw new InvalidOperationException("Stock insuficiente");
                        ingredient.StockQuantity -= quantity;
                        break;
                    case "ADJUSTMENT":
                        ingredient.StockQuantity = quantity;
                        break;
                    default:
                        throw new InvalidOperationException("Tipo de transacción no válido");
                }

                return await _inventoryRepository.UpdateIngredientAsync(ingredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del ingrediente");
                throw;
            }
        }

        private void ValidateIngredient(IngredientModel ingredient)
        {
            if (string.IsNullOrWhiteSpace(ingredient.Name))
                throw new InvalidOperationException("El nombre del ingrediente es requerido");

            if (string.IsNullOrWhiteSpace(ingredient.UnitOfMeasure))
                throw new InvalidOperationException("La unidad de medida es requerida");

            if (ingredient.MinimumStock < 0)
                throw new InvalidOperationException("El stock mínimo no puede ser negativo");

            if (ingredient.PurchasePrice < 0)
                throw new InvalidOperationException("El costo no puede ser negativo");
        }

        private void ValidateTransaction(InventoryTransactionModel transaction)
        {
            if (transaction.Quantity <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero");

            if (transaction.UnitPrice < 0)
                throw new InvalidOperationException("El precio unitario no puede ser negativo");

            if (string.IsNullOrWhiteSpace(transaction.TransactionType))
                throw new InvalidOperationException("El tipo de transacción es requerido");

            var validTypes = new[] { "PURCHASE", "CONSUMPTION", "ADJUSTMENT", "LOSS" };
            if (!validTypes.Contains(transaction.TransactionType.ToUpper()))
                throw new InvalidOperationException("Tipo de transacción no válido");
        }
    }
}
