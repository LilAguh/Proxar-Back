using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories.Implementations;

public class ClientRepository : IClientRepository
{
    private readonly ProxarDbContext _context;

    public ClientRepository(ProxarDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid id, Guid companyId)
    {
        return await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);
    }

    public async Task<IEnumerable<Client>> GetAllByCompanyAsync(Guid companyId)
    {
        return await _context.Clients
            .Where(c => c.CompanyId == companyId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Client>> GetActiveByCompanyAsync(Guid companyId)
    {
        return await _context.Clients
            .Where(c => c.CompanyId == companyId && c.Active)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Client> AddAsync(Client client)
    {
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task UpdateAsync(Client client)
    {
        client.ModifiedAt = DateTime.UtcNow;
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        var client = await GetByIdAsync(id, companyId);
        if (client == null)
            throw new KeyNotFoundException("Cliente no encontrado");

        client.Active = false;
        client.DeletedAt = DateTime.UtcNow;
        client.DeletedBy = deletedBy;

        await UpdateAsync(client);
    }
}