using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Combo
{
    public partial class CreateCombo
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        private bool isLoaded;
        private ComboFormModel model = new();
        private List<FastFoodItemModel> availableProducts = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadProducts();
                isLoaded = true;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar datos: {ex.Message}", 4000);
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

        private async Task AddComboItem()
        {
         
                var itemCombo = new ComboItemDetailModel
                {
                    FastFoodItemID = 1,
     Quantity = 1,
     AllowCustomization = false,
       SpecialInstructions = string.Empty

                };

                // Creamos una nueva lista con todos los elementos existentes más el nuevo
                var newList = new List<ComboItemDetailModel>(model.Items);

                newList.Add(itemCombo);

                // Reemplazamos la lista completa
                model.Items = newList;


                await InvokeAsync(StateHasChanged); // Forzar actualización de la UI          


           
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

                var response = await ApiClient.PostAsync<BaseResponseModel, ComboDTO>(
                    "api/Combo", comboDto);

                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Combo creado exitosamente", 4000);
                    NavigationManager.NavigateTo("/combos");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al crear combo", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al crear combo: {ex.Message}", 4000);
            }
        }

        private class ComboFormModel
        {
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
            public int FastFoodItemID { get; set; }
            public int Quantity { get; set; } = 1;
            public bool AllowCustomization { get; set; }
            public string? SpecialInstructions { get; set; }
        }
    }
}
