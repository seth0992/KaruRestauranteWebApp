using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Models.Orders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface ITableService
    {
        Task<List<TableModel>> GetAllTablesAsync(bool includeInactive = false);
        Task<TableModel?> GetTableByIdAsync(int id);
        Task<TableModel?> GetTableByNumberAsync(int tableNumber);
        Task<List<TableModel>> GetAvailableTablesAsync();
        Task<TableModel> CreateTableAsync(TableDTO tableDto);
        Task UpdateTableAsync(TableDTO tableDto);
        Task<bool> UpdateTableStatusAsync(int id, string status);
        Task<bool> DeleteTableAsync(int id);
    }

    public class TableService : ITableService
    {
        private readonly ITableRepository _tableRepository;
        private readonly ILogger<TableService> _logger;

        public TableService(ITableRepository tableRepository, ILogger<TableService> logger)
        {
            _tableRepository = tableRepository;
            _logger = logger;
        }

        public async Task<List<TableModel>> GetAllTablesAsync(bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de mesas. Incluir inactivas: {IncludeInactive}", includeInactive);
                return await _tableRepository.GetAllAsync(includeInactive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mesas");
                throw;
            }
        }

        public async Task<TableModel?> GetTableByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando mesa con ID: {TableId}", id);
                return await _tableRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la mesa con ID: {TableId}", id);
                throw;
            }
        }

        public async Task<TableModel?> GetTableByNumberAsync(int tableNumber)
        {
            try
            {
                _logger.LogInformation("Buscando mesa con número: {TableNumber}", tableNumber);
                return await _tableRepository.GetByTableNumberAsync(tableNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la mesa con número: {TableNumber}", tableNumber);
                throw;
            }
        }

        public async Task<List<TableModel>> GetAvailableTablesAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo mesas disponibles");
                return await _tableRepository.GetAvailableTablesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mesas disponibles");
                throw;
            }
        }
        public async Task<TableModel> CreateTableAsync(TableDTO tableDto)
        {
            try
            {
                // Validar los datos
                if (tableDto.TableNumber <= 0)
                {
                    throw new ValidationException("El número de mesa debe ser mayor a 0");
                }

                if (tableDto.Capacity <= 0)
                {
                    throw new ValidationException("La capacidad debe ser mayor a 0");
                }

                // Verificar si ya existe una mesa con el mismo número
                var existingTable = await _tableRepository.GetByTableNumberAsync(tableDto.TableNumber);
                if (existingTable != null)
                {
                    throw new ValidationException($"Ya existe una mesa con el número {tableDto.TableNumber}");
                }

                // Crear el modelo
                var table = new TableModel
                {
                    TableNumber = tableDto.TableNumber,
                    Capacity = tableDto.Capacity,
                    Status = tableDto.Status,
                    Location = tableDto.Location,
                    IsActive = tableDto.IsActive
                };

                return await _tableRepository.CreateAsync(table);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la mesa {TableNumber}", tableDto.TableNumber);
                throw;
            }
        }

        public async Task UpdateTableAsync(TableDTO tableDto)
        {
            try
            {
                // Validar los datos
                if (tableDto.TableNumber <= 0)
                {
                    throw new ValidationException("El número de mesa debe ser mayor a 0");
                }

                if (tableDto.Capacity <= 0)
                {
                    throw new ValidationException("La capacidad debe ser mayor a 0");
                }

                // Verificar que la mesa exista
                var existingTable = await _tableRepository.GetByIdAsync(tableDto.ID);
                if (existingTable == null)
                {
                    throw new ValidationException($"No se encontró la mesa con ID: {tableDto.ID}");
                }

                // Verificar si otra mesa tiene el mismo número
                if (existingTable.TableNumber != tableDto.TableNumber)
                {
                    var tableWithSameNumber = await _tableRepository.GetByTableNumberAsync(tableDto.TableNumber);
                    if (tableWithSameNumber != null)
                    {
                        throw new ValidationException($"Ya existe una mesa con el número {tableDto.TableNumber}");
                    }
                }

                // Actualizar los datos
                existingTable.TableNumber = tableDto.TableNumber;
                existingTable.Capacity = tableDto.Capacity;
                existingTable.Status = tableDto.Status;
                existingTable.Location = tableDto.Location;
                existingTable.IsActive = tableDto.IsActive;

                await _tableRepository.UpdateAsync(existingTable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la mesa con ID: {TableId}", tableDto.ID);
                throw;
            }
        }

        public async Task<bool> UpdateTableStatusAsync(int id, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    throw new ValidationException("El estado de la mesa es requerido");
                }

                // Validar que el estado sea válido
                var validStatuses = new[] { "Available", "Occupied", "Reserved", "Maintenance" };
                if (!validStatuses.Contains(status))
                {
                    throw new ValidationException($"Estado de mesa no válido. Valores posibles: {string.Join(", ", validStatuses)}");
                }

                return await _tableRepository.UpdateStatusAsync(id, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el estado de la mesa {TableId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteTableAsync(int id)
        {
            try
            {
                _logger.LogInformation("Intentando eliminar mesa: {TableId}", id);
                return await _tableRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la mesa {TableId}", id);
                throw;
            }
        }
    }
}