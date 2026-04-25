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

    /// <summary>
    /// Get all tickets
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll()
    {
        var tickets = await _ticketService.GetAllTicketsAsync();
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
        var ticket = await _ticketService.GetTicketByIdAsync(id);

        if (ticket == null)
            return NotFound(new { message = $"Ticket with ID {id} not found" });

        return Ok(ticket);
    }

    /// <summary>
    /// Get ticket with full details (history, movements)
    /// </summary>
    [HttpGet("{id}/details")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailDto>> GetWithDetails(Guid id)
    {
        var ticket = await _ticketService.GetTicketWithDetailsAsync(id);

        if (ticket == null)
            return NotFound(new { message = $"Ticket with ID {id} not found" });

        return Ok(ticket);
    }

    /// <summary>
    /// Get tickets by status
    /// </summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetByStatus(TicketState status)
    {
        var tickets = await _ticketService.GetTicketsByStatusAsync(status);
        return Ok(tickets);
    }

    /// <summary>
    /// Get tickets assigned to a user
    /// </summary>
    [HttpGet("assigned/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetByAssignedUser(Guid userId)
    {
        var tickets = await _ticketService.GetTicketsByAssignedUserAsync(userId);
        return Ok(tickets);
    }

    /// <summary>
    /// Get tickets by client
    /// </summary>
    [HttpGet("client/{clientId}")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetByClient(Guid clientId)
    {
        var tickets = await _ticketService.GetTicketsByClientAsync(clientId);
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
            var userId = GetCurrentUserId(); // ← CAMBIO
            var ticket = await _ticketService.CreateTicketAsync(request, userId);
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
            var userId = GetCurrentUserId(); // ← CAMBIO
            var ticket = await _ticketService.UpdateTicketStatusAsync(id, request, userId);
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
    /// Assign ticket to a user
    /// </summary>
    [HttpPut("{id}/assign")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDto>> Assign(Guid id, [FromBody] AssignTicketRequest request)
    {
        try
        {
            var ticket = await _ticketService.AssignTicketAsync(id, request);
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

