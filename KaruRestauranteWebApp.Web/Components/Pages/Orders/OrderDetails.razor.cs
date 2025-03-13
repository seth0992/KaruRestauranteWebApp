using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Orders
{
    public partial class OrderDetails
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

        private OrderModel? order;
        private FastFoodItemModel[]? products;
        private ComboModel[]? combos;

        protected override async Task OnInitializedAsync()
        {
            await LoadOrder();
        }

        private async Task LoadOrder()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Order/{OrderId}");
                if (response?.Success == true)
                {
                    order = JsonConvert.DeserializeObject<OrderModel>(response.Data.ToString());

                    // Cargar productos y combos para mostrar nombres
                    await LoadProductsAndCombos();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al cargar el pedido", 4000);
                    NavigationManager.NavigateTo("/orders");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar el pedido: {ex.Message}", 4000);
                NavigationManager.NavigateTo("/orders");
            }
        }
        private async Task LoadProductsAndCombos()
        {
            try
            {
                // Cargar productos
                var productsResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/FastFood");
                if (productsResponse?.Success == true)
                {
                    products = JsonConvert.DeserializeObject<FastFoodItemModel[]>(productsResponse.Data.ToString());
                }

                // Cargar combos
                var combosResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Combo");
                if (combosResponse?.Success == true)
                {
                    combos = JsonConvert.DeserializeObject<ComboModel[]>(combosResponse.Data.ToString());
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Warning,
                    "Advertencia", "No se pudieron cargar los nombres de algunos productos", 4000);
            }
        }

        private string GetItemName(OrderDetailModel detail)
        {
            if (detail.ItemType == "Product" && products != null)
            {
                var product = products.FirstOrDefault(p => p.ID == detail.ItemID);
                return product?.Name ?? $"Producto #{detail.ItemID}";
            }
            else if (detail.ItemType == "Combo" && combos != null)
            {
                var combo = combos.FirstOrDefault(c => c.ID == detail.ItemID);
                return combo?.Name ?? $"Combo #{detail.ItemID}";
            }
            return $"{detail.ItemType} #{detail.ItemID}";
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

        private BadgeStyle GetInvoiceStatusBadgeStyle(string status)
        {
            return status switch
            {
                "Generated" => BadgeStyle.Warning,
                "Sent" => BadgeStyle.Info,
                "Accepted" => BadgeStyle.Success,
                "Rejected" => BadgeStyle.Danger,
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
    }
}

