using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Exceptions;
using Services.DTOs.Responses;
using Services.Interfaces;
using Services.Utilities;

namespace Services.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IMapper _mapper;

    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        ICompanyRepository companyRepository,
        IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _companyRepository = companyRepository;
        _mapper = mapper;
    }

    public async Task<SubscriptionDto?> GetByIdAsync(Guid id)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id);
        return subscription != null ? _mapper.Map<SubscriptionDto>(subscription) : null;
    }

    public async Task<SubscriptionDto?> GetByCompanyIdAsync(Guid companyId)
    {
        var subscription = await _subscriptionRepository.GetByCompanyIdAsync(companyId);
        return subscription != null ? _mapper.Map<SubscriptionDto>(subscription) : null;
    }

    public async Task<IEnumerable<SubscriptionDto>> GetAllAsync()
    {
        var subscriptions = await _subscriptionRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
    }

    /// <summary>
    /// Verifica si suscripción está activa en timezone de la empresa.
    /// Compara CurrentPeriodEnd contra fecha de negocio local.
    /// </summary>
    public async Task<bool> IsSubscriptionActiveAsync(Guid companyId)
    {
        var subscription = await _subscriptionRepository.GetByCompanyIdAsync(companyId);
        if (subscription == null) return false;

        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        // Verificar período activo en timezone de la empresa
        return BusinessDateTime.IsPeriodActive(subscription.CurrentPeriodEnd, company.TimeZoneId);
    }

    /// <summary>
    /// Verifica si trial expiró en timezone de la empresa.
    /// Compara TrialEndsAt contra fecha de negocio local.
    /// </summary>
    public async Task<bool> IsTrialExpiredAsync(Guid companyId)
    {
        var subscription = await _subscriptionRepository.GetByCompanyIdAsync(companyId);
        if (subscription == null || !subscription.IsOnTrial) return false;

        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        // Verificar expiración en timezone de la empresa
        return BusinessDateTime.IsTrialExpired(subscription.TrialEndsAt, company.TimeZoneId);
    }

    /// <summary>
    /// Calcula próxima fecha de billing sumando 1 mes en timezone de la empresa.
    /// Evita errores de desfase de 1 día al sumar directamente en UTC.
    /// </summary>
    public async Task<DateTime> CalculateNextBillingDateAsync(Guid subscriptionId)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId)
            ?? throw new NotFoundException(AppMessages.Subscription.NotFound);

        var company = await _companyRepository.GetByIdAsync(subscription.CompanyId)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        // Calcular próximo billing en timezone local, retornar en UTC
        return BusinessDateTime.CalculateNextBillingDate(
            subscription.CurrentPeriodEnd,
            1, // 1 mes para suscripciones mensuales
            company.TimeZoneId
        );
    }
}
