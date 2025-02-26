using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Models.Orders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface IPaymentService
    {
        Task<List<PaymentModel>> GetPaymentsByOrderIdAsync(int orderId);
        Task<PaymentModel?> GetPaymentByIdAsync(int id);
        Task<PaymentModel> RegisterPaymentAsync(PaymentDTO paymentDto, int userId);
        Task<bool> DeletePaymentAsync(int id);
        Task<decimal> GetTotalPaidForOrderAsync(int orderId);
        Task UpdateOrderPaymentStatusAsync(int orderId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<List<PaymentModel>> GetPaymentsByOrderIdAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Obteniendo pagos para la orden: {OrderId}", orderId);
                return await _paymentRepository.GetByOrderIdAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos para la orden: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<PaymentModel?> GetPaymentByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando pago con ID: {PaymentId}", id);
                return await _paymentRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el pago con ID: {PaymentId}", id);
                throw;
            }
        }

        public async Task<PaymentModel> RegisterPaymentAsync(PaymentDTO paymentDto, int userId)
        {
            try
            {
                // Validar datos
                if (paymentDto.Amount <= 0)
                {
                    throw new ValidationException("El monto debe ser mayor a 0");
                }

                if (string.IsNullOrWhiteSpace(paymentDto.PaymentMethod))
                {
                    throw new ValidationException("El método de pago es requerido");
                }

                // Validar que el método sea válido
                var validMethods = new[] { "Cash", "CreditCard", "DebitCard", "Transfer", "Other" };
                if (!validMethods.Contains(paymentDto.PaymentMethod))
                {
                    throw new ValidationException($"Método de pago no válido. Valores posibles: {string.Join(", ", validMethods)}");
                }

                // Verificar que la orden exista
                var order = await _orderRepository.GetByIdAsync(paymentDto.OrderID);
                if (order == null)
                {
                    throw new ValidationException($"No se encontró la orden con ID: {paymentDto.OrderID}");
                }

                // No permitir pagos para órdenes canceladas
                if (order.OrderStatus == "Cancelled")
                {
                    throw new ValidationException("No se pueden registrar pagos para órdenes canceladas");
                }

                // Crear el modelo de pago
                var payment = new PaymentModel
                {
                    OrderID = paymentDto.OrderID,
                    PaymentMethod = paymentDto.PaymentMethod,
                    Amount = paymentDto.Amount,
                    ReferenceNumber = paymentDto.ReferenceNumber,
                    PaymentDate = DateTime.UtcNow,
                    ProcessedBy = userId,
                    Notes = paymentDto.Notes
                };

                // Guardar el pago
                await _paymentRepository.CreateAsync(payment);

                // Actualizar el estado de pago de la orden
                await UpdateOrderPaymentStatusAsync(paymentDto.OrderID);

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago para la orden: {OrderId}", paymentDto.OrderID);
                throw;
            }
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            try
            {
                // Obtener el pago para conocer la orden asociada
                var payment = await _paymentRepository.GetByIdAsync(id);
                if (payment == null)
                {
                    throw new ValidationException($"No se encontró el pago con ID: {id}");
                }

                // Eliminar el pago
                var result = await _paymentRepository.DeleteAsync(id);

                // Actualizar el estado de pago de la orden
                await UpdateOrderPaymentStatusAsync(payment.OrderID);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el pago: {PaymentId}", id);
                throw;
            }
        }
        public async Task<decimal> GetTotalPaidForOrderAsync(int orderId)
        {
            try
            {
                return await _paymentRepository.GetTotalPaidForOrderAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el total pagado para la orden: {OrderId}", orderId);
                throw;
            }
        }

        public async Task UpdateOrderPaymentStatusAsync(int orderId)
        {
            try
            {
                // Obtener la orden
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new ValidationException($"No se encontró la orden con ID: {orderId}");
                }

                // Calcular el total pagado
                decimal totalPaid = await _paymentRepository.GetTotalPaidForOrderAsync(orderId);

                // Determinar el estado de pago
                string paymentStatus;

                if (totalPaid >= order.TotalAmount)
                {
                    paymentStatus = "Paid";
                }
                else if (totalPaid > 0)
                {
                    paymentStatus = "Partially Paid";
                }
                else
                {
                    paymentStatus = "Pending";
                }

                // Actualizar el estado de pago
                await _orderRepository.UpdatePaymentStatusAsync(orderId, paymentStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el estado de pago de la orden: {OrderId}", orderId);
                throw;
            }
        }
    }
}
