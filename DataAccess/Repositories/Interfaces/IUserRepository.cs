using Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, Guid companyId);
    Task<User?> GetByEmailAsync(string email, Guid companyId);
    Task<IEnumerable<User>> GetAllByCompanyAsync(Guid companyId);
    Task<IEnumerable<User>> GetActiveByCompanyAsync(Guid companyId);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task SoftDeleteAsync(Guid id, Guid companyId, Guid deletedBy);
}