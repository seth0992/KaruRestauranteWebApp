using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Models.Orders;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Customer
{
    public partial class EditCustomer
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        private CustomerDTO? model;
        private bool isLoading = true;
        private string[] identificationTypes = new[] { "Cédula Física", "Cédula Jurídica", "DIMEX", "NITE", "Pasaporte" };

        protected override async Task OnInitializedAsync()
        {
            await LoadCustomer();
        }

        private async Task LoadCustomer()
        {
            try
            {
                isLoading = true;
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Customer/{Id}");

                if (response?.Success == true && response.Data != null)
                {
                    // Deserializar el cliente obtenido
                    var customer = JsonConvert.DeserializeObject<CustomerModel>(
                        response.Data.ToString());

                    if (customer != null)
                    {
                        // Mapear a DTO
                        model = new CustomerDTO
                        {
                            ID = customer.ID,
                            Name = customer.Name,
                            Email = customer.Email,
                            PhoneNumber = customer.PhoneNumber,
                            IdentificationType = customer.IdentificationType,
                            IdentificationNumber = customer.IdentificationNumber,
                            Address = customer.Address,
                            IsActive = customer.IsActive
                        };
                    }
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al cargar cliente", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar cliente: {ex.Message}", 4000);
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task HandleSubmit()
        {
            try
            {
                if (model == null) return;

                var response = await ApiClient.PutAsync<BaseResponseModel, CustomerDTO>(
                    $"api/Customer/{Id}", model);

                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Cliente actualizado exitosamente", 4000);
                    NavigationManager.NavigateTo("/customers");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al actualizar cliente", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al actualizar cliente: {ex.Message}", 4000);
            }
        }
    }
}
