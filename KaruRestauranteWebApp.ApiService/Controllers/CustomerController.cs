using KaruRestauranteWebApp.BL.Services;
using KaruRestauranteWebApp.Models.Models;
using KaruRestauranteWebApp.Models.Models.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.ApiService.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            ICustomerService customerService,
            ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetAll([FromQuery] bool includeInactive = false)
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync(includeInactive);
                return Ok(new BaseResponseModel { Success = true, Data = customers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener clientes"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetById(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Cliente no encontrado"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = customer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el cliente {CustomerId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener el cliente"
                });
            }
        }

        [HttpGet("identification")]
        public async Task<ActionResult<BaseResponseModel>> GetByIdentification(
            [FromQuery] string type, [FromQuery] string number)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdentificationAsync(type, number);
                if (customer == null)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Cliente no encontrado"
                    });
                }
                return Ok(new BaseResponseModel { Success = true, Data = customer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente por identificación");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al buscar cliente"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> Create([FromBody] CustomerDTO customerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Datos de cliente inválidos"
                    });
                }

                var customer = await _customerService.CreateCustomerAsync(customerDto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = customer,
                    ErrorMessage = "Cliente creado exitosamente"
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al crear cliente"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseModel>> Update(int id, [FromBody] CustomerDTO customerDto)
        {
            try
            {
                if (id != customerDto.ID)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "ID no coincide"
                    });
                }

                await _customerService.UpdateCustomerAsync(customerDto);
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Cliente actualizado exitosamente"
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente {CustomerId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al actualizar cliente"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<BaseResponseModel>> Delete(int id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);
                if (!result)
                {
                    return NotFound(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Cliente no encontrado"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    ErrorMessage = "Cliente eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente {CustomerId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al eliminar cliente"
                });
            }
        }
    }
}
