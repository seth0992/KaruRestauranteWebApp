using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Models.Models.Restaurant;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Product
{
      public partial class EditProduct
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
        private List<ItemIngredientDetailModel> originalIngredients;

        private bool showIngredients = true;

        private bool isLoaded;
        private ProductFormModel model = new();
        private List<CategoryModel> categories = new();
        private List<IngredientModel> availableIngredients = new();
        private List<ProductTypeModel> productTypes = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadCategories();
                await LoadIngredients();
                await LoadProductTypes();
                await LoadProduct();

                // Configurar la visibilidad de ingredientes según el tipo de producto
                showIngredients = model.ProductTypeID == 1;


                isLoaded = true;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar datos: {ex.Message}", 4000);
                NavigationManager.NavigateTo("/products");
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
                var product = JsonConvert.DeserializeObject<FastFoodItemModel>(response.Data.ToString());
                if (product != null)
                {
                    model = new ProductFormModel
                    {
                        ID = product.ID,
                        Name = product.Name,
                        Description = product.Description,
                        CategoryID = product.CategoryID,
                        SellingPrice = product.SellingPrice,
                        EstimatedCost = product.EstimatedCost,
                        ProductTypeID = product.ProductTypeID,
                        IsAvailable = product.IsAvailable,
                        ImageUrl = product.ImageUrl,
                        EstimatedPreparationTime = product.EstimatedPreparationTime,
                        Ingredients = product.Ingredients.Select(i => new ItemIngredientDetailModel
                        {
                            ID = i.ID,
                            IngredientID = i.IngredientID,
                            Quantity = i.Quantity,
                            IsOptional = i.IsOptional,
                            CanBeExtra = i.CanBeExtra,
                            ExtraPrice = i.ExtraPrice
                        }).ToList()
                    };

                    originalIngredients = new List<ItemIngredientDetailModel>(model.Ingredients);
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", "No se encontró el producto", 4000);
                    NavigationManager.NavigateTo("/products");
                }
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", "Error al cargar el producto", 4000);
                NavigationManager.NavigateTo("/products");
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
                //model.Ingredients.Add(new ItemIngredientDetailModel());
                //StateHasChanged(); // Forzar actualización de la UI
            }
        }

        
        private async Task RemoveIngredientAsync(ItemIngredientDetailModel ingredient)
        {
            model.Ingredients.Remove(ingredient);
            await InvokeAsync(StateHasChanged);
        }

        private void IngredientChanged(object value, ItemIngredientDetailModel ingredient)
        {
            if (value != null && int.TryParse(value.ToString(), out int ingredientId))
            {
                ingredient.IngredientID = ingredientId;

                // Buscar el ingrediente seleccionado
                var selectedIngredient = availableIngredients.FirstOrDefault(i => i.ID == ingredientId);

                if (selectedIngredient != null)
                {
                    // Verificar si este ingrediente ya existía en la receta original
                    var existingIngredient = originalIngredients?.FirstOrDefault(
                        i => i.IngredientID == ingredientId);

                    if (existingIngredient != null)
                    {
                        // Si ya existía, mantener sus valores originales
                        ingredient.Quantity = existingIngredient.Quantity;
                        ingredient.IsOptional = existingIngredient.IsOptional;
                        ingredient.CanBeExtra = existingIngredient.CanBeExtra;
                        ingredient.ExtraPrice = existingIngredient.ExtraPrice;
                    }
                    else
                    {
                        // Si es un ingrediente nuevo para este producto, usar valores predeterminados
                        if (ingredient.Quantity <= 0)
                        {
                            ingredient.Quantity = 1;
                        }

                        // Establecer valores predeterminados solo si no han sido configurados
                        if (ingredient.ExtraPrice <= 0 && ingredient.CanBeExtra)
                        {
                            ingredient.ExtraPrice = Math.Round(selectedIngredient.PurchasePrice * 0.5m, 2);
                        }
                    }

                    // Notificar al usuario
                    NotificationService.Notify(
                        NotificationSeverity.Info,
                        "Ingrediente seleccionado",
                        $"Has seleccionado {selectedIngredient.Name}",
                        2000
                    );
                }

                StateHasChanged();
            }
        }



        private async Task ShowImagePreview()
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
                    ID = model.ID,
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

                var response = await ApiClient.PutAsync<BaseResponseModel, FastFoodItemDTO>(
                    $"api/FastFood/{Id}", productDto);

                if (response?.Success == true)
                {
                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Producto actualizado exitosamente", 4000);
                    NavigationManager.NavigateTo("/products");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", response?.ErrorMessage ?? "Error al actualizar producto", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al actualizar producto: {ex.Message}", 4000);
            }
        }

        private class ProductFormModel
        {
            public int ID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int CategoryID { get; set; }
            public decimal SellingPrice { get; set; }
            public decimal EstimatedCost { get; set; }
            public int ProductTypeID { get; set; } = 1;
            public bool IsAvailable { get; set; } = true;
            public string ImageUrl { get; set; } = string.Empty;
            public int? EstimatedPreparationTime { get; set; }
            public List<ItemIngredientDetailModel> Ingredients { get; set; } = new();
        }

        private class ItemIngredientDetailModel
        {
            public int ID { get; set; }
            public int IngredientID { get; set; }
            public decimal Quantity { get; set; } = 1;
            public bool IsOptional { get; set; }
            public bool CanBeExtra { get; set; }
            public decimal ExtraPrice { get; set; }
        }
    }
}
