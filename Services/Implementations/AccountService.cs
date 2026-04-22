using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Models;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace Services.Implementations;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;

    public AccountService(IAccountRepository accountRepository, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
    {
        var accounts = await _accountRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<IEnumerable<AccountDto>> GetActiveAccountsAsync()
    {
        var accounts = await _accountRepository.GetActiveAccountsAsync();
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<AccountDto?> GetAccountByIdAsync(Guid id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        return account != null ? _mapper.Map<AccountDto>(account) : null;
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request)
    {
        var account = _mapper.Map<Account>(request);
        account.CreatedAt = DateTime.UtcNow;
        account.ModifiedAt = DateTime.UtcNow;

        var createdAccount = await _accountRepository.AddAsync(account);
        return _mapper.Map<AccountDto>(createdAccount);
    }

    public async Task<AccountDto> UpdateAccountAsync(Guid id, CreateAccountRequest request)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        
        if (account == null)
            throw new KeyNotFoundException($"Account with ID {id} not found");

        account.Name = request.Name;
        account.Type = request.Type;
        account.ModifiedAt = DateTime.UtcNow;

        await _accountRepository.UpdateAsync(account);
        return _mapper.Map<AccountDto>(account);
    }

    public async Task DeleteAccountAsync(Guid id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        
        if (account == null)
            throw new KeyNotFoundException($"Account with ID {id} not found");

        await _accountRepository.DeleteAsync(account);
    }
}