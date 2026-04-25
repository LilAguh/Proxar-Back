using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly ProxarDbContext _context;

    public UserRepository(ProxarDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, Guid companyId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId);
    }

    public async Task<User?> GetByEmailAsync(string email, Guid companyId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.CompanyId == companyId);
    }

    public async Task<IEnumerable<User>> GetAllByCompanyAsync(Guid companyId)
    {
        return await _context.Users
            .Where(u => u.CompanyId == companyId)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveByCompanyAsync(Guid companyId)
    {
        return await _context.Users
            .Where(u => u.CompanyId == companyId && u.Active)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<User> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        user.ModifiedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        var user = await GetByIdAsync(id, companyId);
        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        user.Active = false;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = deletedBy;

        await UpdateAsync(user);
    }
}