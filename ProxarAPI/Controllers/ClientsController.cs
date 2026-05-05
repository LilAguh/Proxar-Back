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

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _clientService.GetAllByCompanyAsync(companyId));
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetActive()
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _clientService.GetActiveByCompanyAsync(companyId));
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> Search([FromQuery] string? name = null)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _clientService.SearchByNameAsync(companyId, name ?? string.Empty));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetById(Guid id)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _clientService.GetByIdAsync(id, companyId));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientRequest request)
    {
        var companyId = GetCurrentCompanyId();
        var client = await _clientService.CreateClientAsync(request, companyId);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> Update(Guid id, [FromBody] UpdateClientRequest request)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _clientService.UpdateClientAsync(id, request, companyId));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (userId, companyId) = GetCurrentUserAndCompany();
        await _clientService.SoftDeleteClientAsync(id, companyId, userId);
        return NoContent();
    }
}
