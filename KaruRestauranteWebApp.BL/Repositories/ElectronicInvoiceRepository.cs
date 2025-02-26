using KaruRestauranteWebApp.Database.Data;
using KaruRestauranteWebApp.Models.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.BL.Repositories
{
    public interface IElectronicInvoiceRepository
    {
        Task<ElectronicInvoiceModel?> GetByOrderIdAsync(int orderId);
        Task<ElectronicInvoiceModel?> GetByIdAsync(int id);
        Task<ElectronicInvoiceModel?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<ElectronicInvoiceModel> CreateAsync(ElectronicInvoiceModel invoice);
        Task UpdateAsync(ElectronicInvoiceModel invoice);
        Task<bool> UpdateStatusAsync(int id, string status, string? confirmationNumber = null);
        Task<string> GenerateInvoiceNumberAsync();
    }

    public class ElectronicInvoiceRepository : IElectronicInvoiceRepository
    {
        private readonly AppDbContext _context;

        public ElectronicInvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ElectronicInvoiceModel?> GetByOrderIdAsync(int orderId)
        {
            return await _context.ElectronicInvoices
                .Include(ei => ei.Customer)
                .FirstOrDefaultAsync(ei => ei.OrderID == orderId);
        }

        public async Task<ElectronicInvoiceModel?> GetByIdAsync(int id)
        {
            return await _context.ElectronicInvoices
                .Include(ei => ei.Customer)
                .FirstOrDefaultAsync(ei => ei.ID == id);
        }

        public async Task<ElectronicInvoiceModel?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            return await _context.ElectronicInvoices
                .Include(ei => ei.Customer)
                .FirstOrDefaultAsync(ei => ei.InvoiceNumber == invoiceNumber);
        }

        public async Task<ElectronicInvoiceModel> CreateAsync(ElectronicInvoiceModel invoice)
        {
            // Generar número de factura si no está definido
            if (string.IsNullOrEmpty(invoice.InvoiceNumber))
            {
                invoice.InvoiceNumber = await GenerateInvoiceNumberAsync();
            }

            invoice.CreationDate = DateTime.UtcNow;
            await _context.ElectronicInvoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task UpdateAsync(ElectronicInvoiceModel invoice)
        {
            var local = _context.ElectronicInvoices.Local.FirstOrDefault(ei => ei.ID == invoice.ID);
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(invoice).State = EntityState.Modified;
            _context.Entry(invoice).Property(x => x.CreationDate).IsModified = false;
            _context.Entry(invoice).Property(x => x.InvoiceNumber).IsModified = false;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status, string? confirmationNumber = null)
        {
            var invoice = await _context.ElectronicInvoices.FindAsync(id);
            if (invoice == null)
                return false;

            invoice.InvoiceStatus = status;

            if (status == "Accepted" && !string.IsNullOrEmpty(confirmationNumber))
            {
                invoice.HaciendaConfirmationNumber = confirmationNumber;
            }

            if (status == "Rejected" || status == "Accepted")
            {
                invoice.ResponseDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            // Formato: FE-YYYYMMDD-XXX donde XXX es un contador secuencial para el día
            var today = DateTime.Now;
            var dateStr = today.ToString("yyyyMMdd");
            var prefix = $"FE-{dateStr}-";

            // Buscar la última factura del día
            var lastInvoice = await _context.ElectronicInvoices
                .Where(ei => ei.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(ei => ei.InvoiceNumber)
                .FirstOrDefaultAsync();

            int sequenceNumber = 1;
            if (lastInvoice != null)
            {
                var lastSequence = lastInvoice.InvoiceNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastNumber))
                {
                    sequenceNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{sequenceNumber:D3}";
        }
    }
}
