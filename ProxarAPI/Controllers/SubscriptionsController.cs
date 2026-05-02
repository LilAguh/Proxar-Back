using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : BaseApiController
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Obtiene todas las suscripciones (solo Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetAll()
    {
        return Ok(await _subscriptionService.GetAllAsync());
    }

    /// <summary>
    /// Obtiene una suscripción por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionDto>> GetById(Guid id)
    {
        var subscription = await _subscriptionService.GetByIdAsync(id);
        if (subscription == null)
            return NotFound();

        return Ok(subscription);
    }

    /// <summary>
    /// Obtiene la suscripción de la empresa actual
    /// </summary>
    [HttpGet("my-subscription")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionDto>> GetMySubscription()
    {
        var subscription = await _subscriptionService.GetByCompanyIdAsync(CompanyId);
        if (subscription == null)
            return NotFound(new { message = "No se encontró una suscripción activa para esta empresa" });

        return Ok(subscription);
    }

    /// <summary>
    /// Obtiene la suscripción de una empresa específica (solo Admin)
    /// </summary>
    [HttpGet("company/{companyId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionDto>> GetByCompanyId(Guid companyId)
    {
        var subscription = await _subscriptionService.GetByCompanyIdAsync(companyId);
        if (subscription == null)
            return NotFound();

        return Ok(subscription);
    }
}
