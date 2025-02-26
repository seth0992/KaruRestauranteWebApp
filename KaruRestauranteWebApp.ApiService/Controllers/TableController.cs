using KaruRestauranteWebApp.BL.Services;
using KaruRestauranteWebApp.Models.Models.Orders;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.ApiService.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly ILogger<TableController> _logger;

        public TableController(
            ITableService tableService,
            ILogger<TableController> logger)
        {
            _tableService = tableService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetAll([FromQuery] bool includeInactive = false)
        {
            try
            {
                var tables = await _tableService.GetAllTablesAsync(includeInactive);
                return Ok(new BaseResponseModel { Success = true, Data = tables });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mesas");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener mesas"
                });
            }
        }

        [HttpGet("available")]
        public async Task<ActionResult<BaseResponseModel>> GetAvailable()
        {
            try
            {
                var tables = await _tableService.GetAvailableTablesAsync();
                return Ok(new BaseResponseModel { Success = true, Data = tables });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mesas disponibles");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener mesas disponibles"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetById(int id)
        {
            try
            {
                var table = await _tableService.GetTableByIdAsync(id);
                if (table == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Mesa no encontrada"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = table });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la mesa {TableId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener la mesa"
                });
            }
        }

        [HttpGet("number/{tableNumber}")]
        public async Task<ActionResult<BaseResponseModel>> GetByNumber(int tableNumber)
        {
            try
            {
                var table = await _tableService.GetTableByNumberAsync(tableNumber);
                if (table == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Mesa no encontrada"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = table });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la mesa número {TableNumber}", tableNumber);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener la mesa"
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> Create([FromBody] TableDTO tableDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Datos de mesa inválidos"
                    });
                }

                var table = await _tableService.CreateTableAsync(tableDto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = table,
                    ErrorMessage = "Mesa creada exitosamente"
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
                _logger.LogError(ex, "Error al crear mesa");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al crear mesa"
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> Update(int id, [FromBody] TableDTO tableDto)
        {
            try
            {
                if (id != tableDto.ID)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "ID no coincide"
                    });
                }

                await _tableService.UpdateTableAsync(tableDto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Mesa actualizada exitosamente"
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
                _logger.LogError(ex, "Error al actualizar mesa {TableId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al actualizar mesa"
                });
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<ActionResult<BaseResponseModel>> UpdateStatus(int id, [FromBody] string status)
        {
            try
            {
                var result = await _tableService.UpdateTableStatusAsync(id, status);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Mesa no encontrada"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Estado de mesa actualizado exitosamente"
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
                _logger.LogError(ex, "Error al actualizar estado de mesa {TableId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al actualizar estado de mesa"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> Delete(int id)
        {
            try
            {
                var result = await _tableService.DeleteTableAsync(id);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Mesa no encontrada"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Mesa eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar mesa {TableId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al eliminar mesa"
                });
            }
        }
    }
}
