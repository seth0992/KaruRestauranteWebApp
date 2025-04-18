using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IOrderDetailRepository
    {
        Task<OrderDetailModel?> GetByIdAsync(int id);
        Task<List<OrderDetailModel>> GetByOrderIdAsync(int orderId);
        Task<OrderDetailModel> CreateAsync(OrderDetailModel orderDetail);
        Task UpdateAsync(OrderDetailModel orderDetail);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteAsync(int id);
        Task<bool> AddCustomizationsAsync(int orderDetailId, List<OrderItemCustomizationModel> customizations);

    }

    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly AppDbContext _context;

        public OrderDetailRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDetailModel?> GetByIdAsync(int id)
        {
            return await _context.OrderDetails
                .Include(od => od.Customizations)
                    .ThenInclude(c => c.Ingredient)
                .FirstOrDefaultAsync(od => od.ID == id);
        }

        public async Task<List<OrderDetailModel>> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderDetails
                .Include(od => od.Customizations)
                    .ThenInclude(c => c.Ingredient)
                .Where(od => od.OrderID == orderId)
                .ToListAsync();
        }
        public async Task<bool> AddCustomizationsAsync(int orderDetailId, List<OrderItemCustomizationModel> customizations)
        {
            try
            {
                // Primero recuperar el detalle y el producto asociado
                var orderDetail = await _context.OrderDetails
                    .Include(d => d.Customizations)
                    .FirstOrDefaultAsync(d => d.ID == orderDetailId);

                if (orderDetail == null) return false;

                if (orderDetail.ItemType == "Product")
                {
                    var product = await _context.FastFoodItems
                        .Include(p => p.Ingredients)
                        .FirstOrDefaultAsync(p => p.ID == orderDetail.ItemID);

                    if (product == null) return false;

                    // Filtrar las personalizaciones válidas
                    var validCustomizations = new List<OrderItemCustomizationModel>();

                    foreach (var customization in customizations)
                    {
                        bool isValid = false;

                        if (customization.CustomizationType == "Remove")
                        {
                            // Solo se pueden quitar ingredientes del producto
                            isValid = product.Ingredients.Any(i => i.IngredientID == customization.IngredientID);
                        }
                        else if (customization.CustomizationType == "Extra")
                        {
                            // Solo se pueden agregar extras permitidos
                            var ingredient = product.Ingredients.FirstOrDefault(i =>
                                i.IngredientID == customization.IngredientID && i.CanBeExtra);

                            isValid = ingredient != null;

                            // Asegurar que el precio extra es el correcto
                            if (isValid)
                            {
                                customization.ExtraCharge = ingredient.ExtraPrice;
                            }
                        }
                        else
                        {
                            // Otros tipos de personalizaciones
                            isValid = true;
                        }

                        if (isValid)
                        {
                            customization.OrderDetailID = orderDetailId;
                            validCustomizations.Add(customization);
                        }
                    }

                    // Eliminar personalizaciones existentes
                    _context.OrderItemCustomizations
                        .RemoveRange(_context.OrderItemCustomizations
                            .Where(c => c.OrderDetailID == orderDetailId));

                    // Añadir las nuevas personalizaciones válidas
                    await _context.OrderItemCustomizations.AddRangeAsync(validCustomizations);
                    await _context.SaveChangesAsync();

                    return true;
                }

                // Para combos u otros tipos
                return false;
            }
            catch (Exception ex)
            {
              // _logger.LogError(ex, "Error al añadir personalizaciones al detalle {OrderDetailId}", orderDetailId);
                return false;
            }
        }
        public async Task<OrderDetailModel> CreateAsync(OrderDetailModel orderDetail)
        {
            await _context.OrderDetails.AddAsync(orderDetail);
            await _context.SaveChangesAsync();
            return orderDetail;
        }

        public async Task UpdateAsync(OrderDetailModel orderDetail)
        {
            var local = _context.OrderDetails.Local.FirstOrDefault(od => od.ID == orderDetail.ID);
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(orderDetail).State = EntityState.Modified;

            // Asegurar que los campos de descuento se actualizan correctamente
            _context.Entry(orderDetail).Property(x => x.DiscountPercentage).IsModified = true;
            _context.Entry(orderDetail).Property(x => x.DiscountAmount).IsModified = true;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null)
                return false;

            orderDetail.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null)
                return false;

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
