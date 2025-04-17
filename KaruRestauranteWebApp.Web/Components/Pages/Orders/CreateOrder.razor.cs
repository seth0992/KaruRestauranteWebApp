using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Models.Models.Orders;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Orders
{
    public partial class CreateOrder
    {
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
        private OrderFormModel model = new();
        private List<CustomerModel> customers = new();
        private List<TableModel> availableTables = new();
        private List<FastFoodItemModel> products = new();
        private List<ComboModel> combos = new();
        private List<IngredientModel> ingredients = new();
        private List<CategoryModel> categories = new();
        private List<FastFoodItemModel> filteredProducts = new();
        private List<ComboModel> filteredCombos = new();

        private Dictionary<string, string> customizationTypeMap = new Dictionary<string, string>
        {
            { "Add", "Agregar" },
            { "Remove", "Quitar" },
            { "Extra", "Extra" }
        };

        // Tipos de pedido con traducción
        private object[] orderTypesDisplay = new[]
        {
            new { value = "DineIn", text = "En sitio" },
            new { value = "TakeOut", text = "Pide y Espera" },
            new { value = "Delivery", text = "Express" }
        };

        private object[] paymentMethods = new[]
        {
            new { value = "Cash", name = "Efectivo" },
            new { value = "CreditCard", name = "Tarjeta de Crédito" },
            new { value = "DebitCard", name = "Tarjeta de Débito" },
            new { value = "Transfer", name = "Transferencia" },
            new { value = "SIMPE", name = "SINPE Movil" },
            new { value = "Other", name = "Otro" }
        };

        // Método para traducir el tipo de personalización
        private string TranslateCustomizationType(string type)
        {
            return customizationTypeMap.TryGetValue(type, out var translation) ? translation : type;
        }



        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadData();
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

            // Cargar mesas disponibles
            var tablesResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Table/available");
            if (tablesResponse?.Success == true)
            {
                availableTables = JsonConvert.DeserializeObject<List<TableModel>>(tablesResponse.Data.ToString()) ?? new();
            }

            // Cargar productos
            var productsResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/FastFood");
            if (productsResponse?.Success == true)
            {
                products = new List<FastFoodItemModel>(); // Lista existente
                var productDTOs = JsonConvert.DeserializeObject<List<FastFoodItemDTO>>(productsResponse.Data.ToString()) ?? new();

                // Para cada DTO, cargar la versión detallada del producto
                foreach (var productDTO in productDTOs)
                {
                    if (productDTO.IsAvailable)
                    {
                        var detailResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/FastFood/{productDTO.ID}");
                        if (detailResponse?.Success == true)
                        {
                            var detailedProduct = JsonConvert.DeserializeObject<FastFoodItemDetailDTO>(detailResponse.Data.ToString());
                            if (detailedProduct != null)
                            {
                                // Crear un modelo a partir del DTO detallado
                                var product = new FastFoodItemModel
                                {
                                    ID = detailedProduct.ID,
                                    Name = detailedProduct.Name,
                                    Description = detailedProduct.Description,
                                    CategoryID = detailedProduct.CategoryID,
                                    SellingPrice = detailedProduct.SellingPrice,
                                    EstimatedCost = detailedProduct.EstimatedCost,
                                    ProductTypeID = detailedProduct.ProductTypeID,
                                    IsAvailable = detailedProduct.IsAvailable,
                                    ImageUrl = detailedProduct.ImageUrl,
                                    EstimatedPreparationTime = detailedProduct.EstimatedPreparationTime,
                                    // Crear lista de ingredientes a partir del DTO
                                    Ingredients = detailedProduct.Ingredients?.Select(i => new ItemIngredientModel
                                    {
                                        ID = i.ID,
                                        FastFoodItemID = detailedProduct.ID,
                                        IngredientID = i.IngredientID,
                                        Quantity = i.Quantity,
                                        IsOptional = i.IsOptional,
                                        CanBeExtra = i.CanBeExtra,
                                        ExtraPrice = i.ExtraPrice
                                    }).ToList() ?? new List<ItemIngredientModel>()
                                };

                                products.Add(product);
                            }
                        }
                    }
                }
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
            // Implementar diálogo para crear nuevo cliente
            // Agregar la lógica aquí
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

        private bool CanCustomize(OrderDetailDTO detail)
        {
            if (detail.ItemType != "Product")
                return false;

            var product = products.FirstOrDefault(p => p.ID == detail.ItemID);

            // Verificar que sea de tipo preparado (ID 1) y tenga ingredientes personalizables
            if (product?.ProductTypeID == 1 && product.Ingredients != null)
            {
                return product.Ingredients.Any(i => i.IsOptional || i.CanBeExtra);
            }

            return false;
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
                { "AllIngredients", ingredients ?? new List<IngredientModel>() },
                { "CustomizationTypeMap", customizationTypeMap }
                    },
                    new DialogOptions
                    {
                        Width = "700px",
                        Height = "600px",
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
            // Calcular el descuento por producto
            detail.DiscountAmount = detail.UnitPrice * detail.Quantity * (detail.DiscountPercentage / 100);

            // Calcular subtotal con descuento
            detail.SubTotal = (detail.UnitPrice * detail.Quantity) - detail.DiscountAmount;

            // Recalcular el total de la orden
            CalculateTotal();
        }

        private decimal CalculateSubtotal()
        {
            return model.OrderDetails.Sum(d => d.SubTotal);
        }

        private decimal CalculateTax()
        {
            decimal subtotal = CalculateSubtotal();
            return Math.Round(subtotal * 0.13m, 2); // IVA del 13%
        }

        private decimal CalculateTotal()
        {
            decimal subtotal = CalculateSubtotal();
            decimal tax = CalculateTax();
            decimal generalDiscountAmount = CalculateDiscountAmount();
            return subtotal + tax - generalDiscountAmount;
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

                // Verificar disponibilidad de inventario usando el servicio API
                bool inventoryOk = true;
                List<string> unavailableItems = new List<string>();

                foreach (var detail in model.OrderDetails)
                {
                    if (detail.ItemType == "Product")
                    {
                        var product = products.FirstOrDefault(p => p.ID == detail.ItemID);
                        if (product == null) continue;

                        if (product.ProductTypeID == 2) // Producto de inventario
                        {
                            // Verificar stock de producto
                            var productResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/ProductInventory/product/{detail.ItemID}");

                            if (productResponse?.Success == true)
                            {
                                var inventory = JsonConvert.DeserializeObject<ProductInventoryModel>(productResponse.Data.ToString());
                                if (inventory == null || inventory.CurrentStock < detail.Quantity)
                                {
                                    inventoryOk = false;
                                    unavailableItems.Add(detail.ItemName);
                                }
                            }
                        }
                        else if (product.ProductTypeID == 1) // Producto preparado
                        {
                            // Verificar stock de ingredientes
                            if (product.Ingredients != null && product.Ingredients.Any())
                            {
                                foreach (var ingredient in product.Ingredients)
                                {
                                    // Obtener el estado actual del ingrediente
                                    var ingredientResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Inventory/ingredients/{ingredient.IngredientID}");
                                    if (ingredientResponse?.Success == true)
                                    {
                                        var ingredientInfo = JsonConvert.DeserializeObject<IngredientModel>(ingredientResponse.Data.ToString());
                                        decimal requiredQuantity = ingredient.Quantity * detail.Quantity;

                                        if (ingredientInfo == null || ingredientInfo.StockQuantity < requiredQuantity)
                                        {
                                            inventoryOk = false;
                                            unavailableItems.Add($"{detail.ItemName} (falta: {ingredientInfo?.Name ?? "ingrediente"})");
                                            break; // Basta con un ingrediente faltante para marcar el producto como no disponible
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (detail.ItemType == "Combo")
                    {
                        // Para combos, obtener sus componentes
                        var comboResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Combo/{detail.ItemID}");
                        if (comboResponse?.Success == true)
                        {
                            var combo = JsonConvert.DeserializeObject<ComboModel>(comboResponse.Data.ToString());
                            if (combo?.Items != null)
                            {
                                foreach (var comboItem in combo.Items)
                                {
                                    var product = products.FirstOrDefault(p => p.ID == comboItem.FastFoodItemID);
                                    if (product == null) continue;

                                    if (product.ProductTypeID == 2) // Producto de inventario
                                    {
                                        // Verificar stock de cada producto en el combo
                                        var productResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/ProductInventory/product/{comboItem.FastFoodItemID}");

                                        if (productResponse?.Success == true)
                                        {
                                            var inventory = JsonConvert.DeserializeObject<ProductInventoryModel>(productResponse.Data.ToString());
                                            int requiredQuantity = comboItem.Quantity * detail.Quantity;

                                            if (inventory == null || inventory.CurrentStock < requiredQuantity)
                                            {
                                                inventoryOk = false;
                                                var productName = product.Name ?? $"Producto #{comboItem.FastFoodItemID}";
                                                unavailableItems.Add($"{productName} (en combo {combo.Name})");
                                            }
                                        }
                                    }
                                    else if (product.ProductTypeID == 1) // Producto preparado
                                    {
                                        // Verificar stock de ingredientes para productos preparados
                                        if (product.Ingredients != null && product.Ingredients.Any())
                                        {
                                            foreach (var ingredient in product.Ingredients)
                                            {
                                                // Obtener el estado actual del ingrediente
                                                var ingredientResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Inventory/ingredients/{ingredient.IngredientID}");
                                                if (ingredientResponse?.Success == true)
                                                {
                                                    var ingredientInfo = JsonConvert.DeserializeObject<IngredientModel>(ingredientResponse.Data.ToString());
                                                    decimal requiredQuantity = ingredient.Quantity * comboItem.Quantity * detail.Quantity;

                                                    if (ingredientInfo == null || ingredientInfo.StockQuantity < requiredQuantity)
                                                    {
                                                        inventoryOk = false;
                                                        var productName = product.Name ?? $"Producto #{comboItem.FastFoodItemID}";
                                                        unavailableItems.Add($"{productName} (en combo {combo.Name}) - falta: {ingredientInfo?.Name ?? "ingrediente"}");
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Recalcular descuento por producto
                    detail.DiscountAmount = detail.UnitPrice * detail.Quantity * (detail.DiscountPercentage / 100);

                }



                if (!inventoryOk)
                {
                    NotificationService.Notify(NotificationSeverity.Warning,
                        "Inventario insuficiente",
                        $"No hay suficiente inventario para: {string.Join(", ", unavailableItems)}",
                        6000);
                    return;
                }

                // Mapear a DTO para enviar a la API (valores en inglés para la BD)
                var orderDto = new OrderDTO
                {
                    OrderType = model.OrderType, // Ya tiene el valor en inglés
                    CustomerID = model.CustomerID,
                    TableID = model.TableID,
                    Notes = model.Notes,
                    DiscountAmount = CalculateDiscountAmount(), // Enviamos el monto calculado, no el porcentaje
                    OrderDetails = model.OrderDetails,
                    // Por defecto, el estado de pago es Pendiente pero guardamos "Pending"
                    PaymentStatus = "Pending"
                };

                // Calcular el total
                decimal total = CalculateTotal();

                // Preguntar si desea procesar pago inmediatamente
                var processPaymentNow = await DialogService.Confirm(
                    "¿Desea procesar el pago ahora?",
                    "Procesar Pago",
                    new ConfirmOptions() { OkButtonText = "Sí", CancelButtonText = "No, guardar sin pago" });

                // Crear la orden
                var response = await ApiClient.PostAsync<BaseResponseModel, OrderDTO>(
                    "api/Order", orderDto);

                if (response?.Success == true)
                {
                    var createdOrder = JsonConvert.DeserializeObject<OrderModel>(response.Data.ToString());

                    if (createdOrder != null)
                    {
                        // Actualizar inventario
                        await UpdateInventoryAfterOrder(createdOrder.ID);

                        // Imprimir ticket de cocina independientemente de si se procesa el pago o no
                        await PrintKitchenTicket(createdOrder);

                        if (processPaymentNow == true)
                        {
                            // Redirigir a la página de pago específica para esta orden
                            NavigationManager.NavigateTo($"/orders/payment/{createdOrder.ID}");
                        }
                        else
                        {
                            // La orden se creó sin pago
                            NotificationService.Notify(NotificationSeverity.Success,
                                "Éxito", "Pedido creado exitosamente (pendiente de pago)", 4000);
                            NavigationManager.NavigateTo("/orders");
                        }
                    }
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al crear pedido", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al crear pedido: {ex.Message}", 4000);
            }
        }
        private decimal CalculateDiscountAmount()
        {
            // Calcular el descuento general (aplica sobre el subtotal después de descuentos por producto)
            decimal subtotal = CalculateSubtotal();
            return Math.Round(subtotal * (model.DiscountPercentage / 100), 2);
        }

        private async Task PrintKitchenTicket(OrderModel order)
        {
            // Preparar datos para impresión solamente del ticket de cocina
            var printData = new
            {
                orderNumber = order.OrderNumber,
                customerName = customers.FirstOrDefault(c => c.ID == model.CustomerID)?.Name ?? "Cliente General",
                table = availableTables.FirstOrDefault(t => t.ID == model.TableID)?.TableNumber.ToString() ?? "",
                orderType = GetOrderTypeDisplayName(model.OrderType),
                items = model.OrderDetails.Select(d => new
                {
                    name = d.ItemName,
                    quantity = d.Quantity,
                    price = d.UnitPrice,
                    discountPercentage = d.DiscountPercentage,
                    discountAmount = d.DiscountAmount,
                    notes = d.Notes,
                    isCombo = d.ItemType == "Combo",
                    comboItems = (d.ComboItems ?? new List<ComboItemDetail>()).Select(ci => new
                    {
                        name = ci.ItemName,
                        quantity = ci.Quantity,
                        specialInstructions = ci.SpecialInstructions
                    }).ToList(),
                    customizations = d.Customizations.Select(c => new
                    {
                        type = TranslateCustomizationType(c.CustomizationType),
                        name = c.IngredientName,
                        quantity = c.Quantity
                    }).ToList()
                }).ToList(),
                generalDiscountPercentage = model.DiscountPercentage,
                generalDiscountAmount = CalculateDiscountAmount(),
                notes = model.Notes
            };

            // Imprimir sólo el ticket de cocina
            await JSRuntime.InvokeVoidAsync("printerService.printKitchenTicket", printData);
        }
        // Método para traducir el tipo de pedido para mostrar
        private string GetOrderTypeDisplayName(string orderType)
        {
            return orderType switch
            {
                "DineIn" => "En sitio",
                "TakeOut" => "Pide y Espera",
                "Delivery" => "Express",
                _ => orderType
            };
        }

        private string GetPaymentMethodName(string method)
        {
            return method switch
            {
                "Cash" => "Efectivo",
                "CreditCard" => "Tarjeta de Crédito",
                "DebitCard" => "Tarjeta de Débito",
                "Transfer" => "Transferencia",
                "SINPE" => "SINPE Móvil",
                "Other" => "Otro",
                _ => method
            };
        }

        private async Task UpdateInventoryAfterOrder(int orderId)
        {
            try
            {
                // Para cada detalle de la orden, actualizar el inventario correspondiente
                foreach (var detail in model.OrderDetails)
                {
                    if (detail.ItemType == "Product")
                    {
                        // Primero verificar qué tipo de producto es
                        var product = products.FirstOrDefault(p => p.ID == detail.ItemID);
                        if (product == null) continue;

                        if (product.ProductTypeID == 2) // Tipo Inventario
                        {
                            // Obtener primero el inventario del producto para verificar su existencia
                            var productInventoryResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/ProductInventory/product/{detail.ItemID}");
                            if (productInventoryResponse?.Success == true)
                            {
                                var inventory = JsonConvert.DeserializeObject<ProductInventoryModel>(productInventoryResponse.Data.ToString());
                                if (inventory != null)
                                {
                                    // Actualizar inventario del producto
                                    await ApiClient.PostAsync<BaseResponseModel, StockMovementDTO>(
                                        "api/ProductInventory/movement",
                                        new StockMovementDTO
                                        {
                                            ProductInventoryID = inventory.ID, // Usar el ID del inventario, NO el ID del producto
                                            MovementType = "Salida",
                                            Quantity = detail.Quantity,
                                            Notes = $"Venta en orden #{orderId}"
                                        });
                                }
                            }
                        }
                        else if (product.ProductTypeID == 1) // Tipo Preparado
                        {
                            // Para productos preparados, ajustar el inventario de ingredientes
                            if (product.Ingredients != null && product.Ingredients.Any())
                            {
                                foreach (var ingredient in product.Ingredients)
                                {
                                    // Calcular la cantidad total del ingrediente a restar
                                    decimal totalQuantity = ingredient.Quantity * detail.Quantity;

                                    // Registrar el consumo del ingrediente
                                    await ApiClient.PostAsync<BaseResponseModel, InventoryTransactionDTO>(
                                        "api/Inventory/transactions",
                                        new InventoryTransactionDTO
                                        {
                                            IngredientID = ingredient.IngredientID,
                                            TransactionType = "Consumption",
                                            Quantity = totalQuantity,
                                            UnitPrice = 0, // No aplica para consumo
                                            Notes = $"Consumo en producto #{product.ID}, orden #{orderId}",
                                            TransactionDate = DateTime.Now
                                        });
                                }
                            }
                        }
                    }
                    else if (detail.ItemType == "Combo")
                    {
                        // Para combos, necesitamos obtener sus componentes y actualizar cada uno
                        var comboResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Combo/{detail.ItemID}");
                        if (comboResponse?.Success == true)
                        {
                            var combo = JsonConvert.DeserializeObject<ComboModel>(comboResponse.Data.ToString());
                            if (combo?.Items != null)
                            {
                                foreach (var comboItem in combo.Items)
                                {
                                    // Obtener el producto para saber su tipo
                                    var product = products.FirstOrDefault(p => p.ID == comboItem.FastFoodItemID);
                                    if (product == null) continue;

                                    if (product.ProductTypeID == 2) // Tipo Inventario
                                    {
                                        // Obtener primero el inventario del producto
                                        var productInventoryResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/ProductInventory/product/{comboItem.FastFoodItemID}");
                                        if (productInventoryResponse?.Success == true)
                                        {
                                            var inventory = JsonConvert.DeserializeObject<ProductInventoryModel>(productInventoryResponse.Data.ToString());
                                            if (inventory != null)
                                            {
                                                // Actualizar inventario del producto en el combo
                                                await ApiClient.PostAsync<BaseResponseModel, StockMovementDTO>(
                                                    "api/ProductInventory/movement",
                                                    new StockMovementDTO
                                                    {
                                                        ProductInventoryID = inventory.ID, // Usar el ID del inventario
                                                        MovementType = "Salida",
                                                        Quantity = comboItem.Quantity * detail.Quantity,
                                                        Notes = $"Venta en combo #{combo.ID}, orden #{orderId}"
                                                    });
                                            }
                                        }
                                    }
                                    else if (product.ProductTypeID == 1) // Tipo Preparado
                                    {
                                        // Para productos preparados, ajustar el inventario de ingredientes
                                        if (product.Ingredients != null && product.Ingredients.Any())
                                        {
                                            foreach (var ingredient in product.Ingredients)
                                            {
                                                // Calcular la cantidad total del ingrediente a restar
                                                decimal totalQuantity = ingredient.Quantity * comboItem.Quantity * detail.Quantity;

                                                // Registrar el consumo del ingrediente
                                                await ApiClient.PostAsync<BaseResponseModel, InventoryTransactionDTO>(
                                                    "api/Inventory/transactions",
                                                    new InventoryTransactionDTO
                                                    {
                                                        IngredientID = ingredient.IngredientID,
                                                        TransactionType = "Consumption",
                                                        Quantity = totalQuantity,
                                                        UnitPrice = 0, // No aplica para consumo
                                                        Notes = $"Consumo en producto #{product.ID}, combo #{combo.ID}, orden #{orderId}",
                                                        TransactionDate = DateTime.Now
                                                    });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Warning,
                    "Advertencia", $"La orden se creó, pero hubo problemas actualizando el inventario: {ex.Message}", 6000);
            }
        }

        private class OrderFormModel
        {
            public string OrderType { get; set; } = "DineIn";
            public int? CustomerID { get; set; }
            public int? TableID { get; set; }
            public string Notes { get; set; } = string.Empty;
            //public decimal DiscountAmount { get; set; } = 0;
            public List<OrderDetailDTO> OrderDetails { get; set; } = new();
            public string PaymentMethod { get; set; } = "Cash"; // Valor predeterminado en inglés
            public decimal DiscountPercentage { get; set; } = 0; // Cambiado de DiscountAmount a DiscountPercentage

        }
    }
}
