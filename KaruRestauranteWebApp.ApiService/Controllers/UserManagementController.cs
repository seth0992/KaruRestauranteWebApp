using KaruRestauranteWebApp.BL.Services;
using KaruRestauranteWebApp.Models.Entities.Users;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Models.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaruRestauranteWebApp.ApiService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {

        private readonly IUserService _userService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(IUserService userService, ILogger<UserManagementController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Obtener todos los usuarios (Solo SuperAdmin y Admin)
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();

                return Ok(new BaseResponseModel { Success = true, Data = users });


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Internal server error while getting users"
                });
            }
        }

        // GET: api/UserManagement/{id}
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Usuario no encontrado"
                    });
                }

                return Ok(new BaseResponseModel { Success = true, Data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener el usuario"
                });
            }
        }


        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> CreateUser([FromBody] CreateUserDTO createUserDto)
        {
            try
            {
                var user = new UserModel
                {
                    Username = createUserDto.Username,
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    Email = createUserDto.Email,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UserRoles = createUserDto.RoleIds.Select(roleId => new UserRolModel
                    {
                        RoleID = roleId
                    }).ToList()
                };

                var createdUser = await _userService.CreateUser(user, createUserDto.Password);

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = createdUser,
                    ErrorMessage = "Usuario creado exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al crear el usuario"
                });
            }
        }

        // PUT: api/UserManagement/{id}
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseModel>> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDto)
        {
            try
            {
                if (id != updateUserDto.ID)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "El ID del usuario no coincide con el ID de la ruta"
                    });
                }

                var existingUser = await _userService.GetUserById(id);
                if (existingUser == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Usuario no encontrado"
                    });
                }

                // Verificar que el username no haya cambiado
                if (existingUser.Username != updateUserDto.Username)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "No se permite modificar el nombre de usuario"
                    });
                }

                existingUser.FirstName = updateUserDto.FirstName;
                existingUser.LastName = updateUserDto.LastName;
                existingUser.Email = updateUserDto.Email;
                existingUser.IsActive = updateUserDto.IsActive;

                await _userService.UpdateUser(existingUser);
                await _userService.UpdateUserRoles(id, updateUserDto.RoleIds);

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Usuario actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al actualizar el usuario"
                });
            }
        }

        [Authorize]
        [HttpPut("{id}/changepassword")]
        public async Task<ActionResult<BaseResponseModel>> ChangePassword(int id, [FromBody] ChangePasswordModel model)
        {
            try
            {
                var result = await _userService.ChangePassword(id, model.CurrentPassword, model.NewPassword);
                if (!result)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Contraseña actual incorrecta"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Contraseña actualizada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al cambiar la contraseña"
                });
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseModel>> DeactivateUser(int id)
        {
            try
            {
                var result = await _userService.DeactivateUser(id);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Usuario no encontrado"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Usuario desactivado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al desactivar el usuario"
                });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPatch("{id}/reactivate")]
        public async Task<ActionResult<BaseResponseModel>> ReactivateUser(int id)
        {
            try
            {
                var result = await _userService.ReactivateUser(id);

                if (!result)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "El usuario no existe o ya está activo"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Usuario reactivado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivando usuario {UserId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al reactivar el usuario"
                });
            }
        }
    }

}
