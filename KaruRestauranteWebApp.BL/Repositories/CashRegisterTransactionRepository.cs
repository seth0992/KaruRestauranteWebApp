using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.CashRegister;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface ICashRegisterTransactionRepository
    {
        Task<List<CashRegisterTransactionModel>> GetBySessionIdAsync(int sessionId);
        Task<CashRegisterTransactionModel?> GetByIdAsync(int id);
        Task<CashRegisterTransactionModel> CreateAsync(CashRegisterTransactionModel transaction);
        Task UpdateAsync(CashRegisterTransactionModel transaction);
        Task<bool> DeleteAsync(int id);
        Task<List<CashRegisterTransactionModel>> GetByDateRangeAsync(DateTime start, DateTime end);
    }
    public class CashRegisterTransactionRepository : ICashRegisterTransactionRepository
    {
        private readonly AppDbContext _context;

        public CashRegisterTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CashRegisterTransactionModel>> GetBySessionIdAsync(int sessionId)
        {
            return await _context.CashRegisterTransactions
                .Include(t => t.User)
                .Include(t => t.RelatedOrder)
                .Where(t => t.SessionID == sessionId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<CashRegisterTransactionModel?> GetByIdAsync(int id)
        {
            return await _context.CashRegisterTransactions
                .Include(t => t.User)
                .Include(t => t.RelatedOrder)
                .FirstOrDefaultAsync(t => t.ID == id);
        }

        public async Task<CashRegisterTransactionModel> CreateAsync(CashRegisterTransactionModel transaction)
        {
            await _context.CashRegisterTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task UpdateAsync(CashRegisterTransactionModel transaction)
        {
            _context.Entry(transaction).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var transaction = await _context.CashRegisterTransactions.FindAsync(id);
            if (transaction == null)
                return false;

            _context.CashRegisterTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CashRegisterTransactionModel>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.CashRegisterTransactions
                .Include(t => t.User)
                .Include(t => t.Session)
                .Include(t => t.RelatedOrder)
                .Where(t => t.TransactionDate >= start && t.TransactionDate <= end)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}
