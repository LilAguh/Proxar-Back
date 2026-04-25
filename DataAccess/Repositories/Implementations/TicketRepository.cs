using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace DataAccess.Repositories.Implementations;

public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
{
    public TicketRepository(ProxarDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Ticket>> GetAllWithClientsAsync()
    {
        return await _dbSet
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByStatusAsync(TicketState status)
    {
        return await _dbSet
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByAssignedUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Where(t => t.AssignedToId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByClientAsync(Guid clientId)
    {
        return await _dbSet
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Ticket?> GetByNumberAsync(int number)
    {
        return await _dbSet
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Number == number);
    }

    public async Task<Ticket?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.History)
                .ThenInclude(h => h.User)
            .Include(t => t.Movements)
                .ThenInclude(m => m.Account)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}