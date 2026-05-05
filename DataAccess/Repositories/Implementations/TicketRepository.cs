using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace DataAccess.Repositories.Implementations;

public class TicketRepository : ITicketRepository
{
    private readonly ProxarDbContext _context;

    public TicketRepository(ProxarDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id, Guid companyId)
    {
        return await _context.Tickets
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);
    }

    public async Task<IEnumerable<Ticket>> GetAllByCompanyAsync(Guid companyId)
    {
        return await _context.Tickets
            .Where(t => t.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetAllWithDetailsAsync(Guid companyId)
    {
        return await _context.Tickets
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByStatusAsync(TicketState status, Guid companyId)
    {
        return await _context.Tickets
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.Status == status && t.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByClientAsync(Guid clientId, Guid companyId)
    {
        return await _context.Tickets
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.ClientId == clientId && t.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByAssignedUserAsync(Guid userId, Guid companyId)
    {
        return await _context.Tickets
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.AssignedToId == userId && t.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Ticket>> GetFilteredAsync(
        Guid companyId,
        DateTime? dateFrom,
        DateTime? dateTo,
        Guid? clientId,
        string? state,
        string? type,
        string? priority,
        Guid? assignedToId,
        Guid? createdById)
    {
        var query = _context.Tickets
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.CompanyId == companyId);

        if (dateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= dateTo.Value);

        if (clientId.HasValue)
            query = query.Where(t => t.ClientId == clientId.Value);

        if (!string.IsNullOrWhiteSpace(state) && Enum.TryParse<TicketState>(state, out var ticketState))
            query = query.Where(t => t.Status == ticketState);

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<TicketType>(type, out var ticketType))
            query = query.Where(t => t.Type == ticketType);

        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<Priority>(priority, out var ticketPriority))
            query = query.Where(t => t.Priority == ticketPriority);

        if (assignedToId.HasValue)
            query = query.Where(t => t.AssignedToId == assignedToId.Value);

        if (createdById.HasValue)
            query = query.Where(t => t.CreatedById == createdById.Value);

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<Ticket> AddAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();
        
        // Reload with relations
        return (await GetByIdAsync(ticket.Id, ticket.CompanyId))!;
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        ticket.LastUpdatedAt = DateTime.UtcNow;

        // Evita propagar modificaciones sobre entidades de navegación incluidas (Client/User).
        _context.Entry(ticket).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        var ticket = await GetByIdAsync(id, companyId);
        if (ticket == null)
            throw new KeyNotFoundException("Ticket no encontrado");

        ticket.Active = false;
        ticket.DeletedAt = DateTime.UtcNow;
        ticket.DeletedBy = deletedBy;

        await UpdateAsync(ticket);
    }
}
