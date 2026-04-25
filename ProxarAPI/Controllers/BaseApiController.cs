using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ProxarAPI.Controllers;

[ApiController]
public class BaseApiController : ControllerBase
{
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("Usuario no autenticado");
    }

    protected Guid GetCurrentCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (Guid.TryParse(companyIdClaim, out var companyId))
        {
            return companyId;
        }
        throw new UnauthorizedAccessException("Empresa no identificada");
    }

    protected (Guid userId, Guid companyId) GetCurrentUserAndCompany()
    {
        return (GetCurrentUserId(), GetCurrentCompanyId());
    }
}