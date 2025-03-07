using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using Microsoft.Extensions.Logging;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryModel>> GetAllCategoriesAsync(bool includeInactive = false);
        Task<CategoryModel?> GetCategoryByIdAsync(int id);
        Task<CategoryModel> CreateCategoryAsync(CategoryModel category);
        Task UpdateCategoryAsync(CategoryModel category);
        Task<bool> DeleteCategoryAsync(int id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository,
                             ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<List<CategoryModel>> GetAllCategoriesAsync(bool includeInactive = false)
        {
            try
            {
                // Obtenemos todas las categorías, incluyendo inactivas si se especifica
                _logger.LogInformation("Obteniendo lista de categorías. Incluir inactivas: {IncludeInactive}",
                    includeInactive);

                return await _categoryRepository.GetAllAsync(includeInactive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las categorías");
                throw;
            }
        }

        public async Task<CategoryModel?> GetCategoryByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando categoría con ID: {CategoryId}", id);

                var category = await _categoryRepository.GetByIdAsync(id);

                if (category == null)
                {
                    _logger.LogWarning("No se encontró la categoría con ID: {CategoryId}", id);
                }

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la categoría con ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<CategoryModel> CreateCategoryAsync(CategoryModel category)
        {
            try
            {
                // Validamos los datos de entrada
                ValidateCategory(category);

                // Verificamos que no exista otra categoría con el mismo nombre
                if (await _categoryRepository.ExistsAsync(category.Name))
                {
                    _logger.LogWarning("Intento de crear categoría duplicada: {CategoryName}",
                        category.Name);
                    throw new InvalidOperationException(
                        $"Ya existe una categoría con el nombre: {category.Name}");
                }

                // Aseguramos que los campos de auditoría estén correctamente establecidos
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;
                category.IsActive = true;

                _logger.LogInformation("Creando nueva categoría: {CategoryName}", category.Name);
                return await _categoryRepository.CreateAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la categoría {CategoryName}", category.Name);
                throw;
            }
        }

        public async Task UpdateCategoryAsync(CategoryModel category)
        {
            try
            {
                // Validamos los datos de entrada
                ValidateCategory(category);

                // Verificamos que la categoría exista
                var existingCategory = await _categoryRepository.GetByIdAsync(category.ID);
                if (existingCategory == null)
                {
                    throw new InvalidOperationException(
                        $"No se encontró la categoría con ID: {category.ID}");
                }

                // Actualizamos solo las propiedades permitidas
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.IsActive = category.IsActive;
                existingCategory.UpdatedAt = DateTime.UtcNow;

                // Actualizamos la categoría usando la instancia existente
                await _categoryRepository.UpdateAsync(existingCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la categoría {CategoryId}", category.ID);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                _logger.LogInformation("Intentando eliminar categoría: {CategoryId}", id);

                // Verificamos si la categoría existe
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Intento de eliminar categoría inexistente: {CategoryId}", id);
                    return false;
                }

                // Si la categoría tiene items asociados, la desactivamos en lugar de eliminarla
                if (category.Items.Any())
                {
                    _logger.LogInformation(
                        "La categoría {CategoryId} tiene {ItemCount} items asociados. Se desactivará en lugar de eliminar",
                        id, category.Items.Count);

                    category.IsActive = false;
                    category.UpdatedAt = DateTime.UtcNow;
                    await _categoryRepository.UpdateAsync(category);
                    return true;
                }

                // Si no tiene items asociados, procedemos con la eliminación
                return await _categoryRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la categoría {CategoryId}", id);
                throw;
            }
        }

        private void ValidateCategory(CategoryModel category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                throw new InvalidOperationException("El nombre de la categoría es obligatorio");
            }

            if (category.Name.Length > 50)
            {
                throw new InvalidOperationException(
                    "El nombre de la categoría no puede exceder los 50 caracteres");
            }

            if (!string.IsNullOrEmpty(category.Description) && category.Description.Length > 200)
            {
                throw new InvalidOperationException(
                    "La descripción de la categoría no puede exceder los 200 caracteres");
            }
        }
    }

}
