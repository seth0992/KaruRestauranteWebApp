using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IAuthRepository
    {
        Task<UserModel> GetUserByLogin(string username, string password);
        Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel);
        Task RemoveRefreshTokenByUserID(int userID);
        Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken);
        Task<UserModel> GetUserByUsername(string username);
        Task<bool> UpdateLastLogin(int userId);
    }
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _appDbContext;

        public AuthRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel)
        {
            await _appDbContext.RefreshTokens.AddAsync(refreshTokenModel);
            await _appDbContext.SaveChangesAsync();
        }

        //public async Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken)
        //{
        //    try
        //    {  // Imprimir o registrar el token recibido
        //        System.Diagnostics.Debug.WriteLine($"Token recibido: {refreshToken}");

        //        // Obtener todos los tokens para verificar
        //        var allTokens = await _appDbContext.RefreshTokens.ToListAsync();


        //        // Primero verificamos si el token existe por sí solo
        //        var tokenExists = await _appDbContext.RefreshTokens
        //            .AnyAsync(n => n.RefreshToken.Equals(refreshToken));

        //        if (!tokenExists)
        //            throw new Exception("Token no encontrado en la base de datos");

        //        // Verificamos la relación con User
        //        var tokenWithUser = await _appDbContext.RefreshTokens
        //            .Include(n => n.User)
        //            .FirstOrDefaultAsync(n => n.RefreshToken == refreshToken);

        //        if (tokenWithUser?.User == null)
        //            throw new Exception("Token encontrado pero User es null");

        //        // Verificamos la relación con UserRoles
        //        var tokenWithUserRoles = await _appDbContext.RefreshTokens
        //            .Include(n => n.User)
        //            .ThenInclude(n => n.UserRoles)
        //            .FirstOrDefaultAsync(n => n.RefreshToken == refreshToken);

        //        if (tokenWithUserRoles?.User?.UserRoles == null)
        //            throw new Exception("UserRoles es null");

        //        // Finalmente la consulta completa
        //        var completeToken = await _appDbContext.RefreshTokens
        //            .Include(n => n.User)
        //            .ThenInclude(n => n.UserRoles)
        //            .ThenInclude(n => n.Role)
        //            .FirstOrDefaultAsync(n => n.RefreshToken == refreshToken);

        //        return completeToken;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Aquí podrías agregar logging
        //        var x = ex.InnerException;
        //        throw;
        //    }
        //}

        public async Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken)
        {
            try
            {
                // Buscar el token directamente
                var tokenWithUser = await _appDbContext.RefreshTokens
                    .Include(n => n.User)
                    .ThenInclude(n => n.UserRoles)
                    .ThenInclude(n => n.Role)
                    .FirstOrDefaultAsync(n => n.RefreshToken == refreshToken);

                if (tokenWithUser == null)
                {
                    // En lugar de lanzar Exception, retornar null
                    return null;
                }

                if (tokenWithUser.User == null)
                {
                    // En lugar de lanzar Exception, retornar null
                    return null;
                }

                return tokenWithUser;
            }
            catch (Exception ex)
            {
                // Loguear la excepción pero no lanzarla
                // Logger.LogError(ex, "Error al obtener RefreshToken");
                return null;
            }
        }

        public async Task<UserModel> GetUserByLogin(string username, string password)
        {
            return await _appDbContext.Users
                    .Include(n => n.UserRoles)
                    .ThenInclude(n => n.Role)
                    .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);
        }

        public async Task RemoveRefreshTokenByUserID(int userID)
        {
            try
            {
                // Obtenemos todos los tokens del usuario de forma asíncrona
                var refreshTokens = await _appDbContext.RefreshTokens
                    .Where(n => n.UserID == userID)
                    .ToListAsync();

                if (refreshTokens.Any())
                {
                    // Removemos todos los tokens encontrados
                    _appDbContext.RefreshTokens.RemoveRange(refreshTokens);

                    try
                    {
                        await _appDbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // Si ocurre un error de concurrencia, intentamos nuevamente
                        // Primero refrescamos el contexto
                        foreach (var entry in _appDbContext.ChangeTracker.Entries())
                        {
                            entry.State = EntityState.Detached;
                        }

                        // Obtenemos los tokens nuevamente y volvemos a intentar
                        refreshTokens = await _appDbContext.RefreshTokens
                            .Where(n => n.UserID == userID)
                            .ToListAsync();

                        if (refreshTokens.Any())
                        {
                            _appDbContext.RefreshTokens.RemoveRange(refreshTokens);
                            await _appDbContext.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error al remover refresh tokens para el usuario {UserId}", userID);
                throw;
            }
        }

        public async Task<UserModel> GetUserByUsername(string username)
        {
            // Obtenemos el usuario incluyendo sus roles y la información del rol
            return await _appDbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<bool> UpdateLastLogin(int userId)
        {
            var user = await _appDbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLogin = DateTime.UtcNow;
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }

}
