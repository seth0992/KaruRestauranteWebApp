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
    public interface ICashRegisterTransactionService
    {
        Task<List<CashRegisterTransactionDTO>> GetTransactionsBySessionIdAsync(int sessionId);
        Task<CashRegisterTransactionDTO?> GetTransactionByIdAsync(int id);
        Task<CashRegisterTransactionDTO> CreateTransactionAsync(CashRegisterTransactionDTO transactionDto, int userId);
        Task<bool> DeleteTransactionAsync(int id);
        Task<List<CashRegisterTransactionDTO>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end);
    }

    public class CashRegisterTransactionService : ICashRegisterTransactionService
    {
        private readonly ICashRegisterTransactionRepository _transactionRepository;
        private readonly ICashRegisterSessionRepository _sessionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<CashRegisterTransactionService> _logger;

        public CashRegisterTransactionService(
            ICashRegisterTransactionRepository transactionRepository,
            ICashRegisterSessionRepository sessionRepository,
            IOrderRepository orderRepository,
            ILogger<CashRegisterTransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _sessionRepository = sessionRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<List<CashRegisterTransactionDTO>> GetTransactionsBySessionIdAsync(int sessionId)
        {
            try
            {
                var transactions = await _transactionRepository.GetBySessionIdAsync(sessionId);
                return transactions.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones para la sesión {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<CashRegisterTransactionDTO?> GetTransactionByIdAsync(int id)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(id);
                return transaction != null ? MapToDto(transaction) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la transacción {TransactionId}", id);
                throw;
            }
        }

        public async Task<CashRegisterTransactionDTO> CreateTransactionAsync(CashRegisterTransactionDTO transactionDto, int userId)
        {
            try
            {
                // Verificar que haya una sesión abierta
                var openSession = await _sessionRepository.GetCurrentSessionAsync();
                if (openSession == null)
                {
                    throw new ValidationException("No hay sesión de caja abierta");
                }

                // Validar valores
                if (transactionDto.AmountCRC < 0 || transactionDto.AmountUSD < 0)
                {
                    throw new ValidationException("Los montos no pueden ser negativos");
                }

                if (transactionDto.AmountCRC == 0 && transactionDto.AmountUSD == 0)
                {
                    throw new ValidationException("Al menos uno de los montos debe ser mayor a 0");
                }

                // Si es un gasto, verificar que haya suficiente saldo
                if (transactionDto.TransactionType == "Expense")
                {
                    decimal currentBalanceCRC = await _sessionRepository.GetCurrentBalanceCRCAsync(openSession.ID);
                    decimal currentBalanceUSD = await _sessionRepository.GetCurrentBalanceUSDAsync(openSession.ID);

                    if (transactionDto.AmountCRC > currentBalanceCRC)
                    {
                        throw new ValidationException("No hay suficiente saldo en colones para realizar este gasto");
                    }

                    if (transactionDto.AmountUSD > currentBalanceUSD)
                    {
                        throw new ValidationException("No hay suficiente saldo en dólares para realizar este gasto");
                    }
                }

                // Verificar orden relacionada si existe
                if (transactionDto.RelatedOrderID.HasValue)
                {
                    var order = await _orderRepository.GetByIdAsync(transactionDto.RelatedOrderID.Value);
                    if (order == null)
                    {
                        throw new ValidationException($"No se encontró la orden con ID: {transactionDto.RelatedOrderID.Value}");
                    }
                }

                var transaction = new CashRegisterTransactionModel
                {
                    SessionID = openSession.ID,
                    UserID = userId,
                    TransactionDate = DateTime.Now,
                    TransactionType = transactionDto.TransactionType,
                    Description = transactionDto.Description,
                    AmountCRC = transactionDto.AmountCRC,
                    AmountUSD = transactionDto.AmountUSD,
                    PaymentMethod = transactionDto.PaymentMethod,
                    ReferenceNumber = transactionDto.ReferenceNumber,
                    RelatedOrderID = transactionDto.RelatedOrderID
                };

                var createdTransaction = await _transactionRepository.CreateAsync(transaction);
                return MapToDto(createdTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear transacción de caja");
                throw;
            }
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            try
            {
                return await _transactionRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la transacción {TransactionId}", id);
                throw;
            }
        }

        public async Task<List<CashRegisterTransactionDTO>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end)
        {
            try
            {
                // Aseguramos que la fecha final incluya todo el día
                end = end.Date.AddDays(1).AddTicks(-1);

                var transactions = await _transactionRepository.GetByDateRangeAsync(start, end);
                return transactions.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones por rango de fechas");
                throw;
            }
        }

        private CashRegisterTransactionDTO MapToDto(CashRegisterTransactionModel model)
        {
            return new CashRegisterTransactionDTO
            {
                ID = model.ID,
                SessionID = model.SessionID,
                UserID = model.UserID,
                UserName = model.User?.FullName,
                TransactionDate = model.TransactionDate,
                TransactionType = model.TransactionType,
                Description = model.Description,
                AmountCRC = model.AmountCRC,
                AmountUSD = model.AmountUSD,
                PaymentMethod = model.PaymentMethod,
                ReferenceNumber = model.ReferenceNumber,
                RelatedOrderID = model.RelatedOrderID,
                RelatedOrderNumber = model.RelatedOrder?.OrderNumber
            };
        }
    }
}
