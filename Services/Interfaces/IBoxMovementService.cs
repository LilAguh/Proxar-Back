using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IBoxMovementService
{
    Task<BoxMovementDto> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<BoxMovementDto>> GetAllByCompanyAsync(Guid companyId);
    Task<IEnumerable<BoxMovementDto>> GetByAccountAsync(Guid accountId, Guid companyId);
    Task<IEnumerable<BoxMovementDto>> GetByTicketAsync(Guid ticketId, Guid companyId);
    Task<BoxMovementDto> RegisterMovementAsync(RegisterMovementRequest request, Guid userId, Guid companyId);
    Task SoftDeleteMovementAsync(Guid id, Guid companyId, Guid deletedBy);
}