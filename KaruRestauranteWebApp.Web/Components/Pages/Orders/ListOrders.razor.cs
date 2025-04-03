using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;

namespace KaruRestauranteWebApp.Web.Components.Pages.Orders
{
    public partial class ListOrders
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Inject]
        public required DialogService DialogService { get; set; }

        private IEnumerable<OrderModel>? orders;
        private DateTime? fromDate;
        private DateTime? toDate;
        private string? selectedStatus;
        private string? selectedOrderType;
        private string? customerSearch;
        private bool isLoading = true;
        private bool isShowingTodayOnly = false;

        // Opciones para los dropdowns
        private List<DropDownItem> orderStatuses = new List<DropDownItem>
        {
            new DropDownItem { Text = "Pendiente", Value = "Pending" },
            new DropDownItem { Text = "En Proceso", Value = "InProgress" },
            new DropDownItem { Text = "Listo", Value = "Ready" },
            new DropDownItem { Text = "Entregado", Value = "Delivered" },
            new DropDownItem { Text = "Cancelado", Value = "Cancelled" }
        };

        private List<DropDownItem> orderTypes = new List<DropDownItem>
        {
            new DropDownItem { Text = "En Sitio", Value = "DineIn" },
            new DropDownItem { Text = "Para Llevar", Value = "TakeOut" },
            new DropDownItem { Text = "Entrega", Value = "Delivery" }
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadOrders();
        }

        private async Task LoadOrders()
        {
            try
            {
                isLoading = true;
                StateHasChanged();

                // Construir la URL con todos los filtros aplicables
                var queryParams = new List<string>();

                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value.ToString("yyyy-MM-dd")}");

                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value.ToString("yyyy-MM-dd")}");

                // Incluir el filtro de estado directamente en la solicitud al servidor
                if (!string.IsNullOrEmpty(selectedStatus))
                    queryParams.Add($"status={selectedStatus}");

                string url = "api/Order";
                if (queryParams.Any())
                    url += $"?{string.Join("&", queryParams)}";

                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>(url);

                if (response?.Success == true)
                {
                    var ordersList = JsonConvert.DeserializeObject<List<OrderModel>>(
                        response.Data.ToString());

                    // Aplicar filtros adicionales que no se pueden enviar directamente al servidor
                    if (ordersList != null)
                    {
                        // Filtro por tipo de orden
                        if (!string.IsNullOrEmpty(selectedOrderType))
                        {
                            ordersList = ordersList.Where(o => o.OrderType == selectedOrderType).ToList();
                        }

                        // Filtro por búsqueda de cliente
                        if (!string.IsNullOrEmpty(customerSearch))
                        {
                            ordersList = ordersList.Where(o =>
                                o.Customer != null &&
                                o.Customer.Name != null &&
                                o.Customer.Name.Contains(customerSearch, StringComparison.OrdinalIgnoreCase)
                            ).ToList();
                        }
                    }

                    orders = ordersList?.AsEnumerable();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al cargar pedidos", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar pedidos: {ex.Message}", 4000);
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }
        private void ShowTodayOrders()
        {
            isShowingTodayOnly = !isShowingTodayOnly;

            if (isShowingTodayOnly)
            {
                fromDate = DateTime.Today;
                toDate = DateTime.Today.AddDays(1).AddTicks(-1);
            }
            else
            {
                fromDate = null;
                toDate = null;
            }

            LoadOrders();
        }

        private void FilterByStatus(string status)
        {
            if (selectedStatus == status)
            {
                selectedStatus = null; // Desactivar el filtro si ya estaba seleccionado
            }
            else
            {
                selectedStatus = status;
            }

            LoadOrders();
        }

        private void ClearFilters()
        {
            fromDate = null;
            toDate = null;
            selectedStatus = null;
            selectedOrderType = null;
            customerSearch = null;
            isShowingTodayOnly = false;

            LoadOrders();
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

        private BadgeStyle GetPaymentStatusBadgeStyle(string status)
        {
            return status switch
            {
                "Pending" => BadgeStyle.Warning,
                "Partially Paid" => BadgeStyle.Info,
                "Paid" => BadgeStyle.Success,
                "Cancelled" => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }

        private BadgeStyle GetOrderTypeBadgeStyle(string type)
        {
            return type switch
            {
                "DineIn" => BadgeStyle.Primary,
                "TakeOut" => BadgeStyle.Secondary,
                "Delivery" => BadgeStyle.Info,
                _ => BadgeStyle.Light
            };
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

        private string GetPaymentStatusName(string status)
        {
            return status switch
            {
                "Pending" => "Pendiente",
                "Partially Paid" => "Pago Parcial",
                "Paid" => "Pagado",
                "Cancelled" => "Cancelado",
                _ => status
            };
        }

        private async Task ShowDeleteConfirmation(OrderModel order)
        {
            var result = await DialogService.Confirm(
                $"¿Está seguro que desea cancelar el pedido {order.OrderNumber}?",
                "Confirmar Cancelación",
                new ConfirmOptions()
                {
                    OkButtonText = "Sí",
                    CancelButtonText = "No"
                });

            if (result == true)
            {
                await CancelOrder(order);
            }
        }

        private async Task CancelOrder(OrderModel order)
        {
            try
            {
                var response = await ApiClient.PatchAsync<BaseResponseModel>($"api/Order/{order.ID}/status/Cancelled");
                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Pedido cancelado exitosamente", 4000);
                    await LoadOrders();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al cancelar pedido", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cancelar pedido: {ex.Message}", 4000);
            }
        }

        private void NavigateToOrderDetails(int orderId)
        {
            NavigationManager.NavigateTo($"/orders/details/{orderId}");
        }

        private void NavigateToPayment(int orderId)
        {
            NavigationManager.NavigateTo($"/orders/payment/{orderId}");
        }

        private void NavigateToEditOrder(int orderId)
        {
            NavigationManager.NavigateTo($"/orders/edit/{orderId}");
        }

        // Método para colorear las filas según el estado
        private void RowRender(RowRenderEventArgs<OrderModel> args)
        {
            if (args.Data.OrderStatus == "Cancelled")
            {
                args.Attributes.Add("style", "background-color: var(--rz-danger-lighter); opacity: 0.7;");
            }
            else if (args.Data.OrderStatus == "Delivered")
            {
                args.Attributes.Add("style", "background-color: var(--rz-success-lighter);");
            }
            else if (args.Data.OrderStatus == "Ready")
            {
                args.Attributes.Add("style", "background-color: var(--rz-success-lighter-alpha);");
            }
            else if (args.Data.OrderStatus == "InProgress")
            {
                args.Attributes.Add("style", "background-color: var(--rz-info-lighter-alpha);");
            }
            else if (args.Data.CreatedAt.Date == DateTime.Today)
            {
                args.Attributes.Add("style", "background-color: var(--rz-warning-lighter-alpha);");
            }
        }
    }

    // Clase auxiliar para los dropdowns
    public class DropDownItem
    {
        public string Text { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}