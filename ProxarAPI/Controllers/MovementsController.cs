using Microsoft.AspNetCore.Mvc;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovementsController : ControllerBase
{
    private readonly IBoxMovementService _movementService;

    public MovementsController(IBoxMovementService movementService)
    {
        _movementService = movementService;
    }

    private Guid GetCurrentUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
        {
            if (Guid.TryParse(userIdHeader, out var userId))
            {
                return userId;
            }
        }
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
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
    /// Get all movements
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BoxMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BoxMovementDto>>> GetAll()
    {
        var companyId = GetCurrentCompanyId();
        var movements = await _movementService.GetAllByCompanyAsync(companyId);
        return Ok(movements);
    }

    /// <summary>
    /// Get movement by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BoxMovementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BoxMovementDto>> GetById(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var movement = await _movementService.GetByIdAsync(id, companyId);
            return Ok(movement);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get movements by account
    /// </summary>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(typeof(IEnumerable<BoxMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BoxMovementDto>>> GetByAccount(Guid accountId)
    {
        var companyId = GetCurrentCompanyId();
        var movements = await _movementService.GetByAccountAsync(accountId, companyId);
        return Ok(movements);
    }

    /// <summary>
    /// Get movements by ticket
    /// </summary>
    [HttpGet("ticket/{ticketId}")]
    [ProducesResponseType(typeof(IEnumerable<BoxMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BoxMovementDto>>> GetByTicket(Guid ticketId)
    {
        var companyId = GetCurrentCompanyId();
        var movements = await _movementService.GetByTicketAsync(ticketId, companyId);
        return Ok(movements);
    }

    // TODO: Implementar GetByDateRangeAsync en IBoxMovementService
    // /// <summary>
    // /// Get movements by date range
    // /// </summary>
    // [HttpGet("date-range")]
    // [ProducesResponseType(typeof(IEnumerable<BoxMovementDto>), StatusCodes.Status200OK)]
    // public async Task<ActionResult<IEnumerable<BoxMovementDto>>> GetByDateRange(
    //     [FromQuery] DateTime startDate,
    //     [FromQuery] DateTime endDate)
    // {
    //     var companyId = GetCurrentCompanyId();
    //     var movements = await _movementService.GetByDateRangeAsync(startDate, endDate, companyId);
    //     return Ok(movements);
    // }

    // NOTE: Account balances moved to AccountsController - use /api/accounts/balances

    /// <summary>
    /// Register a new movement (income or expense)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BoxMovementDto>> Register([FromBody] RegisterMovementRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var companyId = GetCurrentCompanyId();
            var movement = await _movementService.RegisterMovementAsync(request, userId, companyId);
            return CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}