using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using KaruRestauranteWebApp.Models.Models.Restaurant;

namespace KaruRestauranteWebApp.Web.Components.Pages.ProductInventory
{
    public partial class CreateProductInventory
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        private bool isLoaded;
        private ProductInventoryDTO model = new();
        private List<FastFoodItemModel> availableProducts = new();
        private List<FastFoodItemModel> productsWithInventory = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadProducts();
                isLoaded = true;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar datos: {ex.Message}", 4000);
            }
        }

        private async Task LoadProducts()
        {
            // Cargar todos los productos
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/FastFood");
            if (response?.Success == true)
            {
                var allProducts = JsonConvert.DeserializeObject<List<FastFoodItemModel>>(response.Data.ToString()) ?? new();

                // Cargar productos que ya tienen inventario
                var inventoryResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/ProductInventory");
                if (inventoryResponse?.Success == true)
                {
                    var inventories = JsonConvert.DeserializeObject<List<ProductInventoryModel>>(inventoryResponse.Data.ToString()) ?? new();
                    var productIdsWithInventory = inventories.Select(i => i.FastFoodItemID).ToHashSet();

                    // Filtrar productos que no tienen inventario
                    availableProducts = allProducts.Where(p => !productIdsWithInventory.Contains(p.ID)).ToList();
                }
                else
                {
                    availableProducts = allProducts;
                }
            }
        }

        private async Task HandleSubmit()
        {
            try
            {
                if (model.FastFoodItemID <= 0)
                {
                    NotificationService.Notify(NotificationSeverity.Warning,
                        "Validación", "Debe seleccionar un producto", 4000);
                    return;
                }

                var response = await ApiClient.PostAsync<BaseResponseModel, ProductInventoryDTO>(
                    "api/ProductInventory", model);

                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Inventario creado exitosamente", 4000);
                    NavigationManager.NavigateTo("/product-inventory");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al crear inventario", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al crear inventario: {ex.Message}", 4000);
            }
        }
    }
}
