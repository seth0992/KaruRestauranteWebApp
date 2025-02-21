using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IUserRepository
    {

        Task<List<UserModel>> GetAllUsersAsync();
        Task<UserModel> CreateUser(UserModel user);
        Task UpdateUser(UserModel user);
        Task<UserModel> GetUserById(int id);
        Task<bool> UpdateUserRoles(int userId, List<int> roleIds);
        Task<bool> DeactivateUser(int userId);
    }

    public class UserRepository(AppDbContext appDbContext) : IUserRepository
    {
        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            return await appDbContext.Users
                 .Include(u => u.UserRoles)
                 .ThenInclude(u => u.Role)
                 .ToListAsync();
        }
        public async Task<UserModel> GetUserById(int id)
        {
            // Obtenemos el usuario por ID incluyendo sus roles
            return await appDbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.ID == id);
        }

        public async Task<UserModel> CreateUser(UserModel user)
        {
            // Verificamos si ya existe un usuario con el mismo username o email
            if (await appDbContext.Users.AnyAsync(u => u.Username == user.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }

            if (await appDbContext.Users.AnyAsync(u => u.Email == user.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Establecemos los valores por defecto
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            // Agregamos el usuario a la base de datos
            await appDbContext.Users.AddAsync(user);
            await appDbContext.SaveChangesAsync();

            return user;
        }

        public async Task UpdateUser(UserModel user)
        {
            // Verificamos si existe otro usuario con el mismo email (excluyendo el usuario actual)
            if (await appDbContext.Users.AnyAsync(u => u.Email == user.Email && u.ID != user.ID))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Actualizamos el usuario
            appDbContext.Entry(user).State = EntityState.Modified;

            // No actualizamos la contraseña ni la fecha de creación
            appDbContext.Entry(user).Property(x => x.PasswordHash).IsModified = false;
            appDbContext.Entry(user).Property(x => x.CreatedAt).IsModified = false;

            await appDbContext.SaveChangesAsync();
        }
        public async Task<bool> DeactivateUser(int userId)
        {
            var user = await appDbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = false;
                await appDbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateUserRoles(int userId, List<int> roleIds)
        {
            var user = await appDbContext.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.ID == userId);

            if (user == null)
                return false;

            // Eliminamos los roles actuales
            appDbContext.UserRoles.RemoveRange(user.UserRoles);

            // Agregamos los nuevos roles
            foreach (var roleId in roleIds)
            {
                user.UserRoles.Add(new UserRolModel
                {
                    UserID = userId,
                    RoleID = roleId
                });
            }

            await appDbContext.SaveChangesAsync();
            return true;
        }
    }

}
