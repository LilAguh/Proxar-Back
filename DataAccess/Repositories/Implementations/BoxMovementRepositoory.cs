using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace DataAccess.Repositories.Implementations;

public class BoxMovementRepository : GenericRepository<BoxMovement>, IBoxMovementRepository
{
    public BoxMovementRepository(ProxarDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<BoxMovement>> GetAllAsync()
    {
        return await _dbSet
            .Include(m => m.Account)
            .Include(m => m.Ticket)
            .Include(m => m.User)
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoxMovement>> GetByAccountAsync(Guid accountId)
    {
        return await _dbSet
            .Include(m => m.Account)
            .Include(m => m.Ticket)
            .Include(m => m.User)
            .Where(m => m.AccountId == accountId)
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoxMovement>> GetByTicketAsync(Guid ticketId)
    {
        return await _dbSet
            .Include(m => m.Account)
            .Include(m => m.User)
            .Where(m => m.TicketId == ticketId)
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoxMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(m => m.Account)
            .Include(m => m.Ticket)
            .Include(m => m.User)
            .Where(m => m.MovementDate >= startDate && m.MovementDate <= endDate)
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoxMovement>> GetByTypeAsync(MovementType type)
    {
        return await _dbSet
            .Include(m => m.Account)
            .Include(m => m.Ticket)
            .Include(m => m.User)
            .Where(m => m.Type == type)
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync();
    }
}