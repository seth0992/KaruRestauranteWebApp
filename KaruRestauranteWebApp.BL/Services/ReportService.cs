using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.Models.Models.Reports;
using Microsoft.Extensions.Logging;

namespace KaruRestauranteWebApp.BL.Services
{
    public interface IReportService
    {
        Task<List<SalesReportDTO>> GetDailySalesAsync(DateTime startDate, DateTime endDate);
        Task<List<SalesReportDTO>> GetMonthlySalesAsync(int year);
        Task<List<SalesReportDTO>> GetYearlySalesAsync(int startYear, int endYear);
        Task<List<ProductSalesReportDTO>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 10);
        Task<List<InventoryStatusReportDTO>> GetInventoryStatusReportAsync();
    }
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IReportRepository reportRepository,
            ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _logger = logger;
        }

        public async Task<List<SalesReportDTO>> GetDailySalesAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _reportRepository.GetDailySalesAsync(startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte de ventas diarias");
                throw;
            }
        }

        public async Task<List<SalesReportDTO>> GetMonthlySalesAsync(int year)
        {
            try
            {
                return await _reportRepository.GetMonthlySalesAsync(year);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte de ventas mensuales");
                throw;
            }
        }

        public async Task<List<SalesReportDTO>> GetYearlySalesAsync(int startYear, int endYear)
        {
            try
            {
                return await _reportRepository.GetYearlySalesAsync(startYear, endYear);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte de ventas anuales");
                throw;
            }
        }

        public async Task<List<ProductSalesReportDTO>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 10)
        {
            try
            {
                return await _reportRepository.GetTopSellingProductsAsync(startDate, endDate, limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte de productos más vendidos");
                throw;
            }
        }

        public async Task<List<InventoryStatusReportDTO>> GetInventoryStatusReportAsync()
        {
            try
            {
                return await _reportRepository.GetInventoryStatusReportAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte de estado de inventario");
                throw;
            }
        }
    }
}
