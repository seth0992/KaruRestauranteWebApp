using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Common.Security;
using KaruRestauranteWebApp.Models.Entities.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllUsersAsync();
        Task<UserModel> GetUserById(int id);
        Task<UserModel> CreateUser(UserModel user, string password);
        Task UpdateUser(UserModel user);
        Task<bool> ChangePassword(int userId, string currentPassword, string newPassword);
        Task<bool> UpdateUserRoles(int userId, List<int> roleIds);
        Task<bool> DeactivateUser(int userId);

        Task<bool> ReactivateUser(int userId);

    }

    public class UserServices(IUserRepository userRepository, ILogger<UserModel> logger) : IUserService
    {
        public async Task<UserModel> CreateUser(UserModel user, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("La contraseña no puede estar vacía");

                // Guardar los roles seleccionados
                var selectedRoles = user.UserRoles.Select(ur => ur.RoleID).ToList();

                // Limpiar la colección de UserRoles antes de crear el usuario
                user.UserRoles.Clear();

                // Establecer el hash de la contraseña
                user.PasswordHash = PasswordHasher.HashPassword(password);

                // Crear el usuario
                var createdUser = await userRepository.CreateUser(user);

                // Añadir los roles seleccionados
                if (selectedRoles.Any())
                {
                    foreach (var roleId in selectedRoles)
                    {
                        createdUser.UserRoles.Add(new UserRolModel
                        {
                            UserID = createdUser.ID,
                            RoleID = roleId
                        });
                    }
                    await userRepository.UpdateUser(createdUser);
                }

                return createdUser;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear usuario {Username}", user.Username);
                throw;
            }
        }

        public async Task<bool> ChangePassword(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await userRepository.GetUserById(userId);
                if (user != null && PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                {
                    user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                    await userRepository.UpdateUser(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cambiar contraseña para el usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<UserModel?> GetUserById(int id)
        {
            try
            {
                var user = await userRepository.GetUserById(id);
                // Validar la existencia del usuario, pero no su estado
                if (user == null)
                {
                    logger.LogWarning("Usuario no encontrado: {UserId}", id);
                    return null;
                }
                return user;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener usuario por ID {UserId}", id);
                throw;
            }
        }


        public async Task<bool> DeactivateUser(int userId)
        {
            try
            {
                return await userRepository.DeactivateUser(userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al desactivar usuario {UserId}", userId);
                throw;
            }
        }

        public Task<List<UserModel>> GetAllUsersAsync()
        {
            return userRepository.GetAllUsersAsync();
        }


        public async Task UpdateUser(UserModel user)
        {
            try
            {
                // Obtener el usuario existente
                var existingUser = await userRepository.GetUserById(user.ID);
                if (existingUser == null)
                {
                    throw new InvalidOperationException("Usuario no encontrado");
                }

                // Actualizar solo los campos permitidos
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;
                existingUser.IsActive = user.IsActive;

                await userRepository.UpdateUser(existingUser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar usuario {UserId}", user.ID);
                throw;
            }
        }

        public async Task<bool> UpdateUserRoles(int userId, List<int> roleIds)
        {
            try
            {
                return await userRepository.UpdateUserRoles(userId, roleIds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar roles para usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ReactivateUser(int userId)
        {
            try
            {
                var user = await userRepository.GetUserById(userId);
                if (user == null || user.IsActive)
                {
                    return false;
                }

                user.IsActive = true;
                await userRepository.UpdateUser(user);
                logger.LogInformation("Usuario {UserId} reactivado exitosamente", userId);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al reactivar usuario {UserId}", userId);
                throw;
            }
        }
    }

}
