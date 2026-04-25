using Models;
using Models.Enums;

namespace DataAccess.Repositories.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<Ticket>> GetAllByCompanyAsync(Guid companyId);
    Task<IEnumerable<Ticket>> GetAllWithDetailsAsync(Guid companyId);
    Task<IEnumerable<Ticket>> GetByStatusAsync(TicketState status, Guid companyId);
    Task<IEnumerable<Ticket>> GetByClientAsync(Guid clientId, Guid companyId);
    Task<IEnumerable<Ticket>> GetByAssignedUserAsync(Guid userId, Guid companyId);
    Task<Ticket> AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);
    Task SoftDeleteAsync(Guid id, Guid companyId, Guid deletedBy);
}