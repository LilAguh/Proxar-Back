using Models;

namespace DataAccess.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<Account>> GetAllByCompanyAsync(Guid companyId);
    Task<IEnumerable<Account>> GetActiveByCompanyAsync(Guid companyId);
    Task<Account> AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task SoftDeleteAsync(Guid id, Guid companyId, Guid deletedBy);
}