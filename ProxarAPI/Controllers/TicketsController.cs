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

    /// <summary>
    /// Get all tickets of the company
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll()
    {
        var companyId = GetCurrentCompanyId();
        var tickets = await _ticketService.GetAllByCompanyAsync(companyId);
        return Ok(tickets);
    }

    /// <summary>
    /// Get ticket by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> GetById(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var ticket = await _ticketService.GetByIdAsync(id, companyId);
            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get ticket details with history
    /// </summary>
    [HttpGet("{id}/details")]
    [ProducesResponseType(typeof(TicketDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailsDto>> GetDetails(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var ticket = await _ticketService.GetDetailsAsync(id, companyId);
            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get tickets by status
    /// </summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetByStatus(TicketState status)
    {
        var companyId = GetCurrentCompanyId();
        var tickets = await _ticketService.GetByStatusAsync(status, companyId);
        return Ok(tickets);
    }

    /// <summary>
    /// Get tickets by client
    /// </summary>
    [HttpGet("client/{clientId}")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetByClient(Guid clientId)
    {
        var companyId = GetCurrentCompanyId();
        var tickets = await _ticketService.GetByClientAsync(clientId, companyId);
        return Ok(tickets);
    }

    /// <summary>
    /// Get tickets assigned to user
    /// </summary>
    [HttpGet("assigned/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetByAssignedUser(Guid userId)
    {
        var companyId = GetCurrentCompanyId();
        var tickets = await _ticketService.GetByAssignedUserAsync(userId, companyId);
        return Ok(tickets);
    }

    /// <summary>
    /// Create new ticket
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketRequest request)
    {
        try
        {
            var (userId, companyId) = GetCurrentUserAndCompany();
            var ticket = await _ticketService.CreateTicketAsync(request, userId, companyId);
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update ticket
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDto>> Update(Guid id, [FromBody] UpdateTicketRequest request)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var ticket = await _ticketService.UpdateTicketAsync(id, request, companyId);
            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update ticket status
    /// </summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDto>> UpdateStatus(Guid id, [FromBody] UpdateTicketStatusRequest request)
    {
        try
        {
            var (userId, companyId) = GetCurrentUserAndCompany();
            var ticket = await _ticketService.UpdateTicketStatusAsync(id, request, userId, companyId);
            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete ticket (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var (userId, companyId) = GetCurrentUserAndCompany();
            await _ticketService.SoftDeleteTicketAsync(id, companyId, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}