using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

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
        Task<OrderModel> UpdateOrderAsync(OrderDTO orderDto);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus);
        Task<string> GenerateOrderNumberAsync();
        Task<OrderDetailModel> AddOrderDetailAsync(int orderId, OrderDetailDTO detailDto);
        Task<bool> UpdateOrderDetailStatusAsync(int orderDetailId, string status);
        Task<bool> RemoveOrderDetailAsync(int orderDetailId);
        Task<decimal> CalculateOrderTotalAsync(int orderId);

        Task<bool> DeleteOrderAsync(int orderId);
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
        private readonly IProductInventoryRepository _productInventoryRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            IPaymentRepository paymentRepository,
            ICustomerRepository customerRepository,
            ITableRepository tableRepository,
            IFastFoodRepository productRepository,
            IComboRepository comboRepository,
            IProductInventoryRepository productInventoryRepository,
            IInventoryRepository inventoryRepository,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _paymentRepository = paymentRepository;
            _customerRepository = customerRepository;
            _tableRepository = tableRepository;
            _productRepository = productRepository;
            _comboRepository = comboRepository;
            _productInventoryRepository = productInventoryRepository;
            _inventoryRepository = inventoryRepository;
            _logger = logger;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var strategy = _orderRepository.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _orderRepository.BeginTransactionAsync();
                try
                {
                    // Verificar que la orden exista
                    var order = await _orderRepository.GetByIdAsync(orderId);
                    if (order == null)
                    {
                        throw new ValidationException($"No se encontró la orden con ID: {orderId}");
                    }

                    // Verificar si hay pagos asociados
                    if (order.Payments.Any())
                    {
                        throw new ValidationException("No se puede eliminar una orden que ya tiene pagos registrados. Considere cancelarla en su lugar.");
                    }

                    // Verificar si hay factura
                    if (order.ElectronicInvoice != null)
                    {
                        throw new ValidationException("No se puede eliminar una orden que ya tiene factura electrónica. Considere cancelarla en su lugar.");
                    }

                    // Liberar la mesa si existe
                    if (order.TableID.HasValue)
                    {
                        await _tableRepository.UpdateStatusAsync(order.TableID.Value, "Available");
                        _logger.LogInformation("Mesa {TableId} liberada al eliminar orden {OrderId}", order.TableID.Value, orderId);
                    }

                    // Revertir ajustes de inventario para cada detalle
                    foreach (var detail in order.OrderDetails)
                    {
                        await ReverseInventoryAdjustmentForDetail(detail);
                    }

                    // Eliminar detalles de la orden
                    foreach (var detail in order.OrderDetails.ToList())
                    {
                        await _orderDetailRepository.DeleteAsync(detail.ID);
                    }

                    // Eliminar la orden
                    var result = await _orderRepository.DeleteAsync(orderId);

                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al eliminar la orden {OrderId}", orderId);
                    throw;
                }
            });
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
            var strategy = _orderRepository.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Iniciar la transacción dentro del bloque de ejecución
                using var transaction = await _orderRepository.BeginTransactionAsync();
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

                    // Funcionalidad de trigger: Si es orden en sitio, actualizar el estado de la mesa a ocupada
                    if (table != null)
                    {
                        await _tableRepository.UpdateStatusAsync(table.ID, "Occupied");
                        _logger.LogInformation("Mesa {TableId} marcada como ocupada para la orden {OrderId}", table.ID, order.ID);
                    }

                    // Agregar detalles a la orden si se proporcionaron
                    if (orderDto.OrderDetails != null && orderDto.OrderDetails.Any())
                    {
                        foreach (var detailDto in orderDto.OrderDetails)
                        {
                            // Agregar detalle a la orden
                            var orderDetail = await AddOrderDetailWithoutTotal(order.ID, detailDto);

                            orderDetail.DiscountPercentage = detailDto.DiscountPercentage;
                            orderDetail.DiscountAmount = detailDto.DiscountAmount;

                            // Funcionalidad de trigger: Actualizar inventario según tipo de ítem
                            await UpdateInventoryForOrderDetail(orderDetail);
                        }

                        // Calcular total de la orden después de agregar todos los detalles
                        await CalculateOrderTotalAsync(order.ID);
                    }

                    // Funcionalidad de trigger: Registrar actividad de creación de orden
                    await LogOrderActivityAsync(order.ID, "Created", userId);

                    // Confirmar todas las operaciones
                    await transaction.CommitAsync();

                    // Obtener orden completa con todas sus relaciones
                    return await _orderRepository.GetByIdAsync(order.ID) ?? order;
                }
                catch (Exception ex)
                {
                    // Deshacer todas las operaciones en caso de error
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al crear la orden");
                    throw;
                }
            });
        }

      public async Task<OrderModel> UpdateOrderAsync(OrderDTO orderDto)
{
    var strategy = _orderRepository.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
        using var transaction = await _orderRepository.BeginTransactionAsync();
        try
        {
         

            var order = await _orderRepository.GetByIdAsync(orderDto.ID);
            if (order == null)
            {
                throw new ValidationException($"No se encontró la orden con ID: {orderDto.ID}");
            }

            // Guardar estado y mesa anteriores para comparar
            var previousStatus = order.OrderStatus;
            var previousTableId = order.TableID;

            // Actualizar datos básicos
            order.CustomerID = orderDto.CustomerID;
            order.TableID = orderDto.TableID;
            order.OrderType = orderDto.OrderType;
            order.Notes = orderDto.Notes;
            
            // IMPORTANTE: Actualizar el monto de descuento general
            order.DiscountAmount = orderDto.DiscountAmount;
            
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            // Actualizar detalles
            if (orderDto.OrderDetails != null && orderDto.OrderDetails.Any())
            {
                // Obtener detalles actuales para comparar
                var existingDetails = await _orderDetailRepository.GetByOrderIdAsync(order.ID);

                // Identificar detalles a eliminar
                var detailsToRemove = existingDetails
                    .Where(ed => !orderDto.OrderDetails.Any(nd => nd.ID == ed.ID))
                    .ToList();

                // Eliminar detalles que ya no existen
                foreach (var detail in detailsToRemove)
                {
                    // Revertir ajustes de inventario
                    await ReverseInventoryAdjustmentForDetail(detail);
                    await _orderDetailRepository.DeleteAsync(detail.ID);
                }

                // Actualizar o agregar detalles
                foreach (var detailDto in orderDto.OrderDetails)
                {
                    if (detailDto.ID > 0)
                    {
                        // Es un detalle existente - actualizar
                        var existingDetail = existingDetails.FirstOrDefault(d => d.ID == detailDto.ID);
                        if (existingDetail != null)
                        {
                            // Guardar cantidad anterior para ajustar inventario
                            int previousQuantity = existingDetail.Quantity;

                            // Actualizar datos
                            existingDetail.Quantity = detailDto.Quantity;
                            existingDetail.Notes = detailDto.Notes;
                            existingDetail.SubTotal = detailDto.SubTotal;
                            
                            // IMPORTANTE: Actualizar descuentos por producto
                            existingDetail.DiscountPercentage = detailDto.DiscountPercentage;
                            existingDetail.DiscountAmount = detailDto.DiscountAmount;

                            await _orderDetailRepository.UpdateAsync(existingDetail);

                            // Actualizar personalizaciones si hay cambios
                            if (detailDto.Customizations != null && detailDto.Customizations.Any())
                            {
                                // Convertir DTO a modelos de customizaciones
                                var customizations = detailDto.Customizations.Select(c => new OrderItemCustomizationModel
                                {
                                    OrderDetailID = existingDetail.ID,
                                    IngredientID = c.IngredientID,
                                    CustomizationType = c.CustomizationType,
                                    Quantity = c.Quantity,
                                    ExtraCharge = c.ExtraCharge
                                }).ToList();

                                // Usar el método específico para guardar personalizaciones
                                await _orderDetailRepository.AddCustomizationsAsync(existingDetail.ID, customizations);
                            }
                            else
                            {
                                // Si no hay personalizaciones, eliminar las existentes
                                await _orderDetailRepository.AddCustomizationsAsync(existingDetail.ID, new List<OrderItemCustomizationModel>());
                            }

                            // Ajustar inventario si cambió la cantidad
                            if (previousQuantity != detailDto.Quantity)
                            {
                                // Revertir ajuste anterior
                                var tempDetail = new OrderDetailModel
                                {
                                    ItemID = existingDetail.ItemID,
                                    ItemType = existingDetail.ItemType,
                                    Quantity = previousQuantity
                                };
                                await ReverseInventoryAdjustmentForDetail(tempDetail);

                                // Aplicar nuevo ajuste
                                await UpdateInventoryForOrderDetail(existingDetail);
                            }
                        }
                    }
                    else
                    {
                        // Es un nuevo detalle - agregarlo
                        var newDetail = await AddOrderDetailWithoutTotal(order.ID, detailDto);
                        await UpdateInventoryForOrderDetail(newDetail);
                    }
                }
            }

            // Recalcular el total
            await CalculateOrderTotalAsync(order.ID);

            await transaction.CommitAsync();
            return await _orderRepository.GetByIdAsync(order.ID) ?? order;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al actualizar la orden {OrderId}", orderDto.ID);
            throw;
        }
    });
}
        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            var strategy = _orderRepository.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _orderRepository.BeginTransactionAsync();
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

                    // Guardar estado anterior para comparar
                    var previousStatus = order.OrderStatus;

                    // Actualizar estado
                    var result = await _orderRepository.UpdateStatusAsync(id, status);

                    // Funcionalidad de trigger: Si cambió a entregado o cancelado, liberar la mesa
                    if ((status == "Delivered" || status == "Cancelled") && order.TableID.HasValue)
                    {
                        await _tableRepository.UpdateStatusAsync(order.TableID.Value, "Available");
                        _logger.LogInformation("Mesa {TableId} liberada por cambio de estado de orden a {Status}", order.TableID.Value, status);
                    }

                    // Funcionalidad de trigger: Si cambió a cancelado, reversar ajustes de inventario
                    if (status == "Cancelled" && previousStatus != "Cancelled")
                    {
                        await ReverseInventoryAdjustments(id);
                    }

                    // Funcionalidad de trigger: Registrar cambio de estado
                    await LogOrderStatusChangeAsync(id, previousStatus, status);

                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al actualizar el estado de la orden {OrderId}", id);
                    throw;
                }
            });
        }
        public async Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus)
        {
            var strategy = _orderRepository.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _orderRepository.BeginTransactionAsync();
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

                    var result = await _orderRepository.UpdatePaymentStatusAsync(id, paymentStatus);

                    // Funcionalidad de trigger: Registrar cambio de estado de pago
                    await LogPaymentStatusChangeAsync(id, paymentStatus);

                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al actualizar el estado de pago de la orden {OrderId}", id);
                    throw;
                }
            });
        }

        public async Task<OrderDetailModel> AddOrderDetailAsync(int orderId, OrderDetailDTO detailDto)
        {
            var strategy = _orderRepository.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _orderRepository.BeginTransactionAsync();
                try
                {
                    var orderDetail = await AddOrderDetailWithoutTotal(orderId, detailDto);

                    // Funcionalidad de trigger: Actualizar inventario
                    await UpdateInventoryForOrderDetail(orderDetail);

                    // Recalcular el total de la orden
                    await CalculateOrderTotalAsync(orderId);

                    await transaction.CommitAsync();
                    return orderDetail;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al agregar detalle a la orden {OrderId}", orderId);
                    throw;
                }
            });
        }

        public async Task<bool> UpdateOrderDetailStatusAsync(int orderDetailId, string status)
        {
            var strategy = _orderRepository.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _orderRepository.BeginTransactionAsync();
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

                    var detail = await _orderDetailRepository.GetByIdAsync(orderDetailId);
                    if (detail == null)
                    {
                        return false;
                    }

                    // Guardar estado anterior
                    var previousStatus = detail.Status;

                    // Actualizar estado
                    var result = await _orderDetailRepository.UpdateStatusAsync(orderDetailId, status);

                    // Funcionalidad de trigger: Si se cancela un detalle, reversar ajustes de inventario
                    if (status == "Cancelled" && previousStatus != "Cancelled")
                    {
                        await ReverseInventoryAdjustmentForDetail(detail);
                    }

                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al actualizar el estado del detalle {OrderDetailId}", orderDetailId);
                    throw;
                }
            });
        }

        public async Task<bool> RemoveOrderDetailAsync(int orderDetailId)
        {
            var strategy = _orderRepository.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _orderRepository.BeginTransactionAsync();
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

                    // Funcionalidad de trigger: Reversar ajustes de inventario
                    await ReverseInventoryAdjustmentForDetail(detail);

                    // Eliminar el detalle
                    var result = await _orderDetailRepository.DeleteAsync(orderDetailId);

                    // Recalcular total de la orden
                    await CalculateOrderTotalAsync(detail.OrderID);

                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al eliminar el detalle {OrderDetailId}", orderDetailId);
                    throw;
                }
            });
        }


        public async Task<string> GenerateOrderNumberAsync()
        {
            return await _orderRepository.GenerateOrderNumberAsync();
        }


        private async Task<OrderDetailModel> AddOrderDetailWithoutTotal(int orderId, OrderDetailDTO detailDto)
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
                // Mapear las personalizaciones del DTO al modelo
                var customizations = new List<OrderItemCustomizationModel>();

                foreach (var customizationDto in detailDto.Customizations)
                {
                    // Validar según el tipo de personalización
                    bool isValid = true;

                    // Para productos, validar que solo se quiten ingredientes del producto
                    if (detailDto.ItemType == "Product" && customizationDto.CustomizationType == "Remove")
                    {
                        var product = await _productRepository.GetByIdAsync(detailDto.ItemID);
                        isValid = product?.Ingredients?.Any(i => i.IngredientID == customizationDto.IngredientID) ?? false;
                    }

                    // Para productos, validar que solo se añadan extras permitidos
                    if (detailDto.ItemType == "Product" && customizationDto.CustomizationType == "Extra")
                    {
                        var product = await _productRepository.GetByIdAsync(detailDto.ItemID);
                        var ingredientItem = product?.Ingredients?.FirstOrDefault(i =>
                            i.IngredientID == customizationDto.IngredientID && i.CanBeExtra);

                        isValid = ingredientItem != null;

                        // Asegurarse de que el precio extra es correcto
                        if (isValid && ingredientItem != null)
                        {
                            customizationDto.ExtraCharge = ingredientItem.ExtraPrice;
                        }
                    }

                    if (isValid)
                    {
                        customizations.Add(new OrderItemCustomizationModel
                        {
                            OrderDetailID = createdDetail.ID,
                            IngredientID = customizationDto.IngredientID,
                            CustomizationType = customizationDto.CustomizationType,
                            Quantity = customizationDto.Quantity,
                            ExtraCharge = customizationDto.ExtraCharge
                        });
                    }
                }

                // Importante: usar el método específico para guardar personalizaciones
                await _orderDetailRepository.AddCustomizationsAsync(createdDetail.ID, customizations);

                // Recargar el detalle con sus personalizaciones
                createdDetail = await _orderDetailRepository.GetByIdAsync(createdDetail.ID);
            }

            return createdDetail;
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

        public async Task<(bool IsAvailable, List<string> UnavailableItems)> VerifyInventoryForOrder(List<OrderDetailDTO> orderDetails)
        {
            var unavailableItems = new List<string>();

            try
            {
                foreach (var detail in orderDetails)
                {
                    if (detail.ItemType == "Product")
                    {
                        // Verificar producto individual
                        var product = await _productRepository.GetByIdAsync(detail.ItemID);
                        if (product == null) continue;

                        bool isAvailable = await VerifyProductAvailability(product, detail.Quantity);
                        if (!isAvailable)
                        {
                            unavailableItems.Add(product.Name);
                        }
                    }
                    else if (detail.ItemType == "Combo")
                    {
                        // Verificar cada producto del combo
                        var combo = await _comboRepository.GetByIdAsync(detail.ItemID);
                        if (combo?.Items == null) continue;

                        foreach (var comboItem in combo.Items)
                        {
                            var comboProduct = await _productRepository.GetByIdAsync(comboItem.FastFoodItemID);
                            if (comboProduct == null) continue;

                            bool isAvailable = await VerifyProductAvailability(comboProduct, comboItem.Quantity * detail.Quantity);
                            if (!isAvailable)
                            {
                                unavailableItems.Add($"{comboProduct.Name} (en combo {combo.Name})");
                            }
                        }
                    }
                }

                return (unavailableItems.Count == 0, unavailableItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad de inventario para la orden");
                throw;
            }
        }

        private async Task<bool> VerifyProductAvailability(FastFoodItemModel product, int quantity)
        {
            if (product == null) return false;

            try
            {
                // Verificar si es producto de inventario (ID=2)
                if (product.ProductTypeID == 2) // Tipo "Inventory"
                {
                    // Verificar stock directo del producto
                    var inventory = await _productInventoryRepository.GetByProductIdAsync(product.ID);
                    if (inventory == null) return false;

                    return inventory.CurrentStock >= quantity;
                }
                else // Tipo "Prepared" (ID=1)
                {
                    // Verificar ingredientes
                    if (product.Ingredients == null || !product.Ingredients.Any())
                        return true; // Si no tiene ingredientes registrados, asumimos que está disponible

                    foreach (var itemIngredient in product.Ingredients)
                    {
                        // Obtener ingrediente actualizado
                        var ingredient = await _inventoryRepository.GetIngredientByIdAsync(itemIngredient.IngredientID);
                        if (ingredient == null) return false;

                        // Verificar stock suficiente
                        decimal requiredQuantity = itemIngredient.Quantity * quantity;
                        if (ingredient.StockQuantity < requiredQuantity)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad del producto {ProductId}", product.ID);
                return false;
            }
        }


        #region Métodos privados para manejar la lógica que estaba en los triggers

        private async Task UpdateInventoryForOrderDetail(OrderDetailModel detail)
        {
            try
            {
                if (detail.ItemType == "Product")
                {
                    // Buscar el producto y su inventario
                    var product = await _productRepository.GetByIdAsync(detail.ItemID);
                    if (product?.Inventory != null)
                    {
                        // Verificar si hay suficiente stock
                        if (product.Inventory.CurrentStock < detail.Quantity)
                        {
                            _logger.LogWarning("Stock insuficiente para el producto {ProductId}. Stock actual: {CurrentStock}, Solicitado: {Quantity}",
                                detail.ItemID, product.Inventory.CurrentStock, detail.Quantity);

                            // Opcional: lanzar excepción o simplemente registrar la advertencia
                            // throw new ValidationException($"Stock insuficiente para el producto {product.Name}");
                        }

                        // Reducir stock
                        product.Inventory.CurrentStock -= detail.Quantity;
                        await _productInventoryRepository.UpdateAsync(product.Inventory);

                        _logger.LogInformation("Stock actualizado para producto {ProductId}. Nuevo stock: {NewStock}",
                            detail.ItemID, product.Inventory.CurrentStock);
                    }

                    // Si hay ingredientes personalizados, ajustar el inventario de ingredientes
                    foreach (var customization in detail.Customizations)
                    {
                        if (customization.CustomizationType == "Extra")
                        {
                            // Reducir stock del ingrediente extra
                            await UpdateIngredientStock(customization.IngredientID, -customization.Quantity);
                        }
                    }
                }
                else if (detail.ItemType == "Combo")
                {
                    // Para combos, obtener sus elementos y actualizar inventario de cada uno
                    var combo = await _comboRepository.GetByIdAsync(detail.ItemID);
                    if (combo?.Items != null)
                    {
                        foreach (var item in combo.Items)
                        {
                            // Obtener el producto y su inventario
                            var product = await _productRepository.GetByIdAsync(item.FastFoodItemID);
                            if (product?.Inventory != null)
                            {
                                // Reducir stock considerando la cantidad del combo y la cantidad del ítem en el combo
                                int totalQuantity = detail.Quantity * item.Quantity;

                                // Verificar stock
                                if (product.Inventory.CurrentStock < totalQuantity)
                                {
                                    _logger.LogWarning("Stock insuficiente para el producto {ProductId} en combo. Stock actual: {CurrentStock}, Solicitado: {Quantity}",
                                        item.FastFoodItemID, product.Inventory.CurrentStock, totalQuantity);
                                }

                                // Reducir stock
                                product.Inventory.CurrentStock -= totalQuantity;
                                await _productInventoryRepository.UpdateAsync(product.Inventory);

                                _logger.LogInformation("Stock actualizado para producto {ProductId} en combo. Nuevo stock: {NewStock}",
                                    item.FastFoodItemID, product.Inventory.CurrentStock);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar inventario para detalle de orden {DetailId}", detail.ID);
                throw;
            }
        }

        private async Task ReverseInventoryAdjustmentForDetail(OrderDetailModel detail)
        {
            try
            {
                if (detail.ItemType == "Product")
                {
                    // Buscar el producto y su inventario
                    var product = await _productRepository.GetByIdAsync(detail.ItemID);
                    if (product?.Inventory != null)
                    {
                        // Devolver al stock
                        product.Inventory.CurrentStock += detail.Quantity;
                        await _productInventoryRepository.UpdateAsync(product.Inventory);

                        _logger.LogInformation("Stock revertido para producto {ProductId}. Nuevo stock: {NewStock}",
                            detail.ItemID, product.Inventory.CurrentStock);
                    }

                    // Si hay ingredientes personalizados, revertir ajustes
                    foreach (var customization in detail.Customizations)
                    {
                        if (customization.CustomizationType == "Extra")
                        {
                            // Devolver stock del ingrediente extra
                            await UpdateIngredientStock(customization.IngredientID, customization.Quantity);
                        }
                    }
                }
                else if (detail.ItemType == "Combo")
                {
                    // Para combos, revertir ajustes de cada elemento
                    var combo = await _comboRepository.GetByIdAsync(detail.ItemID);
                    if (combo?.Items != null)
                    {
                        foreach (var item in combo.Items)
                        {
                            // Obtener el producto y su inventario
                            var product = await _productRepository.GetByIdAsync(item.FastFoodItemID);
                            if (product?.Inventory != null)
                            {
                                // Devolver al stock considerando la cantidad del combo y la cantidad del ítem en el combo
                                int totalQuantity = detail.Quantity * item.Quantity;

                                // Devolver stock
                                product.Inventory.CurrentStock += totalQuantity;
                                await _productInventoryRepository.UpdateAsync(product.Inventory);

                                _logger.LogInformation("Stock revertido para producto {ProductId} en combo. Nuevo stock: {NewStock}",
                                    item.FastFoodItemID, product.Inventory.CurrentStock);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revertir ajustes de inventario para detalle {DetailId}", detail.ID);
                throw;
            }
        }

        private async Task ReverseInventoryAdjustments(int orderId)
        {
            try
            {
                // Obtener todos los detalles de la orden
                var details = await _orderDetailRepository.GetByOrderIdAsync(orderId);

                // Revertir ajustes para cada detalle
                foreach (var detail in details)
                {
                    if (detail.Status != "Cancelled") // Solo si no estaba ya cancelado
                    {
                        await ReverseInventoryAdjustmentForDetail(detail);

                        // Actualizar estado del detalle a cancelado
                        await _orderDetailRepository.UpdateStatusAsync(detail.ID, "Cancelled");
                    }
                }

                _logger.LogInformation("Ajustes de inventario revertidos para todos los detalles de la orden {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revertir ajustes de inventario para la orden {OrderId}", orderId);
                throw;
            }
        }

        private async Task UpdateIngredientStock(int ingredientId, decimal quantityChange)
        {
            try
            {
                var ingredient = await _inventoryRepository.GetIngredientByIdAsync(ingredientId);
                if (ingredient == null)
                {
                    _logger.LogWarning("No se encontró el ingrediente {IngredientId} para actualizar stock", ingredientId);
                    return;
                }

                // Calcular nuevo stock
                ingredient.StockQuantity += quantityChange;

                // Si el stock queda negativo, ajustar a 0 y registrar
                if (ingredient.StockQuantity < 0)
                {
                    _logger.LogWarning("Stock negativo para ingrediente {IngredientId}. Ajustando a 0", ingredientId);
                    ingredient.StockQuantity = 0;
                }

                // Actualizar
                await _inventoryRepository.UpdateIngredientAsync(ingredient);

                _logger.LogInformation("Stock de ingrediente {IngredientId} actualizado. Nuevo stock: {NewStock}",
                    ingredientId, ingredient.StockQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del ingrediente {IngredientId}", ingredientId);
                throw;
            }
        }

        private async Task HandleOrderStatusChange(OrderModel order, string previousStatus)
        {
            try
            {
                // Si cambia a Delivered o Cancelled y hay mesa asignada, liberarla
                if ((order.OrderStatus == "Delivered" || order.OrderStatus == "Cancelled") && order.TableID.HasValue)
                {
                    await _tableRepository.UpdateStatusAsync(order.TableID.Value, "Available");
                    _logger.LogInformation("Mesa {TableId} liberada por cambio de estado de orden {OrderId} a {Status}",
                        order.TableID.Value, order.ID, order.OrderStatus);
                }

                // Si cambia a Cancelled, revertir ajustes de inventario
                if (order.OrderStatus == "Cancelled" && previousStatus != "Cancelled")
                {
                    await ReverseInventoryAdjustments(order.ID);
                }

                // Registrar cambio de estado
                await LogOrderStatusChangeAsync(order.ID, previousStatus, order.OrderStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al manejar cambio de estado de orden {OrderId}", order.ID);
                throw;
            }
        }

        private async Task HandleTableChange(int? oldTableId, int? newTableId)
        {
            try
            {
                // Liberar mesa anterior si existía
                if (oldTableId.HasValue)
                {
                    await _tableRepository.UpdateStatusAsync(oldTableId.Value, "Available");
                    _logger.LogInformation("Mesa {TableId} liberada por cambio de mesa", oldTableId.Value);
                }

                // Ocupar nueva mesa si existe
                if (newTableId.HasValue)
                {
                    await _tableRepository.UpdateStatusAsync(newTableId.Value, "Occupied");
                    _logger.LogInformation("Mesa {TableId} ocupada por cambio de mesa", newTableId.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al manejar cambio de mesa. Mesa anterior: {OldTableId}, Mesa nueva: {NewTableId}",
                    oldTableId, newTableId);
                throw;
            }
        }

        private async Task LogOrderActivityAsync(int orderId, string action, int userId)
        {
            try
            {
                // Como es solo para logging, simplemente registramos en el log
                _logger.LogInformation("Actividad de orden {OrderId}: {Action} por usuario {UserId}",
                    orderId, action, userId);
            }
            catch (Exception ex)
            {
                // No propagamos excepciones de logging para no interrumpir el flujo principal
                _logger.LogError(ex, "Error al registrar actividad para orden {OrderId}", orderId);
            }
        }

        private async Task LogOrderStatusChangeAsync(int orderId, string oldStatus, string newStatus)
        {
            try
            {
                _logger.LogInformation("Cambio de estado de orden {OrderId}: {OldStatus} → {NewStatus}",
                    orderId, oldStatus, newStatus);

                // Si tuvieras una tabla de historial de estados, aquí es donde insertarías el registro
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar cambio de estado para orden {OrderId}", orderId);
            }
        }

        private async Task LogPaymentStatusChangeAsync(int orderId, string newStatus)
        {
            try
            {
                _logger.LogInformation("Cambio de estado de pago de orden {OrderId}: {NewStatus}",
                    orderId, newStatus);

                // Si tuvieras una tabla de historial de pagos, aquí es donde insertarías el registro
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar cambio de estado de pago para orden {OrderId}", orderId);
            }
        }

        #endregion
    
    }
}
