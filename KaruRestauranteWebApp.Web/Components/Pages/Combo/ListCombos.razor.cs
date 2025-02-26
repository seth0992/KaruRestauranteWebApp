using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Combo
{
    public partial class ListCombos
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Inject]
        public required DialogService DialogService { get; set; }

        private IEnumerable<ComboModel>? combos;

        protected override async Task OnInitializedAsync()
        {
            await LoadCombos();
        }

        private async Task LoadCombos()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Combo");
                if (response?.Success == true)
                {
                    var combosList = JsonConvert.DeserializeObject<List<ComboModel>>(
                        response.Data.ToString());
                    combos = combosList?.AsEnumerable();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", "Error al cargar combos", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar combos: {ex.Message}", 4000);
            }
        }

        private async Task ShowDeleteConfirmation(ComboModel combo)
        {
            var result = await DialogService.Confirm(
                $"¿Está seguro que desea eliminar el combo {combo.Name}?",
                "Confirmar Eliminación",
                new ConfirmOptions()
                {
                    OkButtonText = "Sí",
                    CancelButtonText = "No"
                });

            if (result == true)
            {
                await DeleteCombo(combo);
            }
        }

        private async Task DeleteCombo(ComboModel combo)
        {
            try
            {
                var response = await ApiClient.DeleteAsync<BaseResponseModel>($"api/Combo/{combo.ID}");
                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Combo eliminado exitosamente", 4000);
                    await LoadCombos();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al eliminar combo", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al eliminar combo: {ex.Message}", 4000);
            }
        }
    }
}
