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

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BoxMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BoxMovementDto>>> GetAll()
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _movementService.GetAllByCompanyAsync(companyId));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BoxMovementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BoxMovementDto>> GetById(Guid id)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _movementService.GetByIdAsync(id, companyId));
    }

    [HttpGet("account/{accountId}")]
    [ProducesResponseType(typeof(IEnumerable<BoxMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BoxMovementDto>>> GetByAccount(Guid accountId)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _movementService.GetByAccountAsync(accountId, companyId));
    }

    [HttpGet("ticket/{ticketId}")]
    [ProducesResponseType(typeof(IEnumerable<BoxMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BoxMovementDto>>> GetByTicket(Guid ticketId)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _movementService.GetByTicketAsync(ticketId, companyId));
    }

    [HttpPost]
    [ProducesResponseType(typeof(BoxMovementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BoxMovementDto>> Register([FromBody] RegisterMovementRequest request)
    {
        var (userId, companyId) = GetCurrentUserAndCompany();
        var movement = await _movementService.RegisterMovementAsync(request, userId, companyId);
        return CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (userId, companyId) = GetCurrentUserAndCompany();
        await _movementService.SoftDeleteMovementAsync(id, companyId, userId);
        return NoContent();
    }
}
