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

    /// <summary>
    /// Get all movements
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BoxMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BoxMovementDto>>> GetAll()
    {
        var movements = await _movementService.GetAllMovementsAsync();
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
        var movement = await _movementService.GetMovementByIdAsync(id);

        if (movement == null)
            return NotFound(new { message = $"Movement with ID {id} not found" });

        return Ok(movement);
    }

    /// <summary>
    /// Get movements by account
    /// </summary>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(typeof(IEnumerable<BoxMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BoxMovementDto>>> GetByAccount(Guid accountId)
    {
        var movements = await _movementService.GetMovementsByAccountAsync(accountId);
        return Ok(movements);
    }

    /// <summary>
    /// Get movements by ticket
    /// </summary>
    [HttpGet("ticket/{ticketId}")]
    [ProducesResponseType(typeof(IEnumerable<BoxMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BoxMovementDto>>> GetByTicket(Guid ticketId)
    {
        var movements = await _movementService.GetMovementsByTicketAsync(ticketId);
        return Ok(movements);
    }

    /// <summary>
    /// Get movements by date range
    /// </summary>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(IEnumerable<BoxMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BoxMovementDto>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var movements = await _movementService.GetMovementsByDateRangeAsync(startDate, endDate);
        return Ok(movements);
    }

    /// <summary>
    /// Get account balances
    /// </summary>
    [HttpGet("balances")]
    [ProducesResponseType(typeof(Dictionary<Guid, decimal>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<Guid, decimal>>> GetBalances()
    {
        var balances = await _movementService.GetAccountBalancesAsync();
        return Ok(balances);
    }

    /// <summary>
    /// Register a new movement (income or expense)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BoxMovementDto>> Register([FromBody] RegisterMovementRequest request)
    {
        try
        {
            var userId = GetCurrentUserId(); // ← CAMBIO
            var movement = await _movementService.RegisterMovementAsync(request, userId);
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