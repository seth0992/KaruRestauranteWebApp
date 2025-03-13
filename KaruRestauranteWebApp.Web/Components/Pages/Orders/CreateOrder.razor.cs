using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models.Orders;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
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
        private string[] orderTypes = new[] { "DineIn", "TakeOut", "Delivery" };
        private object[] paymentMethods = new[]
        {
            new { value = "Cash", name = "Efectivo" },
            new { value = "CreditCard", name = "Tarjeta de Crédito" },
            new { value = "DebitCard", name = "Tarjeta de Débito" },
            new { value = "Transfer", name = "Transferencia" },
            new { value = "Other", name = "Otro" }
        };

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

        private void AddComboToOrder(ComboModel combo)
        {
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
                    Customizations = new List<OrderItemCustomizationDTO>()
                };

                model.OrderDetails.Add(detail);
                CalculateTotal();
            }

            // Notificar al usuario que se agregó el combo
            NotificationService.Notify(NotificationSeverity.Success,
                "Combo agregado", $"{combo.Name} agregado al pedido", 2000);
        }

        private async Task OpenProductSelectionDialog()
        {
            try
            {
                // Usando Radzen DialogService para mostrar un diálogo de selección de productos
                var selectedProduct = await DialogService.OpenAsync<ProductSelectionDialog>("Seleccionar Producto",
                    new Dictionary<string, object>
                    {
                        { "Products", products.Where(p => p.IsAvailable).ToList() }
                    },
                    new DialogOptions
                    {
                        Width = "700px",
                        Height = "530px",
                        CloseDialogOnOverlayClick = false
                    });

                if (selectedProduct != null)
                {
                    var product = (FastFoodItemModel)selectedProduct;
                    AddProductToOrder(product);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al seleccionar producto: {ex.Message}", 4000);
            }
        }

        private async Task OpenComboSelectionDialog()
        {
            try
            {
                // Usando Radzen DialogService para mostrar un diálogo de selección de combos
                var selectedCombo = await DialogService.OpenAsync<ComboSelectionDialog>("Seleccionar Combo",
                    new Dictionary<string, object>
                    {
                        { "Combos", combos.Where(c => c.IsAvailable).ToList() }
                    },
                    new DialogOptions
                    {
                        Width = "700px",
                        Height = "530px",
                        CloseDialogOnOverlayClick = false
                    });

                if (selectedCombo != null)
                {
                    var combo = (ComboModel)selectedCombo;
                    AddComboToOrder(combo);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al seleccionar combo: {ex.Message}", 4000);
            }
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
            detail.SubTotal = detail.UnitPrice * detail.Quantity;
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
            return subtotal + tax - model.DiscountAmount;
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

                // Mapear a DTO para enviar a la API
                var orderDto = new OrderDTO
                {
                    OrderType = model.OrderType,
                    CustomerID = model.CustomerID,
                    TableID = model.TableID,
                    Notes = model.Notes,
                    DiscountAmount = model.DiscountAmount,
                    OrderDetails = model.OrderDetails
                };

                var response = await ApiClient.PostAsync<BaseResponseModel, OrderDTO>(
                    "api/Order", orderDto);

                if (response?.Success == true)
                {
                    var createdOrder = JsonConvert.DeserializeObject<OrderModel>(response.Data.ToString());

                    // Si se creó exitosamente y hay método de pago, registramos el pago
                    if (createdOrder != null && model.OrderDetails.Any())
                    {
                        // Registrar pago si el pedido está pagado
                        if (!string.IsNullOrEmpty(model.PaymentMethod))
                        {
                            var paymentDto = new PaymentDTO
                            {
                                OrderID = createdOrder.ID,
                                PaymentMethod = model.PaymentMethod,
                                Amount = CalculateTotal(),
                                ReferenceNumber = "",
                                Notes = "Pago realizado al crear la orden"
                            };

                            await ApiClient.PostAsync<BaseResponseModel, PaymentDTO>(
                                $"api/Order/{createdOrder.ID}/payments", paymentDto);
                        }
                    }

                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Pedido creado exitosamente", 4000);
                    NavigationManager.NavigateTo("/orders");
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

        private class OrderFormModel
        {
            public string OrderType { get; set; } = "DineIn";
            public int? CustomerID { get; set; }
            public int? TableID { get; set; }
            public string Notes { get; set; } = string.Empty;
            public decimal DiscountAmount { get; set; } = 0;
            public List<OrderDetailDTO> OrderDetails { get; set; } = new();
            public string PaymentMethod { get; set; } = "Cash"; // Valor predeterminado en inglés
        }
    }

}
