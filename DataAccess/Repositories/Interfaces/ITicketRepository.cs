using Models;
using Models.Enums;

namespace DataAccess.Repositories.Interfaces;

public interface ITicketRepository : IGenericRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetAllWithClientsAsync();
    Task<IEnumerable<Ticket>> GetByStatusAsync(TicketState status);
    Task<IEnumerable<Ticket>> GetByAssignedUserAsync(Guid userId);
    Task<IEnumerable<Ticket>> GetByClientAsync(Guid clientId);
    Task<Ticket?> GetByNumberAsync(int number);
    Task<Ticket?> GetWithDetailsAsync(Guid id);
}