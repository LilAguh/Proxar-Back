using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionDto?> GetByIdAsync(Guid id);
    Task<SubscriptionDto?> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<SubscriptionDto>> GetAllAsync();
}
