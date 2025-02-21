using Blazored.Toast.Services;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Inventory
{
    public partial class UpdateIngredient
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required IToastService ToastService { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }
        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Parameter]
        public int Id { get; set; }

        private IngredientModel ingredient = new();
        private bool isLoaded;

        protected override async Task OnInitializedAsync()
        {
            await LoadIngredient();
        }

        private async Task LoadIngredient()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Inventory/ingredients/{Id}");
                if (response?.Success == true && response.Data != null)
                {
                    var loadedIngredient = JsonConvert.DeserializeObject<IngredientModel>(response.Data.ToString());
                    if (loadedIngredient != null)
                    {
                        ingredient = loadedIngredient;
                        isLoaded = true;
                    }
                    else
                    {
                        NotificationService.Notify(NotificationSeverity.Error,
                            "Error", "Error al cargar los datos del ingrediente", 4000);
                        NavigationManager.NavigateTo("/inventory/ingredients");
                    }
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al cargar el ingrediente", 4000);
                    NavigationManager.NavigateTo("/inventory/ingredients");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error: {ex.Message}", 4000);
                NavigationManager.NavigateTo("/inventory/ingredients");
            }
        }
        private async Task HandleSubmit()
        {
            try
            {
                ingredient.UpdatedAt = DateTime.UtcNow;
                var response = await ApiClient.PutAsync<BaseResponseModel, IngredientModel>(
                    $"api/Inventory/ingredients/{Id}", ingredient);

                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Ingrediente actualizado exitosamente", 4000);
                    NavigationManager.NavigateTo("/inventory/ingredients");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al actualizar el ingrediente", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error: {ex.Message}", 4000);
            }
        }
    }
}
