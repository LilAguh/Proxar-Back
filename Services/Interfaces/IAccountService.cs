using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAllAccountsAsync();
    Task<IEnumerable<AccountDto>> GetActiveAccountsAsync();
    Task<AccountDto?> GetAccountByIdAsync(Guid id);
    Task<AccountDto> CreateAccountAsync(CreateAccountRequest request);
    Task<AccountDto> UpdateAccountAsync(Guid id, CreateAccountRequest request);
    Task DeleteAccountAsync(Guid id);
}