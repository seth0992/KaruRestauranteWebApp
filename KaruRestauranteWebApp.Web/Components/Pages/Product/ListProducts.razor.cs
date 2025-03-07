using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Product
{
    public partial class ListProducts
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Inject]
        public required DialogService DialogService { get; set; }

        private IEnumerable<FastFoodItemModel>? products;
        private List<ProductTypeModel> productTypes = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadProductTypes();
            await LoadProducts();
        }

        private async Task LoadProductTypes()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/ProductType");
                if (response?.Success == true)
                {
                    productTypes = JsonConvert.DeserializeObject<List<ProductTypeModel>>(
                        response.Data.ToString()) ?? new();
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar tipos de productos: {ex.Message}", 4000);
            }
        }

        private async Task LoadProducts()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/FastFood");
                if (response?.Success == true)
                {
                    var productsList = JsonConvert.DeserializeObject<List<FastFoodItemModel>>(
                        response.Data.ToString());
                    products = productsList?.AsEnumerable();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", "Error al cargar productos", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar productos: {ex.Message}", 4000);
            }
        }

        private string GetProductTypeName(int productTypeId)
        {
            var productType = productTypes.FirstOrDefault(pt => pt.ID == productTypeId);
            return productType?.Name ?? "Desconocido";
        }

        private BadgeStyle GetProductTypeBadgeStyle(int productTypeId)
        {
            // Asumiendo que ID 1 es "Preparado" e ID 2 es "Inventario"
            return productTypeId == 1 ? BadgeStyle.Info : BadgeStyle.Secondary;
        }

        private async Task ShowDeleteConfirmation(FastFoodItemModel product)
        {
            var result = await DialogService.Confirm(
                $"¿Está seguro que desea eliminar el producto {product.Name}?",
                "Confirmar Eliminación",
                new ConfirmOptions()
                {
                    OkButtonText = "Sí",
                    CancelButtonText = "No"
                });

            if (result == true)
            {
                await DeleteProduct(product);
            }
        }

        private async Task DeleteProduct(FastFoodItemModel product)
        {
            try
            {
                var response = await ApiClient.DeleteAsync<BaseResponseModel>($"api/FastFood/{product.ID}");
                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Producto eliminado exitosamente", 4000);
                    await LoadProducts();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al eliminar producto", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al eliminar producto: {ex.Message}", 4000);
            }
        }
    }
}
