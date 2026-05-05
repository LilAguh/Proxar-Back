using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AfipController : BaseApiController
{
    private readonly IAfipService _afipService;

    public AfipController(IAfipService afipService)
    {
        _afipService = afipService;
    }

    /// <summary>
    /// Obtiene datos fiscales de un contribuyente desde AFIP por CUIT/CUIL/DNI
    /// </summary>
    /// <param name="documento">CUIT, CUIL o DNI (con o sin guiones)</param>
    /// <returns>Datos fiscales del contribuyente</returns>
    [HttpGet("contribuyente/{documento}")]
    [ProducesResponseType(typeof(AfipContribuyenteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AfipContribuyenteDto>> GetContribuyente(string documento)
    {
        var data = await _afipService.GetContribuyenteDataAsync(documento);
        return Ok(data);
    }
}
