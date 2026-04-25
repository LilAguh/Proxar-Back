using Microsoft.AspNetCore.Mvc;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;



namespace ProxarAPI.Controllers;



[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    private Guid GetCurrentUserId()
    {
        // Temporal: leer del header
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
        {
            if (Guid.TryParse(userIdHeader, out var userId))
            {
                return userId;
            }
        }

        // Fallback: Agustín admin
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    private Guid GetCurrentCompanyId()
    {
        // Temporal: leer del header
        if (Request.Headers.TryGetValue("X-Company-Id", out var companyIdHeader))
        {
            if (Guid.TryParse(companyIdHeader, out var companyId))
            {
                return companyId;
            }
        }

        // Fallback: Aberturas Sagitario
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }

    /// <summary>
    /// Get all tickets
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
    /// Get ticket with full details (history, movements)
    /// </summary>
    [HttpGet("{id}/details")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailDto>> GetWithDetails(Guid id)
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
    /// Get tickets assigned to a user
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
    /// Create a new ticket
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var companyId = GetCurrentCompanyId();
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
            var userId = GetCurrentUserId();
            var companyId = GetCurrentCompanyId();
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
}

