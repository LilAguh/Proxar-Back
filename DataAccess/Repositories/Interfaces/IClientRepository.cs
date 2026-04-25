using Models;

namespace DataAccess.Repositories.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<Client>> GetAllByCompanyAsync(Guid companyId);
    Task<IEnumerable<Client>> GetActiveByCompanyAsync(Guid companyId);
    Task<Client> AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task SoftDeleteAsync(Guid id, Guid companyId, Guid deletedBy);
}