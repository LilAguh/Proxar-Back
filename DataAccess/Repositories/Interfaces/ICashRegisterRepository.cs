using Models;

namespace DataAccess.Repositories.Interfaces;

public interface ICashRegisterRepository
{
    Task<CashRegister?> GetByIdAsync(Guid id, Guid companyId);
    Task<CashRegister?> GetTodayAsync(Guid companyId, DateTime businessDate);
    Task<CashRegister?> GetPreviousClosedAsync(Guid companyId, DateTime beforeDate);
    Task<IEnumerable<CashRegister>> GetHistoryAsync(Guid companyId, int page, int pageSize);
    Task<CashRegister> AddAsync(CashRegister register);
    Task UpdateAsync(CashRegister register);
}
