using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
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
    /// Get all clients
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
    {
        var companyId = GetCurrentCompanyId();
        var clients = await _clientService.GetActiveByCompanyAsync(companyId);
        return Ok(clients);
    }

    /// <summary>
    /// Get client by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetById(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var client = await _clientService.GetByIdAsync(id, companyId);
            return Ok(client);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // TODO: Implementar búsqueda por nombre en IClientService
    // /// <summary>
    // /// Search clients by name
    // /// </summary>
    // [HttpGet("search")]
    // [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    // public async Task<ActionResult<IEnumerable<ClientDto>>> Search([FromQuery] string name)
    // {
    //     if (string.IsNullOrWhiteSpace(name))
    //         return BadRequest(new { message = "Search term is required" });
    //
    //     var companyId = GetCurrentCompanyId();
    //     var clients = await _clientService.SearchByNameAsync(name, companyId);
    //     return Ok(clients);
    // }

    /// <summary>
    /// Create a new client
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientRequest request)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var client = await _clientService.CreateClientAsync(request, companyId);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> Update(Guid id, [FromBody] UpdateClientRequest request)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var client = await _clientService.UpdateClientAsync(id, request, companyId);
            return Ok(client);
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
    /// Delete a client
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var deletedBy = GetCurrentUserId();
            await _clientService.SoftDeleteClientAsync(id, companyId, deletedBy);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}