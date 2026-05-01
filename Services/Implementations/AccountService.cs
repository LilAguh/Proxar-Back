using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Exceptions;
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

    public async Task<AccountDto> GetByIdAsync(Guid id, Guid companyId)
    {
        var account = await _accountRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Account.NotFound);

        return _mapper.Map<AccountDto>(account);
    }

    public async Task<IEnumerable<AccountDto>> GetAllByCompanyAsync(Guid companyId)
    {
        var accounts = await _accountRepository.GetAllByCompanyAsync(companyId);
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<IEnumerable<AccountDto>> GetActiveByCompanyAsync(Guid companyId)
    {
        var accounts = await _accountRepository.GetActiveByCompanyAsync(companyId);
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<Dictionary<Guid, decimal>> GetBalancesByCompanyAsync(Guid companyId)
    {
        var accounts = await _accountRepository.GetActiveByCompanyAsync(companyId);
        return accounts.ToDictionary(a => a.Id, a => a.CurrentBalance);
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request, Guid companyId)
    {
        var account = new Account
        {
            CompanyId = companyId,
            Name = request.Name.Trim(),
            Type = request.Type,
            CurrentBalance = request.InitialBalance,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var created = await _accountRepository.AddAsync(account);
        return _mapper.Map<AccountDto>(created);
    }

    public async Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountRequest request, Guid companyId)
    {
        var account = await _accountRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Account.NotFound);

        account.Name = request.Name.Trim();
        account.Type = request.Type;
        account.Active = request.Active;

        await _accountRepository.UpdateAsync(account);
        return _mapper.Map<AccountDto>(account);
    }

    public async Task DeleteAccountAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        await _accountRepository.SoftDeleteAsync(id, companyId, deletedBy);
    }
}
