using KaruRestauranteWebApp.Models.Models.Orders;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Customer
{
    public partial class CreateCustomer
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        private CustomerDTO model = new() { IsActive = true };
        private string[] identificationTypes = new[] { "Cédula Física", "Cédula Jurídica", "DIMEX", "NITE", "Pasaporte" };

        private async Task HandleSubmit()
        {
            try
            {
                var response = await ApiClient.PostAsync<BaseResponseModel, CustomerDTO>(
                    "api/Customer", model);

                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Cliente creado exitosamente", 4000);
                    NavigationManager.NavigateTo("/customers");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al crear cliente", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al crear cliente: {ex.Message}", 4000);
            }
        }
    }
}
