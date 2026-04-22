using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface ITicketService
{
    Task<IEnumerable<TicketDto>> GetAllTicketsAsync();
    Task<TicketDto?> GetTicketByIdAsync(Guid id);
    Task<TicketDetailDto?> GetTicketWithDetailsAsync(Guid id);
    Task<TicketDto> CreateTicketAsync(CreateTicketRequest request, Guid createdByUserId);
    Task<TicketDto> UpdateTicketStatusAsync(Guid id, UpdateTicketStatusRequest request, Guid userId);
    Task<TicketDto> AssignTicketAsync(Guid id, AssignTicketRequest request);
    Task<IEnumerable<TicketDto>> GetTicketsByStatusAsync(TicketState status);
    Task<IEnumerable<TicketDto>> GetTicketsByAssignedUserAsync(Guid userId);
    Task<IEnumerable<TicketDto>> GetTicketsByClientAsync(Guid clientId);
}