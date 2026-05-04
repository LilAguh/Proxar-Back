using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetsController : BaseApiController
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BudgetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _budgetService.GetAllAsync(companyId, page, pageSize));
    }

    [HttpGet("ticket/{ticketId}")]
    [ProducesResponseType(typeof(IEnumerable<BudgetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetByTicket(Guid ticketId)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _budgetService.GetByTicketIdAsync(ticketId, companyId));
    }

    [HttpPost("direct")]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BudgetDto>> CreateDirect([FromBody] CreateDirectBudgetRequest request)
    {
        var (userId, companyId) = GetCurrentUserAndCompany();
        var budget = await _budgetService.CreateDirectBudgetAsync(request, userId, companyId);
        return CreatedAtAction(nameof(GetById), new { id = budget.Id }, budget);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BudgetDto>> Create([FromBody] CreateBudgetRequest request)
    {
        var (userId, companyId) = GetCurrentUserAndCompany();
        var budget = await _budgetService.CreateBudgetAsync(request, userId, companyId);
        return CreatedAtAction(nameof(GetById), new { id = budget.Id }, budget);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetDto>> GetById(Guid id)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _budgetService.GetByIdAsync(id, companyId));
    }

    [HttpGet("{id}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPdf(Guid id)
    {
        var companyId = GetCurrentCompanyId();
        var pdfBytes = await _budgetService.GetBudgetPdfAsync(id, companyId);
        return File(pdfBytes, "application/pdf", $"presupuesto_{id}.pdf");
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetDto>> UpdateStatus(Guid id, [FromBody] UpdateBudgetStatusRequest request)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _budgetService.UpdateStatusAsync(id, request.Status, companyId));
    }
}
