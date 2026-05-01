using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Reporte de tickets con filtros avanzados y resúmenes
    /// </summary>
    [HttpGet("tickets")]
    [ProducesResponseType(typeof(TicketsReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketsReportDto>> GetTicketsReport([FromQuery] TicketsReportRequest request)
    {
        var companyId = GetCurrentCompanyId();
        var report = await _reportService.GetTicketsReportAsync(request, companyId);
        return Ok(report);
    }

    /// <summary>
    /// Reporte de movimientos de caja con filtros y totales
    /// </summary>
    [HttpGet("movements")]
    [ProducesResponseType(typeof(MovementsReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MovementsReportDto>> GetMovementsReport([FromQuery] MovementsReportRequest request)
    {
        var companyId = GetCurrentCompanyId();
        var report = await _reportService.GetMovementsReportAsync(request, companyId);
        return Ok(report);
    }

    /// <summary>
    /// Métricas generales y KPIs de la empresa
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(MetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MetricsDto>> GetMetrics()
    {
        var companyId = GetCurrentCompanyId();
        var metrics = await _reportService.GetMetricsAsync(companyId);
        return Ok(metrics);
    }
}
