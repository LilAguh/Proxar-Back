using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories.Implementations;

public class CompanyRepository : ICompanyRepository
{
    private readonly ProxarDbContext _context;

    public CompanyRepository(ProxarDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(Guid id)
    {
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == id && c.Active);
    }

    public async Task<Company?> GetBySlugAsync(string slug)
    {
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.Slug == slug && c.Active);
    }

    public async Task<IEnumerable<Company>> GetAllActiveAsync()
    {
        return await _context.Companies
            .Where(c => c.Active)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Company> CreateAsync(Company company)
    {
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return company;
    }

    public async Task<Company> UpdateAsync(Company company)
    {
        _context.Companies.Update(company);
        await _context.SaveChangesAsync();
        return company;
    }

    public async Task SoftDeleteAsync(Guid id, Guid deletedBy)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company != null)
        {
            company.Active = false;
            company.DeletedAt = DateTime.UtcNow;
            company.DeletedBy = deletedBy;
            await _context.SaveChangesAsync();
        }
    }
}