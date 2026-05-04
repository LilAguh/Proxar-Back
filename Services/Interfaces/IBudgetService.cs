using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IBudgetService
{
    Task<BudgetDto> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<BudgetDto>> GetAllAsync(Guid companyId, int page = 1, int pageSize = 50);
    Task<IEnumerable<BudgetDto>> GetByTicketIdAsync(Guid ticketId, Guid companyId);
    Task<BudgetDto> CreateBudgetAsync(CreateBudgetRequest request, Guid userId, Guid companyId);
    Task<BudgetDto> CreateDirectBudgetAsync(CreateDirectBudgetRequest request, Guid userId, Guid companyId);
    Task<BudgetDto> UpdateStatusAsync(Guid id, BudgetStatus status, Guid companyId);
    Task<byte[]> GetBudgetPdfAsync(Guid id, Guid companyId);
}
