using Models;

namespace DataAccess.Repositories.Interfaces;

public interface ITicketHistoryRepository
{
    Task<IEnumerable<TicketHistory>> GetByTicketIdAsync(Guid ticketId);
    Task<TicketHistory> AddAsync(TicketHistory history);
    Task SoftDeleteByTicketAsync(Guid ticketId, Guid companyId, Guid deletedBy);
}