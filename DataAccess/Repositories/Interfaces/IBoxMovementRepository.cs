using Models;
using Models.Enums;

namespace DataAccess.Repositories.Interfaces;

public interface IBoxMovementRepository : IGenericRepository<BoxMovement>
{
    Task<IEnumerable<BoxMovement>> GetByAccountAsync(Guid accountId);
    Task<IEnumerable<BoxMovement>> GetByTicketAsync(Guid ticketId);
    Task<IEnumerable<BoxMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<BoxMovement>> GetByTypeAsync(MovementType type);
}