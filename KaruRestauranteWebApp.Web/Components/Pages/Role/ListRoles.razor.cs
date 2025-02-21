using Blazored.Toast.Services;
using KaruRestauranteWebApp.Models.Entities.Users;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Web.Components.BaseComponent;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace KaruRestauranteWebApp.Web.Components.Pages.Role
{
    public partial class ListRoles
    {
        [Inject]
        public required ApiClient apiClient { get; set; }
        [Inject]
        public required IToastService toastService { get; set; }
        [Inject]
        public required NavigationManager navigationManager { get; set; }

        private List<RoleModel>? roles;
        private RoleModel? selectedRole;
        private AppModal deleteModal = null!;

        protected override async Task OnInitializedAsync()
        {
            await LoadRoles();
        }

        private async Task LoadRoles()
        {
            try
            {
                var response = await apiClient.GetFromJsonAsync<BaseResponseModel>("api/Role");
                if (response?.Success == true)
                {
                    roles = JsonConvert.DeserializeObject<List<RoleModel>>(response.Data.ToString());
                }
                else
                {
                    toastService.ShowError("Error al cargar roles");
                }
            }
            catch (Exception)
            {
                toastService.ShowError("Error al cargar roles");
            }
        }

        private void ShowDeleteConfirmation(RoleModel role)
        {
            selectedRole = role;
            deleteModal.Open();
        }

        private async Task DeleteRole()
        {
            if (selectedRole == null) return;

            try
            {
                var response = await apiClient.DeleteAsync<BaseResponseModel>($"api/Role/{selectedRole.ID}");
                if (response?.Success == true)
                {
                    toastService.ShowSuccess("Rol eliminado exitosamente");
                    await LoadRoles();
                }
                else
                {
                    toastService.ShowError(response?.ErrorMessage ?? "Error al eliminar rol");
                }
            }
            catch (Exception)
            {
                toastService.ShowError("Error al eliminar rol");
            }
            finally
            {
                deleteModal.Close();
            }
        }
    }
}
