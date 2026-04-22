using Models;

namespace DataAccess.Repositories.Interfaces;

public interface IAccountRepository : IGenericRepository<Account>
{
    Task<IEnumerable<Account>> GetActiveAccountsAsync();
    Task<Account?> GetByNameAsync(string name);
}