using Blazored.Toast.Services;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace KaruRestauranteWebApp.Web.Components.Pages.Category
{
    public partial class UpdateCategory
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required IToastService ToastService { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Parameter]
        public int Id { get; set; }

        private CategoryModel category = new();
        private bool isLoaded;

        protected override async Task OnInitializedAsync()
        {
            await LoadCategory();
        }

        private async Task LoadCategory()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Category/{Id}");
                if (response?.Success == true && response.Data != null)
                {
                    var loadedCategory = JsonConvert.DeserializeObject<CategoryModel>(response.Data.ToString());
                    if (loadedCategory != null)
                    {
                        category = loadedCategory;
                        isLoaded = true;
                    }
                    else
                    {
                        ToastService.ShowError("Error al cargar los datos de la categoría");
                        NavigationManager.NavigateTo("/categories");
                    }
                }
                else
                {
                    ToastService.ShowError(response?.ErrorMessage ?? "Error al cargar la categoría");
                    NavigationManager.NavigateTo("/categories");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Error al cargar la categoría: {ex.Message}");
                NavigationManager.NavigateTo("/categories");
            }
        }

        private async Task HandleSubmit()
        {
            try
            {
                // Solo enviamos las propiedades que necesitamos actualizar
                var categoryToUpdate = new CategoryModel
                {
                    ID = category.ID,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive,
                    UpdatedAt = DateTime.UtcNow
                };

                var response = await ApiClient.PutAsync<BaseResponseModel, CategoryModel>(
                    $"api/Category/{Id}", categoryToUpdate);

                if (response?.Success == true)
                {
                    ToastService.ShowSuccess("Categoría actualizada exitosamente");
                    NavigationManager.NavigateTo("/categories");
                }
                else
                {
                    ToastService.ShowError(response?.ErrorMessage ?? "Error al actualizar la categoría");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Error al actualizar la categoría: {ex.Message}");
            }
        }
    }
}
