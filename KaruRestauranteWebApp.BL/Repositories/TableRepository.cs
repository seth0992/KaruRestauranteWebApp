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
    public interface ITableRepository
    {
        Task<List<TableModel>> GetAllAsync(bool includeInactive = false);
        Task<TableModel?> GetByIdAsync(int id);
        Task<TableModel?> GetByTableNumberAsync(int tableNumber);
        Task<List<TableModel>> GetAvailableTablesAsync();
        Task<TableModel> CreateAsync(TableModel table);
        Task UpdateAsync(TableModel table);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteAsync(int id);
    }

    public class TableRepository : ITableRepository
    {
        private readonly AppDbContext _context;

        public TableRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TableModel>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Tables.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive);
            }

            return await query.OrderBy(t => t.TableNumber).ToListAsync();
        }

        public async Task<TableModel?> GetByIdAsync(int id)
        {
            return await _context.Tables.FindAsync(id);
        }

        public async Task<TableModel?> GetByTableNumberAsync(int tableNumber)
        {
            return await _context.Tables
                .Where(t => t.TableNumber == tableNumber && t.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TableModel>> GetAvailableTablesAsync()
        {
            return await _context.Tables
                .Where(t => t.Status == "Available" && t.IsActive)
                .OrderBy(t => t.TableNumber)
                .ToListAsync();
        }

        public async Task<TableModel> CreateAsync(TableModel table)
        {
            _context.Tables.Add(table);
            await _context.SaveChangesAsync();
            return table;
        }

        public async Task UpdateAsync(TableModel table)
        {
            var local = _context.Tables.Local.FirstOrDefault(t => t.ID == table.ID);
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return false;

            table.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return false;

            // Soft delete
            table.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
