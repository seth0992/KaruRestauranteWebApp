using KaruRestauranteWebApp.BL.Services;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KaruRestauranteWebApp.ApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController : ControllerBase
    {
        private readonly IProductTypeService _productTypeService;
        private readonly ILogger<ProductTypeController> _logger;

        public ProductTypeController(
            IProductTypeService productTypeService,
            ILogger<ProductTypeController> logger)
        {
            _productTypeService = productTypeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetProductTypes()
        {
            try
            {
                var productTypes = await _productTypeService.GetAllProductTypesAsync();
                return Ok(new BaseResponseModel { Success = true, Data = productTypes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de producto");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetProductType(int id)
        {
            try
            {
                var productType = await _productTypeService.GetProductTypeByIdAsync(id);
                if (productType == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Tipo de producto no encontrado"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = productType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el tipo de producto {ProductTypeId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }
    }
}
