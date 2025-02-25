using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Product
{

    public partial class ProductDetails
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Inject]
        public required DialogService DialogService { get; set; }

        [Parameter]
        public int Id { get; set; }

        private bool isLoaded;
        private FastFoodItemModel? product;
        private List<ProductTypeModel> productTypes = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadProductTypes();
                await LoadProduct();
                isLoaded = true;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar el producto: {ex.Message}", 4000);
            }
        }

        private async Task LoadProductTypes()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/ProductType");
            if (response?.Success == true)
            {
                productTypes = JsonConvert.DeserializeObject<List<ProductTypeModel>>(response.Data.ToString()) ?? new();
            }
        }

        private async Task LoadProduct()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/FastFood/{Id}");
            if (response?.Success == true)
            {
                product = JsonConvert.DeserializeObject<FastFoodItemModel>(response.Data.ToString());
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", "Error al cargar el producto", 4000);
            }
        }

        private string GetProductTypeName(int productTypeId)
        {
            var productType = productTypes.FirstOrDefault(pt => pt.ID == productTypeId);
            return productType?.Name ?? "Desconocido";
        }

        private async Task ShowImagePreview()
        {
            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                await DialogService.OpenAsync<ImagePreviewDialog>("Vista previa de imagen",
                    new Dictionary<string, object>
                    {
                        { "ImageUrl", product.ImageUrl }
                    },
                    new DialogOptions
                    {
                        Width = "500px",
                        Height = "auto",
                        CloseDialogOnOverlayClick = true
                    });
            }
        }
    }
}
