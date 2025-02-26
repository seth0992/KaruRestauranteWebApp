using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Web.Components.Pages.Product;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Combo
{
    public partial class ComboDetails
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Inject]
        public required DialogService DialogService { get; set; }

        [Parameter]
        public int Id { get; set; }

        private bool isLoaded;
        private ComboModel? combo;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadCombo();
                isLoaded = true;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar el combo: {ex.Message}", 4000);
            }
        }

        private async Task LoadCombo()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Combo/{Id}");
            if (response?.Success == true)
            {
                combo = JsonConvert.DeserializeObject<ComboModel>(response.Data.ToString());
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", "Error al cargar el combo", 4000);
            }
        }

        private async Task ShowImagePreview()
        {
            if (combo != null && !string.IsNullOrEmpty(combo.ImageUrl))
            {
                await DialogService.OpenAsync<ImagePreviewDialog>("Vista previa de imagen",
                    new Dictionary<string, object>
                    {
                        { "ImageUrl", combo.ImageUrl }
                    },
                    new DialogOptions
                    {
                        Width = "500px",
                        Height = "auto",
                        CloseDialogOnOverlayClick = true
                    });
            }
        }
    }
}
