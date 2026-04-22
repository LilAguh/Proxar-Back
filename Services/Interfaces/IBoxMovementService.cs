using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IBoxMovementService
{
    Task<IEnumerable<BoxMovementDto>> GetAllMovementsAsync();
    Task<BoxMovementDto?> GetMovementByIdAsync(Guid id);
    Task<BoxMovementDto> RegisterMovementAsync(RegisterMovementRequest request, Guid userId);
    Task<IEnumerable<BoxMovementDto>> GetMovementsByAccountAsync(Guid accountId);
    Task<IEnumerable<BoxMovementDto>> GetMovementsByTicketAsync(Guid ticketId);
    Task<IEnumerable<BoxMovementDto>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Dictionary<Guid, decimal>> GetAccountBalancesAsync();
}