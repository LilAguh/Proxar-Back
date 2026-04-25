using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var userId = GetCurrentUserId();
        var companyId = GetCurrentCompanyId();
        var user = await _authService.GetUserByIdAsync(userId, companyId);
        return Ok(user);
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var companyId = GetCurrentCompanyId();
            await _authService.ChangePasswordAsync(userId, request, companyId);
            return Ok(new { message = "Contraseña actualizada correctamente" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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

        // Alternativa: leer del JWT claim
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var jwtUserId))
        {
            return jwtUserId;
        }

        // Fallback
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

        // Alternativa: leer del JWT claim
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (Guid.TryParse(companyIdClaim, out var jwtCompanyId))
        {
            return jwtCompanyId;
        }

        // Fallback
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}