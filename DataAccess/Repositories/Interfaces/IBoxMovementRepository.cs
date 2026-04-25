using Models;

namespace DataAccess.Repositories.Interfaces;

public interface IBoxMovementRepository
{
    Task<BoxMovement?> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<BoxMovement>> GetAllByCompanyAsync(Guid companyId);
    Task<IEnumerable<BoxMovement>> GetAllWithDetailsAsync(Guid companyId);
    Task<IEnumerable<BoxMovement>> GetByAccountAsync(Guid accountId, Guid companyId);
    Task<IEnumerable<BoxMovement>> GetByTicketAsync(Guid ticketId, Guid companyId);
    Task<IEnumerable<BoxMovement>> GetByDateRangeAsync(DateTime from, DateTime to, Guid companyId);
    Task<BoxMovement> AddAsync(BoxMovement movement);
    Task UpdateAsync(BoxMovement movement);
    Task SoftDeleteAsync(Guid id, Guid companyId, Guid deletedBy);
}