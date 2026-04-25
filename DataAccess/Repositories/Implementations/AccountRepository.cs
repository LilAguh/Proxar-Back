using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories.Implementations;

public class AccountRepository : IAccountRepository
{
    private readonly ProxarDbContext _context;

    public AccountRepository(ProxarDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id, Guid companyId)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.CompanyId == companyId);
    }

    public async Task<IEnumerable<Account>> GetAllByCompanyAsync(Guid companyId)
    {
        return await _context.Accounts
            .Where(a => a.CompanyId == companyId)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetActiveByCompanyAsync(Guid companyId)
    {
        return await _context.Accounts
            .Where(a => a.CompanyId == companyId && a.Active)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Account> AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAsync(Account account)
    {
        account.ModifiedAt = DateTime.UtcNow;
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        var account = await GetByIdAsync(id, companyId);
        if (account == null)
            throw new KeyNotFoundException("Cuenta no encontrada");

        account.Active = false;
        account.DeletedAt = DateTime.UtcNow;
        account.DeletedBy = deletedBy;

        await UpdateAsync(account);
    }
}