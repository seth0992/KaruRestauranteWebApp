using KaruRestauranteWebApp.BL.Services;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Models.Models.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace KaruRestauranteWebApp.ApiService.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IElectronicInvoiceService _invoiceService;
        private readonly ITableService _tableService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            IPaymentService paymentService,
            IElectronicInvoiceService invoiceService,
            ITableService tableService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            _tableService = tableService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetOrders(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync(fromDate, toDate);

                // Filtrar por estado si se proporciona
                if (!string.IsNullOrEmpty(status))
                {
                    orders = orders.Where(o => o.OrderStatus == status).ToList();
                }

                return Ok(new BaseResponseModel { Success = true, Data = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<ActionResult<BaseResponseModel>> UpdateOrder(int id, [FromBody] OrderDTO orderDto)
        {
            try
            {
                if (id != orderDto.ID)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "ID no coincide"
                    });
                }

                var order = await _orderService.UpdateOrderAsync(orderDto);

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = order,
                    ErrorMessage = "Orden actualizada exitosamente"
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
                _logger.LogError(ex, "Error al actualizar la orden {OrderId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> DeleteOrder(int id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Orden no encontrada"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Orden eliminada exitosamente"
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
                _logger.LogError(ex, "Error al eliminar la orden {OrderId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetOrder(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Orden no encontrada"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = order });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la orden {OrderId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("number/{orderNumber}")]
        public async Task<ActionResult<BaseResponseModel>> GetOrderByNumber(string orderNumber)
        {
            try
            {
                var order = await _orderService.GetOrderByNumberAsync(orderNumber);
                if (order == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Orden no encontrada"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = order });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la orden {OrderNumber}", orderNumber);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<BaseResponseModel>> GetOrdersByStatus(string status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status);
                return Ok(new BaseResponseModel { Success = true, Data = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes con estado {Status}", status);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("table/{tableId}")]
        public async Task<ActionResult<BaseResponseModel>> GetOrdersByTable(int tableId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByTableAsync(tableId);
                return Ok(new BaseResponseModel { Success = true, Data = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes para la mesa {TableId}", tableId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<BaseResponseModel>> GetOrdersByCustomer(int customerId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
                return Ok(new BaseResponseModel { Success = true, Data = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes para el cliente {CustomerId}", customerId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> CreateOrder([FromBody] OrderDTO orderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Datos de orden inválidos"
                    });
                }

                // Obtener el ID del usuario del token
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Usuario no autorizado"
                    });
                }

                var order = await _orderService.CreateOrderAsync(orderDto, userId);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = order,
                    ErrorMessage = "Orden creada exitosamente"
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear orden");
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la orden");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }



        [HttpPatch("{id}/status/{status}")]
        public async Task<ActionResult<BaseResponseModel>> UpdateOrderStatus(int id, string status)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(id, status);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Orden no encontrada"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Estado de la orden actualizado exitosamente"
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
                _logger.LogError(ex, "Error al actualizar el estado de la orden {OrderId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPost("{orderId}/details")]
        public async Task<ActionResult<BaseResponseModel>> AddOrderDetail(int orderId, [FromBody] OrderDetailDTO detailDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Datos de detalle inválidos"
                    });
                }

                var orderDetail = await _orderService.AddOrderDetailAsync(orderId, detailDto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = orderDetail,
                    ErrorMessage = "Detalle agregado exitosamente"
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
                _logger.LogError(ex, "Error al agregar detalle a la orden {OrderId}", orderId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPatch("details/{detailId}/status/{status}")]
        public async Task<ActionResult<BaseResponseModel>> UpdateOrderDetailStatus(int detailId, string status)
        {
            try
            {
                var result = await _orderService.UpdateOrderDetailStatusAsync(detailId, status);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Detalle de orden no encontrado"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Estado del detalle actualizado exitosamente"
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
                _logger.LogError(ex, "Error al actualizar el estado del detalle {DetailId}", detailId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpDelete("details/{detailId}")]
        public async Task<ActionResult<BaseResponseModel>> RemoveOrderDetail(int detailId)
        {
            try
            {
                var result = await _orderService.RemoveOrderDetailAsync(detailId);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Detalle de orden no encontrado"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Detalle eliminado exitosamente"
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
                _logger.LogError(ex, "Error al eliminar el detalle {DetailId}", detailId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPost("{orderId}/payments")]
        public async Task<ActionResult<BaseResponseModel>> RegisterPayment(int orderId, [FromBody] PaymentDTO paymentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Datos de pago inválidos"
                    });
                }

                if (orderId != paymentDto.OrderID)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "ID de orden no coincide"
                    });
                }

                // Obtener el ID del usuario del token
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Usuario no autorizado"
                    });
                }

                var payment = await _paymentService.RegisterPaymentAsync(paymentDto, userId);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = payment,
                    ErrorMessage = "Pago registrado exitosamente"
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
                _logger.LogError(ex, "Error al registrar pago para la orden {OrderId}", orderId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("{orderId}/payments")]
        public async Task<ActionResult<BaseResponseModel>> GetPayments(int orderId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
                return Ok(new BaseResponseModel { Success = true, Data = payments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos para la orden {OrderId}", orderId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpDelete("payments/{paymentId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> DeletePayment(int paymentId)
        {
            try
            {
                var result = await _paymentService.DeletePaymentAsync(paymentId);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Pago no encontrado"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Pago eliminado exitosamente"
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
                _logger.LogError(ex, "Error al eliminar el pago {PaymentId}", paymentId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPost("{orderId}/invoice")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> GenerateInvoice(int orderId, [FromBody] ElectronicInvoiceDTO invoiceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Datos de factura inválidos"
                    });
                }

                if (orderId != invoiceDto.OrderID)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "ID de orden no coincide"
                    });
                }

                var invoice = await _invoiceService.GenerateInvoiceAsync(invoiceDto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = invoice,
                    ErrorMessage = "Factura generada exitosamente"
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
                _logger.LogError(ex, "Error al generar factura para la orden {OrderId}", orderId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("{orderId}/invoice")]
        public async Task<ActionResult<BaseResponseModel>> GetInvoice(int orderId)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByOrderIdAsync(orderId);
                if (invoice == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Factura no encontrada"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = invoice });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener factura para la orden {OrderId}", orderId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPost("{orderId}/invoice/send")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> SendInvoiceToHacienda(int orderId)
        {
            try
            {
                var result = await _invoiceService.SendInvoiceToHaciendaAsync(orderId);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = result,
                    ErrorMessage = "Factura enviada exitosamente"
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
                _logger.LogError(ex, "Error al enviar factura a Hacienda para la orden {OrderId}", orderId);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPatch("{id}/payment-status/{status}")]
        public async Task<ActionResult<BaseResponseModel>> UpdateOrderPaymentStatus(int id, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "El estado de pago es requerido"
                    });
                }

                // Validar que el estado sea válido
                var validStatuses = new[] { "Pending", "Partially Paid", "Paid", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = $"Estado de pago no válido. Valores posibles: {string.Join(", ", validStatuses)}"
                    });
                }

                var result = await _orderService.UpdatePaymentStatusAsync(id, status);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Orden no encontrada"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = $"Estado de pago de la orden actualizado a {status}"
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
                _logger.LogError(ex, "Error al actualizar el estado de pago de la orden {OrderId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpGet("check-reference")]
        public async Task<ActionResult<BaseResponseModel>> CheckReferenceNumber(
        [FromQuery] string reference, [FromQuery] string method)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reference) || string.IsNullOrWhiteSpace(method))
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Se requiere número de referencia y método de pago"
                    });
                }

                var result = await _paymentService.CheckReferenceNumberAsync(reference, method);

                if (result.Exists)
                {
                    return new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = $"El número de referencia ya existe para un pago con {GetPaymentMethodName(method)} realizado el {result.PaymentDate?.ToString("dd/MM/yyyy HH:mm") ?? "fecha desconocida"}",
                        Data = new { Exists = true, PaymentDate = result.PaymentDate }
                    };
                }

                return new BaseResponseModel
                {
                    Success = true,
                    Data = new { Exists = false }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el número de referencia {Reference}", reference);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al verificar el número de referencia"
                });
            }
        }

        [HttpPost("{id}/cancel-register-in-cash")]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<ActionResult<BaseResponseModel>> RegisterCancellationInCash(int id)
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

                await _orderService.RegisterCancellationInCashRegister(id, userId);

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Cancelación registrada en caja exitosamente"
                });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = vex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar cancelación en caja para la orden {OrderId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        // Método auxiliar para obtener nombres amigables de métodos de pago
        private string GetPaymentMethodName(string method)
        {
            return method switch
            {
                "Cash" => "Efectivo",
                "CreditCard" => "Tarjeta de Crédito",
                "DebitCard" => "Tarjeta de Débito",
                "Transfer" => "Transferencia",
                "SINPE" => "SINPE Móvil",
                "Other" => "Otro método",
                _ => method
            };
        }
    }
}
