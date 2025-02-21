using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IRoleRepository
    {
        Task<List<RoleModel>> GetAllRolesAsync();
        Task<RoleModel?> GetRoleByIdAsync(int id);
        Task<RoleModel> CreateRoleAsync(RoleModel role);
        Task UpdateRoleAsync(RoleModel role);
        Task<bool> DeleteRoleAsync(int id);
    }

    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoleModel>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<RoleModel?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<RoleModel> CreateRoleAsync(RoleModel role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task UpdateRoleAsync(RoleModel role)
        {
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
