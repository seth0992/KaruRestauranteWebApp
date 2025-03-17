
using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.CashRegister;
using KaruRestauranteWebApp.Models.Models.CashRegister;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Services
{

    public interface ICashRegisterSessionService
    {
        Task<List<CashRegisterSessionDTO>> GetAllSessionsAsync();
        Task<CashRegisterSessionDTO?> GetSessionByIdAsync(int id);
        Task<CashRegisterSessionDTO?> GetCurrentSessionAsync();
        Task<CashRegisterSessionDTO> OpenSessionAsync(CashRegisterSessionDTO sessionDto, int userId);
        Task<bool> CloseSessionAsync(int id, CashRegisterSessionDTO sessionDto, int userId);
        Task<bool> HasOpenSessionAsync();
        Task<decimal> GetCurrentBalanceCRCAsync();
        Task<decimal> GetCurrentBalanceUSDAsync();
    }

    public class CashRegisterSessionService : ICashRegisterSessionService
    {
        private readonly ICashRegisterSessionRepository _sessionRepository;
        private readonly ICashRegisterTransactionRepository _transactionRepository;
        private readonly ILogger<CashRegisterSessionService> _logger;

        public CashRegisterSessionService(
            ICashRegisterSessionRepository sessionRepository,
            ICashRegisterTransactionRepository transactionRepository,
            ILogger<CashRegisterSessionService> logger)
        {
            _sessionRepository = sessionRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public async Task<List<CashRegisterSessionDTO>> GetAllSessionsAsync()
        {
            try
            {
                var sessions = await _sessionRepository.GetAllAsync();
                return sessions.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las sesiones de caja");
                throw;
            }
        }

        public async Task<CashRegisterSessionDTO?> GetSessionByIdAsync(int id)
        {
            try
            {
                var session = await _sessionRepository.GetByIdAsync(id);
                if (session == null)
                    return null;

                var dto = MapToDto(session);
                dto.CurrentBalanceCRC = await _sessionRepository.GetCurrentBalanceCRCAsync(id);
                dto.CurrentBalanceUSD = await _sessionRepository.GetCurrentBalanceUSDAsync(id);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la sesión de caja con ID: {SessionId}", id);
                throw;
            }
        }

        public async Task<CashRegisterSessionDTO?> GetCurrentSessionAsync()
        {
            try
            {
                var session = await _sessionRepository.GetCurrentSessionAsync();
                if (session == null)
                    return null;

                var dto = MapToDto(session);
                dto.CurrentBalanceCRC = await _sessionRepository.GetCurrentBalanceCRCAsync(session.ID);
                dto.CurrentBalanceUSD = await _sessionRepository.GetCurrentBalanceUSDAsync(session.ID);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la sesión de caja actual");
                throw;
            }
        }

        public async Task<CashRegisterSessionDTO> OpenSessionAsync(CashRegisterSessionDTO sessionDto, int userId)
        {
            try
            {
                // Verificar que no haya una sesión abierta
                if (await _sessionRepository.HasOpenSessionAsync())
                {
                    throw new ValidationException("Ya existe una sesión de caja abierta");
                }

                // Validar que los montos iniciales coincidan con la suma de billetes y monedas
                if (sessionDto.InitialBillsCRC + sessionDto.InitialCoinsCRC != sessionDto.InitialAmountCRC)
                {
                    throw new ValidationException("El monto inicial en colones no coincide con la suma de billetes y monedas");
                }

                if (sessionDto.InitialBillsUSD + sessionDto.InitialCoinsUSD != sessionDto.InitialAmountUSD)
                {
                    throw new ValidationException("El monto inicial en dólares no coincide con la suma de billetes y monedas");
                }

                var session = new CashRegisterSessionModel
                {
                    OpeningDate = DateTime.Now,
                    OpeningUserID = userId,
                    InitialAmountCRC = sessionDto.InitialAmountCRC,
                    InitialAmountUSD = sessionDto.InitialAmountUSD,
                    InitialBillsCRC = sessionDto.InitialBillsCRC,
                    InitialCoinsCRC = sessionDto.InitialCoinsCRC,
                    InitialBillsUSD = sessionDto.InitialBillsUSD,
                    InitialCoinsUSD = sessionDto.InitialCoinsUSD,
                    Status = "Open",
                    Notes = sessionDto.Notes
                };

                var createdSession = await _sessionRepository.CreateAsync(session);
                return MapToDto(createdSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir la sesión de caja");
                throw;
            }
        }

        public async Task<bool> CloseSessionAsync(int id, CashRegisterSessionDTO sessionDto, int userId)
        {
            try
            {
                // Validar que los montos finales coincidan con la suma de billetes y monedas
                if (sessionDto.FinalBillsCRC + sessionDto.FinalCoinsCRC != sessionDto.FinalAmountCRC)
                {
                    throw new ValidationException("El monto final en colones no coincide con la suma de billetes y monedas");
                }

                if (sessionDto.FinalBillsUSD + sessionDto.FinalCoinsUSD != sessionDto.FinalAmountUSD)
                {
                    throw new ValidationException("El monto final en dólares no coincide con la suma de billetes y monedas");
                }

                return await _sessionRepository.CloseSessionAsync(
                    id, userId,
                    sessionDto.FinalAmountCRC ?? 0,
                    sessionDto.FinalAmountUSD ?? 0,
                    sessionDto.FinalBillsCRC ?? 0,
                    sessionDto.FinalCoinsCRC ?? 0,
                    sessionDto.FinalBillsUSD ?? 0,
                    sessionDto.FinalCoinsUSD ?? 0,
                    sessionDto.Notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar la sesión de caja {SessionId}", id);
                throw;
            }
        }

        public async Task<bool> HasOpenSessionAsync()
        {
            try
            {
                return await _sessionRepository.HasOpenSessionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si hay una sesión de caja abierta");
                throw;
            }
        }
        public async Task<decimal> GetCurrentBalanceCRCAsync()
        {
            try
            {
                var session = await _sessionRepository.GetCurrentSessionAsync();
                if (session == null)
                    return 0;

                return await _sessionRepository.GetCurrentBalanceCRCAsync(session.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el balance actual en colones");
                throw;
            }
        }

        public async Task<decimal> GetCurrentBalanceUSDAsync()
        {
            try
            {
                var session = await _sessionRepository.GetCurrentSessionAsync();
                if (session == null)
                    return 0;

                return await _sessionRepository.GetCurrentBalanceUSDAsync(session.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el balance actual en dólares");
                throw;
            }
        }

        private CashRegisterSessionDTO MapToDto(CashRegisterSessionModel model)
        {
            return new CashRegisterSessionDTO
            {
                ID = model.ID,
                OpeningDate = model.OpeningDate,
                ClosingDate = model.ClosingDate,
                OpeningUserID = model.OpeningUserID,
                OpeningUserName = model.OpeningUser?.FullName,
                ClosingUserID = model.ClosingUserID,
                ClosingUserName = model.ClosingUser?.FullName,
                InitialAmountCRC = model.InitialAmountCRC,
                InitialAmountUSD = model.InitialAmountUSD,
                FinalAmountCRC = model.FinalAmountCRC,
                FinalAmountUSD = model.FinalAmountUSD,
                InitialBillsCRC = model.InitialBillsCRC,
                InitialCoinsCRC = model.InitialCoinsCRC,
                InitialBillsUSD = model.InitialBillsUSD,
                InitialCoinsUSD = model.InitialCoinsUSD,
                FinalBillsCRC = model.FinalBillsCRC,
                FinalCoinsCRC = model.FinalCoinsCRC,
                FinalBillsUSD = model.FinalBillsUSD,
                FinalCoinsUSD = model.FinalCoinsUSD,
                Status = model.Status,
                Notes = model.Notes
            };
        }
    }
}
