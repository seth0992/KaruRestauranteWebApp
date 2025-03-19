using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

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
        private string[] orderStatuses = { "Pending", "InProgress", "Ready", "Delivered", "Cancelled" };

        protected override async Task OnInitializedAsync()
        {
            await LoadOrders();
        }

        private async Task LoadOrders()
        {
            try
            {
                string queryParams = "";

                if (fromDate.HasValue)
                    queryParams += $"fromDate={fromDate.Value.ToString("yyyy-MM-dd")}&";

                if (toDate.HasValue)
                    queryParams += $"toDate={toDate.Value.ToString("yyyy-MM-dd")}&";

                string url = "api/Order";
                if (!string.IsNullOrEmpty(queryParams))
                    url += $"?{queryParams.TrimEnd('&')}";

                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>(url);

                if (response?.Success == true)
                {
                    var ordersList = JsonConvert.DeserializeObject<List<OrderModel>>(
                        response.Data.ToString());

                    // Filtrar por estado si se ha seleccionado uno
                    if (!string.IsNullOrEmpty(selectedStatus))
                    {
                        ordersList = ordersList?.Where(o => o.OrderStatus == selectedStatus).ToList();
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

        //private void ViewOrderDetails(DataGridRowMouseEventArgs<OrderModel> args)
        //{
        //    //NavigationManager.NavigateTo($"/orders/details/{args.Data.ID}");
        //}

        private void NavigateToEdit(int orderId)
        {
            NavigationManager.NavigateTo($"/orders/edit/{orderId}");
        }
    }
}
