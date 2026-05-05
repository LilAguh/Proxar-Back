using Models;
using Models.Enums;

namespace DataAccess.Repositories.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<Budget>> GetAllAsync(Guid companyId, int page = 1, int pageSize = 50);
    Task<IEnumerable<Budget>> GetByTicketIdAsync(Guid ticketId, Guid companyId, int page = 1, int pageSize = 50);
    Task<Budget> AddAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task UpdateStatusAsync(Guid id, BudgetStatus status, Guid companyId);
    Task<int> GetNextNumberAsync(Guid companyId);
    Task<bool> ExistsAsync(Guid id, Guid companyId);
}
