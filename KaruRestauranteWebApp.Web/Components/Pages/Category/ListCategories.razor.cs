using Blazored.Toast.Services;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Web.Components.BaseComponent;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace KaruRestauranteWebApp.Web.Components.Pages.Category
{
    public partial class ListCategories
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required IToastService ToastService { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        private List<CategoryModel>? categories;
        private CategoryModel? selectedCategory;
        private AppModal deleteModal = null!;

        protected override async Task OnInitializedAsync()
        {
            await LoadCategories();
        }
        private async Task LoadCategories()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Category");
                if (response?.Success == true && response.Data != null)
                {
                    categories = JsonConvert.DeserializeObject<List<CategoryModel>>(response.Data.ToString());
                }
                else
                {
                    ToastService.ShowError("Error al cargar las categorías");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Error al cargar las categorías: {ex.Message}");
            }
        }

        private void ShowDeleteConfirmation(CategoryModel category)
        {
            selectedCategory = category;
            deleteModal.Open();
        }

        private async Task DeleteCategory()
        {
            if (selectedCategory == null) return;

            try
            {
                var response = await ApiClient.DeleteAsync<BaseResponseModel>($"api/Category/{selectedCategory.ID}");
                if (response?.Success == true)
                {
                    ToastService.ShowSuccess(selectedCategory.Items.Any()
                        ? "Categoría desactivada exitosamente"
                        : "Categoría eliminada exitosamente");
                    await LoadCategories();
                }
                else
                {
                    ToastService.ShowError(response?.ErrorMessage ?? "Error al eliminar la categoría");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Error al eliminar la categoría: {ex.Message}");
            }
            finally
            {
                deleteModal.Close();
            }
        }
    }
}
