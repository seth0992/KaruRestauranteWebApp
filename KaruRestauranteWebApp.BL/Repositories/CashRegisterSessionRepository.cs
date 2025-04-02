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
    public interface ICashRegisterSessionRepository
    {
        Task<List<CashRegisterSessionModel>> GetAllAsync();
        Task<CashRegisterSessionModel?> GetByIdAsync(int id);
        Task<CashRegisterSessionModel?> GetCurrentSessionAsync();
        Task<CashRegisterSessionModel> CreateAsync(CashRegisterSessionModel session);
        Task UpdateAsync(CashRegisterSessionModel session);
        Task<bool> CloseSessionAsync(int id, int userId, decimal finalAmountCRC, decimal finalAmountUSD,
            decimal finalBillsCRC, decimal finalCoinsCRC, decimal finalBillsUSD, decimal finalCoinsUSD, string notes);
        Task<bool> HasOpenSessionAsync();
        Task<decimal> GetCurrentBalanceCRCAsync(int sessionId);
        Task<decimal> GetCurrentBalanceUSDAsync(int sessionId);
    }

    public class CashRegisterSessionRepository : ICashRegisterSessionRepository
    {
        private readonly AppDbContext _context;

        public CashRegisterSessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CashRegisterSessionModel>> GetAllAsync()
        {
            return await _context.CashRegisterSessions
                .Include(s => s.OpeningUser)
                .Include(s => s.ClosingUser)
                .OrderByDescending(s => s.OpeningDate)
                .ToListAsync();
        }

        public async Task<CashRegisterSessionModel?> GetByIdAsync(int id)
        {
            return await _context.CashRegisterSessions
                .Include(s => s.OpeningUser)
                .Include(s => s.ClosingUser)
                .Include(s => s.Transactions)
                .FirstOrDefaultAsync(s => s.ID == id);
        }

        public async Task<CashRegisterSessionModel?> GetCurrentSessionAsync()
        {
            return await _context.CashRegisterSessions
     .Include(s => s.OpeningUser)
     .Include(s => s.ClosingUser)
     .FirstOrDefaultAsync(s => s.Status == "Open");
        }

        public async Task<CashRegisterSessionModel> CreateAsync(CashRegisterSessionModel session)
        {
            await _context.CashRegisterSessions.AddAsync(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task UpdateAsync(CashRegisterSessionModel session)
        {
            _context.Entry(session).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CloseSessionAsync(int id, int userId, decimal finalAmountCRC, decimal finalAmountUSD,
            decimal finalBillsCRC, decimal finalCoinsCRC, decimal finalBillsUSD, decimal finalCoinsUSD, string notes)
        {
            var session = await _context.CashRegisterSessions.FindAsync(id);
            if (session == null || session.Status != "Open")
                return false;

            session.ClosingDate = DateTime.Now;
            session.ClosingUserID = userId;
            session.FinalAmountCRC = finalAmountCRC;
            session.FinalAmountUSD = finalAmountUSD;
            session.FinalBillsCRC = finalBillsCRC;
            session.FinalCoinsCRC = finalCoinsCRC;
            session.FinalBillsUSD = finalBillsUSD;
            session.FinalCoinsUSD = finalCoinsUSD;
            session.Status = "Closed";
            session.Notes = notes;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasOpenSessionAsync()
        {
            return await _context.CashRegisterSessions
                .AnyAsync(s => s.Status == "Open");
        }

        public async Task<decimal> GetCurrentBalanceCRCAsync(int sessionId)
        {
            var session = await _context.CashRegisterSessions.FindAsync(sessionId);
            if (session == null)
                return 0;

            var incomes = await _context.CashRegisterTransactions
                .Where(t => t.SessionID == sessionId && t.TransactionType == "Income")
                .SumAsync(t => t.AmountCRC);

            var expenses = await _context.CashRegisterTransactions
                .Where(t => t.SessionID == sessionId && t.TransactionType == "Expense")
                .SumAsync(t => t.AmountCRC);

            return session.InitialAmountCRC + incomes - expenses;
        }

        public async Task<decimal> GetCurrentBalanceUSDAsync(int sessionId)
        {
            var session = await _context.CashRegisterSessions.FindAsync(sessionId);
            if (session == null)
                return 0;

            var incomes = await _context.CashRegisterTransactions
                .Where(t => t.SessionID == sessionId && t.TransactionType == "Income")
                .SumAsync(t => t.AmountUSD);

            var expenses = await _context.CashRegisterTransactions
                .Where(t => t.SessionID == sessionId && t.TransactionType == "Expense")
                .SumAsync(t => t.AmountUSD);

            return session.InitialAmountUSD + incomes - expenses;
        }
    }
}
