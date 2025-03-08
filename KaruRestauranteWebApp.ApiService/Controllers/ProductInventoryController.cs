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
    public class ProductInventoryController : ControllerBase
    {
        private readonly IProductInventoryService _inventoryService;
        private readonly ILogger<ProductInventoryController> _logger;

        public ProductInventoryController(
            IProductInventoryService inventoryService,
            ILogger<ProductInventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetAll()
        {
            try
            {
                var inventories = await _inventoryService.GetAllProductInventoryAsync();
                return Ok(new BaseResponseModel { Success = true, Data = inventories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventarios");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetById(int id)
        {
            try
            {
                var inventory = await _inventoryService.GetProductInventoryByIdAsync(id);
                if (inventory == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Inventario no encontrado"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = inventory });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el inventario {InventoryId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<BaseResponseModel>> GetByProductId(int productId)
        {
            try
            {
                var inventory = await _inventoryService.GetProductInventoryByProductIdAsync(productId);
                if (inventory == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Inventario no encontrado para este producto"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = inventory });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el inventario para el producto {ProductId}", productId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<BaseResponseModel>> GetLowStock()
        {
            try
            {
                var lowStockItems = await _inventoryService.GetLowStockAsync();
                return Ok(new BaseResponseModel { Success = true, Data = lowStockItems });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con bajo stock");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> Create([FromBody] ProductInventoryDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Datos de inventario inválidos"
                    });
                }

                var inventory = await _inventoryService.CreateProductInventoryAsync(dto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = inventory,
                    ErrorMessage = "Inventario creado exitosamente"
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear inventario");
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear inventario");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> Update(int id, [FromBody] ProductInventoryDTO dto)
        {
            try
            {
                if (id != dto.ID)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "ID no coincide"
                    });
                }

                await _inventoryService.UpdateProductInventoryAsync(dto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Inventario actualizado exitosamente"
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
                _logger.LogError(ex, "Error al actualizar inventario {InventoryId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPost("movement")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> ProcessMovement([FromBody] StockMovementDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Datos de movimiento inválidos"
                    });
                }

                var inventory = await _inventoryService.ProcessStockMovementAsync(dto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = inventory,
                    ErrorMessage = "Movimiento procesado exitosamente"
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
                _logger.LogError(ex, "Error al procesar movimiento");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<BaseResponseModel>> Delete(int id)
        {
            try
            {
                var result = await _inventoryService.DeleteProductInventoryAsync(id);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Inventario no encontrado"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Inventario eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar inventario {InventoryId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }
    }
}
