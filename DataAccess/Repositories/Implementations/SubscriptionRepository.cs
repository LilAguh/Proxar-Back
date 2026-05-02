using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories.Implementations;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ProxarDbContext _context;

    public SubscriptionRepository(ProxarDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(Guid id)
    {
        return await _context.Subscriptions
            .Include(s => s.Company)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Subscription?> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Subscriptions
            .Include(s => s.Company)
            .Include(s => s.Payments.OrderByDescending(p => p.AttemptedAt))
            .FirstOrDefaultAsync(s => s.CompanyId == companyId);
    }

    public async Task<IEnumerable<Subscription>> GetAllAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.Company)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Subscription> AddAsync(Subscription subscription)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    public async Task<Subscription> UpdateAsync(Subscription subscription)
    {
        subscription.UpdatedAt = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }
}
