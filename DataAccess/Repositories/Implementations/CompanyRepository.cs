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
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Company?> GetBySlugAsync(string slug)
    {
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.Slug == slug.ToLower());
    }

    public async Task<IEnumerable<Company>> GetAllActiveAsync()
    {
        return await _context.Companies
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Company> AddAsync(Company company)
    {
        company.Slug = company.Slug.ToLower().Trim();
        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();
        return company;
    }

    public async Task UpdateAsync(Company company)
    {
        _context.Companies.Update(company);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id, Guid deletedBy)
    {
        var company = await GetByIdAsync(id);
        if (company == null)
            throw new KeyNotFoundException("Empresa no encontrada");

        company.Active = false;
        company.DeletedAt = DateTime.UtcNow;

        await UpdateAsync(company);
    }
}