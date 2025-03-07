using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Components;
using Radzen;
using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Models;
using Newtonsoft.Json;

namespace KaruRestauranteWebApp.Web.Components.Pages.Customer
{
    public partial class ListCustomers
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Inject]
        public required DialogService DialogService { get; set; }

        private IEnumerable<CustomerModel>? customers;

        protected override async Task OnInitializedAsync()
        {
            await LoadCustomers();
        }

        private async Task LoadCustomers()
        {
            try
            {
                var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Customer");
                if (response?.Success == true)
                {
                    var customersList = JsonConvert.DeserializeObject<List<CustomerModel>>(
                        response.Data.ToString());
                    customers = customersList?.AsEnumerable();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", "Error al cargar clientes", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar clientes: {ex.Message}", 4000);
            }
        }

        private async Task ShowDeleteConfirmation(CustomerModel customer)
        {
            var result = await DialogService.Confirm(
                $"¿Está seguro que desea eliminar el cliente {customer.Name}?",
                "Confirmar Eliminación",
                new ConfirmOptions()
                {
                    OkButtonText = "Sí",
                    CancelButtonText = "No"
                });

            if (result == true)
            {
                await DeleteCustomer(customer);
            }
        }

        private async Task DeleteCustomer(CustomerModel customer)
        {
            try
            {
                var response = await ApiClient.DeleteAsync<BaseResponseModel>($"api/Customer/{customer.ID}");
                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Cliente eliminado exitosamente", 4000);
                    await LoadCustomers();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al eliminar cliente", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al eliminar cliente: {ex.Message}", 4000);
            }
        }
    }
}
