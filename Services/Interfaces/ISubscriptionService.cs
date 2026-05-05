using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface ISubscriptionService
{
    // Queries
    Task<SubscriptionDto?> GetByIdAsync(Guid id);
    Task<SubscriptionDto?> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<SubscriptionDto>> GetAllAsync();

    // Business logic (timezone-aware)
    Task<bool> IsSubscriptionActiveAsync(Guid companyId);
    Task<bool> IsTrialExpiredAsync(Guid companyId);
    Task<DateTime> CalculateNextBillingDateAsync(Guid subscriptionId);
}
