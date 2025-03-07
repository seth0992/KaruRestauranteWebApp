using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.ProductInventory
{
    public partial class ListProductInventory
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Inject]
        public required DialogService DialogService { get; set; }

        private IEnumerable<ProductInventoryModel>? inventories;
        private List<ProductInventoryModel> lowStockItems = new();
        private StockMovementDTO stockMovement = new();
        private ProductInventoryModel? selectedInventory;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }
        private async Task LoadData()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/ProductInventory");
                if (response?.Success == true)
                {
                    var inventoryList = JsonConvert.DeserializeObject<List<ProductInventoryModel>>(
                        response.Data.ToString());
                    inventories = inventoryList?.AsEnumerable();

                    // Cargar productos con bajo stock
                    var lowStockResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/ProductInventory/low-stock");
                    if (lowStockResponse?.Success == true)
                    {
                        lowStockItems = JsonConvert.DeserializeObject<List<ProductInventoryModel>>(
                            lowStockResponse.Data.ToString()) ?? new();
                    }
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", "Error al cargar inventarios", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar inventarios: {ex.Message}", 4000);
            }
        }

        private async Task ShowStockMovementDialog(ProductInventoryModel inventory)
        {
            selectedInventory = inventory;
            stockMovement = new StockMovementDTO
            {
                ProductInventoryID = inventory.ID,
                Quantity = 1
            };

            var result = await DialogService.OpenAsync<StockMovementDialog>("Registrar Movimiento de Stock",
                new Dictionary<string, object>
                {
            { "ProductName", inventory.FastFoodItem?.Name ?? "Producto" },
            { "CurrentStock", inventory.CurrentStock },
            { "StockMovement", stockMovement }
                },
                new DialogOptions
                {
                    Width = "500px",
                    Height = "400px",
                    CloseDialogOnOverlayClick = false
                });

            if (result == true)
            {
                await ProcessStockMovement();
            }
        }

        private async Task ProcessStockMovement()
        {
            try
            {
                var response = await ApiClient.PostAsync<BaseResponseModel, StockMovementDTO>(
                    "api/ProductInventory/movement", stockMovement);

                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Movimiento registrado exitosamente", 4000);
                    await LoadData();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al registrar movimiento", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al registrar movimiento: {ex.Message}", 4000);
            }
        }

        private async Task ShowDeleteConfirmation(ProductInventoryModel inventory)
        {
            var result = await DialogService.Confirm(
                $"¿Está seguro que desea eliminar el inventario para {inventory.FastFoodItem?.Name}?",
                "Confirmar Eliminación",
                new ConfirmOptions
                {
                    OkButtonText = "Sí",
                    CancelButtonText = "No"
                });

            if (result == true)
            {
                await DeleteInventory(inventory);
            }
        }

        private async Task DeleteInventory(ProductInventoryModel inventory)
        {
            try
            {
                var response = await ApiClient.DeleteAsync<BaseResponseModel>($"api/ProductInventory/{inventory.ID}");
                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Inventario eliminado exitosamente", 4000);
                    await LoadData();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al eliminar inventario", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al eliminar inventario: {ex.Message}", 4000);
            }
        }
    }
}
