using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace DataAccess.Repositories.Implementations;

public class BudgetRepository : IBudgetRepository
{
    private readonly ProxarDbContext _context;

    public BudgetRepository(ProxarDbContext context)
    {
        _context = context;
    }

    public async Task<Budget?> GetByIdAsync(Guid id, Guid companyId)
    {
        return await _context.Budgets
            .Include(b => b.Items)
            .Include(b => b.Ticket)
            .Include(b => b.Client)
            .Include(b => b.CreatedBy)
            .FirstOrDefaultAsync(b => b.Id == id && b.CompanyId == companyId);
    }

    public async Task<IEnumerable<Budget>> GetAllAsync(Guid companyId, int page = 1, int pageSize = 50)
    {
        return await _context.Budgets
            .Include(b => b.Items)
            .Include(b => b.Ticket)
            .Include(b => b.Client)
            .Include(b => b.CreatedBy)
            .Where(b => b.CompanyId == companyId)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Budget>> GetByTicketIdAsync(Guid ticketId, Guid companyId, int page = 1, int pageSize = 50)
    {
        return await _context.Budgets
            .Include(b => b.Items)
            .Include(b => b.CreatedBy)
            .Where(b => b.TicketId == ticketId && b.CompanyId == companyId)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Budget> AddAsync(Budget budget)
    {
        await _context.Budgets.AddAsync(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task UpdateAsync(Budget budget)
    {
        budget.ModifiedAt = DateTime.UtcNow;
        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(Guid id, BudgetStatus status, Guid companyId)
    {
        var budget = await GetByIdAsync(id, companyId);
        if (budget != null)
        {
            budget.Status = status;
            budget.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetNextNumberAsync(Guid companyId)
    {
        var maxNumber = await _context.Budgets
            .Where(b => b.CompanyId == companyId)
            .MaxAsync(b => (int?)b.Number) ?? 0;

        return maxNumber + 1;
    }

    public async Task<bool> ExistsAsync(Guid id, Guid companyId)
    {
        return await _context.Budgets
            .AnyAsync(b => b.Id == id && b.CompanyId == companyId);
    }
}
