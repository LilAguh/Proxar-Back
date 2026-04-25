using AutoMapper;
using DataAccess.Repositories.Interfaces;
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
}