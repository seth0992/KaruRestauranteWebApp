using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface IComboService
    {
        Task<List<ComboModel>> GetAllCombosAsync(bool includeInactive = false);
        Task<ComboModel?> GetComboByIdAsync(int id);
        Task<ComboModel> CreateComboAsync(ComboDTO comboDto);
        Task UpdateComboAsync(int id, ComboDTO comboDto);
        Task<bool> DeleteComboAsync(int id);
    }

    public class ComboService : IComboService
    {
        private readonly IComboRepository _comboRepository;
        private readonly IFastFoodRepository _fastFoodRepository;
        private readonly ILogger<ComboService> _logger;

        public ComboService(
            IComboRepository comboRepository,
            IFastFoodRepository fastFoodRepository,
            ILogger<ComboService> logger)
        {
            _comboRepository = comboRepository;
            _fastFoodRepository = fastFoodRepository;
            _logger = logger;
        }

        public async Task<List<ComboModel>> GetAllCombosAsync(bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de combos. Incluir inactivos: {IncludeInactive}", includeInactive);
                return await _comboRepository.GetAllAsync(includeInactive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener combos");
                throw;
            }
        }

        public async Task<ComboModel?> GetComboByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando combo con ID: {ComboId}", id);
                var combo = await _comboRepository.GetByIdAsync(id);

                if (combo == null)
                {
                    _logger.LogWarning("No se encontró el combo con ID: {ComboId}", id);
                }

                return combo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el combo con ID: {ComboId}", id);
                throw;
            }
        }

        public async Task<ComboModel> CreateComboAsync(ComboDTO comboDto)
        {
            try
            {
                await ValidateCombo(comboDto);

                // Calcular precio con descuento si es necesario
                if (comboDto.DiscountPercentage > 0 && comboDto.RegularPrice > 0)
                {
                    comboDto.SellingPrice = comboDto.RegularPrice * (1 - (comboDto.DiscountPercentage / 100));
                }

                // Crear modelo base sin items
                var combo = new ComboModel
                {
                    Name = comboDto.Name,
                    Description = comboDto.Description,
                    RegularPrice = comboDto.RegularPrice,
                    SellingPrice = comboDto.SellingPrice,
                    DiscountPercentage = comboDto.DiscountPercentage,
                    IsAvailable = comboDto.IsAvailable,
                    ImageUrl = comboDto.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Crear primero el combo (sin items)
                var createdCombo = await _comboRepository.CreateAsync(combo);

                // Ahora crear los items uno por uno
                if (comboDto.Items != null && comboDto.Items.Any())
                {
                    var comboItems = new List<ComboItemModel>();
                    foreach (var itemDto in comboDto.Items)
                    {
                        // Validar que el producto existe
                        var fastFoodItem = await _fastFoodRepository.GetByIdAsync(itemDto.FastFoodItemID);
                        if (fastFoodItem == null)
                        {
                            throw new ValidationException($"Producto con ID {itemDto.FastFoodItemID} no encontrado");
                        }

                        var comboItem = new ComboItemModel
                        {
                            ComboID = createdCombo.ID,
                            FastFoodItemID = itemDto.FastFoodItemID,
                            Quantity = itemDto.Quantity,
                            AllowCustomization = itemDto.AllowCustomization,
                            SpecialInstructions = itemDto.SpecialInstructions
                        };

                        comboItems.Add(comboItem);
                    }

                    // Agregar items al combo
                    await _comboRepository.AddComboItemsAsync(createdCombo.ID, comboItems);
                }

                // Obtener combo completo con relaciones
                return await _comboRepository.GetByIdAsync(createdCombo.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el combo {ComboName}", comboDto.Name);
                throw;
            }
        }

        public async Task UpdateComboAsync(int id, ComboDTO comboDto)
        {
            try
            {
                if (id != comboDto.ID)
                {
                    throw new InvalidOperationException("ID no coincide");
                }

                await ValidateCombo(comboDto, id);

                // Calcular precio con descuento si es necesario
                if (comboDto.DiscountPercentage > 0 && comboDto.RegularPrice > 0)
                {
                    comboDto.SellingPrice = comboDto.RegularPrice * (1 - (comboDto.DiscountPercentage / 100));
                }

                // Obtener combo existente
                var existingCombo = await _comboRepository.GetByIdAsync(id);
                if (existingCombo == null)
                {
                    throw new InvalidOperationException($"No se encontró el combo con ID: {id}");
                }

                // Actualizar propiedades básicas
                existingCombo.Name = comboDto.Name;
                existingCombo.Description = comboDto.Description;
                existingCombo.RegularPrice = comboDto.RegularPrice;
                existingCombo.SellingPrice = comboDto.SellingPrice;
                existingCombo.DiscountPercentage = comboDto.DiscountPercentage;
                existingCombo.IsAvailable = comboDto.IsAvailable;
                existingCombo.ImageUrl = comboDto.ImageUrl;
                existingCombo.UpdatedAt = DateTime.UtcNow;

                // Actualizar el combo base
                await _comboRepository.UpdateAsync(existingCombo);

                // Actualizar items por separado
                if (comboDto.Items != null)
                {
                    List<ComboItemModel> comboItems = comboDto.Items.Select(i => new ComboItemModel
                    {
                        ComboID = id,
                        FastFoodItemID = i.FastFoodItemID,
                        Quantity = i.Quantity,
                        AllowCustomization = i.AllowCustomization,
                        SpecialInstructions = i.SpecialInstructions
                    }).ToList();

                    await _comboRepository.UpdateComboItemsAsync(id, comboItems);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el combo {ComboId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteComboAsync(int id)
        {
            try
            {
                _logger.LogInformation("Intentando eliminar combo: {ComboId}", id);
                return await _comboRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el combo {ComboId}", id);
                throw;
            }
        }

        private async Task ValidateCombo(ComboDTO combo, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(combo.Name))
            {
                throw new ValidationException("El nombre del combo es obligatorio");
            }

            if (combo.RegularPrice <= 0)
            {
                throw new ValidationException("El precio regular debe ser mayor a 0");
            }

            if (combo.SellingPrice <= 0)
            {
                throw new ValidationException("El precio de venta debe ser mayor a 0");
            }

            if (combo.DiscountPercentage < 0 || combo.DiscountPercentage > 100)
            {
                throw new ValidationException("El descuento debe estar entre 0 y 100");
            }

            if (await _comboRepository.ExistsAsync(combo.Name, excludeId))
            {
                throw new ValidationException($"Ya existe un combo con el nombre: {combo.Name}");
            }

            // Validar que haya al menos un item
            if (combo.Items == null || !combo.Items.Any())
            {
                throw new ValidationException("El combo debe tener al menos un producto");
            }

            // Validar items
            foreach (var item in combo.Items)
            {
                if (item.FastFoodItemID <= 0)
                {
                    throw new ValidationException("Todos los productos deben ser seleccionados");
                }

                if (item.Quantity <= 0)
                {
                    throw new ValidationException("La cantidad de cada producto debe ser mayor a 0");
                }
            }
        }
    }
}
