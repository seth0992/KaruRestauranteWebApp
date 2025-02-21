using Blazored.Toast.Services;
using KaruRestauranteWebApp.Models.Entities.Users;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace KaruRestauranteWebApp.Web.Components.Pages.Role
{
    public partial class UpdateRole
    {
        [Inject]
        public required ApiClient apiClient { get; set; }
        [Inject]
        public required IToastService toastService { get; set; }
        [Inject]
        public required NavigationManager navigationManager { get; set; }
        [Parameter]
        public int Id { get; set; }

        private RoleModel? role;

        protected override async Task OnInitializedAsync()
        {
            await LoadRole();
        }

        private async Task LoadRole()
        {
            try
            {
                var response = await apiClient.GetFromJsonAsync<BaseResponseModel>($"api/Role/{Id}");
                if (response?.Success == true)
                {
                    role = JsonConvert.DeserializeObject<RoleModel>(response.Data.ToString());
                }
                else
                {
                    toastService.ShowError("Error al cargar rol");
                    navigationManager.NavigateTo("/roles");
                }
            }
            catch (Exception)
            {
                toastService.ShowError("Error al cargar rol");
                navigationManager.NavigateTo("/roles");
            }
        }

        private async Task HandleSubmit()
        {
            try
            {
                var response = await apiClient.PutAsync<BaseResponseModel, RoleModel>($"api/Role/{Id}", role);
                if (response?.Success == true)
                {
                    toastService.ShowSuccess("Rol actualizado exitosamente");
                    navigationManager.NavigateTo("/roles");
                }
                else
                {
                    toastService.ShowError(response?.ErrorMessage ?? "Error al actualizar rol");
                }
            }
            catch (Exception)
            {
                toastService.ShowError("Error al actualizar rol");
            }
        }
    }
}
