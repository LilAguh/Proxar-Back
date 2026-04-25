using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MovementsController : BaseApiController
{
    private readonly IBoxMovementService _movementService;

    public MovementsController(IBoxMovementService movementService)
    {
        _movementService = movementService;
    }

    /// <summary>
    /// Get all movements of the company
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

    /// <summary>
    /// Register new movement
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BoxMovementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BoxMovementDto>> Register([FromBody] RegisterMovementRequest request)
    {
        try
        {
            var (userId, companyId) = GetCurrentUserAndCompany();
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

    /// <summary>
    /// Delete movement (soft delete + revert balance)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var (userId, companyId) = GetCurrentUserAndCompany();
            await _movementService.SoftDeleteMovementAsync(id, companyId, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}