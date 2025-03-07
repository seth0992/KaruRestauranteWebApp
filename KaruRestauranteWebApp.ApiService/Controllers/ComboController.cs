using KaruRestauranteWebApp.BL.Services;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.ApiService.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class ComboController : ControllerBase
    {
        private readonly IComboService _comboService;
        private readonly ILogger<ComboController> _logger;

        public ComboController(
            IComboService comboService,
            ILogger<ComboController> logger)
        {
            _comboService = comboService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetCombos([FromQuery] bool includeInactive = false)
        {
            try
            {
                var combos = await _comboService.GetAllCombosAsync(includeInactive);
                return Ok(new BaseResponseModel { Success = true, Data = combos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener combos");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetCombo(int id)
        {
            try
            {
                var combo = await _comboService.GetComboByIdAsync(id);
                if (combo == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Combo no encontrado"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = combo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el combo {ComboId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> CreateCombo([FromBody] ComboDTO comboDto)
        {
            try
            {
                // Validaciones básicas
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Datos de combo inválidos"
                    });
                }

                var combo = await _comboService.CreateComboAsync(comboDto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = combo,
                    ErrorMessage = "Combo creado exitosamente"
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear combo");
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el combo");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> UpdateCombo(int id, [FromBody] ComboDTO comboDto)
        {
            try
            {
                if (id != comboDto.ID)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "ID no coincide"
                    });
                }

                await _comboService.UpdateComboAsync(id, comboDto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Combo actualizado exitosamente"
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el combo {ComboId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> DeleteCombo(int id)
        {
            try
            {
                var result = await _comboService.DeleteComboAsync(id);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Combo no encontrado"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Combo eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el combo {ComboId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }
    }
}
