using Blazored.Toast.Services;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;

namespace KaruRestauranteWebApp.Web.Components.Pages.Category
{
    public partial class CreateCategory
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required IToastService ToastService { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        private CategoryModel category = new()
        {
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        private async Task HandleSubmit()
        {
            try
            {
                var response = await ApiClient.PostAsync<BaseResponseModel, CategoryModel>("api/Category", category);
                if (response?.Success == true)
                {
                    ToastService.ShowSuccess("Categoría creada exitosamente");
                    NavigationManager.NavigateTo("/categories");
                }
                else
                {
                    ToastService.ShowError(response?.ErrorMessage ?? "Error al crear la categoría");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Error al crear la categoría: {ex.Message}");
            }
        }
    }
}
