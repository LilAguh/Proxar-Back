using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/cash-registers")]
[Authorize]
public class CashRegistersController : BaseApiController
{
    private readonly ICashRegisterService _service;

    public CashRegistersController(ICashRegisterService service)
    {
        _service = service;
    }

    [HttpGet("preview")]
    [ProducesResponseType(typeof(CashRegisterPreviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CashRegisterPreviewDto>> GetOpenPreview()
    {
        var companyId = GetCurrentCompanyId();
        var preview = await _service.GetOpenPreviewAsync(companyId);
        return Ok(preview);
    }

    [HttpGet("today")]
    [ProducesResponseType(typeof(CashRegisterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<CashRegisterDto>> GetToday()
    {
        var companyId = GetCurrentCompanyId();
        var register = await _service.GetTodayAsync(companyId);
        return register == null ? NoContent() : Ok(register);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CashRegisterDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CashRegisterDto>>> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var companyId = GetCurrentCompanyId();
        var history = await _service.GetHistoryAsync(companyId, page, pageSize);
        return Ok(history);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CashRegisterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CashRegisterDto>> GetById(Guid id)
    {
        var companyId = GetCurrentCompanyId();
        var register = await _service.GetByIdAsync(id, companyId);
        return register == null ? NotFound() : Ok(register);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CashRegisterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CashRegisterDto>> Open([FromBody] OpenCashRegisterRequest request)
    {
        try
        {
            var (userId, companyId) = GetCurrentUserAndCompany();
            var register = await _service.OpenAsync(request, userId, companyId);
            return CreatedAtAction(nameof(GetById), new { id = register.Id }, register);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/close")]
    [ProducesResponseType(typeof(CashRegisterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CashRegisterDto>> Close(Guid id, [FromBody] CloseCashRegisterRequest request)
    {
        try
        {
            var (userId, companyId) = GetCurrentUserAndCompany();
            var register = await _service.CloseAsync(id, request, userId, companyId);
            return Ok(register);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
