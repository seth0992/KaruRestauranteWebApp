using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using Microsoft.Extensions.Logging;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface IProductTypeService
    {
        Task<List<ProductTypeModel>> GetAllProductTypesAsync();
        Task<ProductTypeModel?> GetProductTypeByIdAsync(int id);
    }

    public class ProductTypeService : IProductTypeService
    {
        private readonly IProductTypeRepository _repository;
        private readonly ILogger<ProductTypeService> _logger;

        public ProductTypeService(
            IProductTypeRepository repository,
            ILogger<ProductTypeService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<ProductTypeModel>> GetAllProductTypesAsync()
        {
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de producto");
                throw;
            }
        }

        public async Task<ProductTypeModel?> GetProductTypeByIdAsync(int id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipo de producto {ProductTypeId}", id);
                throw;
            }
        }
    }
}
