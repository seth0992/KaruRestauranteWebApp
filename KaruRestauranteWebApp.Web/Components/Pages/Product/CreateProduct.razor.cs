using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;

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

        private bool showIngredients = true; // Por defecto, no mostramos la sección de ingredientes

        RadzenDataGrid<ItemIngredientDetailModel> gridIngredients = new RadzenDataGrid<ItemIngredientDetailModel>();

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

                // Inicialmente, verificamos el tipo de producto seleccionado
                showIngredients = model.ProductTypeID == 1;

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

        private void ProductTypeChanged(object value)
        {
            if (value != null)
            {
                int productTypeId = Convert.ToInt32(value);

                // Si cambia a tipo "Inventario", limpiamos la lista de ingredientes
                if (productTypeId == 2)
                {
                    model.Ingredients.Clear();
                    showIngredients = false;
                }
                else
                {
                    showIngredients = true;
                }
            }
        }

        private async Task AddIngredientAsync()
        {
            // Solo agregamos ingredientes si es producto tipo "Preparado"
            if (model.ProductTypeID == 1)
            {
                var newIngredient = new ItemIngredientDetailModel
                {
                    IngredientID = 0,
                    Quantity = 1,
                    IsOptional = false,
                    CanBeExtra = false,
                    ExtraPrice = 0
                };

                // Creamos una nueva lista con todos los elementos existentes más el nuevo
                var newList = new List<ItemIngredientDetailModel>(model.Ingredients);

                newList.Add(newIngredient);

                // Reemplazamos la lista completa
                model.Ingredients = newList;
          
                
                await InvokeAsync(StateHasChanged); // Forzar actualización de la UI
            }
        }

        private async Task RemoveIngredientAsync(ItemIngredientDetailModel ingredient)
        {
            model.Ingredients.Remove(ingredient);
            await InvokeAsync(StateHasChanged); // Forzar actualización de la UI
        }

        private void IngredientChanged(object value, ItemIngredientDetailModel ingredient)
        {
            // Asegurarse de que se haya seleccionado un valor válido
            if (value != null && int.TryParse(value.ToString(), out int ingredientId))
            {
                ingredient.IngredientID = ingredientId;

                // Buscar el ingrediente seleccionado en la lista de ingredientes disponibles
                var selectedIngredient = availableIngredients.FirstOrDefault(i => i.ID == ingredientId);

                if (selectedIngredient != null)
                {
                    // Establecer valores por defecto basados en el ingrediente seleccionado

                    // Si es la primera vez que se selecciona este ingrediente (cantidad = 0)
                    if (ingredient.Quantity <= 0)
                    {
                        // Establecer una cantidad predeterminada de 1 o la que corresponda
                        ingredient.Quantity = 1;
                    }

                    // Si el campo de unidad de medida está disponible en tu modelo, 
                    // podrías configurarlo según el ingrediente seleccionado
                    // ingredient.UnitOfMeasure = selectedIngredient.UnitOfMeasure;

                    // Por defecto, no es opcional ni extra
                    if (ingredient.ExtraPrice <= 0 && ingredient.CanBeExtra)
                    {
                        // Si se marca como extra pero no tiene precio, sugerir un precio
                        ingredient.ExtraPrice = Math.Round(selectedIngredient.PurchasePrice * 0.5m, 2);
                    }

                    // Notificar al usuario sobre la selección (opcional)
                    NotificationService.Notify(
                        NotificationSeverity.Info,
                        "Ingrediente seleccionado",
                        $"Has seleccionado {selectedIngredient.Name}",
                        2000 // duración corta
                    );
                }

                // Forzar actualización de la UI
                StateHasChanged();
            }
        }

        private async Task HandleSubmit()
        {
            try
            {
                // Para productos tipo "Preparado", validamos los ingredientes si se han añadido
                if (model.ProductTypeID == 1 && model.Ingredients.Any())
                {
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
