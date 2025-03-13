using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models.Orders;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Orders
{

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

            private bool isLoaded;
            private OrderModel? order;
            private OrderDTO model = new();
            private List<CustomerModel> customers = new();
            private List<TableModel> availableTables = new();
            private List<FastFoodItemModel> products = new();
            private List<ComboModel> combos = new();
            private List<IngredientModel> ingredients = new();
            private string[] orderTypes = new[] { "DineIn", "TakeOut", "Delivery" };

            protected override async Task OnInitializedAsync()
            {
                try
                {
                    await LoadData();
                    await LoadOrder();
                    isLoaded = true;
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
                            model = new OrderDTO
                            {
                                ID = order.ID,
                                OrderNumber = order.OrderNumber,
                                CustomerID = order.CustomerID,
                                TableID = order.TableID,
                                OrderType = order.OrderType,
                                Notes = order.Notes,
                                DiscountAmount = order.DiscountAmount,
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

                                var detailDto = new OrderDetailDTO
                                {
                                    ID = detail.ID,
                                    OrderID = detail.OrderID,
                                    ItemType = detail.ItemType,
                                    ItemID = detail.ItemID,
                                    ItemName = itemName,
                                    Quantity = detail.Quantity,
                                    UnitPrice = detail.UnitPrice,
                                    SubTotal = detail.SubTotal,
                                    Notes = detail.Notes,
                                    Status = detail.Status,
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
                // Implementar diálogo para crear nuevo cliente
                // Agregar la lógica aquí
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

                        // Añadir producto al pedido
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

                        // Añadir combo al pedido
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
                        { "ProductIngredients", product.Ingredients },
                        { "Customizations", detail.Customizations },
                        { "AllIngredients", ingredients }
                        },
                        new DialogOptions
                        {
                            Width = "600px",
                            Height = "500px",
                            CloseDialogOnOverlayClick = false
                        });

                    if (customizations != null)
                    {
                        var customizationsList = customizations as List<OrderItemCustomizationDTO>;
                        if (customizationsList != null)
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

                    var response = await ApiClient.PutAsync<BaseResponseModel, OrderDTO>(
                        $"api/Order/{OrderId}", model);

                    if (response?.Success == true)
                    {
                        NotificationService.Notify(NotificationSeverity.Success,
                            "Éxito", "Pedido actualizado exitosamente", 4000);
                        NavigationManager.NavigateTo("/orders");
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
        }
    }
