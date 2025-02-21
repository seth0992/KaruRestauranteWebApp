using Blazored.Toast.Services;
using KaruRestauranteWebApp.Models.Entities.Users;
using KaruRestauranteWebApp.Models.Models.User;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace KaruRestauranteWebApp.Web.Components.Pages.User
{
    public partial class CreateUser
    {
        [Inject]
        public required ApiClient apiClient { get; set; }
        [Inject]
        public required IToastService toastService { get; set; }
        [Inject]
        public required NavigationManager navigationManager { get; set; }

        private bool isLoaded;
        private CreateUserFormModel model = new();


        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadRoles();
                isLoaded = true;
            }
            catch (Exception)
            {
                toastService.ShowError("Error al cargar los roles");
                navigationManager.NavigateTo("/users");
            }
        }

        private async Task LoadRoles()
        {
            var response = await apiClient.GetFromJsonAsync<BaseResponseModel>("api/Role");
            if (response?.Success == true && response.Data != null)
            {
                var roles = JsonConvert.DeserializeObject<List<RoleModel>>(response.Data.ToString()) ?? new();
                model.RoleSelections = roles.Select(r => new RoleSelectionModel
                {
                    RoleId = r.ID,
                    RoleName = r.RoleName,
                    IsSelected = false
                }).ToList();
            }
        }

        private async Task HandleSubmit()
        {
            try
            {
                if (!model.RoleSelections.Any(r => r.IsSelected))
                {
                    toastService.ShowWarning("Debe seleccionar al menos un rol");
                    return;
                }

                var createUserDto = new CreateUserDTO
                {
                    Username = model.UserData.Username,
                    Password = model.UserData.Password,
                    FirstName = model.UserData.FirstName,
                    LastName = model.UserData.LastName,
                    Email = model.UserData.Email,
                    RoleIds = model.RoleSelections
                        .Where(r => r.IsSelected)
                        .Select(r => r.RoleId)
                        .ToList()
                };

                var response = await apiClient.PostAsync<BaseResponseModel, CreateUserDTO>(
                    "api/UserManagement", createUserDto);

                if (response?.Success == true)
                {
                    toastService.ShowSuccess("Usuario creado exitosamente");
                    navigationManager.NavigateTo("/users");
                }
                else
                {
                    toastService.ShowError(response?.ErrorMessage ?? "Error al crear usuario");
                }
            }
            catch (Exception)
            {
                toastService.ShowError("Error al crear usuario");
            }
        }

        private class CreateUserFormModel
        {
            public CreateUserDTO UserData { get; set; } = new();
            public List<RoleSelectionModel> RoleSelections { get; set; } = new();
        }

        private class RoleSelectionModel
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; } = string.Empty;
            public bool IsSelected { get; set; }
        }
    }

}
