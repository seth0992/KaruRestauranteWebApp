﻿using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;
using System.Diagnostics;

namespace KaruRestauranteWebApp.Web.Components.Pages.Orders
{
    public partial class KitchenView : IDisposable
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

        private List<OrderModel> pendingOrders = new();
        private List<OrderModel> filteredOrders = new();
        private List<OrderModel> needsAttentionOrders = new();
        private List<FastFoodItemModel> products = new();
        private List<ComboModel> combos = new();
        private bool isLoading = true;
        private System.Timers.Timer? refreshTimer;
        private string selectedStatus = "All";
        private string sortBy = "CreatedAt";

        private string[] orderStatuses = { "All", "Pending", "InProgress" };
        private Dictionary<string, string> sortOptions = new()
        {
            { "CreatedAt", "Tiempo de espera" },
            { "OrderNumber", "Número de orden" },
            { "TableNumber", "Número de mesa" }
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadPendingOrders();

            // Configurar temporizador para actualizar automáticamente
            refreshTimer = new System.Timers.Timer(30000); // 30 segundos
            refreshTimer.Elapsed += async (sender, e) => await RefreshOrdersTimerCallback();
            refreshTimer.AutoReset = true;
            refreshTimer.Enabled = true;
        }

        private async Task RefreshOrdersTimerCallback()
        {
            await InvokeAsync(async () =>
            {
                await LoadPendingOrders();
                StateHasChanged();
            });
        }

        public async Task LoadPendingOrders()
        {
            try
            {
                isLoading = true;
                StateHasChanged();

                // Cargar productos y combos primero para asegurar que estén disponibles
                await LoadProductsAndCombos();

                // Modificar para cargar órdenes con todos sus detalles
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Order");
                if (response?.Success == true)
                {
                    var allOrders = JsonConvert.DeserializeObject<List<OrderModel>>(response.Data.ToString()) ?? new();

                    // Filtrar pedidos pendientes o en proceso
                    pendingOrders = new List<OrderModel>();

                    foreach (var order in allOrders.Where(o => o.OrderStatus == "Pending" || o.OrderStatus == "InProgress"))
                    {
                        // Obtener detalles completos de cada orden
                        var detailedResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Order/{order.ID}");
                        if (detailedResponse?.Success == true)
                        {
                            var detailedOrder = JsonConvert.DeserializeObject<OrderModel>(detailedResponse.Data.ToString());
                            if (detailedOrder != null)
                            {
                                pendingOrders.Add(detailedOrder);
                            }
                        }
                    }

                    // Identificar pedidos que necesitan atención (más de 20 minutos de espera)
                    needsAttentionOrders = pendingOrders
                        .Where(o => (DateTime.Now - o.CreatedAt).TotalMinutes > 20)
                        .ToList();

                    ApplyFilters();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al cargar pedidos", 4000);
                }

                isLoading = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                isLoading = false;
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar pedidos: {ex.Message}", 4000);
                StateHasChanged();
            }
        }

