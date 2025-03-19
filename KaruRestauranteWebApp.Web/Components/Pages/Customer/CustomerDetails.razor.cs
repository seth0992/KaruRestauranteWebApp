using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Customer
{
    public partial class CustomerDetails
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        private CustomerModel? customer;
        private bool isLoading = true;

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
                    customer = JsonConvert.DeserializeObject<CustomerModel>(
                        response.Data.ToString());
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
    }
}
