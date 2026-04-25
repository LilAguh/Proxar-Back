using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAllByCompanyAsync(Guid companyId);
    Task<IEnumerable<AccountDto>> GetActiveByCompanyAsync(Guid companyId);
    Task<Dictionary<Guid, decimal>> GetBalancesByCompanyAsync(Guid companyId);
}