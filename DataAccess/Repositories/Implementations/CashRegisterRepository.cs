using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace DataAccess.Repositories.Implementations;

public class CashRegisterRepository : ICashRegisterRepository
{
    private readonly ProxarDbContext _context;

    public CashRegisterRepository(ProxarDbContext context)
    {
        _context = context;
    }

    public async Task<CashRegister?> GetByIdAsync(Guid id, Guid companyId)
    {
        return await _context.CashRegisters
            .Include(r => r.Entries).ThenInclude(e => e.Account)
            .Include(r => r.OpenedBy)
            .Include(r => r.ClosedBy)
            .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);
    }

    public async Task<CashRegister?> GetOpenAsync(Guid companyId)
    {
        return await _context.CashRegisters
            .Include(r => r.Entries).ThenInclude(e => e.Account)
            .Include(r => r.OpenedBy)
            .Include(r => r.ClosedBy)
            .FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Status == CashRegisterStatus.Open);
    }

    public async Task<CashRegister?> GetTodayAsync(Guid companyId, DateTime businessDate)
    {
        return await _context.CashRegisters
            .Include(r => r.Entries).ThenInclude(e => e.Account)
            .Include(r => r.OpenedBy)
            .Include(r => r.ClosedBy)
            .FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Date == businessDate.Date);
    }

    public async Task<CashRegister?> GetPreviousClosedAsync(Guid companyId, DateTime beforeDate)
    {
        return await _context.CashRegisters
            .Include(r => r.Entries).ThenInclude(e => e.Account)
            .Where(r => r.CompanyId == companyId &&
                        r.Date < beforeDate.Date &&
                        r.Status == CashRegisterStatus.Closed)
            .OrderByDescending(r => r.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CashRegister>> GetHistoryAsync(Guid companyId, int page, int pageSize)
    {
        return await _context.CashRegisters
            .Include(r => r.Entries).ThenInclude(e => e.Account)
            .Include(r => r.OpenedBy)
            .Include(r => r.ClosedBy)
            .Where(r => r.CompanyId == companyId)
            .OrderByDescending(r => r.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<CashRegister> AddAsync(CashRegister register)
    {
        await _context.CashRegisters.AddAsync(register);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(register.Id, register.CompanyId))!;
    }

    public async Task UpdateAsync(CashRegister register)
    {
        _context.CashRegisters.Update(register);
        await _context.SaveChangesAsync();
    }
}
