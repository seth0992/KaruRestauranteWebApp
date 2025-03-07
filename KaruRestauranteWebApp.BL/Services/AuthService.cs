using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Common.Security;
using KaruRestauranteWebApp.Models.Entities.Users;
using Microsoft.Extensions.Logging;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface IAuthService
    {
        Task<UserModel> GetUserByLogin(string username, string password);

        Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel);

        Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken);

    }

    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository, ILogger<AuthService> logger)
        {
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel)
        {
            await _authRepository.RemoveRefreshTokenByUserID(refreshTokenModel.UserID);
            await _authRepository.AddRefreshTokenModel(refreshTokenModel);
        }

        public Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken)
        {
            return _authRepository.GetRefreshTokenModel(refreshToken);
        }

        public async Task<UserModel> GetUserByLogin(string username, string password)
        {
            try
            {
                var user = await _authRepository.GetUserByUsername(username);
                if (user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    await _authRepository.UpdateLastLogin(user.ID);
                    return user;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetUserByLogin para el usuario {Username}", username);
                throw;
            }

        }
    }

}
