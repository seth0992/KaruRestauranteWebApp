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
    public interface IOrderService
    {
        Task<List<OrderModel>> GetAllOrdersAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<OrderModel?> GetOrderByIdAsync(int id);
        Task<OrderModel?> GetOrderByNumberAsync(string orderNumber);
        Task<List<OrderModel>> GetOrdersByStatusAsync(string status);
        Task<List<OrderModel>> GetOrdersByTableAsync(int tableId);
        Task<List<OrderModel>> GetOrdersByCustomerAsync(int customerId);
        Task<OrderModel> CreateOrderAsync(OrderDTO orderDto, int userId);
        Task UpdateOrderAsync(OrderDTO orderDto);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus);
        Task<string> GenerateOrderNumberAsync();
        Task<OrderDetailModel> AddOrderDetailAsync(int orderId, OrderDetailDTO detailDto);
        Task<bool> UpdateOrderDetailStatusAsync(int orderDetailId, string status);
        Task<bool> RemoveOrderDetailAsync(int orderDetailId);
        Task<decimal> CalculateOrderTotalAsync(int orderId);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ITableRepository _tableRepository;
        private readonly IFastFoodRepository _productRepository;
        private readonly IComboRepository _comboRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            IPaymentRepository paymentRepository,
            ICustomerRepository customerRepository,
            ITableRepository tableRepository,
            IFastFoodRepository productRepository,
            IComboRepository comboRepository,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _paymentRepository = paymentRepository;
            _customerRepository = customerRepository;
            _tableRepository = tableRepository;
            _productRepository = productRepository;
            _comboRepository = comboRepository;
            _logger = logger;
        }

        public async Task<List<OrderModel>> GetAllOrdersAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de órdenes");
                return await _orderRepository.GetAllAsync(fromDate, toDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes");
                throw;
            }
        }

        public async Task<OrderModel?> GetOrderByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando orden con ID: {OrderId}", id);
                return await _orderRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la orden con ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderModel?> GetOrderByNumberAsync(string orderNumber)
        {
            try
            {
                _logger.LogInformation("Buscando orden con número: {OrderNumber}", orderNumber);
                return await _orderRepository.GetByOrderNumberAsync(orderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la orden con número: {OrderNumber}", orderNumber);
                throw;
            }
        }

        public async Task<List<OrderModel>> GetOrdersByStatusAsync(string status)
        {
            try
            {
                _logger.LogInformation("Obteniendo órdenes con estado: {OrderStatus}", status);
                return await _orderRepository.GetByStatusAsync(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes con estado: {OrderStatus}", status);
                throw;
            }
        }

        public async Task<List<OrderModel>> GetOrdersByTableAsync(int tableId)
        {
            try
            {
                _logger.LogInformation("Obteniendo órdenes para la mesa: {TableId}", tableId);
                return await _orderRepository.GetByTableAsync(tableId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes para la mesa: {TableId}", tableId);
                throw;
            }
        }

        public async Task<List<OrderModel>> GetOrdersByCustomerAsync(int customerId)
        {
            try
            {
                _logger.LogInformation("Obteniendo órdenes para el cliente: {CustomerId}", customerId);
                return await _orderRepository.GetByCustomerAsync(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes para el cliente: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<OrderModel> CreateOrderAsync(OrderDTO orderDto, int userId)
        {
            try
            {
                // Validar tipo de orden
                if (string.IsNullOrWhiteSpace(orderDto.OrderType))
                {
                    throw new ValidationException("El tipo de orden es requerido");
                }

                // Validar que el tipo sea válido
                var validTypes = new[] { "DineIn", "TakeOut", "Delivery" };
                if (!validTypes.Contains(orderDto.OrderType))
                {
                    throw new ValidationException($"Tipo de orden no válido. Valores posibles: {string.Join(", ", validTypes)}");
                }

                // Si es tipo DineIn, validar que se haya seleccionado una mesa
                if (orderDto.OrderType == "DineIn" && !orderDto.TableID.HasValue)
                {
                    throw new ValidationException("Se requiere seleccionar una mesa para órdenes en sitio");
                }

                // Verificar mesa si está seleccionada
                TableModel? table = null;
                if (orderDto.TableID.HasValue)
                {
                    table = await _tableRepository.GetByIdAsync(orderDto.TableID.Value);
                    if (table == null)
                    {
                        throw new ValidationException($"No se encontró la mesa con ID: {orderDto.TableID.Value}");
                    }

                    // Verificar que la mesa esté disponible
                    if (table.Status != "Available" && table.Status != "Reserved")
                    {
                        throw new ValidationException($"La mesa {table.TableNumber} no está disponible");
                    }
                }

                // Verificar cliente si está seleccionado
                CustomerModel? customer = null;
                if (orderDto.CustomerID.HasValue)
                {
                    customer = await _customerRepository.GetByIdAsync(orderDto.CustomerID.Value);
                    if (customer == null)
                    {
                        throw new ValidationException($"No se encontró el cliente con ID: {orderDto.CustomerID.Value}");
                    }
                }

                // Crear la orden
                var orderNumber = await GenerateOrderNumberAsync();
                var order = new OrderModel
                {
                    OrderNumber = orderNumber,
                    CustomerID = orderDto.CustomerID,
                    TableID = orderDto.TableID,
                    UserID = userId,
                    OrderType = orderDto.OrderType,
                    OrderStatus = "Pending",
                    PaymentStatus = "Pending",
                    TotalAmount = 0, // Se calculará al agregar detalles
                    TaxAmount = 0,   // Se calculará al agregar detalles
                    DiscountAmount = orderDto.DiscountAmount,
                    Notes = orderDto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                // Guardar la orden
                await _orderRepository.CreateAsync(order);

                // Si es orden en sitio, actualizar el estado de la mesa a ocupada
                if (table != null)
                {
                    await _tableRepository.UpdateStatusAsync(table.ID, "Occupied");
                }

                // Agregar detalles a la orden si se proporcionaron
                if (orderDto.OrderDetails != null && orderDto.OrderDetails.Any())
                {
                    foreach (var detailDto in orderDto.OrderDetails)
                    {
                        await AddOrderDetailAsync(order.ID, detailDto);
                    }

                    // Calcular total de la orden
                    await CalculateOrderTotalAsync(order.ID);
                }

                // Obtener orden completa
                return await _orderRepository.GetByIdAsync(order.ID) ?? order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la orden");
                throw;
            }
        }

        public async Task UpdateOrderAsync(OrderDTO orderDto)
        {
            try
            {
                // Verificar que la orden exista
                var existingOrder = await _orderRepository.GetByIdAsync(orderDto.ID);
                if (existingOrder == null)
                {
                    throw new ValidationException($"No se encontró la orden con ID: {orderDto.ID}");
                }

                // No se permite cambiar pedidos que ya están entregados o cancelados
                if (existingOrder.OrderStatus == "Delivered" || existingOrder.OrderStatus == "Cancelled")
                {
                    throw new ValidationException($"No se puede modificar una orden {existingOrder.OrderStatus}");
                }

                // Actualizar solo algunos campos permitidos
                existingOrder.Notes = orderDto.Notes;
                existingOrder.DiscountAmount = orderDto.DiscountAmount;
                existingOrder.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(existingOrder);

                // Recalcular el total si se modificó el descuento
                await CalculateOrderTotalAsync(existingOrder.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la orden {OrderId}", orderDto.ID);
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    throw new ValidationException("El estado de la orden es requerido");
                }

                // Validar que el estado sea válido
                var validStatuses = new[] { "Pending", "InProgress", "Ready", "Delivered", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    throw new ValidationException($"Estado de orden no válido. Valores posibles: {string.Join(", ", validStatuses)}");
                }

                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    throw new ValidationException($"No se encontró la orden con ID: {id}");
                }

                // Si cambia a Delivered o Cancelled, liberar la mesa si hay una asignada
                if ((status == "Delivered" || status == "Cancelled") && order.TableID.HasValue)
                {
                    await _tableRepository.UpdateStatusAsync(order.TableID.Value, "Available");
                }

                // Si cambia a Cancelled y ya está pagada o parcialmente pagada, se necesita lógica adicional para devoluciones
                if (status == "Cancelled" && (order.PaymentStatus == "Paid" || order.PaymentStatus == "Partially Paid"))
                {
                    _logger.LogWarning("La orden {OrderId} ha sido cancelada pero ya tiene pagos registrados", id);
                }

                return await _orderRepository.UpdateStatusAsync(id, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el estado de la orden {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentStatus))
                {
                    throw new ValidationException("El estado de pago es requerido");
                }

                // Validar que el estado sea válido
                var validStatuses = new[] { "Pending", "Paid", "Partially Paid", "Cancelled" };
                if (!validStatuses.Contains(paymentStatus))
                {
                    throw new ValidationException($"Estado de pago no válido. Valores posibles: {string.Join(", ", validStatuses)}");
                }

                return await _orderRepository.UpdatePaymentStatusAsync(id, paymentStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el estado de pago de la orden {OrderId}", id);
                throw;
            }
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            return await _orderRepository.GenerateOrderNumberAsync();
        }

        public async Task<OrderDetailModel> AddOrderDetailAsync(int orderId, OrderDetailDTO detailDto)
        {
            try
            {
                // Validar tipo de ítem
                if (string.IsNullOrWhiteSpace(detailDto.ItemType))
                {
                    throw new ValidationException("El tipo de ítem es requerido");
                }

                // Validar que el tipo sea válido
                var validTypes = new[] { "Product", "Combo" };
                if (!validTypes.Contains(detailDto.ItemType))
                {
                    throw new ValidationException($"Tipo de ítem no válido. Valores posibles: {string.Join(", ", validTypes)}");
                }

                // Validar cantidad
                if (detailDto.Quantity <= 0)
                {
                    throw new ValidationException("La cantidad debe ser mayor a 0");
                }

                // Verificar que la orden exista
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new ValidationException($"No se encontró la orden con ID: {orderId}");
                }

                // No se permite agregar detalles a pedidos entregados o cancelados
                if (order.OrderStatus == "Delivered" || order.OrderStatus == "Cancelled")
                {
                    throw new ValidationException($"No se puede modificar una orden {order.OrderStatus}");
                }

                decimal unitPrice = 0;
                string? itemName = null;

                // Verificar que el ítem exista según su tipo
                if (detailDto.ItemType == "Product")
                {
                    var product = await _productRepository.GetByIdAsync(detailDto.ItemID);
                    if (product == null)
                    {
                        throw new ValidationException($"No se encontró el producto con ID: {detailDto.ItemID}");
                    }

                    unitPrice = product.SellingPrice;
                    itemName = product.Name;
                }
                else if (detailDto.ItemType == "Combo")
                {
                    var combo = await _comboRepository.GetByIdAsync(detailDto.ItemID);
                    if (combo == null)
                    {
                        throw new ValidationException($"No se encontró el combo con ID: {detailDto.ItemID}");
                    }

                    unitPrice = combo.SellingPrice;
                    itemName = combo.Name;
                }

                // Calcular subtotal
                decimal subtotal = unitPrice * detailDto.Quantity;

                // Crear el detalle de orden
                var orderDetail = new OrderDetailModel
                {
                    OrderID = orderId,
                    ItemType = detailDto.ItemType,
                    ItemID = detailDto.ItemID,
                    Quantity = detailDto.Quantity,
                    UnitPrice = unitPrice,
                    SubTotal = subtotal,
                    Notes = detailDto.Notes,
                    Status = "Pending"
                };

                // Guardar el detalle
                var createdDetail = await _orderDetailRepository.CreateAsync(orderDetail);

                // Procesar personalizaciones si hay
                if (detailDto.Customizations != null && detailDto.Customizations.Any())
                {
                    foreach (var customizationDto in detailDto.Customizations)
                    {
                        var customization = new OrderItemCustomizationModel
                        {
                            OrderDetailID = createdDetail.ID,
                            IngredientID = customizationDto.IngredientID,
                            CustomizationType = customizationDto.CustomizationType,
                            Quantity = customizationDto.Quantity,
                            ExtraCharge = customizationDto.ExtraCharge
                        };

                        createdDetail.Customizations.Add(customization);
                    }

                    await _orderDetailRepository.UpdateAsync(createdDetail);
                }

                // Recalcular total de la orden
                await CalculateOrderTotalAsync(orderId);

                return createdDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar detalle a la orden {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> UpdateOrderDetailStatusAsync(int orderDetailId, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    throw new ValidationException("El estado del detalle es requerido");
                }

                // Validar que el estado sea válido
                var validStatuses = new[] { "Pending", "InPreparation", "Ready", "Delivered", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    throw new ValidationException($"Estado de detalle no válido. Valores posibles: {string.Join(", ", validStatuses)}");
                }

                return await _orderDetailRepository.UpdateStatusAsync(orderDetailId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el estado del detalle {OrderDetailId}", orderDetailId);
                throw;
            }
        }

        public async Task<bool> RemoveOrderDetailAsync(int orderDetailId)
        {
            try
            {
                // Verificar que el detalle exista
                var detail = await _orderDetailRepository.GetByIdAsync(orderDetailId);
                if (detail == null)
                {
                    throw new ValidationException($"No se encontró el detalle con ID: {orderDetailId}");
                }

                // Verificar que la orden no esté entregada o cancelada
                var order = await _orderRepository.GetByIdAsync(detail.OrderID);
                if (order == null)
                {
                    throw new ValidationException($"No se encontró la orden para el detalle: {orderDetailId}");
                }

                if (order.OrderStatus == "Delivered" || order.OrderStatus == "Cancelled")
                {
                    throw new ValidationException($"No se puede modificar una orden {order.OrderStatus}");
                }

                // Eliminar el detalle
                var result = await _orderDetailRepository.DeleteAsync(orderDetailId);

                // Recalcular total de la orden
                await CalculateOrderTotalAsync(detail.OrderID);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el detalle {OrderDetailId}", orderDetailId);
                throw;
            }
        }

        public async Task<decimal> CalculateOrderTotalAsync(int orderId)
        {
            try
            {
                // Obtener todos los detalles de la orden
                var details = await _orderDetailRepository.GetByOrderIdAsync(orderId);

                // Calcular subtotal de todos los detalles
                decimal subtotal = details.Sum(d => d.SubTotal);

                // Calcular cargos extras por personalizaciones
                decimal extraCharges = details
                    .SelectMany(d => d.Customizations)
                    .Sum(c => c.ExtraCharge * c.Quantity);

                // Obtener la orden para el descuento
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new ValidationException($"No se encontró la orden con ID: {orderId}");
                }

                // Aplicar impuestos (13% en Costa Rica)
                decimal taxRate = 0.13m;
                decimal taxAmount = (subtotal + extraCharges) * taxRate;

                // Calcular total
                decimal total = subtotal + extraCharges + taxAmount - order.DiscountAmount;

                // Actualizar montos en la orden
                order.TotalAmount = total;
                order.TaxAmount = taxAmount;
                await _orderRepository.UpdateAsync(order);

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular el total de la orden {OrderId}", orderId);
                throw;
            }
        }
    }
}
