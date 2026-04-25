using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : BaseApiController
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    /// <summary>
    /// Get all clients of the company
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
    {
        var companyId = GetCurrentCompanyId();
        var clients = await _clientService.GetAllByCompanyAsync(companyId);
        return Ok(clients);
    }

    /// <summary>
    /// Get active clients of the company
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetActive()
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

    /// <summary>
    /// Create new client
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
    /// Update client
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
    /// Delete client (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var (userId, companyId) = GetCurrentUserAndCompany();
            await _clientService.SoftDeleteClientAsync(id, companyId, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}