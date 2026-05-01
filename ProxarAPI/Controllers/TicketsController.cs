using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;
using Models.Enums;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : BaseApiController
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll()
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _ticketService.GetAllByCompanyAsync(companyId));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> GetById(Guid id)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _ticketService.GetByIdAsync(id, companyId));
    }

    [HttpGet("{id}/details")]
    [ProducesResponseType(typeof(TicketDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailsDto>> GetDetails(Guid id)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _ticketService.GetDetailsAsync(id, companyId));
    }

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetByStatus(TicketState status)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _ticketService.GetByStatusAsync(status, companyId));
    }

    [HttpGet("client/{clientId}")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetByClient(Guid clientId)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _ticketService.GetByClientAsync(clientId, companyId));
    }

    [HttpGet("assigned/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetByAssignedUser(Guid userId)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _ticketService.GetByAssignedUserAsync(userId, companyId));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketRequest request)
    {
        var (userId, companyId) = GetCurrentUserAndCompany();
        var ticket = await _ticketService.CreateTicketAsync(request, userId, companyId);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> Update(Guid id, [FromBody] UpdateTicketRequest request)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _ticketService.UpdateTicketAsync(id, request, companyId));
    }

    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDto>> UpdateStatus(Guid id, [FromBody] UpdateTicketStatusRequest request)
    {
        var (userId, companyId) = GetCurrentUserAndCompany();
        return Ok(await _ticketService.UpdateTicketStatusAsync(id, request, userId, companyId));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (userId, companyId) = GetCurrentUserAndCompany();
        await _ticketService.SoftDeleteTicketAsync(id, companyId, userId);
        return NoContent();
    }
}
