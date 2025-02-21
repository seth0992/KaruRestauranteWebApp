using KaruRestauranteWebApp.Models.Entities.Users;
using KaruRestauranteWebApp.Models.Models.User;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.User
{
    public partial class UpdateUser
    {

        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Parameter]
        public int Id { get; set; }

        private UpdateUserFormModel model = new();
        private bool isLoaded;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadRoles();
                await LoadUser();
                isLoaded = true;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    "Error al cargar la información del usuario",
                    4000);
                NavigationManager.NavigateTo("/users");
            }
        }

        private async Task LoadUser()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/UserManagement/{Id}");
            if (response?.Success == true && response.Data != null)
            {
                var user = JsonConvert.DeserializeObject<UserModel>(response.Data.ToString());
                if (user != null)
                {
                    model.UserData = new UpdateUserDTO
                    {
                        ID = user.ID,
                        Username = user.Username,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        IsActive = user.IsActive,
                        LastLogin = user.LastLogin
                    };

                    foreach (var userRole in user.UserRoles)
                    {
                        var roleSelection = model.RoleSelections
                            .FirstOrDefault(r => r.RoleId == userRole.RoleID);
                        if (roleSelection != null)
                        {
                            roleSelection.IsSelected = true;
                        }
                    }
                }
            }
            else
            {
                NotificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    "Error al cargar la información del usuario",
                    4000);
                NavigationManager.NavigateTo("/users");
            }
        }

        private async Task LoadRoles()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Role");
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
                    NotificationService.Notify(
                        NotificationSeverity.Warning,
                        "Validación",
                        "Debe seleccionar al menos un rol",
                        4000);
                    return;
                }

                var updateUserDto = new UpdateUserDTO
                {
                    ID = Id,
                    Username = model.UserData.Username,
                    FirstName = model.UserData.FirstName,
                    LastName = model.UserData.LastName,
                    Email = model.UserData.Email,
                    IsActive = model.UserData.IsActive,
                    RoleIds = model.RoleSelections
                        .Where(r => r.IsSelected)
                        .Select(r => r.RoleId)
                        .ToList()
                };

                var response = await ApiClient.PutAsync<BaseResponseModel, UpdateUserDTO>(
                    $"api/UserManagement/{Id}", updateUserDto);

                if (response?.Success == true)
                {
                    NotificationService.Notify(
                        NotificationSeverity.Success,
                        "Éxito",
                        "Usuario actualizado exitosamente",
                        4000);
                    NavigationManager.NavigateTo("/users");
                }
                else
                {
                    NotificationService.Notify(
                        NotificationSeverity.Error,
                        "Error",
                        response?.ErrorMessage ?? "Error al actualizar usuario",
                        4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    $"Error al actualizar usuario: {ex.Message}",
                    4000);
            }
        }

        private class UpdateUserFormModel
        {
            public UpdateUserDTO UserData { get; set; } = new();
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
