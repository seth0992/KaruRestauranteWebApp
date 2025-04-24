using KaruRestauranteWebApp.BL.Services;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaruRestauranteWebApp.ApiService.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IReportService reportService,
            ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("sales/daily")]
        public async Task<ActionResult<BaseResponseModel>> GetDailySales(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var sales = await _reportService.GetDailySalesAsync(startDate, endDate);
                return Ok(new BaseResponseModel { Success = true, Data = sales });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas diarias");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener reporte de ventas diarias"
                });
            }
        }

        [HttpGet("sales/monthly")]
        public async Task<ActionResult<BaseResponseModel>> GetMonthlySales([FromQuery] int year)
        {
            try
            {
                var sales = await _reportService.GetMonthlySalesAsync(year);
                return Ok(new BaseResponseModel { Success = true, Data = sales });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas mensuales");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener reporte de ventas mensuales"
                });
            }
        }

        [HttpGet("sales/yearly")]
        public async Task<ActionResult<BaseResponseModel>> GetYearlySales(
            [FromQuery] int startYear, [FromQuery] int endYear)
        {
            try
            {
                var sales = await _reportService.GetYearlySalesAsync(startYear, endYear);
                return Ok(new BaseResponseModel { Success = true, Data = sales });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas anuales");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener reporte de ventas anuales"
                });
            }
        }

        //[HttpGet("products/top-selling")]
        //public async Task<ActionResult<BaseResponseModel>> GetTopSellingProducts(
        //    [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int limit = 10)
        //{
        //    try
        //    {
        //        var products = await _reportService.GetTopSellingProductsAsync(startDate, endDate, limit);
        //        return Ok(new BaseResponseModel { Success = true, Data = products });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al obtener productos más vendidos");
        //        return StatusCode(500, new BaseResponseModel
        //        {
        //            Success = false,
        //            ErrorMessage = "Error interno del servidor al obtener reporte de productos más vendidos"
        //        });
        //    }
        //}

        [HttpGet("inventory/status")]
        public async Task<ActionResult<BaseResponseModel>> GetInventoryStatus()
        {
            try
            {
                var inventory = await _reportService.GetInventoryStatusReportAsync();
                return Ok(new BaseResponseModel { Success = true, Data = inventory });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado del inventario");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener reporte de estado del inventario"
                });
            }
        }

        [HttpGet("products/top-selling")]
        public async Task<ActionResult<BaseResponseModel>> GetTopSellingProducts(
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate,
    [FromQuery] int limit = 10,
    [FromQuery] int? categoryId = null)
        {
            try
            {
                var products = await _reportService.GetTopSellingProductsAsync(startDate, endDate, limit, categoryId);
                return Ok(new BaseResponseModel { Success = true, Data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor al obtener reporte de productos más vendidos"
                });
            }
        }
    }
}
