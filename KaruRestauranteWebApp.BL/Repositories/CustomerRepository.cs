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
    public interface ICustomerRepository
    {
        Task<List<CustomerModel>> GetAllAsync(bool includeInactive = false);
        Task<CustomerModel?> GetByIdAsync(int id);
        Task<CustomerModel?> GetByIdentificationAsync(string identificationType, string identificationNumber);
        Task<CustomerModel> CreateAsync(CustomerModel customer);
        Task UpdateAsync(CustomerModel customer);
        Task<bool> DeleteAsync(int id);
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerModel>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Customers.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<CustomerModel?> GetByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<CustomerModel?> GetByIdentificationAsync(string identificationType, string identificationNumber)
        {
            return await _context.Customers
                .Where(c => c.IdentificationType == identificationType &&
                           c.IdentificationNumber == identificationNumber &&
                           c.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<CustomerModel> CreateAsync(CustomerModel customer)
        {
            customer.CreatedAt = DateTime.UtcNow;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task UpdateAsync(CustomerModel customer)
        {
            var local = _context.Customers.Local.FirstOrDefault(c => c.ID == customer.ID);
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            customer.UpdatedAt = DateTime.UtcNow;
            _context.Entry(customer).State = EntityState.Modified;
            _context.Entry(customer).Property(x => x.CreatedAt).IsModified = false;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return false;

            // Soft delete
            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
