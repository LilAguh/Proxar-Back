using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private Guid GetCurrentCompanyId()
    {
        if (Request.Headers.TryGetValue("X-Company-Id", out var companyIdHeader))
        {
            if (Guid.TryParse(companyIdHeader, out var companyId))
            {
                return companyId;
            }
        }
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }

    /// <summary>
    /// Get dashboard summary
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var companyId = GetCurrentCompanyId();
        var summary = await _dashboardService.GetSummaryAsync(companyId);
        return Ok(summary);
    }
}