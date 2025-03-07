using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using KaruRestauranteWebApp.Web.Components.Pages.Product;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Combo
{
    public partial class EditCombo
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
        private ComboFormModel model = new();
        private List<FastFoodItemModel> availableProducts = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadProducts();
                await LoadCombo();
                isLoaded = true;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar datos: {ex.Message}", 4000);
                NavigationManager.NavigateTo("/combos");
            }
        }

        private async Task LoadProducts()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/FastFood");
            if (response?.Success == true)
            {
                availableProducts = JsonConvert.DeserializeObject<List<FastFoodItemModel>>(response.Data.ToString()) ?? new();
            }
        }

        private async Task LoadCombo()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Combo/{Id}");
            if (response?.Success == true)
            {
                var combo = JsonConvert.DeserializeObject<ComboModel>(response.Data.ToString());
                if (combo != null)
                {
                    model = new ComboFormModel
                    {
                        ID = combo.ID,
                        Name = combo.Name,
                        Description = combo.Description,
                        RegularPrice = combo.RegularPrice,
                        SellingPrice = combo.SellingPrice,
                        DiscountPercentage = combo.DiscountPercentage,
                        IsAvailable = combo.IsAvailable,
                        ImageUrl = combo.ImageUrl,
                        Items = combo.Items.Select(i => new ComboItemDetailModel
                        {
                            ID = i.ID,
                            FastFoodItemID = i.FastFoodItemID,
                            Quantity = i.Quantity,
                            AllowCustomization = i.AllowCustomization,
                            SpecialInstructions = i.SpecialInstructions
                        }).ToList()
                    };
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", "No se encontró el combo", 4000);
                    NavigationManager.NavigateTo("/combos");
                }
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", "Error al cargar el combo", 4000);
                NavigationManager.NavigateTo("/combos");
            }
        }

        private void AddComboItem()
        {
            model.Items.Add(new ComboItemDetailModel());
        }

        private void RemoveComboItem(ComboItemDetailModel item)
        {
            model.Items.Remove(item);
        }

        private void CalculateDiscount()
        {
            if (model.RegularPrice > 0 && model.DiscountPercentage >= 0)
            {
                model.SellingPrice = Math.Round(model.RegularPrice * (1 - (model.DiscountPercentage / 100)), 2);
            }
        }

        private async Task ShowImagePreview()
        {
            if (!string.IsNullOrEmpty(model.ImageUrl))
            {
                await DialogService.OpenAsync<ImagePreviewDialog>("Vista previa de imagen",
                    new Dictionary<string, object>
                    {
                        { "ImageUrl", model.ImageUrl }
                    },
                    new DialogOptions
                    {
                        Width = "500px",
                        Height = "auto",
                        CloseDialogOnOverlayClick = true
                    });
            }
        }

        private async Task HandleSubmit()
        {
            try
            {
                if (!model.Items.Any())
                {
                    NotificationService.Notify(NotificationSeverity.Warning,
                        "Validación", "Debe agregar al menos un producto al combo", 4000);
                    return;
                }

                // Validar que todos los productos tengan cantidad
                foreach (var item in model.Items)
                {
                    if (item.FastFoodItemID == 0)
                    {
                        NotificationService.Notify(NotificationSeverity.Warning,
                            "Validación", "Todos los productos deben ser seleccionados", 4000);
                        return;
                    }

                    if (item.Quantity <= 0)
                    {
                        NotificationService.Notify(NotificationSeverity.Warning,
                            "Validación", "Todos los productos deben tener una cantidad mayor a 0", 4000);
                        return;
                    }
                }

                // Mapear el modelo a DTO
                var comboDto = new ComboDTO
                {
                    ID = model.ID,
                    Name = model.Name,
                    Description = model.Description,
                    RegularPrice = model.RegularPrice,
                    SellingPrice = model.SellingPrice,
                    DiscountPercentage = model.DiscountPercentage,
                    IsAvailable = model.IsAvailable,
                    ImageUrl = model.ImageUrl,
                    Items = model.Items.Select(i => new ComboItemDetailDTO
                    {
                        FastFoodItemID = i.FastFoodItemID,
                        Quantity = i.Quantity,
                        AllowCustomization = i.AllowCustomization,
                        SpecialInstructions = i.SpecialInstructions
                    }).ToList()
                };

                var response = await ApiClient.PutAsync<BaseResponseModel, ComboDTO>(
                    $"api/Combo/{Id}", comboDto);

                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Combo actualizado exitosamente", 4000);
                    NavigationManager.NavigateTo("/combos");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al actualizar combo", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al actualizar combo: {ex.Message}", 4000);
            }
        }

        private class ComboFormModel
        {
            public int ID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal RegularPrice { get; set; }
            public decimal SellingPrice { get; set; }
            public decimal DiscountPercentage { get; set; }
            public bool IsAvailable { get; set; } = true;
            public string ImageUrl { get; set; } = string.Empty;
            public List<ComboItemDetailModel> Items { get; set; } = new();
        }
        private class ComboItemDetailModel
        {
            public int ID { get; set; }
            public int FastFoodItemID { get; set; }
            public int Quantity { get; set; } = 1;
            public bool AllowCustomization { get; set; }
            public string? SpecialInstructions { get; set; }
        }

    }
}
