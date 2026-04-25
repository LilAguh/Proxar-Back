using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
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
    /// Get all accounts
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
    /// Get active accounts only
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
    /// Get account balances
    /// </summary>
    [HttpGet("balances")]
    [ProducesResponseType(typeof(Dictionary<Guid, decimal>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<Guid, decimal>>> GetBalances()
    {
        var companyId = GetCurrentCompanyId();
        var balances = await _accountService.GetBalancesByCompanyAsync(companyId);
        return Ok(balances);
    }

    // TODO: Implementar CRUD completo de Accounts en IAccountService
    // /// <summary>
    // /// Get account by ID
    // /// </summary>
    // [HttpGet("{id}")]
    // [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<AccountDto>> GetById(Guid id)
    // {
    //     try
    //     {
    //         var companyId = GetCurrentCompanyId();
    //         var account = await _accountService.GetByIdAsync(id, companyId);
    //         return Ok(account);
    //     }
    //     catch (KeyNotFoundException ex)
    //     {
    //         return NotFound(new { message = ex.Message });
    //     }
    // }
    //
    // /// <summary>
    // /// Create a new account
    // /// </summary>
    // [HttpPost]
    // [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountRequest request)
    // {
    //     try
    //     {
    //         var companyId = GetCurrentCompanyId();
    //         var account = await _accountService.CreateAsync(request, companyId);
    //         return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(new { message = ex.Message });
    //     }
    // }
    //
    // /// <summary>
    // /// Update an existing account
    // /// </summary>
    // [HttpPut("{id}")]
    // [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<ActionResult<AccountDto>> Update(Guid id, [FromBody] UpdateAccountRequest request)
    // {
    //     try
    //     {
    //         var companyId = GetCurrentCompanyId();
    //         var account = await _accountService.UpdateAsync(id, request, companyId);
    //         return Ok(account);
    //     }
    //     catch (KeyNotFoundException ex)
    //     {
    //         return NotFound(new { message = ex.Message });
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(new { message = ex.Message });
    //     }
    // }
    //
    // /// <summary>
    // /// Delete an account
    // /// </summary>
    // [HttpDelete("{id}")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<IActionResult> Delete(Guid id)
    // {
    //     try
    //     {
    //         var companyId = GetCurrentCompanyId();
    //         await _accountService.SoftDeleteAsync(id, companyId);
    //         return NoContent();
    //     }
    //     catch (KeyNotFoundException ex)
    //     {
    //         return NotFound(new { message = ex.Message });
    //     }
    // }
}