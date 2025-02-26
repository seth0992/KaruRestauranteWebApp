using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IOrderRepository
    {
        Task<List<OrderModel>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<OrderModel?> GetByIdAsync(int id);
        Task<OrderModel?> GetByOrderNumberAsync(string orderNumber);
        Task<List<OrderModel>> GetByStatusAsync(string status);
        Task<List<OrderModel>> GetByTableAsync(int tableId);
        Task<List<OrderModel>> GetByCustomerAsync(int customerId);
        Task<OrderModel> CreateAsync(OrderModel order);
        Task UpdateAsync(OrderModel order);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus);
        Task<string> GenerateOrderNumberAsync();
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderModel>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .Include(o => o.User)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.CreatedAt <= endDate);
            }

            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<OrderModel?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Customizations)
                        .ThenInclude(c => c.Ingredient)
                .Include(o => o.Payments)
                .Include(o => o.ElectronicInvoice)
                .FirstOrDefaultAsync(o => o.ID == id);
        }

        public async Task<OrderModel?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<List<OrderModel>> GetByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .Include(o => o.User)
                .Where(o => o.OrderStatus == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<OrderModel>> GetByTableAsync(int tableId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .Include(o => o.User)
                .Where(o => o.TableID == tableId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<OrderModel>> GetByCustomerAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .Include(o => o.User)
                .Where(o => o.CustomerID == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<OrderModel> CreateAsync(OrderModel order)
        {
            // Generar número de orden si no está definido
            if (string.IsNullOrEmpty(order.OrderNumber))
            {
                order.OrderNumber = await GenerateOrderNumberAsync();
            }

            order.CreatedAt = DateTime.UtcNow;

            // Crear la orden primero (sin detalles)
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task UpdateAsync(OrderModel order)
        {
            var local = _context.Orders.Local.FirstOrDefault(o => o.ID == order.ID);
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            order.UpdatedAt = DateTime.UtcNow;
            _context.Entry(order).State = EntityState.Modified;
            _context.Entry(order).Property(x => x.CreatedAt).IsModified = false;
            _context.Entry(order).Property(x => x.OrderNumber).IsModified = false;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            order.OrderStatus = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            order.PaymentStatus = paymentStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            // Formato: OD-YYYYMMDD-XXX donde XXX es un contador secuencial para el día
            var today = DateTime.Now;
            var dateStr = today.ToString("yyyyMMdd");
            var prefix = $"OD-{dateStr}-";

            // Buscar la última orden del día
            var lastOrder = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            int sequenceNumber = 1;
            if (lastOrder != null)
            {
                var lastSequence = lastOrder.OrderNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastNumber))
                {
                    sequenceNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{sequenceNumber:D3}";
        }
    }
}
