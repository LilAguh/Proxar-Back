using Models;

namespace DataAccess.Repositories.Interfaces;

public interface IClientRepository : IGenericRepository<Client>
{
    Task<IEnumerable<Client>> SearchByNameAsync(string name);
    Task<Client?> GetByPhoneAsync(string phone);
}