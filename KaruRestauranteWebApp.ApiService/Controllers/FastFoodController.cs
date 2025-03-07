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
    public class FastFoodController : ControllerBase
    {
        private readonly IFastFoodService _fastFoodService;
        private readonly ILogger<FastFoodController> _logger;

        public FastFoodController(
            IFastFoodService fastFoodService,
            ILogger<FastFoodController> logger)
        {
            _fastFoodService = fastFoodService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetProducts([FromQuery] bool includeInactive = false)
        {
            try
            {
                var products = await _fastFoodService.GetAllProductsAsync(includeInactive);
                return Ok(new BaseResponseModel { Success = true, Data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetProduct(int id)
        {
            try
            {
                var product = await _fastFoodService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Producto no encontrado"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = product });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el producto {ProductId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> CreateProduct([FromBody] FastFoodItemDTO productDto)
        {
            try
            {
                // Validaciones básicas
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Datos de producto inválidos"
                    });
                }

                var product = await _fastFoodService.CreateProductAsync(productDto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = product,
                    ErrorMessage = "Producto creado exitosamente"
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear producto");
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el producto");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> UpdateProduct(int id, [FromBody] FastFoodItemDTO productDto)
        {
            try
            {
                if (id != productDto.ID)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "ID no coincide"
                    });
                }

                await _fastFoodService.UpdateProductAsync(id, productDto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Producto actualizado exitosamente"
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
                _logger.LogError(ex, "Error al actualizar el producto {ProductId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> DeleteProduct(int id)
        {
            try
            {
                var result = await _fastFoodService.DeleteProductAsync(id);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Producto no encontrado"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Producto eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el producto {ProductId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<BaseResponseModel>> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _fastFoodService.GetProductsByCategoryAsync(categoryId);
                return Ok(new BaseResponseModel { Success = true, Data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoría {CategoryId}", categoryId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }
    }
}
