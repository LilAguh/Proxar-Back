using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories.Implementations;

public class ClientRepository : GenericRepository<Client>, IClientRepository
{
    public ClientRepository(ProxarDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Client>> SearchByNameAsync(string name)
    {
        return await _dbSet
            .Where(c => c.Name.ToLower().Contains(name.ToLower()))
            .ToListAsync();
    }

    public async Task<Client?> GetByPhoneAsync(string phone)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Phone == phone);
    }
}