        private async Task LoadProductsAndCombos()
        {
            try
            {
                // Cargar productos primero
                var productsResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/FastFood");
                if (productsResponse?.Success == true)
                {
                    products = JsonConvert.DeserializeObject<List<FastFoodItemModel>>(productsResponse.Data.ToString()) ?? new();
                    Debug.WriteLine($"Productos cargados: {products.Count}");
                }
                else
                {
                    Debug.WriteLine("Error al cargar productos: " + productsResponse?.ErrorMessage);
                }

                // Luego cargar combos
                var combosResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Combo");
                if (combosResponse?.Success == true)
                {
                    combos = JsonConvert.DeserializeObject<List<ComboModel>>(combosResponse.Data.ToString()) ?? new();
                    Debug.WriteLine($"Combos cargados: {combos.Count}");

                    // Complementar los productos dentro de cada combo
                    foreach (var combo in combos)
                    {
                        if (combo.Items != null)
                        {
                            foreach (var item in combo.Items)
                            {
                                // Asociar el producto si no está ya
                                if (item.FastFoodItem == null)
                                {
                                    item.FastFoodItem = products.FirstOrDefault(p => p.ID == item.FastFoodItemID);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Error al cargar combos: " + combosResponse?.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en LoadProductsAndCombos: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Warning,
                    "Advertencia", "No se pudieron cargar los nombres de algunos productos", 4000);
            }
        }

        private List<(string Name, int Quantity)> GetComboItems(OrderDetailModel detail)
        {
            var result = new List<(string Name, int Quantity)>();

            if (detail.ItemType == "Combo")
            {
                var combo = combos.FirstOrDefault(c => c.ID == detail.ItemID);
                if (combo?.Items != null && combo.Items.Any())
                {
                    foreach (var item in combo.Items)
                    {
                        string productName = item.FastFoodItem?.Name ??
                            products.FirstOrDefault(p => p.ID == item.FastFoodItemID)?.Name ??
                            $"Producto #{item.FastFoodItemID}";

                        result.Add((productName, item.Quantity));
                    }
                }
            }

            return result;
        }

        private void ApplyFilters()
        {
            if (pendingOrders == null) return;

            var query = pendingOrders.AsEnumerable();

            // Filtrar por estado
            if (selectedStatus != "All")
            {
                query = query.Where(o => o.OrderStatus == selectedStatus);
            }

            // Ordenar
            query = sortBy switch
            {
                "CreatedAt" => query.OrderBy(o => o.CreatedAt),
                "OrderNumber" => query.OrderBy(o => o.OrderNumber),
                "TableNumber" => query.OrderBy(o => o.Table != null ? o.Table.TableNumber : 9999),
                _ => query.OrderBy(o => o.CreatedAt)
            };

            filteredOrders = query.ToList();
        }

        private async Task StartPreparation(OrderDetailModel detail)
        {
            try
            {
                var response = await ApiClient.PatchAsync<BaseResponseModel>(
                    $"api/Order/details/{detail.ID}/status/InPreparation");

                if (response?.Success == true)
                {
                    detail.Status = "InPreparation";

                    // Actualizar estado de la orden a "InProgress" si estaba en "Pending"
                    var order = pendingOrders.FirstOrDefault(o => o.ID == detail.OrderID);
                    if (order != null && order.OrderStatus == "Pending")
                    {
                        await UpdateOrderStatus(order.ID, "InProgress");
                        order.OrderStatus = "InProgress";
                    }

                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Preparación iniciada", 2000);
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al actualizar estado", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error: {ex.Message}", 4000);
            }
        }

        private async Task MarkDetailReady(OrderDetailModel detail)
        {
            try
            {
                var response = await ApiClient.PatchAsync<BaseResponseModel>(
                    $"api/Order/details/{detail.ID}/status/Ready");

                if (response?.Success == true)
                {
                    detail.Status = "Ready";

                    // Verificar si todos los detalles están listos
                    var order = pendingOrders.FirstOrDefault(o => o.ID == detail.OrderID);
                    if (order != null && order.OrderDetails.All(d => d.Status == "Ready"))
                    {
                        NotificationService.Notify(NotificationSeverity.Info,
                            "Información", "Todos los productos están listos. ¿Desea marcar el pedido como listo?", 5000);
                    }

                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Producto marcado como listo", 2000);
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al actualizar estado", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error: {ex.Message}", 4000);
            }
        }

        private async Task MarkOrderReady(OrderModel order)
        {
            try
            {
                var response = await ApiClient.PatchAsync<BaseResponseModel>(
                    $"api/Order/{order.ID}/status/Ready");

                if (response?.Success == true)
                {
                    order.OrderStatus = "Ready";

                    // Marcar todos los detalles como listos si no lo están
                    foreach (var detail in order.OrderDetails)
                    {
                        if (detail.Status != "Ready")
                        {
                            await ApiClient.PatchAsync<BaseResponseModel>(
                                $"api/Order/details/{detail.ID}/status/Ready");
                            detail.Status = "Ready";
                        }
                    }

                    // Reproducir sonido de notificación
                    await JSRuntime.InvokeVoidAsync("playNotificationSound");

                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", $"Pedido {order.OrderNumber} marcado como listo", 3000);

                    // Recargar pedidos después de marcar como listo
                    await LoadPendingOrders();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al actualizar estado", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error: {ex.Message}", 4000);
            }
        }

        private async Task UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                await ApiClient.PatchAsync<BaseResponseModel>($"api/Order/{orderId}/status/{status}");
            }
            catch (Exception ex)
            {
                // Solo registrar error, no interrumpir flujo
                Debug.WriteLine($"Error al actualizar estado de orden: {ex.Message}");
            }
        }

        private async Task ShowCustomizations(IEnumerable<OrderItemCustomizationModel> customizations)
        {
            await DialogService.OpenAsync<CustomizationsDialog>("Personalizaciones",
                new Dictionary<string, object>
                {
                    { "Customizations", customizations }
                },
                new DialogOptions
                {
                    Width = "500px",
                    Height = "400px",
                    CloseDialogOnOverlayClick = true
                });
        }

        private async Task ScrollToOrder(int orderId)
        {
            var order = filteredOrders.FirstOrDefault(o => o.ID == orderId);
            if (order != null)
            {
                string elementId = $"order-{orderId}";

                // Usar JavaScript para hacer scroll hasta el elemento
                await JSRuntime.InvokeVoidAsync("scrollToElement", elementId);
            }
        }

        private bool CanMarkOrderReady(OrderModel order)
        {
            // Solo se puede marcar como listo si todos los productos están en preparación o listos
            return order.OrderStatus != "Ready" &&
                   order.OrderDetails.All(d => d.Status == "InPreparation" || d.Status == "Ready");
        }

        private string GetOrderTypeName(string type)
        {
            return type switch
            {
                "DineIn" => "En Sitio",
                "TakeOut" => "Para Llevar",
                "Delivery" => "Entrega",
                _ => type
            };
        }

        private string GetOrderStatusName(string status)
        {
            return status switch
            {
                "Pending" => "Pendiente",
                "InProgress" => "En Proceso",
                "Ready" => "Listo",
                "Delivered" => "Entregado",
                "Cancelled" => "Cancelado",
                _ => status
            };
        }

        private string GetDetailStatusName(string status)
        {
            return status switch
            {
                "Pending" => "Pendiente",
                "InPreparation" => "En Preparación",
                "Ready" => "Listo",
                "Delivered" => "Entregado",
                "Cancelled" => "Cancelado",
                _ => status
            };
        }

        private BadgeStyle GetOrderStatusBadgeStyle(string status)
        {
            return status switch
            {
                "Pending" => BadgeStyle.Warning,
                "InProgress" => BadgeStyle.Info,
                "Ready" => BadgeStyle.Success,
                "Delivered" => BadgeStyle.Primary,
                "Cancelled" => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }

        private BadgeStyle GetDetailStatusBadgeStyle(string status)
        {
            return status switch
            {
                "Pending" => BadgeStyle.Warning,
                "InPreparation" => BadgeStyle.Info,
                "Ready" => BadgeStyle.Success,
                "Delivered" => BadgeStyle.Primary,
                "Cancelled" => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }

        private string GetItemTypeIcon(string itemType)
        {
            return itemType == "Product" ? "fastfood" : "lunch_dining";
        }

        private string GetItemName(OrderDetailModel detail)
        {
            if (detail.ItemType == "Product")
            {
                var product = products.FirstOrDefault(p => p.ID == detail.ItemID);
                return product?.Name ?? $"Producto #{detail.ItemID}";
            }
            else if (detail.ItemType == "Combo")
            {
                var combo = combos.FirstOrDefault(c => c.ID == detail.ItemID);
                return combo?.Name ?? $"Combo #{detail.ItemID}";
            }
            return $"{detail.ItemType} #{detail.ItemID}";
        }

        private string GetTimeSinceCreated(DateTime createdAt)
        {
            TimeSpan timeSpan = DateTime.Now - createdAt;

            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            else
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }
        private async Task PrintKitchenTicket(OrderModel order)
        {
            try
            {
                // Preparar datos para imprimir
                var ticketData = new
                {
                    orderNumber = order.OrderNumber,
                    table = order.Table?.TableNumber.ToString() ?? "Para llevar",
                    createdAt = order.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    items = order.OrderDetails.Select(d => new
                    {
                        name = GetItemName(d),
                        quantity = d.Quantity,
                        notes = d.Notes,
                        isCombo = d.ItemType == "Combo",
                        comboItems = GetComboItems(d).Select(ci => new
                        {
                            name = ci.Name,
                            quantity = ci.Quantity
                        }).ToList(),
                        customizations = d.Customizations.Select(c => new
                        {
                            type = c.CustomizationType,
                            name = c.Ingredient?.Name ?? "",
                            quantity = c.Quantity
                        }).ToList()
                    }).ToList(),
                    notes = order.Notes
                };

                // Llamar a la función JavaScript para imprimir
                await JSRuntime.InvokeVoidAsync("printerService.printKitchenTicket", ticketData);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al imprimir ticket: {ex.Message}", 4000);
            }
        }

        public void Dispose()
        {
            // Limpiar el temporizador al desmontar el componente
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
        }
    }
}
