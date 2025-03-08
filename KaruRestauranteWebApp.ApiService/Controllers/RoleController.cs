using KaruRestauranteWebApp.BL.Services;
using KaruRestauranteWebApp.Models.Entities.Users;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaruRestauranteWebApp.ApiService.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRoleService roleService, ILogger<RoleController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetAllRoles()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Ok(new BaseResponseModel { Success = true, Data = roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo roles");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener roles"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetRole(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Rol no encontrado"
                });
            }

            return Ok(new BaseResponseModel { Success = true, Data = role });
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> CreateRole([FromBody] RoleModel role)
        {
            try
            {
                var createdRole = await _roleService.CreateRoleAsync(role);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = createdRole,
                    ErrorMessage = "Rol creado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando rol");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al crear rol"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseModel>> UpdateRole(int id, [FromBody] RoleModel role)
        {
            if (id != role.ID)
            {
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "ID no coincide"
                });
            }

            try
            {
                await _roleService.UpdateRoleAsync(role);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Rol actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando rol");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al actualizar rol"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseModel>> DeleteRole(int id)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(id);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Rol no encontrado"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Rol eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando rol");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al eliminar rol"
                });
            }
        }
    }
}
