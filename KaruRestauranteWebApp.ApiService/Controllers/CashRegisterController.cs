using KaruRestauranteWebApp.BL.Services;
using KaruRestauranteWebApp.Models.Models.CashRegister;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Newtonsoft.Json;

namespace KaruRestauranteWebApp.ApiService.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class CashRegisterController : ControllerBase
    {
        private readonly ICashRegisterSessionService _sessionService;
        private readonly ICashRegisterTransactionService _transactionService;
        private readonly ILogger<CashRegisterController> _logger;

        public CashRegisterController(
            ICashRegisterSessionService sessionService,
            ICashRegisterTransactionService transactionService,
            ILogger<CashRegisterController> logger)
        {
            _sessionService = sessionService;
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet("sessions")]
        public async Task<ActionResult<BaseResponseModel>> GetAllSessions()
        {
            try
            {
                var sessions = await _sessionService.GetAllSessionsAsync();
                return Ok(new BaseResponseModel { Success = true, Data = sessions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones de caja");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener sesiones de caja"
                });
            }
        }

        [HttpGet("sessions/current")]
        public async Task<ActionResult<BaseResponseModel>> GetCurrentSession()
        {
            try
            {
                var session = await _sessionService.GetCurrentSessionAsync();
                if (session == null)
                {
                    return Ok(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "No hay sesión de caja abierta",
                        Data = null
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = session });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la sesión de caja actual");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener la sesión de caja actual"
                });
            }
        }

        [HttpGet("sessions/{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetSessionById(int id)
        {
            try
            {
                var session = await _sessionService.GetSessionByIdAsync(id);
                if (session == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Sesión de caja no encontrada"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = session });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la sesión de caja {SessionId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener la sesión de caja"
                });
            }
        }

        [HttpPost("sessions/open")]
        public async Task<ActionResult<BaseResponseModel>> OpenSession([FromBody] CashRegisterSessionDTO sessionDto)
        {
            try
            {
                // Obtener ID del usuario del token
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Usuario no autorizado"
                    });
                }

                var session = await _sessionService.OpenSessionAsync(sessionDto, userId);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = session,
                    ErrorMessage = "Sesión de caja abierta exitosamente"
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al abrir sesión de caja");
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir sesión de caja");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al abrir sesión de caja"
                });
            }
        }

        [HttpPost("sessions/{id}/close")]
        public async Task<ActionResult<BaseResponseModel>> CloseSession(int id, [FromBody] CashRegisterSessionDTO sessionDto)
        {
            try
            {
                // Obtener ID del usuario del token
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Usuario no autorizado"
                    });
                }

                var result = await _sessionService.CloseSessionAsync(id, sessionDto, userId);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Sesión de caja no encontrada o ya está cerrada"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Sesión de caja cerrada exitosamente"
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al cerrar sesión de caja");
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión de caja {SessionId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al cerrar sesión de caja"
                });
            }
        }

        [HttpGet("transactions/session/{sessionId}")]
        public async Task<ActionResult<BaseResponseModel>> GetTransactionsBySession(int sessionId)
        {
            try
            {
                var transactions = await _transactionService.GetTransactionsBySessionIdAsync(sessionId);

                // Asegúrate de que transactions no sea null
                if (transactions == null)
                {
                    transactions = new List<CashRegisterTransactionDTO>();
                }

                // Usa JSON.NET para verificar el formato
                var jsonString = JsonConvert.SerializeObject(transactions);
                Console.WriteLine($"Sending transactions JSON: {jsonString}");

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones para la sesión {SessionId}", sessionId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = $"Error interno del servidor al obtener transacciones: {ex.Message}"
                });
            }
        }

        [HttpGet("transactions/{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetTransactionById(int id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Transacción no encontrada"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = transaction });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la transacción {TransactionId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener la transacción"
                });
            }
        }

        [HttpPost("transactions")]
        public async Task<ActionResult<BaseResponseModel>> CreateTransaction([FromBody] CashRegisterTransactionDTO transactionDto)
        {
            try
            {
                // Obtener ID del usuario del token
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Usuario no autorizado"
                    });
                }

                var transaction = await _transactionService.CreateTransactionAsync(transactionDto, userId);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = transaction,
                    ErrorMessage = "Transacción registrada exitosamente"
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear transacción");
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear transacción");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al crear transacción"
                });
            }
        }

        [HttpDelete("transactions/{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> DeleteTransaction(int id)
        {
            try
            {
                var result = await _transactionService.DeleteTransactionAsync(id);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Transacción no encontrada"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Transacción eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la transacción {TransactionId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al eliminar la transacción"
                });
            }
        }

        [HttpGet("transactions/by-date")]
        public async Task<ActionResult<BaseResponseModel>> GetTransactionsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);
                return Ok(new BaseResponseModel { Success = true, Data = transactions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones por rango de fechas");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener transacciones"
                });
            }
        }

        [HttpGet("balance")]
        public async Task<ActionResult<BaseResponseModel>> GetCurrentBalance()
        {
            try
            {
                decimal balanceCRC = await _sessionService.GetCurrentBalanceCRCAsync();
                decimal balanceUSD = await _sessionService.GetCurrentBalanceUSDAsync();

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = new { BalanceCRC = balanceCRC, BalanceUSD = balanceUSD }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el balance actual");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener el balance"
                });
            }
        }
    }
}
