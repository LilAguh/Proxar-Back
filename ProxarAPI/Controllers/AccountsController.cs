using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : BaseApiController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> GetById(Guid id)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _accountService.GetByIdAsync(id, companyId));
    }

    /// <summary>
    /// Get all accounts of the company
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAll()
    {
        var companyId = GetCurrentCompanyId();
        var accounts = await _accountService.GetAllByCompanyAsync(companyId);
        return Ok(accounts);
    }

    /// <summary>
    /// Get active accounts of the company
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetActive()
    {
        var companyId = GetCurrentCompanyId();
        var accounts = await _accountService.GetActiveByCompanyAsync(companyId);
        return Ok(accounts);
    }

    /// <summary>
    /// Get balances of all accounts
    /// </summary>
    [HttpGet("balances")]
    [ProducesResponseType(typeof(Dictionary<Guid, decimal>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<Guid, decimal>>> GetBalances()
    {
        var companyId = GetCurrentCompanyId();
        var balances = await _accountService.GetBalancesByCompanyAsync(companyId);
        return Ok(balances);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountRequest request)
    {
        var companyId = GetCurrentCompanyId();
        var account = await _accountService.CreateAccountAsync(request, companyId);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountDto>> Update(Guid id, [FromBody] UpdateAccountRequest request)
    {
        var companyId = GetCurrentCompanyId();
        return Ok(await _accountService.UpdateAccountAsync(id, request, companyId));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (userId, companyId) = GetCurrentUserAndCompany();
        await _accountService.DeleteAccountAsync(id, companyId, userId);
        return NoContent();
    }

    /// <summary>
    /// Recalculate account balance from movements
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <param name="autoCorrect">If true, corrects the balance automatically if discrepancy is found</param>
    [HttpPost("{id}/recalculate")]
    [ProducesResponseType(typeof(RecalculateBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecalculateBalanceDto>> Recalculate(Guid id, [FromQuery] bool autoCorrect = false)
    {
        var companyId = GetCurrentCompanyId();
        var result = await _accountService.RecalculateBalanceAsync(id, companyId, autoCorrect);
        return Ok(result);
    }
}
