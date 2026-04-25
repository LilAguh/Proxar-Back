using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories.Implementations;

public class BoxMovementRepository : IBoxMovementRepository
{
    private readonly ProxarDbContext _context;

    public BoxMovementRepository(ProxarDbContext context)
    {
        _context = context;
    }

    public async Task<BoxMovement?> GetByIdAsync(Guid id, Guid companyId)
    {
        return await _context.BoxMovements
            .Include(bm => bm.Account)
            .Include(bm => bm.Ticket)
            .Include(bm => bm.User)
            .FirstOrDefaultAsync(bm => bm.Id == id && bm.CompanyId == companyId);
    }

    public async Task<IEnumerable<BoxMovement>> GetAllByCompanyAsync(Guid companyId)
    {
        return await _context.BoxMovements
            .Where(bm => bm.CompanyId == companyId)
            .OrderByDescending(bm => bm.MovementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoxMovement>> GetAllWithDetailsAsync(Guid companyId)
    {
        return await _context.BoxMovements
            .Include(bm => bm.Account)
            .Include(bm => bm.Ticket)
            .Include(bm => bm.User)
            .Where(bm => bm.CompanyId == companyId)
            .OrderByDescending(bm => bm.MovementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoxMovement>> GetByAccountAsync(Guid accountId, Guid companyId)
    {
        return await _context.BoxMovements
            .Include(bm => bm.Account)
            .Include(bm => bm.Ticket)
            .Include(bm => bm.User)
            .Where(bm => bm.AccountId == accountId && bm.CompanyId == companyId)
            .OrderByDescending(bm => bm.MovementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoxMovement>> GetByTicketAsync(Guid ticketId, Guid companyId)
    {
        return await _context.BoxMovements
            .Include(bm => bm.Account)
            .Include(bm => bm.Ticket)
            .Include(bm => bm.User)
            .Where(bm => bm.TicketId == ticketId && bm.CompanyId == companyId)
            .OrderByDescending(bm => bm.MovementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoxMovement>> GetByDateRangeAsync(DateTime from, DateTime to, Guid companyId)
    {
        return await _context.BoxMovements
            .Include(bm => bm.Account)
            .Include(bm => bm.Ticket)
            .Include(bm => bm.User)
            .Where(bm => bm.MovementDate >= from && 
                         bm.MovementDate <= to && 
                         bm.CompanyId == companyId)
            .OrderByDescending(bm => bm.MovementDate)
            .ToListAsync();
    }

    public async Task<BoxMovement> AddAsync(BoxMovement movement)
    {
        await _context.BoxMovements.AddAsync(movement);
        await _context.SaveChangesAsync();
        
        // Reload with relations
        return (await GetByIdAsync(movement.Id, movement.CompanyId))!;
    }

    public async Task UpdateAsync(BoxMovement movement)
    {
        _context.BoxMovements.Update(movement);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        var movement = await GetByIdAsync(id, companyId);
        if (movement == null)
            throw new KeyNotFoundException("Movimiento no encontrado");

        movement.Active = false;
        movement.DeletedAt = DateTime.UtcNow;
        movement.DeletedBy = deletedBy;

        await UpdateAsync(movement);
    }
}