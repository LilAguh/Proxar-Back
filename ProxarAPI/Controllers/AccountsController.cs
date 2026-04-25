using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}