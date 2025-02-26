using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Models.Orders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerModel>> GetAllCustomersAsync(bool includeInactive = false);
        Task<CustomerModel?> GetCustomerByIdAsync(int id);
        Task<CustomerModel?> GetCustomerByIdentificationAsync(string identificationType, string identificationNumber);
        Task<CustomerModel> CreateCustomerAsync(CustomerDTO customerDto);
        Task UpdateCustomerAsync(CustomerDTO customerDto);
        Task<bool> DeleteCustomerAsync(int id);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ICustomerRepository customerRepository, ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<List<CustomerModel>> GetAllCustomersAsync(bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de clientes. Incluir inactivos: {IncludeInactive}", includeInactive);
                return await _customerRepository.GetAllAsync(includeInactive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes");
                throw;
            }
        }

        public async Task<CustomerModel?> GetCustomerByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando cliente con ID: {CustomerId}", id);
                return await _customerRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el cliente con ID: {CustomerId}", id);
                throw;
            }
        }

        public async Task<CustomerModel?> GetCustomerByIdentificationAsync(string identificationType, string identificationNumber)
        {
            try
            {
                _logger.LogInformation("Buscando cliente con identificación: {IdentificationType} {IdentificationNumber}",
                    identificationType, identificationNumber);
                return await _customerRepository.GetByIdentificationAsync(identificationType, identificationNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el cliente con identificación: {IdentificationType} {IdentificationNumber}",
                    identificationType, identificationNumber);
                throw;
            }
        }

        public async Task<CustomerModel> CreateCustomerAsync(CustomerDTO customerDto)
        {
            try
            {
                // Validar los datos
                if (string.IsNullOrWhiteSpace(customerDto.Name))
                {
                    throw new ValidationException("El nombre del cliente es requerido");
                }

                // Verificar si existe un cliente con la misma identificación
                if (!string.IsNullOrEmpty(customerDto.IdentificationType) && !string.IsNullOrEmpty(customerDto.IdentificationNumber))
                {
                    var existingCustomer = await _customerRepository.GetByIdentificationAsync(
                        customerDto.IdentificationType, customerDto.IdentificationNumber);

                    if (existingCustomer != null)
                    {
                        throw new ValidationException("Ya existe un cliente con esta identificación");
                    }
                }

                // Crear el modelo
                var customer = new CustomerModel
                {
                    Name = customerDto.Name,
                    Email = customerDto.Email,
                    PhoneNumber = customerDto.PhoneNumber,
                    IdentificationType = customerDto.IdentificationType,
                    IdentificationNumber = customerDto.IdentificationNumber,
                    Address = customerDto.Address,
                    IsActive = customerDto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                return await _customerRepository.CreateAsync(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el cliente {CustomerName}", customerDto.Name);
                throw;
            }
        }

        public async Task UpdateCustomerAsync(CustomerDTO customerDto)
        {
            try
            {
                // Validar los datos
                if (string.IsNullOrWhiteSpace(customerDto.Name))
                {
                    throw new ValidationException("El nombre del cliente es requerido");
                }

                // Verificar que el cliente exista
                var existingCustomer = await _customerRepository.GetByIdAsync(customerDto.ID);
                if (existingCustomer == null)
                {
                    throw new ValidationException($"No se encontró el cliente con ID: {customerDto.ID}");
                }

                // Actualizar los datos
                existingCustomer.Name = customerDto.Name;
                existingCustomer.Email = customerDto.Email;
                existingCustomer.PhoneNumber = customerDto.PhoneNumber;
                existingCustomer.IdentificationType = customerDto.IdentificationType;
                existingCustomer.IdentificationNumber = customerDto.IdentificationNumber;
                existingCustomer.Address = customerDto.Address;
                existingCustomer.IsActive = customerDto.IsActive;
                existingCustomer.UpdatedAt = DateTime.UtcNow;

                await _customerRepository.UpdateAsync(existingCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el cliente con ID: {CustomerId}", customerDto.ID);
                throw;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                _logger.LogInformation("Intentando eliminar cliente: {CustomerId}", id);
                return await _customerRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el cliente {CustomerId}", id);
                throw;
            }
        }
    }
}
