using Blazored.Toast.Services;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Web.Components.BaseComponent;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace KaruRestauranteWebApp.Web.Components.Pages.Inventory
{
    public partial class ListIngredients
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required IToastService ToastService { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        private List<IngredientModel>? ingredients;
        private List<IngredientModel> lowStockItems = new();
        private IngredientModel? selectedIngredient;
        private InventoryTransactionModel newTransaction = new();
        private AppModal transactionModal = null!;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Inventory/ingredients");
                if (response?.Success == true && response.Data != null)
                {
                    ingredients = JsonConvert.DeserializeObject<List<IngredientModel>>(response.Data.ToString());

                    // Obtener items con bajo stock
                    var lowStockResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Inventory/low-stock");
                    if (lowStockResponse?.Success == true && lowStockResponse.Data != null)
                    {
                        lowStockItems = JsonConvert.DeserializeObject<List<IngredientModel>>(lowStockResponse.Data.ToString()) ?? new();
                    }
                }
                else
                {
                    ToastService.ShowError("Error al cargar los ingredientes");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Error: {ex.Message}");
            }
        }

        private void ShowTransactionModal(IngredientModel ingredient)
        {
            selectedIngredient = ingredient;
            newTransaction = new InventoryTransactionModel
            {
                IngredientID = ingredient.ID,
                UnitPrice = ingredient.Cost
            };
            transactionModal.Open();
        }

        private async Task HandleTransactionSubmit()
        {
            if (selectedIngredient == null) return;

            try
            {
                // Creamos un objeto anónimo con solo los campos necesarios
                var transactionDto = new
                {
                    IngredientID = selectedIngredient.ID,
                    TransactionType = newTransaction.TransactionType,
                    Quantity = newTransaction.Quantity,
                    UnitPrice = newTransaction.UnitPrice,
                    Notes = newTransaction.Notes,
                    TransactionDate = DateTime.UtcNow
                    // UserID se asignará en el backend basado en el token JWT
                };

                var response = await ApiClient.PostAsync<BaseResponseModel, object>(
                    "api/Inventory/transactions", transactionDto);

                if (response?.Success == true)
                {
                    ToastService.ShowSuccess("Movimiento registrado exitosamente");
                    await LoadData();
                    transactionModal.Close();
                }
                else
                {
                    ToastService.ShowError(response?.ErrorMessage ?? "Error al registrar el movimiento");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Error: {ex.Message}");
            }
        }
    }

}
