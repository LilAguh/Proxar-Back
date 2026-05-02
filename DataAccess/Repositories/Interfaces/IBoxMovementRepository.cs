using Models;
using Models.Enums;

namespace DataAccess.Repositories.Interfaces;

public interface IBoxMovementRepository
{
    Task<BoxMovement?> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<BoxMovement>> GetAllByCompanyAsync(Guid companyId);
    Task<IEnumerable<BoxMovement>> GetAllWithDetailsAsync(Guid companyId);
    Task<(IEnumerable<BoxMovement> Items, int TotalCount)> GetPagedByCompanyAsync(Guid companyId, int page, int pageSize, MovementType? type = null);
    Task<IEnumerable<BoxMovement>> GetByAccountAsync(Guid accountId, Guid companyId);
    Task<IEnumerable<BoxMovement>> GetByTicketAsync(Guid ticketId, Guid companyId);
    Task<IEnumerable<BoxMovement>> GetByDateRangeAsync(DateTime from, DateTime to, Guid companyId);
    Task<List<BoxMovement>> GetFilteredAsync(
        Guid companyId,
        DateTime? dateFrom,
        DateTime? dateTo,
        Guid? accountId,
        Guid? ticketId,
        string? type,
        string? paymentMethod);
    Task<BoxMovement> AddAsync(BoxMovement movement);
    Task UpdateAsync(BoxMovement movement);
    Task SoftDeleteAsync(Guid id, Guid companyId, Guid deletedBy);
    Task SoftDeleteByTicketAsync(Guid ticketId, Guid companyId, Guid deletedBy);
}
