using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IPaymentRepository
    {
        Task<List<PaymentModel>> GetByOrderIdAsync(int orderId);
        Task<PaymentModel?> GetByIdAsync(int id);
        Task<PaymentModel> CreateAsync(PaymentModel payment);
        Task UpdateAsync(PaymentModel payment);
        Task<bool> DeleteAsync(int id);
        Task<decimal> GetTotalPaidForOrderAsync(int orderId);
        Task<PaymentModel> GetByReferenceNumberAsync(string referenceNumber, string paymentMethod);

    }

    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentModel> GetByReferenceNumberAsync(string referenceNumber, string paymentMethod)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.ReferenceNumber == referenceNumber &&
                    p.PaymentMethod == paymentMethod);
        }

        public async Task<List<PaymentModel>> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Include(p => p.ProcessedByUser)
                .Where(p => p.OrderID == orderId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<PaymentModel?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.ProcessedByUser)
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<PaymentModel> CreateAsync(PaymentModel payment)
        {
            payment.PaymentDate = DateTime.UtcNow;
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task UpdateAsync(PaymentModel payment)
        {
            var local = _context.Payments.Local.FirstOrDefault(p => p.ID == payment.ID);
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(payment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalPaidForOrderAsync(int orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderID == orderId)
                .SumAsync(p => p.Amount);
        }
    }
}
