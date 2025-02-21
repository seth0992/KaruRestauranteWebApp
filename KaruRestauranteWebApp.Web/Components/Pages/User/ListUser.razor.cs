using KaruRestauranteWebApp.Models.Entities.Users;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.User
{
    public partial class ListUser
    {
        [Inject]
        public NotificationService NotificationService { get; set; }

        // Cambiamos el tipo a IEnumerable<UserModel>
        private IEnumerable<UserModel>? users;

        [CascadingParameter]
        private Task<AuthenticationState> AuthState { get; set; } = null!;

        [Inject]
        public DialogService DialogService { get; set; } = null!;


        private bool IsCurrentUserSuperAdmin =>
            AuthState.Result.User.IsInRole("SuperAdmin");

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/UserManagement");
                if (response?.Success == true)
                {
                    // Convertimos explícitamente a IEnumerable
                    var usersList = JsonConvert.DeserializeObject<List<UserModel>>(response.Data.ToString());
                    users = usersList?.AsEnumerable();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", "Error al cargar usuarios", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar usuarios: {ex.Message}", 4000);
            }
        }

        private async Task ShowDeleteConfirmation(UserModel user)
        {
            var result = await DialogService.Confirm(
                $"¿Está seguro que desea eliminar al usuario {user.Username}?",
                "Confirmar Eliminación",
                new ConfirmOptions()
                {
                    OkButtonText = "Sí",
                    CancelButtonText = "No"
                });

            if (result == true)
            {
                await DeleteUser(user);
            }
        }

        private async Task DeleteUser(UserModel user)
        {
            try
            {
                var response = await ApiClient.DeleteAsync<BaseResponseModel>($"api/UserManagement/{user.ID}");
                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Usuario eliminado exitosamente", 4000);
                    await LoadUsers();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al eliminar usuario", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al eliminar usuario: {ex.Message}", 4000);
            }
        }
    }

}
