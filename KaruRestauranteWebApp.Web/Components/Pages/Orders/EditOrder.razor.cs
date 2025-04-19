using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models.Orders;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using Microsoft.JSInterop;

namespace KaruRestauranteWebApp.Web.Components.Pages.Orders
{
    public class OrderFormModel
    {
        public int ID { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int? CustomerID { get; set; }
        public int? TableID { get; set; }
        public string OrderType { get; set; } = "DineIn";
        public string Notes { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public List<OrderDetailDTO> OrderDetails { get; set; } = new();
    }
    public partial class EditOrder
    {
        [Parameter]
        public int OrderId { get; set; }

        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Inject]
        public required DialogService DialogService { get; set; }

        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

        private bool isLoaded;
        private OrderModel? order;
        private OrderFormModel model = new();
        private List<CustomerModel> customers = new();
        private List<TableModel> availableTables = new();
        private List<FastFoodItemModel> products = new();
        private List<ComboModel> combos = new();
        private List<IngredientModel> ingredients = new();
        private List<CategoryModel> categories = new();
        private List<FastFoodItemModel> filteredProducts = new();
        private List<ComboModel> filteredCombos = new();
        private string[] orderTypes = new[] { "DineIn", "TakeOut", "Delivery" };
        private List<object> orderTypesDisplay = new List<object>();
        private bool showPaymentDialog = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Inicializar orden types display
                orderTypesDisplay = new List<object>
                {
                    new { text = "En sitio", value = "DineIn" },
                    new { text = "Para llevar", value = "TakeOut" },
                    new { text = "Entrega a domicilio", value = "Delivery" }
                };

                await LoadData();
                await LoadOrder();
                isLoaded = true;

                // Inicializar listas filtradas
                filteredProducts = products.Where(p => p.IsAvailable).ToList();
                filteredCombos = combos.Where(c => c.IsAvailable).ToList();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar datos: {ex.Message}", 4000);
            }
        }

        private async Task LoadData()
        {
            // Cargar clientes
            var customersResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Customer");
            if (customersResponse?.Success == true)
            {
                customers = JsonConvert.DeserializeObject<List<CustomerModel>>(customersResponse.Data.ToString()) ?? new();
            }

            // Cargar todas las mesas (incluida la que pueda tener asignada la orden actual)
            var tablesResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Table");
            if (tablesResponse?.Success == true)
            {
                availableTables = JsonConvert.DeserializeObject<List<TableModel>>(tablesResponse.Data.ToString()) ?? new();
            }

            // Cargar productos
            var productsResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/FastFood");
            if (productsResponse?.Success == true)
            {
                products = JsonConvert.DeserializeObject<List<FastFoodItemModel>>(productsResponse.Data.ToString()) ?? new();
            }

            // Cargar categorías
            var categoriesResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Category");
            if (categoriesResponse?.Success == true)
            {
                categories = JsonConvert.DeserializeObject<List<CategoryModel>>(categoriesResponse.Data.ToString()) ?? new();
            }

            // Cargar combos
            var combosResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Combo");
            if (combosResponse?.Success == true)
            {
                combos = JsonConvert.DeserializeObject<List<ComboModel>>(combosResponse.Data.ToString()) ?? new();
            }

            // Cargar ingredientes para personalizaciones
            var ingredientsResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Inventory/ingredients");
            if (ingredientsResponse?.Success == true)
            {
                ingredients = JsonConvert.DeserializeObject<List<IngredientModel>>(ingredientsResponse.Data.ToString()) ?? new();
            }
        }

