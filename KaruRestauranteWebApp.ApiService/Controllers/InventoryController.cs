using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KaruRestauranteWebApp.BL.Services;

namespace KaruRestauranteWebApp.ApiService.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpGet("ingredients")]
        public async Task<ActionResult<BaseResponseModel>> GetIngredients()
        {
            try
            {
                var ingredients = await _inventoryService.GetAllIngredientsAsync();
                return Ok(new BaseResponseModel { Success = true, Data = ingredients });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ingredientes");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<BaseResponseModel>> GetTransactions([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var transactions = await _inventoryService.GetTransactionsAsync(fromDate, toDate);
                return Ok(new BaseResponseModel { Success = true, Data = transactions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo transacciones");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPost("ingredients")]
        public async Task<ActionResult<BaseResponseModel>> CreateIngredient([FromBody] IngredientModel ingredient)
        {
            try
            {
                var result = await _inventoryService.CreateIngredientAsync(ingredient);
                return Ok(new BaseResponseModel { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando ingrediente");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }


        [HttpPost("transactions")]
        public async Task<ActionResult<BaseResponseModel>> RegisterTransaction([FromBody] InventoryTransactionDTO transactionDto)
        {
            try
            {
                // Obtener el ID del usuario del token
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var transaction = new InventoryTransactionModel
                {
                    IngredientID = transactionDto.IngredientID,
                    TransactionType = transactionDto.TransactionType,
                    Quantity = transactionDto.Quantity,
                    UnitPrice = transactionDto.UnitPrice,
                    Notes = transactionDto.Notes,
                    TransactionDate = transactionDto.TransactionDate,
                    UserID = userId
                };

                var result = await _inventoryService.RegisterTransactionAsync(transaction);
                return Ok(new BaseResponseModel { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando transacción");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<BaseResponseModel>> GetLowStockItems()
        {
            try
            {
                var items = await _inventoryService.GetLowStockItemsAsync();
                return Ok(new BaseResponseModel { Success = true, Data = items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo items con bajo stock");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("ingredients/{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetIngredient(int id)
        {
            try
            {
                var ingredient = await _inventoryService.GetIngredientByIdAsync(id);

                if (ingredient == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Ingrediente no encontrado"
                    });
                }

                return Ok(new BaseResponseModel { Success = true, Data = ingredient });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ingrediente {IngredientId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPut("ingredients/{id}")]
        public async Task<ActionResult<BaseResponseModel>> UpdateIngredient(int id, [FromBody] IngredientModel ingredient)
        {
            try
            {
                if (id != ingredient.ID)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "ID no coincide"
                    });
                }

                await _inventoryService.UpdateIngredientAsync(ingredient);
                return Ok(new BaseResponseModel { Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando ingrediente {IngredientId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }
    }
}

