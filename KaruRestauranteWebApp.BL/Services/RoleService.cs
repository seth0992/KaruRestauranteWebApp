using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.Users;
using Microsoft.Extensions.Logging;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface IRoleService
    {
        Task<List<RoleModel>> GetAllRolesAsync();
        Task<RoleModel?> GetRoleByIdAsync(int id);
        Task<RoleModel> CreateRoleAsync(RoleModel role);
        Task UpdateRoleAsync(RoleModel role);
        Task<bool> DeleteRoleAsync(int id);
    }

    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<RoleService> _logger;

        public RoleService(IRoleRepository roleRepository, ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<List<RoleModel>> GetAllRolesAsync()
        {
            try
            {
                return await _roleRepository.GetAllRolesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles");
                throw;
            }
        }

        public async Task<RoleModel?> GetRoleByIdAsync(int id)
        {
            return await _roleRepository.GetRoleByIdAsync(id);
        }

        public async Task<RoleModel> CreateRoleAsync(RoleModel role)
        {
            try
            {
                return await _roleRepository.CreateRoleAsync(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol");
                throw;
            }
        }

        public async Task UpdateRoleAsync(RoleModel role)
        {
            try
            {
                await _roleRepository.UpdateRoleAsync(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol");
                throw;
            }
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            try
            {
                return await _roleRepository.DeleteRoleAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol");
                throw;
            }
        }
    }

}
