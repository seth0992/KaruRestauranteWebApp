using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using System.Diagnostics;

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
        private List<FastFoodItemModel>? products;
        private List<ComboModel>? combos;
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            await LoadOrder();
            isLoading = false;
        }

        private async Task LoadOrder()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Order/{OrderId}");
                if (response?.Success == true)
                {
                    order = JsonConvert.DeserializeObject<OrderModel>(response.Data.ToString());

                    // Cargar productos y combos antes de actualizar la UI
                    await LoadProductsAndCombos();

                    // Forzar actualización de la UI después de cargar todos los datos
                    StateHasChanged();
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
                    products = JsonConvert.DeserializeObject<List<FastFoodItemModel>>(productsResponse.Data.ToString());
                    Console.WriteLine($"Productos cargados: {products?.Count ?? 0}");
                }
                else
                {
                    Console.WriteLine("Error al cargar productos: " + productsResponse?.ErrorMessage);
                }

                // Cargar combos
                var combosResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Combo");
                if (combosResponse?.Success == true)
                {
                    combos = JsonConvert.DeserializeObject<List<ComboModel>>(combosResponse.Data.ToString());
                    Console.WriteLine($"Combos cargados: {combos?.Count ?? 0}");

                    // Si hay combos, asegurarse de que tengan cargados sus elementos
                    if (combos != null)
                    {
                        foreach (var combo in combos)
                        {
                            if (combo.Items != null)
                            {
                                foreach (var item in combo.Items)
                                {
                                    // Asegurarse de que el producto dentro del combo tenga nombre
                                    if (item.FastFoodItem == null && products != null)
                                    {
                                        item.FastFoodItem = products.FirstOrDefault(p => p.ID == item.FastFoodItemID);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error al cargar combos: " + combosResponse?.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en LoadProductsAndCombos: {ex.Message}");
                Debug.WriteLine($"Error en LoadProductsAndCombos: {ex}");
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

        private string GetInvoiceStatusName(string status)
        {
            return status switch
            {
                "Generated" => "Generada",
                "Sent" => "Enviada",
                "Accepted" => "Aceptada",
                "Rejected" => "Rechazada",
                _ => status
            };
        }

        private string GetPaymentMethodName(string method)
        {
            return method switch
            {
                "Cash" => "Efectivo",
                "CreditCard" => "Tarjeta de Crédito",
                "DebitCard" => "Tarjeta de Débito",
                "SIMPE" => "SIMPE Móvil",
                "Transfer" => "Transferencia",
                "Other" => "Otro",
                _ => method
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

