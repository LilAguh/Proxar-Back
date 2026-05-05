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

    public async Task UpdateBalanceAtomicAsync(Guid accountId, Guid companyId, decimal delta)
    {
        // CONCURRENCIA: Actualización atómica del saldo usando SQL directo
        // Evita condiciones de carrera (lost updates) cuando múltiples operaciones
        // modifican el mismo saldo simultáneamente.
        // El motor de BD maneja el bloqueo de fila automáticamente.
        var now = DateTime.UtcNow;

        await _context.Accounts
            .Where(a => a.Id == accountId && a.CompanyId == companyId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.CurrentBalance, a => a.CurrentBalance + delta)
                .SetProperty(a => a.ModifiedAt, now));
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