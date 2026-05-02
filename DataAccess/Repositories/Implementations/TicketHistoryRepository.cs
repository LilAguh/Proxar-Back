using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories.Implementations;

public class TicketHistoryRepository : ITicketHistoryRepository
{
    private readonly ProxarDbContext _context;

    public TicketHistoryRepository(ProxarDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TicketHistory>> GetByTicketIdAsync(Guid ticketId)
    {
        return await _context.TicketHistory
            .Include(th => th.User)
            .Where(th => th.TicketId == ticketId)
            .OrderByDescending(th => th.Timestamp)
            .ToListAsync();
    }

    public async Task<TicketHistory> AddAsync(TicketHistory history)
    {
        await _context.TicketHistory.AddAsync(history);
        await _context.SaveChangesAsync();
        return history;
    }

    public async Task SoftDeleteByTicketAsync(Guid ticketId, Guid companyId, Guid deletedBy)
    {
        var now = DateTime.UtcNow;

        await _context.TicketHistory
            .Where(h => h.TicketId == ticketId && h.CompanyId == companyId && h.Active)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(h => h.Active, false)
                .SetProperty(h => h.DeletedAt, now)
                .SetProperty(h => h.DeletedBy, deletedBy));
    }
}