using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Models.Orders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface IElectronicInvoiceService
    {
        Task<ElectronicInvoiceModel?> GetInvoiceByOrderIdAsync(int orderId);
        Task<ElectronicInvoiceModel?> GetInvoiceByIdAsync(int id);
        Task<ElectronicInvoiceModel?> GetInvoiceByNumberAsync(string invoiceNumber);
        Task<ElectronicInvoiceModel> GenerateInvoiceAsync(int orderId, int customerId);
        Task<bool> SendInvoiceToHaciendaAsync(int invoiceId);
        Task<bool> UpdateInvoiceStatusAsync(int id, string status, string? confirmationNumber = null);

        Task<ElectronicInvoiceModel> GenerateInvoiceAsync(ElectronicInvoiceDTO invoiceDto);

    }

    public class ElectronicInvoiceService : IElectronicInvoiceService
    {
        private readonly IElectronicInvoiceRepository _invoiceRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly ILogger<ElectronicInvoiceService> _logger;
        private readonly IConfiguration _configuration;

        public ElectronicInvoiceService(
            IElectronicInvoiceRepository invoiceRepository,
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IOrderDetailRepository orderDetailRepository,
            ILogger<ElectronicInvoiceService> logger,
            IConfiguration configuration)
        {
            _invoiceRepository = invoiceRepository;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _orderDetailRepository = orderDetailRepository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ElectronicInvoiceModel?> GetInvoiceByOrderIdAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Buscando factura para la orden: {OrderId}", orderId);
                return await _invoiceRepository.GetByOrderIdAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la factura para la orden: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<ElectronicInvoiceModel?> GetInvoiceByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando factura con ID: {InvoiceId}", id);
                return await _invoiceRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la factura con ID: {InvoiceId}", id);
                throw;
            }
        }

        public async Task<ElectronicInvoiceModel?> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            try
            {
                _logger.LogInformation("Buscando factura con número: {InvoiceNumber}", invoiceNumber);
                return await _invoiceRepository.GetByInvoiceNumberAsync(invoiceNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la factura con número: {InvoiceNumber}", invoiceNumber);
                throw;
            }
        }

        public async Task<ElectronicInvoiceModel> GenerateInvoiceAsync(int orderId, int customerId)
        {
            try
            {
                // Verificar que no exista ya una factura para la orden
                var existingInvoice = await _invoiceRepository.GetByOrderIdAsync(orderId);
                if (existingInvoice != null)
                {
                    throw new ValidationException($"Ya existe una factura para la orden {orderId}");
                }

                // Verificar que la orden exista
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new ValidationException($"No se encontró la orden con ID: {orderId}");
                }

                // Verificar que la orden esté pagada
                if (order.PaymentStatus != "Paid")
                {
                    throw new ValidationException("Solo se pueden generar facturas para órdenes pagadas completamente");
                }

                // Verificar que el cliente exista
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    throw new ValidationException($"No se encontró el cliente con ID: {customerId}");
                }

                // Generar número de factura
                var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync();

                // Generar XML de factura
                string invoiceXml = await GenerateInvoiceXmlAsync(order, customer);

                // Crear modelo de factura
                var invoice = new ElectronicInvoiceModel
                {
                    OrderID = orderId,
                    InvoiceNumber = invoiceNumber,
                    CustomerID = customerId,
                    TotalAmount = order.TotalAmount,
                    TaxAmount = order.TaxAmount,
                    InvoiceXML = invoiceXml,
                    InvoiceStatus = "Generated",
                    CreationDate = DateTime.UtcNow
                };

                // Guardar la factura
                return await _invoiceRepository.CreateAsync(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar factura para la orden: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> SendInvoiceToHaciendaAsync(int invoiceId)
        {
            try
            {
                // Obtener la factura
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                {
                    throw new ValidationException($"No se encontró la factura con ID: {invoiceId}");
                }

                // Verificar que la factura esté en estado "Generated"
                if (invoice.InvoiceStatus != "Generated")
                {
                    throw new ValidationException($"La factura debe estar en estado 'Generated' para enviarla. Estado actual: {invoice.InvoiceStatus}");
                }

                // Aquí iría la lógica de integración con el API de Hacienda
                // Este es un código simulado que enviaría la factura

                // Obtener la URL del API de facturación electrónica desde configuración
                string apiUrl = _configuration["ElectronicInvoicing:ApiUrl"] ??
                                "https://api.facturacionelectronica.example.com";

                string apiKey = _configuration["ElectronicInvoicing:ApiKey"] ??
                               "api_key_simulada";

                _logger.LogInformation("Enviando factura {InvoiceNumber} a Hacienda", invoice.InvoiceNumber);

                // Simulación de envío exitoso
                bool sendSuccess = true;
                string confirmationNumber = Guid.NewGuid().ToString();

                if (sendSuccess)
                {
                    // Actualizar estado de la factura a "Sent" o directamente a "Accepted" en simulación
                    await _invoiceRepository.UpdateStatusAsync(invoiceId, "Accepted", confirmationNumber);
                    return true;
                }
                else
                {
                    await _invoiceRepository.UpdateStatusAsync(invoiceId, "Rejected", null);
                    invoice.ErrorDescription = "Error en la comunicación con Hacienda";
                    await _invoiceRepository.UpdateAsync(invoice);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar factura a Hacienda: {InvoiceId}", invoiceId);

                // Actualizar estado y registrar error
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice != null)
                {
                    invoice.InvoiceStatus = "Rejected";
                    invoice.ErrorDescription = ex.Message;
                    await _invoiceRepository.UpdateAsync(invoice);
                }

                throw;
            }
        }

        public async Task<bool> UpdateInvoiceStatusAsync(int id, string status, string? confirmationNumber = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    throw new ValidationException("El estado de la factura es requerido");
                }

                // Validar que el estado sea válido
                var validStatuses = new[] { "Generated", "Sent", "Accepted", "Rejected" };
                if (!validStatuses.Contains(status))
                {
                    throw new ValidationException($"Estado de factura no válido. Valores posibles: {string.Join(", ", validStatuses)}");
                }

                return await _invoiceRepository.UpdateStatusAsync(id, status, confirmationNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el estado de la factura {InvoiceId}", id);
                throw;
            }
        }

        private async Task<string> GenerateInvoiceXmlAsync(OrderModel order, CustomerModel customer)
        {
            try
            {
                // Obtener detalles de la orden
                var orderDetails = await _orderDetailRepository.GetByOrderIdAsync(order.ID);

                // Crear documento XML
                var doc = new XmlDocument();

                // Crear nodo raíz
                var root = doc.CreateElement("FacturaElectronica");
                doc.AppendChild(root);

                // Información de la factura
                var infoFactura = doc.CreateElement("InformacionFactura");
                root.AppendChild(infoFactura);

                // Agregar atributos de factura
                AddXmlElement(doc, infoFactura, "NumeroFactura", order.OrderNumber);
                AddXmlElement(doc, infoFactura, "FechaEmision", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
                AddXmlElement(doc, infoFactura, "TipoDocumento", "FE");

                // Información del emisor (restaurante)
                var emisor = doc.CreateElement("Emisor");
                root.AppendChild(emisor);

                AddXmlElement(doc, emisor, "Nombre", "Karu Restaurante");
                AddXmlElement(doc, emisor, "Identificacion", "3101123456");
                AddXmlElement(doc, emisor, "TipoIdentificacion", "02");
                AddXmlElement(doc, emisor, "Email", "facturacion@karurestaurante.com");

                // Información del receptor (cliente)
                var receptor = doc.CreateElement("Receptor");
                root.AppendChild(receptor);

                AddXmlElement(doc, receptor, "Nombre", customer.Name);
                AddXmlElement(doc, receptor, "Identificacion", customer.IdentificationNumber);
                AddXmlElement(doc, receptor, "TipoIdentificacion", MapIdentificationType(customer.IdentificationType));
                AddXmlElement(doc, receptor, "Email", customer.Email);

                // Detalles de la factura
                var detalleFactura = doc.CreateElement("DetalleFactura");
                root.AppendChild(detalleFactura);

                foreach (var detail in orderDetails)
                {
                    var lineaDetalle = doc.CreateElement("LineaDetalle");
                    detalleFactura.AppendChild(lineaDetalle);

                    AddXmlElement(doc, lineaDetalle, "Cantidad", detail.Quantity.ToString());
                    AddXmlElement(doc, lineaDetalle, "Descripcion", $"{detail.ItemType} - {detail.Notes}");
                    AddXmlElement(doc, lineaDetalle, "PrecioUnitario", detail.UnitPrice.ToString("F2"));
                    AddXmlElement(doc, lineaDetalle, "Subtotal", detail.SubTotal.ToString("F2"));

                    // Agregar personalizaciones como observaciones
                    if (detail.Customizations.Any())
                    {
                        var observaciones = string.Join(", ",
                            detail.Customizations.Select(c =>
                                $"{c.CustomizationType} {c.Ingredient.Name} x{c.Quantity}"));

                        AddXmlElement(doc, lineaDetalle, "Observaciones", observaciones);
                    }
                }

                // Resumen de la factura
                var resumenFactura = doc.CreateElement("ResumenFactura");
                root.AppendChild(resumenFactura);

                AddXmlElement(doc, resumenFactura, "SubTotal", (order.TotalAmount - order.TaxAmount).ToString("F2"));
                AddXmlElement(doc, resumenFactura, "Impuesto", order.TaxAmount.ToString("F2"));
                AddXmlElement(doc, resumenFactura, "Descuento", order.DiscountAmount.ToString("F2"));
                AddXmlElement(doc, resumenFactura, "Total", order.TotalAmount.ToString("F2"));

                // Convertir a string
                var stringBuilder = new StringBuilder();
                var writer = new StringWriter(stringBuilder);
                doc.Save(writer);

                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar XML de factura para la orden: {OrderId}", order.ID);
                throw;
            }
        }
        public async Task<ElectronicInvoiceModel> GenerateInvoiceAsync(ElectronicInvoiceDTO invoiceDto)
        {
            try
            {
                // Validar datos
                if (invoiceDto.CustomerID <= 0)
                {
                    throw new ValidationException("Se requiere un cliente para generar la factura");
                }

                // Verificar que la orden exista
                var order = await _orderRepository.GetByIdAsync(invoiceDto.OrderID);
                if (order == null)
                {
                    throw new ValidationException($"No se encontró la orden con ID: {invoiceDto.OrderID}");
                }

                // Verificar que la orden esté pagada
                if (order.PaymentStatus != "Paid")
                {
                    throw new ValidationException("No se puede generar factura para una orden que no está completamente pagada");
                }

                // Verificar que no exista ya una factura para esta orden
                var existingInvoice = await _invoiceRepository.GetByOrderIdAsync(invoiceDto.OrderID);
                if (existingInvoice != null)
                {
                    throw new ValidationException("Ya existe una factura para esta orden");
                }

                // Verificar que el cliente exista
                var customer = await _customerRepository.GetByIdAsync(invoiceDto.CustomerID);
                if (customer == null)
                {
                    throw new ValidationException($"No se encontró el cliente con ID: {invoiceDto.CustomerID}");
                }

                // Generar XML para facturación electrónica
                var invoiceXml = await GenerateInvoiceXmlAsync(order, customer);

                // Crear el modelo de factura
                var invoice = new ElectronicInvoiceModel
                {
                    OrderID = invoiceDto.OrderID,
                    InvoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(),
                    CustomerID = invoiceDto.CustomerID,
                    TotalAmount = order.TotalAmount,
                    TaxAmount = order.TaxAmount,
                    InvoiceXML = invoiceXml,
                    InvoiceStatus = "Generated",
                    CreationDate = DateTime.UtcNow
                };

                // Guardar la factura
                return await _invoiceRepository.CreateAsync(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar factura para la orden {OrderId}", invoiceDto.OrderID);
                throw;
            }
        }

        private void AddXmlElement(XmlDocument doc, XmlElement parent, string name, string value)
        {
            var element = doc.CreateElement(name);
            element.InnerText = value;
            parent.AppendChild(element);
        }

        private string MapIdentificationType(string identificationType)
        {
            // Mapear tipos de identificación según requerimientos de Hacienda
            return identificationType switch
            {
                "Cédula Física" => "01",
                "Cédula Jurídica" => "02",
                "DIMEX" => "03",
                "NITE" => "04",
                _ => "01" // Valor por defecto
            };
        }
    }
}