        private async Task LoadOrder()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Order/{OrderId}");
                if (response?.Success == true)
                {
                    order = JsonConvert.DeserializeObject<OrderModel>(response.Data.ToString());

                    // Verificar si la orden puede ser editada
                    if (order != null && (order.OrderStatus == "Delivered" || order.OrderStatus == "Cancelled"))
                    {
                        NotificationService.Notify(NotificationSeverity.Warning,
                            "Advertencia", $"No se puede editar una orden en estado {order.OrderStatus}", 4000);
                        order = null;
                        return;
                    }

                    // Mapear los datos de la orden al modelo de edición
                    if (order != null)
                    {
                        // Calcular el porcentaje de descuento si existe - CORREGIDO
                        decimal discountPercentage = 0;
                        if (order.DiscountAmount > 0)
                        {
                            // Cálculo correcto del subtotal sin impuestos
                            decimal subtotal = (order.TotalAmount - order.TaxAmount) + order.DiscountAmount; // Sumar el descuento para obtener el subtotal original
                            if (subtotal > 0)
                            {
                                discountPercentage = Math.Round((order.DiscountAmount / subtotal) * 100, 2);
                            }
                        }

                        model = new OrderFormModel
                        {
                            ID = order.ID,
                            OrderNumber = order.OrderNumber,
                            CustomerID = order.CustomerID,
                            TableID = order.TableID,
                            OrderType = order.OrderType,
                            Notes = order.Notes,
                            DiscountAmount = order.DiscountAmount,
                            DiscountPercentage = discountPercentage,
                            OrderDetails = new List<OrderDetailDTO>()
                        };

                        // Mapear detalles de la orden
                        foreach (var detail in order.OrderDetails)
                        {
                            string itemName = "";
                            if (detail.ItemType == "Product")
                            {
                                var product = products.FirstOrDefault(p => p.ID == detail.ItemID);
                                itemName = product?.Name ?? $"Producto #{detail.ItemID}";
                            }
                            else if (detail.ItemType == "Combo")
                            {
                                var combo = combos.FirstOrDefault(c => c.ID == detail.ItemID);
                                itemName = combo?.Name ?? $"Combo #{detail.ItemID}";
                            }

                            // Corregido el cálculo del porcentaje de descuento por producto
                            decimal detailDiscountPercentage = 0;
                            if (detail.DiscountAmount > 0)
                            {
                                // La base correcta es UnitPrice * Quantity, no el subtotal que podría ya tener el descuento aplicado
                                decimal originalSubtotal = detail.UnitPrice * detail.Quantity;
                                if (originalSubtotal > 0)
                                {
                                    detailDiscountPercentage = Math.Round((detail.DiscountAmount / originalSubtotal) * 100, 2);
                                }
                            }

                            var detailDto = new OrderDetailDTO
                            {
                                ID = detail.ID,
                                OrderID = detail.OrderID,
                                ItemType = detail.ItemType,
                                ItemID = detail.ItemID,
                                ItemName = itemName,
                                Quantity = detail.Quantity,
                                UnitPrice = detail.UnitPrice,
                                SubTotal = detail.UnitPrice * detail.Quantity, // Subtotal bruto, recalcularemos después
                                Notes = detail.Notes,
                                Status = detail.Status,
                                DiscountPercentage = detail.DiscountPercentage,
                                DiscountAmount = detail.DiscountAmount,
                                Customizations = new List<OrderItemCustomizationDTO>()
                            };

                            // Mapear personalizaciones
                            foreach (var customization in detail.Customizations)
                            {
                                detailDto.Customizations.Add(new OrderItemCustomizationDTO
                                {
                                    ID = customization.ID,
                                    OrderDetailID = customization.OrderDetailID,
                                    IngredientID = customization.IngredientID,
                                    IngredientName = customization.Ingredient?.Name ?? "",
                                    CustomizationType = customization.CustomizationType,
                                    Quantity = customization.Quantity,
                                    ExtraCharge = customization.ExtraCharge
                                });
                            }

                            model.OrderDetails.Add(detailDto);
                         
                        }

                        foreach (var detail in model.OrderDetails)
                        {
                            CalculateDetailSubtotal(detail);
                        }
                    }
                
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al cargar la orden", 4000);
                    order = null;
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar la orden: {ex.Message}", 4000);
                order = null;
            }
        }

        private void OnOrderTypeChanged(object value)
        {
            if (value != null)
            {
                string orderType = value.ToString();

                if (orderType == "TakeOut" || orderType == "Delivery")
                {
                    model.TableID = null;
                }
            }
        }

        private async Task OpenNewCustomerDialog()
        {
            try
            {
                // Implementar diálogo para crear nuevo cliente
                var result = await DialogService.OpenAsync<Pages.Customer.CreateCustomer>("Nuevo Cliente",
                    new Dictionary<string, object>(),
                    new DialogOptions
                    {
                        Width = "700px",
                        Height = "570px",
                        CloseDialogOnOverlayClick = false
                    });

                if (result != null)
                {
                    // Recargar la lista de clientes
                    var customersResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Customer");
                    if (customersResponse?.Success == true)
                    {
                        customers = JsonConvert.DeserializeObject<List<CustomerModel>>(customersResponse.Data.ToString()) ?? new();
                        StateHasChanged();
                    }

                    // Seleccionar el nuevo cliente
                    if (result is CustomerModel newCustomer)
                    {
                        model.CustomerID = newCustomer.ID;
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al crear nuevo cliente: {ex.Message}", 4000);
            }
        }

        private void SearchProducts(ChangeEventArgs args)
        {
            var searchTerm = args.Value?.ToString().ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredProducts = products.Where(p => p.IsAvailable).ToList();
            }
            else
            {
                filteredProducts = products
                    .Where(p => p.IsAvailable &&
                          (p.Name.ToLower().Contains(searchTerm) ||
                           p.Description?.ToLower().Contains(searchTerm) == true))
                    .ToList();
            }
        }

        private void SearchCombos(ChangeEventArgs args)
        {
            var searchTerm = args.Value?.ToString().ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredCombos = combos.Where(c => c.IsAvailable).ToList();
            }
            else
            {
                filteredCombos = combos
                    .Where(c => c.IsAvailable &&
                          (c.Name.ToLower().Contains(searchTerm) ||
                           c.Description?.ToLower().Contains(searchTerm) == true))
                    .ToList();
            }
        }

        private void AddProductToOrder(FastFoodItemModel product)
        {
            // Verificar si el producto ya está en el pedido
            var existingDetail = model.OrderDetails
                .FirstOrDefault(d => d.ItemType == "Product" && d.ItemID == product.ID);

            if (existingDetail != null)
            {
                // Incrementar cantidad del producto existente
                existingDetail.Quantity++;
                CalculateDetailSubtotal(existingDetail);
            }
            else
            {
                // Añadir como nuevo producto
                var detail = new OrderDetailDTO
                {
                    ItemType = "Product",
                    ItemID = product.ID,
                    ItemName = product.Name,
                    Quantity = 1,
                    UnitPrice = product.SellingPrice,
                    SubTotal = product.SellingPrice,
                    Status = "Pending",
                    Customizations = new List<OrderItemCustomizationDTO>()
                };

                model.OrderDetails.Add(detail);
                CalculateTotal();
            }

            // Notificar al usuario que se agregó el producto
            NotificationService.Notify(NotificationSeverity.Success,
                "Producto agregado", $"{product.Name} agregado al pedido", 2000);
        }

        private async Task AddComboToOrder(ComboModel combo)
        {
            try
            {
                // Mostrar el diálogo con detalles del combo
                var result = await DialogService.OpenAsync<ComboDetailsDialog>("Detalles del Combo",
                    new Dictionary<string, object>
                    {
                        { "Combo", combo },
                        { "ShowPrices", true }
                    },
                    new DialogOptions
                    {
                        Width = "700px",
                        Height = "auto",
                        CloseDialogOnOverlayClick = false
                    });

                // Si el usuario cancela, no hacer nada
                if (result == null) return;

                // Verificar si el combo ya está en el pedido
                var existingDetail = model.OrderDetails
                    .FirstOrDefault(d => d.ItemType == "Combo" && d.ItemID == combo.ID);

                if (existingDetail != null)
                {
                    // Incrementar cantidad del combo existente
                    existingDetail.Quantity++;
                    CalculateDetailSubtotal(existingDetail);
                }
                else
                {
                    // Añadir como nuevo combo
                    var detail = new OrderDetailDTO
                    {
                        ItemType = "Combo",
                        ItemID = combo.ID,
                        ItemName = combo.Name,
                        Quantity = 1,
                        UnitPrice = combo.SellingPrice,
                        SubTotal = combo.SellingPrice,
                        Status = "Pending",
                        Customizations = new List<OrderItemCustomizationDTO>(),
                        // Agregar un campo que contenga los componentes del combo (para mostrar en la cocina)
                        ComboItems = combo.Items.Select(i => new ComboItemDetail
                        {
                            ItemName = i.FastFoodItem?.Name ?? $"Producto #{i.FastFoodItemID}",
                            Quantity = i.Quantity,
                            SpecialInstructions = i.SpecialInstructions
                        }).ToList()
                    };

                    model.OrderDetails.Add(detail);
                    CalculateTotal();
                }

                // Notificar al usuario que se agregó el combo
                NotificationService.Notify(NotificationSeverity.Success,
                    "Combo agregado", $"{combo.Name} agregado al pedido", 2000);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al agregar combo: {ex.Message}", 4000);
            }
        }

        private void IncreaseQuantity(OrderDetailDTO detail)
        {
            detail.Quantity++;
            CalculateDetailSubtotal(detail);
        }

        private void DecreaseQuantity(OrderDetailDTO detail)
        {
            if (detail.Quantity > 1)
            {
                detail.Quantity--;
                CalculateDetailSubtotal(detail);
            }
        }

        private void CalculateDetailSubtotal(OrderDetailDTO detail)
        {
            // Calcular el subtotal base (precio unitario * cantidad)
            decimal baseSubtotal = detail.UnitPrice * detail.Quantity;

            // Calcular descuento para este detalle
            if (detail.DiscountPercentage > 0)
            {
                detail.DiscountAmount = Math.Round(baseSubtotal * (detail.DiscountPercentage / 100), 2);
            }
            // Si no hay porcentaje pero sí hay un monto, recalcular el porcentaje
            else if (detail.DiscountAmount > 0)
            {
                detail.DiscountPercentage = Math.Round((detail.DiscountAmount / baseSubtotal) * 100, 2);
            }
            else
            {
                // Si no hay descuento, asegurar que ambos valores sean cero
                detail.DiscountAmount = 0;
                detail.DiscountPercentage = 0;
            }

            // Calcular el subtotal final después del descuento
            detail.SubTotal = baseSubtotal - detail.DiscountAmount;

            // Recalcular el total general de la orden
            CalculateTotal();
        }
        private decimal CalculateSubtotal()
        {
            // Usar el subtotal ya calculado en cada detalle (que ya incluye los descuentos por producto)
            return model.OrderDetails.Sum(d => d.SubTotal);
        }


        private decimal CalculateTax()
        {
            decimal subtotal = CalculateSubtotal();
            return Math.Round(subtotal * 0.13m, 2); // IVA del 13%
        }

        private decimal CalculateDiscountAmount()
        {
            // El subtotal es la suma de los subtotales de cada producto (que ya tienen sus descuentos aplicados)
            decimal subtotal = CalculateSubtotal();
            if (model.DiscountPercentage > 0)
            {
                model.DiscountAmount = Math.Round(subtotal * (model.DiscountPercentage / 100), 2);
                return model.DiscountAmount;
            }
            return model.DiscountAmount; // Mantener el valor actual si no hay cambio en el porcentaje
        }

        private decimal CalculateTotal()
        {
            decimal subtotal = CalculateSubtotal();
            decimal tax = CalculateTax();

            // Recalcular el descuento general
            decimal discountAmount = CalculateDiscountAmount();
            model.DiscountAmount = discountAmount;

            // Calcular el total final
            return subtotal + tax - discountAmount;
        }


        private string TranslateOrderStatus(string status)
        {
            return status switch
            {
                "Pending" => "Pendiente",
                "InProgress" => "En progreso",
                "Ready" => "Listo",
                "Delivered" => "Entregado",
                "Cancelled" => "Cancelado",
                _ => status ?? "Desconocido"
            };
        }

        private string TranslatePaymentStatus(string status)
        {
            return status switch
            {
                "Pending" => "Pendiente de pago",
                "Paid" => "Pagado",
                "Partially Paid" => "Pago parcial",
                "Cancelled" => "Cancelado",
                _ => status ?? "Desconocido"
            };
        }

        private string TranslateCustomizationType(string type)
        {
            return type switch
            {
                "Add" => "Agregar",
                "Remove" => "Quitar",
                "Extra" => "Extra",
                _ => type ?? "Desconocido"
            };
        }

        private BadgeStyle GetCustomizationBadgeStyle(string type)
        {
            return type switch
            {
                "Add" => BadgeStyle.Success,
                "Remove" => BadgeStyle.Danger,
                "Extra" => BadgeStyle.Warning,
                _ => BadgeStyle.Light
            };
        }

        private bool CanCustomize(OrderDetailDTO detail)
        {
            if (detail.ItemType != "Product")
                return false;

            // Verificar si el producto tiene ingredientes personalizables
            var product = products.FirstOrDefault(p => p.ID == detail.ItemID);
            return product?.Ingredients?.Any(i => i.IsOptional || i.CanBeExtra) == true;
        }

        private async Task OpenCustomizationDialog(OrderDetailDTO detail)
        {
            try
            {
                // Obtener el producto y sus ingredientes
                var product = products.FirstOrDefault(p => p.ID == detail.ItemID);
                if (product == null) return;

                // Usando Radzen DialogService para mostrar un diálogo de personalización
                var customizations = await DialogService.OpenAsync<CustomizationDialog>("Personalizar Producto",
                    new Dictionary<string, object>
                    {
                        { "ProductName", product.Name },
                        { "ProductIngredients", product.Ingredients ?? new List<ItemIngredientModel>() },
                        { "Customizations", detail.Customizations ?? new List<OrderItemCustomizationDTO>() },
                        { "AllIngredients", ingredients ?? new List<IngredientModel>() }
                    },
                    new DialogOptions
                    {
                        Width = "600px",
                        Height = "500px",
                        CloseDialogOnOverlayClick = false
                    });

                if (customizations != null)
                {
                    // Asegurarnos de que el resultado es del tipo esperado
                    if (customizations is List<OrderItemCustomizationDTO> customizationsList)
                    {
                        detail.Customizations = customizationsList;

                        // Recalcular precio con extras
                        decimal extraCharges = detail.Customizations
                            .Where(c => c.CustomizationType == "Extra")
                            .Sum(c => c.ExtraCharge * c.Quantity);

                        detail.UnitPrice = product.SellingPrice + extraCharges;
                        CalculateDetailSubtotal(detail);
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al personalizar producto: {ex.Message}", 4000);
            }
        }

        private void RemoveOrderDetail(OrderDetailDTO detail)
        {
            model.OrderDetails.Remove(detail);
            CalculateTotal();
        }

        private async Task OpenCustomizationsViewDialog(OrderDetailDTO detail)
        {
            if (!detail.Customizations.Any()) return;

            // Convertir las personalizaciones DTO a modelos
            var customizationModels = new List<OrderItemCustomizationModel>();
            foreach (var customDto in detail.Customizations)
            {
                var ingredientModel = ingredients.FirstOrDefault(i => i.ID == customDto.IngredientID);
                if (ingredientModel == null) continue;

                customizationModels.Add(new OrderItemCustomizationModel
                {
                    IngredientID = customDto.IngredientID,
                    Ingredient = ingredientModel,
                    CustomizationType = customDto.CustomizationType,
                    Quantity = customDto.Quantity,
                    ExtraCharge = customDto.ExtraCharge
                });
            }

            await DialogService.OpenAsync<CustomizationsDialog>("Personalizaciones",
                new Dictionary<string, object>
                {
                { "Customizations", customizationModels }
                },
                new DialogOptions
                {
                    Width = "500px",
                    Height = "400px",
                    CloseDialogOnOverlayClick = true
                });
        }
        private async Task HandleSubmit()
        {
            try
            {
                if (!model.OrderDetails.Any())
                {
                    NotificationService.Notify(NotificationSeverity.Warning,
                        "Validación", "Debe agregar al menos un producto al pedido", 4000);
                    return;
                }

                if (model.OrderType == "DineIn" && !model.TableID.HasValue)
                {
                    NotificationService.Notify(NotificationSeverity.Warning,
                        "Validación", "Debe seleccionar una mesa para pedidos en sitio", 4000);
                    return;
                }

                // Recalcular todos los descuentos una vez más para asegurar consistencia
                foreach (var detail in model.OrderDetails)
                {
                    CalculateDetailSubtotal(detail);
                }
                decimal totalFinal = CalculateTotal();

                // Convertir el OrderFormModel a OrderDTO para enviarlo a la API
                var orderDto = new OrderDTO
                {
                    ID = model.ID,
                    OrderNumber = model.OrderNumber,
                    CustomerID = model.CustomerID,
                    TableID = model.TableID,
                    OrderType = model.OrderType,
                    Notes = model.Notes,
                    DiscountAmount = model.DiscountAmount,
                    OrderDetails = model.OrderDetails, // Incluye todos los detalles con sus descuentos
                    OrderStatus = order?.OrderStatus ?? "Pending",
                    PaymentStatus = order?.PaymentStatus ?? "Pending",
                    // Es importante NO enviar TotalAmount ni TaxAmount ya que éstos se calculan en el servidor
                };

                var response = await ApiClient.PutAsync<BaseResponseModel, OrderDTO>(
                    $"api/Order/{OrderId}", orderDto);

                if (response?.Success == true)
                {
                    // Si la orden está pendiente de pago, ofrecer procesarlo ahora
                    if (order?.PaymentStatus != "Paid")
                    {
                        // Preguntar si desea procesar el pago
                        await ProcessPayment();
                    }
                    else
                    {
                        NotificationService.Notify(NotificationSeverity.Success,
                            "Éxito", "Pedido actualizado exitosamente", 4000);
                        NavigationManager.NavigateTo("/orders");
                    }
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al actualizar pedido", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al actualizar pedido: {ex.Message}", 4000);
            }
        }
        //private async Task HandleSubmit()
        //{
        //    try
        //    {
        //        if (!model.OrderDetails.Any())
        //        {
        //            NotificationService.Notify(NotificationSeverity.Warning,
        //                "Validación", "Debe agregar al menos un producto al pedido", 4000);
        //            return;
        //        }

        //        if (model.OrderType == "DineIn" && !model.TableID.HasValue)
        //        {
        //            NotificationService.Notify(NotificationSeverity.Warning,
        //                "Validación", "Debe seleccionar una mesa para pedidos en sitio", 4000);
        //            return;
        //        }

        //        // Convertir el OrderFormModel a OrderDTO para enviarlo a la API
        //        var orderDto = new OrderDTO
        //        {
        //            ID = model.ID,
        //            OrderNumber = model.OrderNumber,
        //            CustomerID = model.CustomerID,
        //            TableID = model.TableID,
        //            OrderType = model.OrderType,
        //            Notes = model.Notes,
        //            DiscountAmount = model.DiscountAmount,
        //            OrderDetails = model.OrderDetails,
        //            OrderStatus = order?.OrderStatus ?? "Pending",
        //            PaymentStatus = order?.PaymentStatus ?? "Pending"
        //        };

        //        var response = await ApiClient.PutAsync<BaseResponseModel, OrderDTO>(
        //            $"api/Order/{OrderId}", orderDto);

        //        if (response?.Success == true)
        //        {
        //            // Si la orden está pendiente de pago, ofrecer procesarlo ahora
        //            if (order?.PaymentStatus != "Paid")
        //            {
        //                // Preguntar si desea procesar el pago
        //                await ProcessPayment();
        //            }
        //            else
        //            {
        //                NotificationService.Notify(NotificationSeverity.Success,
        //                    "Éxito", "Pedido actualizado exitosamente", 4000);
        //                NavigationManager.NavigateTo("/orders");
        //            }
        //        }
        //        else
        //        {
        //            NotificationService.Notify(NotificationSeverity.Error,
        //                "Error", response?.ErrorMessage ?? "Error al actualizar pedido", 4000);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        NotificationService.Notify(NotificationSeverity.Error,
        //            "Error", $"Error al actualizar pedido: {ex.Message}", 4000);
        //    }
        //}

        private async Task ProcessPayment()
        {
            try
            {
                // Verificar si hay una sesión de caja abierta primero
                var cashRegisterResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/CashRegister/sessions/current");
                if (cashRegisterResponse?.Success != true)
                {
                    NotificationService.Notify(NotificationSeverity.Warning,
                        "Advertencia", "No hay una sesión de caja abierta. Para procesar pagos, primero debe abrir la caja.", 4000);

                    // Redirigir a la página de pago específica para esta orden
                    NavigationManager.NavigateTo($"/orders/payment/{model.ID}");
                    return;
                }

                // Calcular monto pendiente
                decimal totalAmount = CalculateTotal();
                decimal paidAmount = 0;

                // Obtener pagos ya realizados para esta orden
                var orderResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Order/{model.ID}");
                if (orderResponse?.Success == true)
                {
                    var orderData = JsonConvert.DeserializeObject<OrderModel>(orderResponse.Data.ToString());
                    if (orderData?.Payments != null && orderData.Payments.Any())
                    {
                        paidAmount = orderData.Payments.Sum(p => p.Amount);
                    }
                }

                decimal pendingAmount = totalAmount - paidAmount;

                if (pendingAmount <= 0)
                {
                    NotificationService.Notify(NotificationSeverity.Info,
                        "Información", "Esta orden ya está completamente pagada", 4000);
                    return;
                }

                // Redirigir a la página de pago específica
                NavigationManager.NavigateTo($"/orders/payment/{model.ID}");
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al procesar el pago: {ex.Message}", 4000);
            }
        }

        private async Task PrintReceipt(PaymentProcessDialog.PaymentResult paymentResult)
        {
            try
            {
                // Preparar datos para la impresión
                var receiptData = new
                {
                    orderNumber = model.OrderNumber,
                    customerName = customers.FirstOrDefault(c => c.ID == model.CustomerID)?.Name,
                    table = availableTables.FirstOrDefault(t => t.ID == model.TableID)?.TableNumber.ToString(),
                    orderType = model.OrderType,
                    items = model.OrderDetails.Select(detail => new
                    {
                        name = detail.ItemName,
                        quantity = detail.Quantity,
                        price = detail.UnitPrice,
                        notes = detail.Notes,
                        customizations = detail.Customizations.Select(c => new
                        {
                            name = c.IngredientName,
                            type = c.CustomizationType,
                            quantity = c.Quantity
                        }).ToList()
                    }).ToList(),
                    subtotal = CalculateSubtotal(),
                    tax = CalculateTax(),
                    discount = model.DiscountAmount,
                    total = CalculateTotal(),
                    paymentMethod = GetPaymentMethodName(paymentResult.PaymentInfo.PaymentMethod),
                    amountReceived = paymentResult.AmountReceived,
                    amountReceivedOriginal = paymentResult.AmountReceivedOriginal,
                    change = paymentResult.Change,
                    changeOriginal = paymentResult.ChangeOriginal,
                    currency = paymentResult.Currency,
                    exchangeRate = paymentResult.ExchangeRate,
                    referenceNumber = paymentResult.PaymentInfo.ReferenceNumber,
                    notes = model.Notes
                };

                // Imprimir ticket de pago
                await JSRuntime.InvokeVoidAsync("printerService.printPaymentReceipt", receiptData);

                // Imprimir ticket de cocina para los items con status "Pending"
                if (model.OrderDetails.Any(d => d.Status == "Pending"))
                {
                    await JSRuntime.InvokeVoidAsync("printerService.printKitchenTicket", receiptData);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Warning,
                                   "Impresión", $"Error al imprimir ticket: {ex.Message}", 4000);
            }
        }

        private string GetPaymentMethodName(string method)
        {
            return method switch
            {
                "Cash" => "Efectivo",
                "CreditCard" => "Tarjeta de Crédito",
                "DebitCard" => "Tarjeta de Débito",
                "SINPE" => "SINPE Móvil",
                "Transfer" => "Transferencia Bancaria",
                "Other" => "Otro medio de pago",
                _ => method
            };
        }

        private BadgeStyle GetOrderStatusBadgeStyle(string status)
        {
            return status switch
            {
                "Pending" => BadgeStyle.Info,
                "InProgress" => BadgeStyle.Warning,
                "Ready" => BadgeStyle.Success,
                "Delivered" => BadgeStyle.Primary,
                "Cancelled" => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }

        private BadgeStyle GetPaymentStatusBadgeStyle(string status)
        {
            return status switch
            {
                "Pending" => BadgeStyle.Warning,
                "Paid" => BadgeStyle.Success,
                "Partially Paid" => BadgeStyle.Info,
                "Cancelled" => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }

        private BadgeStyle GetOrderDetailStatusBadgeStyle(string status)
        {
            return status switch
            {
                "Pending" => BadgeStyle.Info,
                "InPreparation" => BadgeStyle.Warning,
                "Ready" => BadgeStyle.Success,
                "Delivered" => BadgeStyle.Primary,
                "Cancelled" => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }
    }

  
}

