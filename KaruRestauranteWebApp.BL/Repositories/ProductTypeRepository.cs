using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IProductTypeRepository
    {
        Task<List<ProductTypeModel>> GetAllAsync();
        Task<ProductTypeModel?> GetByIdAsync(int id);
    }

    public class ProductTypeRepository : IProductTypeRepository
    {
        private readonly AppDbContext _context;

        public ProductTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductTypeModel>> GetAllAsync()
        {
            return await _context.ProductTypes.ToListAsync();
        }

        public async Task<ProductTypeModel?> GetByIdAsync(int id)
        {
            return await _context.ProductTypes.FindAsync(id);
        }
    }
}
