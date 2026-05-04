using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

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

    public async Task<(IEnumerable<BoxMovement> Items, int TotalCount)> GetPagedByCompanyAsync(
        Guid companyId,
        int page,
        int pageSize,
        MovementType? type = null)
    {
        var query = _context.BoxMovements
            .Include(bm => bm.Account)
            .Include(bm => bm.Ticket)
            .Include(bm => bm.User)
            .Where(bm => bm.CompanyId == companyId);

        if (type.HasValue)
        {
            query = query.Where(bm => bm.Type == type.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(bm => bm.MovementDate)
            .ThenByDescending(bm => bm.RegisteredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
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

    public async Task<List<BoxMovement>> GetFilteredAsync(
        Guid companyId,
        DateTime? dateFrom,
        DateTime? dateTo,
        Guid? accountId,
        Guid? ticketId,
        string? type,
        string? paymentMethod)
    {
        var query = _context.BoxMovements
            .Include(bm => bm.Account)
            .Include(bm => bm.Ticket)
            .Include(bm => bm.User)
            .Where(bm => bm.CompanyId == companyId);

        if (dateFrom.HasValue)
            query = query.Where(bm => bm.MovementDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(bm => bm.MovementDate <= dateTo.Value);

        if (accountId.HasValue)
            query = query.Where(bm => bm.AccountId == accountId.Value);

        if (ticketId.HasValue)
            query = query.Where(bm => bm.TicketId == ticketId.Value);

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<MovementType>(type, out var movementType))
            query = query.Where(bm => bm.Type == movementType);

        if (!string.IsNullOrWhiteSpace(paymentMethod) && Enum.TryParse<PaymentMethod>(paymentMethod, out var method))
            query = query.Where(bm => bm.Method == method);

        return await query.OrderByDescending(bm => bm.MovementDate).ToListAsync();
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
        // CRÍTICO: Usar ExecuteUpdateAsync para evitar sobrescribir saldo de Account
        // Si usamos Update(movement), el grafo cargado (con Account de saldo viejo)
        // sobrescribiría la actualización atómica del saldo hecha previamente
        var now = DateTime.UtcNow;

        var affectedRows = await _context.BoxMovements
            .Where(m => m.Id == id && m.CompanyId == companyId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.Active, false)
                .SetProperty(m => m.DeletedAt, now)
                .SetProperty(m => m.DeletedBy, deletedBy));

        if (affectedRows == 0)
            throw new KeyNotFoundException("Movimiento no encontrado");
    }

    public async Task SoftDeleteByTicketAsync(Guid ticketId, Guid companyId, Guid deletedBy)
    {
        var now = DateTime.UtcNow;

        await _context.BoxMovements
            .Where(m => m.TicketId == ticketId && m.CompanyId == companyId && m.Active)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.Active, false)
                .SetProperty(m => m.DeletedAt, now)
                .SetProperty(m => m.DeletedBy, deletedBy));
    }
}
