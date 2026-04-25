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
}