using Models;

namespace DataAccess.Repositories.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id);
    Task<Company?> GetBySlugAsync(string slug);
    Task<IEnumerable<Company>> GetAllActiveAsync();
    Task<Company> AddAsync(Company company);
    Task UpdateAsync(Company company);
    Task SoftDeleteAsync(Guid id, Guid deletedBy);
}