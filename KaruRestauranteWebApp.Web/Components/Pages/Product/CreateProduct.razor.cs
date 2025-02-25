using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Product
{
    public partial class CreateProduct
    {
        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        private bool isLoaded;
        private ProductFormModel model = new();
        private List<CategoryModel> categories = new();
        private List<ProductTypeModel> productTypes = new();
        private List<IngredientModel> availableIngredients = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadCategories();
                await LoadIngredients();
                await LoadProductTypes();
                isLoaded = true;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar datos: {ex.Message}", 4000);
            }
        }

        private async Task LoadCategories()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Category");
            if (response?.Success == true)
            {
                categories = JsonConvert.DeserializeObject<List<CategoryModel>>(response.Data.ToString()) ?? new();
            }
        }

        private async Task LoadIngredients()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Inventory/ingredients");
            if (response?.Success == true)
            {
                availableIngredients = JsonConvert.DeserializeObject<List<IngredientModel>>(response.Data.ToString()) ?? new();
            }
        }

        private async Task LoadProductTypes()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/ProductType");
            if (response?.Success == true)
            {
                productTypes = JsonConvert.DeserializeObject<List<ProductTypeModel>>(response.Data.ToString()) ?? new();

                // Establecer un valor predeterminado para ProductTypeID si hay tipos disponibles
                if (productTypes.Any())
                {
                    model.ProductTypeID = productTypes.First().ID;
                }
            }
        }

        private void AddIngredient()
        {
            model.Ingredients.Add(new ItemIngredientDetailModel());
        }

        private void RemoveIngredient(ItemIngredientDetailModel ingredient)
        {
            model.Ingredients.Remove(ingredient);
        }

        private void IngredientChanged(object value, ItemIngredientDetailModel ingredient)
        {
            // Lógica para cuando cambia el ingrediente seleccionado
            // Por ejemplo, configurar valores predeterminados según el ingrediente
        }

        private async Task HandleSubmit()
        {
            try
            {
                if (!model.Ingredients.Any())
                {
                    NotificationService.Notify(NotificationSeverity.Warning,
                        "Validación", "Debe agregar al menos un ingrediente", 4000);
                    return;
                }

                // Validar que todos los ingredientes tengan cantidad
                foreach (var ingredient in model.Ingredients)
                {
                    if (ingredient.IngredientID == 0)
                    {
                        NotificationService.Notify(NotificationSeverity.Warning,
                            "Validación", "Todos los ingredientes deben ser seleccionados", 4000);
                        return;
                    }

                    if (ingredient.Quantity <= 0)
                    {
                        NotificationService.Notify(NotificationSeverity.Warning,
                            "Validación", "Todos los ingredientes deben tener una cantidad mayor a 0", 4000);
                        return;
                    }

                    if (ingredient.CanBeExtra && ingredient.ExtraPrice <= 0)
                    {
                        NotificationService.Notify(NotificationSeverity.Warning,
                            "Validación", "Todos los ingredientes extras deben tener un precio extra", 4000);
                        return;
                    }
                }

                // Mapear el modelo a DTO
                var productDto = new FastFoodItemDTO
                {
                    Name = model.Name,
                    Description = model.Description,
                    CategoryID = model.CategoryID,
                    SellingPrice = model.SellingPrice,
                    EstimatedCost = model.EstimatedCost,
                    ProductTypeID = model.ProductTypeID,
                    IsAvailable = model.IsAvailable,
                    ImageUrl = model.ImageUrl,
                    EstimatedPreparationTime = model.EstimatedPreparationTime,
                    Ingredients = model.Ingredients.Select(i => new ItemIngredientDetailDTO
                    {
                        IngredientID = i.IngredientID,
                        Quantity = i.Quantity,
                        IsOptional = i.IsOptional,
                        CanBeExtra = i.CanBeExtra,
                        ExtraPrice = i.ExtraPrice
                    }).ToList()
                };

                var response = await ApiClient.PostAsync<BaseResponseModel, FastFoodItemDTO>(
                    "api/FastFood", productDto);

                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Producto creado exitosamente", 4000);
                    NavigationManager.NavigateTo("/products");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al crear producto", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al crear producto: {ex.Message}", 4000);
            }
        }

        private class ProductFormModel
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int CategoryID { get; set; }
            public decimal SellingPrice { get; set; }
            public decimal EstimatedCost { get; set; }
            public int ProductTypeID { get; set; } = 1; // Valor predeterminado
            public bool IsAvailable { get; set; } = true;
            public string ImageUrl { get; set; } = string.Empty;
            public int? EstimatedPreparationTime { get; set; }
            public List<ItemIngredientDetailModel> Ingredients { get; set; } = new();
        }

        private class ItemIngredientDetailModel
        {
            public int IngredientID { get; set; }
            public decimal Quantity { get; set; } = 1;
            public bool IsOptional { get; set; }
            public bool CanBeExtra { get; set; }
            public decimal ExtraPrice { get; set; }
        }
    }
}
