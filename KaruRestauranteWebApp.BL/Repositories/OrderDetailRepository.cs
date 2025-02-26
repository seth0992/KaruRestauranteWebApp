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
    public interface IOrderDetailRepository
    {
        Task<OrderDetailModel?> GetByIdAsync(int id);
        Task<List<OrderDetailModel>> GetByOrderIdAsync(int orderId);
        Task<OrderDetailModel> CreateAsync(OrderDetailModel orderDetail);
        Task UpdateAsync(OrderDetailModel orderDetail);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteAsync(int id);
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
