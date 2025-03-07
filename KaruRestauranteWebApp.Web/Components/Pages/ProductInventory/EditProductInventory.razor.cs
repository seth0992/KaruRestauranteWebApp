using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.ProductInventory
{
    public partial class EditProductInventory
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Parameter]
        public int Id { get; set; }

        private bool isLoaded;
        private ProductInventoryDTO model = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadInventory();
                isLoaded = true;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar datos: {ex.Message}", 4000);
                NavigationManager.NavigateTo("/product-inventory");
            }
        }

        private async Task LoadInventory()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/ProductInventory/{Id}");
            if (response?.Success == true)
            {
                var inventory = JsonConvert.DeserializeObject<ProductInventoryModel>(response.Data.ToString());
                if (inventory != null)
                {
                    model = new ProductInventoryDTO
                    {
                        ID = inventory.ID,
                        FastFoodItemID = inventory.FastFoodItemID,
                        ProductName = inventory.FastFoodItem?.Name,
                        CurrentStock = inventory.CurrentStock,
                        MinimumStock = inventory.MinimumStock,
                        PurchasePrice = inventory.PurchasePrice,
                        SuggestedMarkup = inventory.SuggestedMarkup,
                        LastRestockDate = inventory.LastRestockDate,
                        SKU = inventory.SKU,
                        UnitOfMeasure = inventory.UnitOfMeasure,
                        LocationCode = inventory.LocationCode
                    };
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", "No se encontró el inventario", 4000);
                    NavigationManager.NavigateTo("/product-inventory");
                }
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", "Error al cargar el inventario", 4000);
                NavigationManager.NavigateTo("/product-inventory");
            }
        }

        private async Task HandleSubmit()
        {
            try
            {
                var response = await ApiClient.PutAsync<BaseResponseModel, ProductInventoryDTO>(
                    $"api/ProductInventory/{Id}", model);

                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Inventario actualizado exitosamente", 4000);
                    NavigationManager.NavigateTo("/product-inventory");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al actualizar inventario", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al actualizar inventario: {ex.Message}", 4000);
            }
        }
    }
}
