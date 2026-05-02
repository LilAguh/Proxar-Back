using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace Services.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;

    public SubscriptionService(ISubscriptionRepository subscriptionRepository, IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
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
}
