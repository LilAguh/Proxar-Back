using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Models.Enums;

namespace Services.Interfaces;

public interface ITicketService
{
    Task<TicketDto> GetByIdAsync(Guid id, Guid companyId);
    Task<TicketDetailDto> GetDetailsAsync(Guid id, Guid companyId);
    Task<IEnumerable<TicketDto>> GetAllByCompanyAsync(Guid companyId);
    Task<IEnumerable<TicketDto>> GetByStatusAsync(TicketState status, Guid companyId);
    Task<IEnumerable<TicketDto>> GetByClientAsync(Guid clientId, Guid companyId);
    Task<IEnumerable<TicketDto>> GetByAssignedUserAsync(Guid userId, Guid companyId);
    Task<TicketDto> CreateTicketAsync(CreateTicketRequest request, Guid userId, Guid companyId);
    Task<TicketDto> UpdateTicketAsync(Guid id, UpdateTicketRequest request, Guid companyId);
    Task<TicketDto> UpdateTicketStatusAsync(Guid id, UpdateTicketStatusRequest request, Guid userId, Guid companyId);
    Task SoftDeleteTicketAsync(Guid id, Guid companyId, Guid deletedBy);
}