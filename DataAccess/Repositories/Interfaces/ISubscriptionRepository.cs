using Models;

namespace DataAccess.Repositories.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id);
    Task<Subscription?> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<Subscription>> GetAllAsync();
    Task<Subscription> AddAsync(Subscription subscription);
    Task<Subscription> UpdateAsync(Subscription subscription);
}
