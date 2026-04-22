using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories.Implementations;

public class AccountRepository : GenericRepository<Account>, IAccountRepository
{
    public AccountRepository(ProxarDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Account>> GetActiveAccountsAsync()
    {
        return await _dbSet
            .Where(a => a.Active)
            .ToListAsync();
    }

    public async Task<Account?> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower());
    }
}