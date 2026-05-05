using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Models.Enums;

namespace Services.Interfaces;

public interface IBoxMovementService
{
    Task<BoxMovementDto> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<BoxMovementDto>> GetAllByCompanyAsync(Guid companyId);
    Task<PagedResultDto<BoxMovementDto>> GetPagedByCompanyAsync(Guid companyId, int page, int pageSize, MovementType? type = null);
    Task<IEnumerable<BoxMovementDto>> GetByAccountAsync(Guid accountId, Guid companyId);
    Task<IEnumerable<BoxMovementDto>> GetByTicketAsync(Guid ticketId, Guid companyId);
    Task<IEnumerable<BoxMovementDto>> GetByDateRangeAsync(DateTime from, DateTime to, Guid companyId);
    Task<BoxMovementDto> RegisterMovementAsync(RegisterMovementRequest request, Guid userId, Guid companyId);
    Task SoftDeleteMovementAsync(Guid id, Guid companyId, Guid deletedBy);
}
