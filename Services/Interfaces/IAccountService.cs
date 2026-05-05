using Services.DTOs.Responses;
using Services.DTOs.Requests;

namespace Services.Interfaces;

public interface IAccountService
{
    Task<AccountDto> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<AccountDto>> GetAllByCompanyAsync(Guid companyId);
    Task<IEnumerable<AccountDto>> GetActiveByCompanyAsync(Guid companyId);
    Task<Dictionary<Guid, decimal>> GetBalancesByCompanyAsync(Guid companyId);
    Task<AccountDto> CreateAccountAsync(CreateAccountRequest request, Guid companyId);
    Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountRequest request, Guid companyId);
    Task DeleteAccountAsync(Guid id, Guid companyId, Guid deletedBy);
    Task<RecalculateBalanceDto> RecalculateBalanceAsync(Guid id, Guid companyId, bool autoCorrect = false);
}
