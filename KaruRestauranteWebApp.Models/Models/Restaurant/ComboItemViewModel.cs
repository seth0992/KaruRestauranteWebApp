using KaruRestauranteWebApp.Models.Entities.Restaurant;

namespace KaruRestauranteWebApp.Models.Models.Restaurant
{
    public class ComboItemViewModel
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public FastFoodItemModel? Item { get; set; }
        public decimal Subtotal => Item?.SellingPrice * Quantity ?? 0;
    }
}
